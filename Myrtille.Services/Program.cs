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
using System.ServiceModel;
using System.ServiceProcess;
using log4net.Config;

namespace Myrtille.Services
{
    public class Program : ServiceBase
    {
        private static ServiceHost _remoteSessionProcess;
        private static ServiceHost _localFileStorage;

        private static ServiceHost OpenService(Type serviceType)
        {
            ServiceHost serviceHost = null;
            
            try
            {
                serviceHost = new ServiceHost(serviceType);
                serviceHost.Open();

                var description = serviceHost.Description;
                Console.WriteLine(string.Format("Service: {0}", description.ConfigurationName));
                foreach (var endpoint in description.Endpoints)
                {
                    Console.WriteLine(string.Format("Endpoint: {0}", endpoint.Name));
                    Console.WriteLine(string.Format("Address: {0}", endpoint.Address));
                    Console.WriteLine(string.Format("Binding: {0}", endpoint.Binding.Name));
                    Console.WriteLine(string.Format("Contract: {0}", endpoint.Contract.ConfigurationName));
                    Console.WriteLine("");
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to start service ({0})", exc);
                if (serviceHost != null)
                {
                    serviceHost.Abort();
                }
            }

            return serviceHost;
        }

        private static void CloseService(ref ServiceHost serviceHost)
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }

        private static void Main(string[] args)
        {
            // logger
            XmlConfigurator.Configure();

            if (!Environment.UserInteractive)
            {
                Run(new Program());
            }
            else
            {
                var consoleTraceListener = new ConsoleTraceListener();
                consoleTraceListener.Filter = new EventTypeFilter(SourceLevels.Information);
                Trace.Listeners.Add(consoleTraceListener);

                _remoteSessionProcess = OpenService(typeof(RemoteSessionProcess));
                _localFileStorage = OpenService(typeof(FileStorage));

                Console.WriteLine("press any key to exit...");
                Console.ReadKey();

                CloseService(ref _remoteSessionProcess);
                CloseService(ref _localFileStorage);
            }
        }

        protected override void OnStart(string[] args)
		{
            _remoteSessionProcess = OpenService(typeof(RemoteSessionProcess));
            _localFileStorage = OpenService(typeof(FileStorage));
		}
 
		protected override void OnStop()
		{
            CloseService(ref _remoteSessionProcess);
            CloseService(ref _localFileStorage);
		}
    }
}