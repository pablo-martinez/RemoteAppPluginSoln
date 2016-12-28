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
using System.Web;
using System.Web.UI;

namespace Myrtille.Web
{
    public partial class GetUpdate : Page
    {
        /// <summary>
        /// retrieve an image update (region or fullscreen) from the rdp session and send it to the browser
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

                // retrieve the remote session manager for requested session
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
                // retrieve params
                var imgIdx = int.Parse(HttpContext.Current.Request.QueryString["imgIdx"]);

                // retrieve image data
                var img = remoteSessionManager.GetCachedUpdate(imgIdx);
                var imgData = img != null ? Convert.FromBase64String(img.Base64Data) : null;
                if (imgData != null && imgData.Length > 0)
                {
                    // write the output
                    HttpContext.Current.Response.OutputStream.Write(imgData, 0, imgData.Length);
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to get display update, remote session {0} ({1})", remoteSessionManager.RemoteSession.Id, exc);
            }
        }
    }
}