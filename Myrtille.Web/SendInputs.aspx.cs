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
    public partial class SendInputs : Page
    {
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
                var data = HttpContext.Current.Request.QueryString["data"];
                var fsu = int.Parse(HttpContext.Current.Request.QueryString["fsu"]) == 1;
                var imgIdx = int.Parse(HttpContext.Current.Request.QueryString["imgIdx"]);
                var imgEncoding = HttpContext.Current.Request.QueryString["imgEncoding"];
                var imgQuality = int.Parse(HttpContext.Current.Request.QueryString["imgQuality"]);
                var imgReturn = int.Parse(HttpContext.Current.Request.QueryString["imgReturn"]) == 1;
                var bandwidthRatio = int.Parse(HttpContext.Current.Request.QueryString["bandwidthRatio"]);

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
                // also, a websocket triggers periodical dummy xhr calls, without data, in order to keep the http session alive; such calls musn't close the websocket!
                if (!string.IsNullOrEmpty(data))
                {
                    remoteSessionManager.SendUserEvent(data);

                    // if a websocket is set at this step, the client had probably changed the rendering mode (html5 -> html4) or configuration while the remote session is active, then reloaded the page (F5)
                    // close the websocket (a new one will be set if the client change back to html5 again...)
                    if (remoteSessionManager.WebSocket != null)
                    {
                        System.Diagnostics.Trace.TraceInformation("Removing no longer used websocket (the client had probably changed the rendering mode from HTML5 to HTML4 then reloaded the page (F5)), remote session {0}", remoteSessionManager.RemoteSession.Id);
                        remoteSessionManager.WebSocket.Close();
                        remoteSessionManager.WebSocket = null;
                    }
                }

                // if requested, ask for a fullscreen update
                if (fsu)
                {
                    System.Diagnostics.Trace.TraceInformation("Requesting fullscreen update, remote session {0}", remoteSessionManager.RemoteSession.Id);
                    remoteSessionManager.SendCommand(RemoteSessionCommand.SendFullscreenUpdate);
                }

                // xhr only
                if (imgReturn)
                {
                    var image = remoteSessionManager.GetNextUpdate(imgIdx);
                    if (image != null)
                    {
                        System.Diagnostics.Trace.TraceInformation("Returning image {0} ({1}), remote session {2}", image.Idx, (image.Fullscreen ? "screen" : "region"), remoteSessionManager.RemoteSession.Id);

                        var imgData =
                            image.Idx + "," +
                            image.PosX + "," +
                            image.PosY + "," +
                            image.Width + "," +
                            image.Height + "," +
                            image.Format.ToString().ToLower() + "," +
                            image.Quality + "," +
                            image.Base64Data + "," +
                            image.Fullscreen.ToString().ToLower();

                        // write the output
                        HttpContext.Current.Response.Write(imgData);
                    }
                    // ensure the remote session is still connected
                    else if (remoteSessionManager.RemoteSession.State == RemoteSessionState.Disconnected)
                    {
                        HttpContext.Current.Response.Write("disconnected");
                    }
                    // the remote clipboard content was requested
                    else if (remoteSessionManager.ClipboardRequested)
                    {
                        HttpContext.Current.Response.Write(string.Format("clipboard|{0}", remoteSessionManager.ClipboardText));
                        remoteSessionManager.ClipboardRequested = false;
                    }
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to send user input(s), remote session {0} ({1})", remoteSessionManager.RemoteSession.Id, exc);
            }
        }
    }
}