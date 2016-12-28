﻿/*
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
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using Myrtille.Services.Contracts;
using System.Collections.Generic;
using System.Web;
using System.Linq;

namespace Myrtille.Web
{
    public class RemoteSessionProcessClient : DuplexClientBase<IRemoteSessionProcess>, IRemoteSessionProcess
    {
        private readonly RemoteSessionManager _remoteSessionManager;

        public RemoteSessionProcessClient(RemoteSessionManager remoteSessionManager, InstanceContext callbackContext)
            : base(callbackContext)
        {
            _remoteSessionManager = remoteSessionManager;
        }

        public void StartProcess(
            int remoteSessionId,
            string serverAddress,
            string userDomain,
            string userName,
            string userPassword,
            string clientWidth,
            string clientHeight,
            string program,
            bool debug)
        {
            Trace.TraceInformation("Calling service start process, remote session {0}", _remoteSessionManager.RemoteSession.Id);

            try
            {
                Channel.StartProcess(remoteSessionId, serverAddress, userDomain, userName, userPassword, clientWidth, clientHeight, program, debug);
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to call service start process, remote session {0} ({1})", _remoteSessionManager.RemoteSession.Id, exc);
                throw;
            }
        }

        public void StopProcess()
        {
            Trace.TraceInformation("Calling service stop process, remote session {0}", _remoteSessionManager.RemoteSession.Id);

            try
            {
                Channel.StopProcess();
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to call service stop process, remote session {0} ({1})", _remoteSessionManager.RemoteSession.Id, exc);
                throw;
            }
        }
    }

    [CallbackBehavior(UseSynchronizationContext = false)]
    public class RemoteSessionProcessClientCallback : IRemoteSessionProcessCallback
    {
        private readonly RemoteSessionManager _remoteSessionManager;

        public RemoteSessionProcessClientCallback(RemoteSessionManager remoteSessionManager)
        {
            _remoteSessionManager = remoteSessionManager;
        }

        public void ProcessExited()
        {
            Trace.TraceInformation("Received rdp client process exit notification, remote session {0}", _remoteSessionManager.RemoteSession.Id);

            try
            {
                // remote session is now disconnected
                _remoteSessionManager.RemoteSession.State = RemoteSessionState.Disconnected;

                // release the communication pipes, if any
                if (_remoteSessionManager.Pipes != null)
                {
                    _remoteSessionManager.Pipes.DeletePipes();
                }

                // if a websocket is set and available (connection not closed by client), send a disconnect notification
                if (_remoteSessionManager.WebSockets.Count > 0)
                {
                    foreach(var webSocket in _remoteSessionManager.WebSockets)
                    {
                        if (webSocket.IsAvailable)
                        {
                            webSocket.Send("disconnected");
                        }
                    }
                }
                // no websocket at this step can mean using xhr (with long-polling or not),
                // but it can also be because the browser wasn't fast enough to open a websocket before the rdp client was closed (due to a crash, connection or authentication issue, etc.)
                // a disconnect notification will be sent to the browser by the socket server if the remote session is disconnected in the context of a newly opened socket
                else
                {
                    lock (_remoteSessionManager.ImageEventLock)
                    {
                        // if waiting for a new image, leave
                        if (_remoteSessionManager.ImageEventPending)
                        {
                            _remoteSessionManager.ImageEventPending = false;
                            Monitor.Pulse(_remoteSessionManager.ImageEventLock);
                        }
                    }
                }

                RemoteSessionManager.CurrentSessions.Remove(_remoteSessionManager.RemoteSession.SessionId);
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to cleanup disconnected session, remote session {0} ({1})", _remoteSessionManager.RemoteSession.Id, exc);
                throw;
            }
        }
    }
}