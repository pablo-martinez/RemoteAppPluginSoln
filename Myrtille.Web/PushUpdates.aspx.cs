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
    public partial class PushUpdates : Page
    {
        /// <summary>
        /// push image(s) updates(s) (region(s) or fullscreen(s)) from the rdp session to the browser
        /// this is done through a long-polling request (also known as reverse ajax or ajax comet) issued by a zero sized iframe
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
                // retrieve the remote session manager for the current http session
                remoteSessionManager = (RemoteSessionManager)HttpContext.Current.Session[HttpSessionStateVariables.RemoteSessionManager.ToString()];
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to retrieve the remote session manager for the http session {0}, ({1})", HttpContext.Current.Session.SessionID, exc);
                return;
            }

            try
            {
                // retrieve params
                var longPollingDuration = int.Parse(HttpContext.Current.Request.QueryString["longPollingDuration"]);
                var imgIdx = int.Parse(HttpContext.Current.Request.QueryString["imgIdx"]);

                // if a websocket is set at this step, the client had probably changed the rendering mode (html5 -> html4) or configuration while the remote session is active, then reloaded the page (F5)
                // close the websocket (a new one will be set if the client change back to html5 again...)
                if (remoteSessionManager.WebSocket != null)
                {
                    System.Diagnostics.Trace.TraceInformation("Removing no longer used websocket (the client had probably changed the rendering mode from HTML5 to HTML4 then reloaded the page (F5)), remote session {0}", remoteSessionManager.RemoteSession.Id);
                    remoteSessionManager.WebSocket.Close();
                    remoteSessionManager.WebSocket = null;
                }

                // stream image(s) data within the response for the given duration
                // the connection will be automatically reseted by the client when the request ends
                var startTime = DateTime.Now;
                var remainingTime = longPollingDuration;
                var currentImgIdx = imgIdx;

                while (remainingTime > 0)
                {
                    // ensure the remote session is still connected
                    if (remoteSessionManager.RemoteSession.State == RemoteSessionState.Disconnected)
                    {
                        // the remote session is disconnected, back to home page
                        HttpContext.Current.Response.Write("<script>parent.location.href = parent.getConfig().getHttpServerUrl();</script>");
                        HttpContext.Current.Response.Flush();
                        break;
                    }
                    // the remote clipboard content was requested
                    else if (remoteSessionManager.ClipboardRequested)
                    {
                        HttpContext.Current.Response.Write(string.Format("<script>parent.showDialogPopup('showDialogPopup', 'ShowDialog.aspx', 'Ctrl+C to copy to local clipboard (Cmd-C on Mac)', '{0}', true);</script>", remoteSessionManager.ClipboardText));
                        HttpContext.Current.Response.Flush();
                        remoteSessionManager.ClipboardRequested = false;
                    }

                    // retrieve the next update, if available; otherwise, wait it for the remaining time
                    var image = remoteSessionManager.GetNextUpdate(currentImgIdx, remainingTime);
                    if (image != null)
                    {
                        System.Diagnostics.Trace.TraceInformation("Pushing image {0} ({1}), remote session {2}", image.Idx, (image.Fullscreen ? "screen" : "region"), remoteSessionManager.RemoteSession.Id);

                        var imgData =
                            image.Idx + "," +
                            image.PosX + "," +
                            image.PosY + "," +
                            image.Width + "," +
                            image.Height + "," +
                            "'" + image.Format.ToString().ToLower() + "'," +
                            image.Quality + "," +
                            "'" + image.Base64Data + "'," +
                            image.Fullscreen.ToString().ToLower();

                        imgData = "<script>parent.pushImage(" + imgData + ");</script>";

                        // write the output
                        HttpContext.Current.Response.Write(imgData);
                        HttpContext.Current.Response.Flush();

                        currentImgIdx = image.Idx;
                    }

                    remainingTime = longPollingDuration - Convert.ToInt32((DateTime.Now - startTime).TotalMilliseconds);
                }
            }
            catch (HttpException)
            {
                // this occurs if the user reloads the page while the long-polling request is going on...
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to push display update(s), remote session {0} ({1})", remoteSessionManager.RemoteSession.Id, exc);
            }
        }
    }
}