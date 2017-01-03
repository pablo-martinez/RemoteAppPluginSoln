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
using System.Web;
using System.Web.UI;

namespace Myrtille.Web
{
    public partial class RemoteClipboard : Page
    {
        /// <summary>
        /// retrieve the remote session clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            // if cookies are enabled, the http session id is added to the http request headers; otherwise, it's added to the http request url
            // in both cases, the given http session is automatically bound to the current http context

            RemoteSessionManager remoteSessionManager = null;

            try
            {
                var sessionId = HttpContext.Current.Request.QueryString["sessionId"];

                // retrieve the remote session manager for the requested session
                if (RemoteSessionManager.CurrentSessions.ContainsKey(sessionId))
                {
                    remoteSessionManager = RemoteSessionManager.CurrentSessions[sessionId];
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to retrieve the remote session manager for the http session {0}, ({1})", HttpContext.Current.Session.SessionID, exc);
                return;
            }

            try
            {
                System.Diagnostics.Trace.TraceInformation("Requesting remote clipboard, remote session {0}", remoteSessionManager.RemoteSession.Id);
                remoteSessionManager.SendCommand(RemoteSessionCommand.RequestRemoteClipboard);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to retrieve remote clipboard, remote session {0} ({1})", remoteSessionManager.RemoteSession.Id, exc);
            }
        }
    }
}