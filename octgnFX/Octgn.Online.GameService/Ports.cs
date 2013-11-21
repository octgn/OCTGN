using System;
using System.Linq;
using System.Net;
using System.Reflection;
using log4net;
using Microsoft.Win32;
using Skylabs.Lobby;

namespace Octgn.Online.GameService
{
    public static class Ports
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Object Locker = new object();

        public static int NextPort
        {
            get
            {
                lock (Locker)
                {
                    var port = GetCurrentValue() + 1;

                    while (GameManager.Instance.Games.Any(x => x.Port == port)
                        || Networking.IsPortAvailable(port) == false
                        || port >= 20000)
                    {
                        port++;
                        if (port >= 20000)
                            port = 10000;
                    }

                    SetCurrentValue(port);
                    return port;
                }
            }
        }

        private static string KeyName
        {
            get
            {
                if (AppConfig.Instance.Test)
                {
                    return "GameService-Test";
                }
                return "GameService";
            }
        }

        private static int GetCurrentValue()
        {
            using (var root = Registry.CurrentUser.OpenSubKey("Software", true)
                .OpenSubKey("OCTGN", true)
                .OpenSubKey(KeyName, true))
            {

                var ret = (int) root.GetValue("CurrentPort", 10000);
                return ret;
            }
        }

        private static void SetCurrentValue(int port)
        {
            using (var root = Registry.CurrentUser.OpenSubKey("Software", true)
                .OpenSubKey("OCTGN", true)
                .OpenSubKey(KeyName, true))
            {
                root.SetValue("CurrentPort", port, RegistryValueKind.DWord);
            }
        }

        private static IPAddress _externalIp = null;

        public static IPAddress ExternalIp
        {
            get
            {
                try
                {
                    if (_externalIp == null)
                    {
                        const string Dyndns = "http://checkip.dyndns.org";
                        var wc = new WebClient();
                        var utf8 = new System.Text.UTF8Encoding();
                        var requestHtml = "";
                        var ipAddress = "";
                        requestHtml = utf8.GetString(wc.DownloadData(Dyndns));
                        var fullStr = requestHtml.Split(':');
                        ipAddress = fullStr[1].Remove(fullStr[1].IndexOf('<')).Trim();
                        var externalIp = IPAddress.Parse(ipAddress);
                        _externalIp = externalIp;
                        return _externalIp;
                    }
                    return _externalIp;
                }
                catch (Exception e)
                {
                    Log.Error("ExternalIp Error",e);
                }
                return IPAddress.Parse("96.31.76.45");
            }
        }
    }
}