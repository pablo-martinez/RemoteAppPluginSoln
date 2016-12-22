/*
    Myrtille: A native HTML4/5 Remote Desktop Protocol client.

    Copyright(c) 2014-2016 Cedric Coste

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Myrtille.Fleck;
using System.Linq;

namespace Myrtille.Web
{
    public class RemoteSessionSocketServer
    {
        private static Global _global;

        private static List<IWebSocketConnection> _sockets;
        private static object _socketsLock;

        public RemoteSessionSocketServer(Global global, IWebSocketServer server)
        {
            // websockets are not bound to an http context... the remote session manager must be retrieved from an application perspective
            _global = global;

            // logs
            FleckLog.Level = LogLevel.Warn;
            FleckLog.LogAction = (level, message, ex) =>
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        Trace.TraceInformation(message, ex);
                        break;

                    case LogLevel.Warn:
                        Trace.TraceWarning(message, ex);
                        break;

                    case LogLevel.Error:
                        Trace.TraceError(message, ex);
                        break;

                    default:
                        Trace.TraceInformation(message, ex);
                        break;
                }
            };

            // sockets
            _sockets = new List<IWebSocketConnection>();
            _socketsLock = new object();

            // start sockets server
            server.Start(socket =>
                {
                    socket.OnOpen = () => { Open(socket); };
                    socket.OnMessage = message => ProcessMessage(socket, message);
                    socket.OnError = exception => { Trace.TraceError("Websocket error {0}", exception); };
                    socket.OnClose = () => { Close(socket); };
                });
        }

        private static void Open(IWebSocketConnection socket)
        {
            try
            {
                lock (_socketsLock)
                {
                    _sockets.Add(socket);
                    Trace.TraceInformation("Added websocket, count: {0}", _sockets.Count);
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to register websocket ({0})", exc);
            }
        }

        private static void Close(IWebSocketConnection socket)
        {
            try
            {
                lock (_socketsLock)
                {
                    _sockets.Remove(socket);
                    Trace.TraceInformation("Removed websocket, count: {0}", _sockets.Count);
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to unregister websocket ({0})", exc);
            }
        }

        private static void ProcessMessage(IWebSocketConnection socket, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                Trace.TraceWarning("Received null or empty websocket message");
                return;
            }

            var msgParams = message.Split(new[] { "|" }, StringSplitOptions.None);
            if (msgParams.Length >= 2)
            {
                var httpSessionId = msgParams[1];

                RemoteSessionManager remoteSessionManager = null;

                try
                {
                    // retrieve the remote session manager for the given http session (the http session id is passed as parameter as the ws protocol doesn't pass it automatically)
                    var remoteSessionsManagers = (IDictionary<string, RemoteSessionManager>)_global.Application[HttpApplicationStateVariables.RemoteSessionsManagers.ToString()];
                    remoteSessionManager = remoteSessionsManagers[httpSessionId];
                }
                catch (Exception exc)
                {
                    Trace.TraceError("Failed to retrieve the remote session manager for the http session {0}, ({1})", httpSessionId, exc);
                    return;
                }

                try
                {
                    switch (msgParams[0])
                    {
                        // user input (mouse, keyboard)
                        case "input":

                            var data = msgParams[2];
                            var fsu = int.Parse(msgParams[3]) == 1;
                            var imgIdx = int.Parse(msgParams[4]);
                            var imgEncoding = msgParams[5];
                            var imgQuality = int.Parse(msgParams[6]);
                            var bandwidthRatio = int.Parse(msgParams[7]);
                            var newSocket = int.Parse(msgParams[8]) == 1;
                            var timestamp = long.Parse(msgParams[9]);

                            // if the socket is new, bind it to the remote session manager
                            // close the old one, if any
                            if (newSocket)
                            {
                                // ensure the remote session is still connected
                                if (remoteSessionManager.RemoteSession.State == RemoteSessionState.Disconnected)
                                {
                                    if (socket.IsAvailable)
                                    {
                                        socket.Send("disconnected");
                                    }
                                    return;
                                }

                                remoteSessionManager.WebSockets.Add(socket);
                            }

                            // image encoding
                            var encoding = ImageEncoding.JPEG;
                            Enum.TryParse(imgEncoding, true, out encoding);
                            remoteSessionManager.ImageEncoding = encoding;

                            // throttle the image quality depending on the bandwidth usage ratio
                            if (bandwidthRatio >= (int)ImageQualityTweakBandwidthRatio.HigherBound)
                            {
                                remoteSessionManager.ImageQuality = imgQuality / 4;
                            }
                            else if (bandwidthRatio >= (int)ImageQualityTweakBandwidthRatio.LowerBound && bandwidthRatio < (int)ImageQualityTweakBandwidthRatio.HigherBound)
                            {
                                remoteSessionManager.ImageQuality = imgQuality / 2;
                            }
                            else
                            {
                                remoteSessionManager.ImageQuality = imgQuality;
                            }

                            // if defined, send the user input(s) through the rdp session
                            if (!string.IsNullOrEmpty(data))
                            {
                                remoteSessionManager.SendUserEvent(data);
                            }

                            // if requested, ask for a fullscreen update
                            if (fsu)
                            {
                                Trace.TraceInformation("Requesting fullscreen update, remote session {0}", remoteSessionManager.RemoteSession.Id);
                                remoteSessionManager.SendCommand(RemoteSessionCommand.SendFullscreenUpdate);
                            }

                            // acknowledge the message processing with the given timestamp; it will be used by the client to compute the roundtrip delay
                            socket.Send("ack," + timestamp);

                            break;

                        // remote session has been disconnected; close the websocket
                        case "close":

                            foreach(var webSocket in remoteSessionManager.WebSockets.ToList())
                            {
                                webSocket.Close();
                                remoteSessionManager.WebSockets.Remove(webSocket);
                            }

                            break;
                    }
                }
                catch (Exception exc)
                {
                    Trace.TraceError("Failed to process websocket message, remote session {0} ({1})", remoteSessionManager.RemoteSession.Id, exc);
                }
            }
            else
            {
                Trace.TraceError("Failed to parse websocket message {0}", message);
            }
        }
    }
}