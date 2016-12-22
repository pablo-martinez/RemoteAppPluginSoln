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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.UI;
using System.Windows.Forms;

namespace Myrtille.Web
{
    public partial class GetCursor : Page
    {
        /// <summary>
        /// retrieve the mouse cursor from the rdp session and send it to the browser
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
                var imgIdx = int.Parse(HttpContext.Current.Request.QueryString["imgIdx"]);

                // retrieve image data
                var img = remoteSessionManager.GetCachedUpdate(imgIdx);
                var imgData = img != null ? Convert.FromBase64String(img.Base64Data) : null;
                if (imgData != null && imgData.Length > 0)
                {
                    var imgStream = new MemoryStream(imgData);
                    var bitmap = new Bitmap(imgStream);

                    //var cursor = CreateCursor(bitmap, img.PosX, img.PosY);
                    // TODO: find a way to save a cursor as .cur file? using .ico instead...

                    var icon = Icon.FromHandle(bitmap.GetHicon());

                    // TODO: IE
                    // IE does support .ico files for cursors, but an icon doesn't have an hotspot and there is no way to define it in IE...
                    // problem is, the user thinks clicking a specific spot and, in fact, isn't...
                    // also, the cursor blinks when it changes, and stays invisible as long as the user doesn't move the mouse...
                    // for these reasons, IE won't display custom cursors...

                    var iconStream = new MemoryStream();
                    icon.Save(iconStream);
                    var iconData = iconStream.ToArray();

                    // write the output
                    HttpContext.Current.Response.OutputStream.Write(iconData, 0, iconData.Length);
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to get mouse cursor, remote session {0} ({1})", remoteSessionManager.RemoteSession.Id, exc);
            }
        }

        #region Icon

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        /// <summary>
        /// create a mouse cursor from a bitmap
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="xHotSpot"></param>
        /// <param name="yHotSpot"></param>
        /// <returns></returns>
        private static Cursor CreateCursor(Bitmap bitmap, int xHotSpot, int yHotSpot)
        {
            var iconInfo = new IconInfo();
            GetIconInfo(bitmap.GetHicon(), ref iconInfo);
            iconInfo.xHotspot = xHotSpot;
            iconInfo.yHotspot = yHotSpot;
            iconInfo.fIcon = false;
            return new Cursor(CreateIconIndirect(ref iconInfo));
        }

        #endregion
    }
}