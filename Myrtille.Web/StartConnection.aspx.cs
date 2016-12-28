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
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Myrtille.Web
{
    public partial class StartConnection: Page
    {
        class RequestParams {
            public int configID;
            public string program;
        }

        /// <summary>
        /// send user input(s) (mouse, keyboard) to the rdp session
        /// if long-polling is disabled (xhr only), also returns image data within the response
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            string sessionId = null;
            RemoteSessionManager remoteSessionManager = null;
            try
            {
                // Reading JSON from Request
                System.IO.Stream body = Request.InputStream;
                System.Text.Encoding encoding = Request.ContentEncoding;
                System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
                string requestBody = reader.ReadToEnd();

                var requestParams = Newtonsoft.Json.JsonConvert.DeserializeObject<RequestParams>(requestBody);

                var configID = requestParams.configID;
                var program = requestParams.program;

                var sessionConfig = SessionConfig.AvailableConfigs.Single(x => x.ID == configID);

                // retrieve params
                var serverAddress = sessionConfig.ServerAddress; //HttpContext.Current.Request.QueryString["serverAddress"];
                var userDomain = sessionConfig.UserDomain;//HttpContext.Current.Request.QueryString["userDomain"];
                var userName = sessionConfig.UserName;// HttpContext.Current.Request.QueryString["userName"];
                var userPassword = sessionConfig.UserPassword;//HttpContext.Current.Request.QueryString["userPassword"];
                var clientWidth = "1440";//HttpContext.Current.Request.QueryString["clientWidth"];
                var clientHeight = "810";// HttpContext.Current.Request.QueryString["clientHeight"];
                var statMode = false;// HttpContext.Current.Request.QueryString["statMode"] == "true";
                var debugMode = false;//HttpContext.Current.Request.QueryString["bandwidthRatio"] == "true";
                var compatibilityMode = false;//HttpContext.Current.Request.QueryString["bandwidthRatio"] == "true";
                //var program = "";// HttpContext.Current.Request.QueryString["bandwidthRatio"];

                var programName = string.IsNullOrEmpty(program) ? "Remote Desktop" : "Remote Apps";
                sessionId = $"{userName} - {programName}";//Guid.NewGuid().ToString().Substring(0, 8);

                // create the remote session manager
                remoteSessionManager = new RemoteSessionManager(
                    new RemoteSession
                    {
                        Id = RemoteSessionManager.NewId,
                        SessionId = sessionId,
                        State = RemoteSessionState.NotConnected,
                        ServerAddress = serverAddress,
                        UserDomain = userDomain,
                        UserName = userName,
                        UserPassword = userPassword,
                        ClientWidth = clientWidth,
                        ClientHeight = clientHeight,
                        StatMode = statMode,
                        DebugMode = debugMode,
                        CompatibilityMode = compatibilityMode,
                        Program = program
                    }
                );

                // register it at application level; used when there is no http context (i.e.: websockets)
                RemoteSessionManager.CurrentSessions.Add(sessionId, remoteSessionManager);

                System.Diagnostics.Trace.TraceError($"Added session {sessionId}. Current sessions: " + string.Join("\",\"", RemoteSessionManager.CurrentSessions.Keys));
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to create remote session ({0})", exc);
            }

            // connect it
            try
            {
                // update the remote session state
                remoteSessionManager.RemoteSession.State = RemoteSessionState.Connecting;

                // create pipes for this web gateway and the rdp client to talk
                remoteSessionManager.Pipes.CreatePipes();

                // the rdp client does connect the pipes when it starts; when it stops (either because it was closed, crashed or because the rdp session had ended), pipes are released
                // use http://technet.microsoft.com/en-us/sysinternals/dd581625 to track the existing pipes
                remoteSessionManager.Client.StartProcess(
                    remoteSessionManager.RemoteSession.Id,
                    remoteSessionManager.RemoteSession.ServerAddress,
                    remoteSessionManager.RemoteSession.UserDomain,
                    remoteSessionManager.RemoteSession.UserName,
                    remoteSessionManager.RemoteSession.UserPassword,
                    remoteSessionManager.RemoteSession.ClientWidth,
                    remoteSessionManager.RemoteSession.ClientHeight,
                    remoteSessionManager.RemoteSession.Program,
                    remoteSessionManager.RemoteSession.DebugMode);

                // write the output
                HttpContext.Current.Response.Write($"{{\"sessionId\":\"{sessionId}\"}}");
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to connect remote session {0} ({1})", remoteSessionManager.RemoteSession.Id, exc);
            }
        }
    }
}