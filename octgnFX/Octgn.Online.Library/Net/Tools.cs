namespace Octgn.Online.Library.Net
{
    public static class Tools
    {
        private static string hostName;
        private static object portLock = new object();
        private static int currentPort = 10000;
        public static string HostName
        {
            get
            {
                if (hostName != null) return hostName;
#if(DEBUG)
                hostName = "localhost";
#else
                const string Dyndns = "http://checkip.dyndns.org";
                var wc = new System.Net.WebClient();
                var utf8 = new System.Text.UTF8Encoding();
                var requestHtml = "";
                var ipAddress = "";
                requestHtml = utf8.GetString(wc.DownloadData(Dyndns));
                var fullStr = requestHtml.Split(':');
                ipAddress = fullStr[1].Remove(fullStr[1].IndexOf('<')).Trim();
                hostName = ipAddress;
#endif
                return hostName;
            }
        }

        public static int GetNextPort()
        {
            lock (portLock)
            {
                if (currentPort == 20000) currentPort = 10000;
                else currentPort++;
                return currentPort;
            }
        }
    }
}
