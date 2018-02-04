using System;

namespace Octgn.Server
{
    public class Config
    {
        public bool IsLocal { get; set; }
        public bool IsDebug { get; set; }
        public string ApiKey { get; set; }
        public int PlayerTimeoutSeconds { get; set; }
        public string ServerName { get; } = "OCTGN.NET";
        public Version ServerVersion { get; }

        public Config() {
            ServerVersion = this.GetType().Assembly.GetName().Version;
        }
    }
}