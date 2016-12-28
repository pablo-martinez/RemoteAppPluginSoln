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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace Myrtille.Web
{
    public partial class Default : Page
    {
        protected RemoteSessionManager RemoteSessionManager;

        /// <summary>
        /// initialization
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            try
            {
                // disable the browser cache; in addition to a "noCache" dummy param, with current time, on long-polling and xhr requests
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to load myrtille ({0})", exc);
            }
        }

        /// <summary>
        /// force remove the .net viewstate hidden fields from page (large bunch of unwanted data in url)
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(
            HtmlTextWriter writer)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            var tw = new HtmlTextWriter(sw);
            base.Render(tw);
            var html = sb.ToString();
            html = Regex.Replace(html, "<input[^>]*id=\"(__VIEWSTATE)\"[^>]*>", string.Empty, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, "<input[^>]*id=\"(__VIEWSTATEGENERATOR)\"[^>]*>", string.Empty, RegexOptions.IgnoreCase);
            writer.Write(html);
        }

        /// <summary>
        /// stop the rdp session
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DisconnectButtonClick(
            object sender,
            EventArgs e)
        {
            // disconnect the active remote session, if any and connected
            if (RemoteSessionManager != null && (RemoteSessionManager.RemoteSession.State == RemoteSessionState.Connecting || RemoteSessionManager.RemoteSession.State == RemoteSessionState.Connected))
            {
                try
                {
                    // update the remote session state
                    RemoteSessionManager.RemoteSession.State = RemoteSessionState.Disconnecting;

                    // send a disconnect command to the rdp client
                    RemoteSessionManager.SendCommand(RemoteSessionCommand.CloseRdpClient);
                }
                catch (Exception exc)
                {
                    System.Diagnostics.Trace.TraceError("Failed to disconnect remote session {0} ({1})", RemoteSessionManager.RemoteSession.Id, exc);
                }
            }
        }
    }
}