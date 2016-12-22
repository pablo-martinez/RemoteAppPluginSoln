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
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Myrtille.Helpers
{
    public static class PipeHelper
    {
        /// <summary>
        /// write data to a named pipe
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="pipeName">named pipes don't have a "Name" property... passing it as param (used for error log)</param>
        /// <param name="message"></param>
        public static void WritePipeMessage(
            PipeStream pipe,
            string pipeName,
            string message)
        {
            try
            {
                if (pipe != null && pipe.IsConnected && pipe.CanWrite)
                {
                    var buffer = Encoding.UTF8.GetBytes(message);
                    pipe.Write(buffer, 0, buffer.Length);
                    pipe.Flush();
                }
            }
            catch (IOException)
            {
                Trace.TraceError("Failed to write message to pipe {0} (I/O error)", (string.IsNullOrEmpty(pipeName) ? "<unknown>" : pipeName));
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to write message to pipe {0} ({1})", (string.IsNullOrEmpty(pipeName) ? "<unknown>" : pipeName), exc);
            }
        }
    }
}