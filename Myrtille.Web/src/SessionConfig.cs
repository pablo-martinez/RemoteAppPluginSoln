using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Myrtille.Web
{
    public class SessionConfig
    {
        public static List<SessionConfig> AvailableConfigs { get; } = new List<SessionConfig>();

        static int _currentID { get; set; } = 1;
        static int NextID { get { return _currentID++; } }

        public int ID { get; set; }
        public string ServerAddress { get; set; }
        public string UserDomain { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public List<string> Programs { get; set; }

        static SessionConfig()
        {
            AvailableConfigs.Add(new SessionConfig
            {
                ID = NextID,
                ServerAddress = "127.0.0.2",
                UserDomain = "PMartinez-PC",
                UserName = "test",
                UserPassword = "test",
                Programs = new List<string> { "", "notepad", "calc", "chrome" }
            });
            AvailableConfigs.Add(new SessionConfig
            {
                ID = NextID,
                ServerAddress = "127.0.0.2",
                UserDomain = "PMartinez-PC",
                UserName = "test1",
                UserPassword = "test",
                Programs = new List<string> { "", "notepad", "calc", "chrome" }
            });
            AvailableConfigs.Add(new SessionConfig
            {
                ID = NextID,
                ServerAddress = "127.0.0.2",
                UserDomain = "PMartinez-PC",
                UserName = "test2",
                UserPassword = "test",
                Programs = new List<string> { "", "notepad", "calc", "chrome" }
            });
            AvailableConfigs.Add(new SessionConfig
            {
                ID = NextID,
                ServerAddress = "127.0.0.2",
                UserDomain = "PMartinez-PC",
                UserName = "test3",
                UserPassword = "test",
                Programs = new List<string> { "", "notepad", "calc", "chrome" }
            });
            AvailableConfigs.Add(new SessionConfig
            {
                ID = NextID,
                ServerAddress = "127.0.0.2",
                UserDomain = "PMartinez-PC",
                UserName = "test4",
                UserPassword = "test",
                Programs = new List<string> { "", "notepad", "calc", "chrome" }
            });
            AvailableConfigs.Add(new SessionConfig
            {
                ID = NextID,
                ServerAddress = "127.0.0.2",
                UserDomain = "PMartinez-PC",
                UserName = "test5",
                UserPassword = "test",
                Programs = new List<string> { "", "notepad", "calc", "chrome" }
            });
        }
    }
}