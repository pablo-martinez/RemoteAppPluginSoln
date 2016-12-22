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
using System.Diagnostics;
using System.IO;
using Myrtille.Helpers;
using Myrtille.Services.Contracts;

namespace Myrtille.Services
{
    public class FileStorage : IFileStorage
    {
        public List<string> GetUserDocumentsFolderFiles(
            string domain,
            string userName,
            string userPassword)
        {
            var documentsFolder = AccountHelper.GetUserDocumentsFolder(domain, userName, userPassword);

            try
            {
                var fileNames = Directory.GetFiles(documentsFolder);
                for (var i = 0; i < fileNames.Length; i++)
                {
                    fileNames[i] = Path.GetFileName(fileNames[i]);
                }
                return new List<string>(fileNames);
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to retrieve file(s) from user {0} documents folder {1} ({2})", userName, documentsFolder, exc);
                throw;
            }
        }

        public void UploadFileToUserDocumentsFolder(
            UploadRequest uploadRequest)
        {
            var documentsFolder = AccountHelper.GetUserDocumentsFolder(uploadRequest.Domain, uploadRequest.UserName, uploadRequest.UserPassword);

            try
            {
                // replace if already exists
                if (File.Exists(Path.Combine(documentsFolder, uploadRequest.FileName)))
                {
                    File.Delete(Path.Combine(documentsFolder, uploadRequest.FileName));
                }

                var fileStream = File.Create(Path.Combine(documentsFolder, uploadRequest.FileName));

                int bytesRead;
                var buffer = new byte[4096];

                while ((bytesRead = uploadRequest.Stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                }

                fileStream.Close();
                uploadRequest.Stream.Close();
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to upload file {0} to user {1} documents folder {2} ({3})", uploadRequest.FileName, uploadRequest.UserName, documentsFolder, exc);
                throw;
            }

            Trace.TraceInformation("Uploaded file {0} to user {1} documents folder {2}", uploadRequest.FileName, uploadRequest.UserName, documentsFolder);
        }

        public Stream DownloadFileFromUserDocumentsFolder(
            string domain,
            string userName,
            string userPassword,
            string fileName)
        {
            var documentsFolder = AccountHelper.GetUserDocumentsFolder(domain, userName, userPassword);

            Trace.TraceInformation("Downloading file {0} from user {1} documents folder {2}", fileName, userName, documentsFolder);

            try
            {
                return File.Open(Path.Combine(documentsFolder, fileName), FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to download file {0} from user {1} documents folder {2} ({3})", fileName, userName, documentsFolder, exc);
                throw;
            }
        }
    }
}