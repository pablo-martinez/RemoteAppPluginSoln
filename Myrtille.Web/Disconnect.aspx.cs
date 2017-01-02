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
    public partial class Disconnect : Page
    {
        class RequestParams {
            public string sessionId;
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

                sessionId = requestParams.sessionId;
                // retrieve the remote session manager for the requested session
                if (RemoteSessionManager.CurrentSessions.ContainsKey(sessionId))
                {
                    remoteSessionManager = RemoteSessionManager.CurrentSessions[sessionId];
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to retrieve the remote session manager for the session ID {0}, ({1})", sessionId ?? "unspecified", exc);

                HttpContext.Current.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                HttpContext.Current.Response.Write($"{{\"message\":\"Failed to retrieve the remote session.\"}}");
                return;
            }

            // disconnect the active remote session, if any and connected
            if (remoteSessionManager.RemoteSession.State == RemoteSessionState.Connecting || remoteSessionManager.RemoteSession.State == RemoteSessionState.Connected)
            {
                try
                {
                    // update the remote session state
                    remoteSessionManager.RemoteSession.State = RemoteSessionState.Disconnecting;

                    // send a disconnect command to the rdp client
                    remoteSessionManager.SendCommand(RemoteSessionCommand.CloseRdpClient);
                }
                catch (Exception exc)
                {
                    System.Diagnostics.Trace.TraceError("Failed to disconnect remote session {0} ({1})", remoteSessionManager.RemoteSession.Id, exc);

                    HttpContext.Current.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    HttpContext.Current.Response.Write($"{{\"message\":\"Failed to disconnect the remote session.\"}}");
                    return;
                }
            }
        }
    }
}