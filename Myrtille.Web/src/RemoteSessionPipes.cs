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
using System.Security.AccessControl;
using Myrtille.Helpers;

namespace Myrtille.Web
{
    public class RemoteSessionPipes
    {
        public RemoteSession RemoteSession { get; private set; }

        // it's possible to have 2 ways pipes (duplex, using overlapped I/O), but it proven difficult to setup and raised concurrency access issues...
        // to keep things simple, using separate pipes...
        private NamedPipeServerStream _inputsPipe;
        public NamedPipeServerStream InputsPipe { get { return _inputsPipe; } }

        private NamedPipeServerStream _updatesPipe;
        public NamedPipeServerStream UpdatesPipe { get { return _updatesPipe; } }

        // the pipes buffers sizes must match the ones defined in the remote session process
        // in order to avoid overloading both the bandwidth and the browser, images are limited to 1024 KB each
        private const int _inputsPipeBufferSize = 131072; // 128 KB
        public int InputsPipeBufferSize { get { return _inputsPipeBufferSize; } }

        private const int _updatesPipeBufferSize = 1048576; // 1024 KB
        public int UpdatesPipeBufferSize { get { return _updatesPipeBufferSize; } }

        public delegate void ProcessUpdatesPipeMessageDelegate(byte[] msg);
        public ProcessUpdatesPipeMessageDelegate ProcessUpdatesPipeMessage { get; set; }

        public RemoteSessionPipes(RemoteSession remoteSession)
        {
            RemoteSession = remoteSession;
        }

        public void CreatePipes()
        {
            try
            {
                // close the pipes if already exist; they will be re-created below
                DeletePipes();

                // set the pipes access rights
                var pipeSecurity = new PipeSecurity();
                var pipeAccessRule = new PipeAccessRule(AccountHelper.GetEveryoneGroupName(), PipeAccessRights.ReadWrite, AccessControlType.Allow);
                pipeSecurity.AddAccessRule(pipeAccessRule);

                // create the pipes
                _inputsPipe = new NamedPipeServerStream(
                    "remotesession_" + RemoteSession.Id + "_inputs",
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous,
                    InputsPipeBufferSize,
                    InputsPipeBufferSize,
                    pipeSecurity);

                _updatesPipe = new NamedPipeServerStream(
                    "remotesession_" + RemoteSession.Id + "_updates",
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous,
                    UpdatesPipeBufferSize,
                    UpdatesPipeBufferSize,
                    pipeSecurity);

                // wait for client connection
                InputsPipe.BeginWaitForConnection(InputsPipeConnected, InputsPipe.GetHashCode());
                UpdatesPipe.BeginWaitForConnection(UpdatesPipeConnected, UpdatesPipe.GetHashCode());
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to create pipes, remote session {0} ({1})", RemoteSession.Id, exc);
            }
        }

        public void DeletePipes()
        {
            DisposePipe("remoteSession_" + RemoteSession.Id + "_inputs", ref _inputsPipe);
            DisposePipe("remoteSession_" + RemoteSession.Id + "_updates", ref _updatesPipe);
        }

        public bool PipesConnected
        {
            get
            {
                return ((InputsPipe != null) && (InputsPipe.IsConnected) &&
                        (UpdatesPipe != null) && (UpdatesPipe.IsConnected));
            }
        }

        private void InputsPipeConnected(IAsyncResult e)
        {
            try
            {
                if (InputsPipe != null)
                {
                    InputsPipe.EndWaitForConnection(e);
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to wait for connection on inputs pipe, remote session {0} ({1})", RemoteSession.Id, exc);
            }
        }

        private void UpdatesPipeConnected(IAsyncResult e)
        {
            try
            {
                if (UpdatesPipe != null)
                {
                    UpdatesPipe.EndWaitForConnection(e);
                    ReadUpdatesPipe();
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to wait for connection on updates pipe, remote session {0} ({1})", RemoteSession.Id, exc);
            }
        }

        private void ReadUpdatesPipe()
        {
            try
            {
                while (UpdatesPipe != null && UpdatesPipe.IsConnected && UpdatesPipe.CanRead)
                {
                    var msg = ReadUpdatesPipeMessage();
                    if (msg != null && msg.Length > 0)
                    {
                        ProcessUpdatesPipeMessage(msg);
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to read updates pipe, remote session {0} ({1})", RemoteSession.Id, exc);
            }
        }

        private byte[] ReadUpdatesPipeMessage()
        {
            try
            {
                var memoryStream = new MemoryStream();
                var buffer = new byte[UpdatesPipeBufferSize];

                do
                {
                    memoryStream.Write(buffer, 0, UpdatesPipe.Read(buffer, 0, buffer.Length));
                } while (UpdatesPipe != null && UpdatesPipe.IsConnected && UpdatesPipe.CanRead && !UpdatesPipe.IsMessageComplete);

                return memoryStream.ToArray();
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to read updates pipe message, remote session {0} ({1})", RemoteSession.Id, exc);
                return null;
            }
        }

        private void DisposePipe(string pipeName, ref NamedPipeServerStream pipe)
        {
            if (pipe != null)
            {
                try
                {
                    // CAUTION! closing a pipe in use can make .NET to crash! disconnect it first...
                    if (pipe.IsConnected)
                    {
                        pipe.WaitForPipeDrain();
                        pipe.Disconnect();
                    }

                    pipe.Close();
                }
                catch (Exception exc)
                {
                    Trace.TraceError("Failed to close pipe {0}, remote session {1} ({2})", pipeName, RemoteSession.Id, exc);
                }
                finally
                {
                    pipe.Dispose();
                    pipe = null;
                }
            }
        }
    }
}