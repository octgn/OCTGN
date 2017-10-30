/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using log4net;
using Microsoft.Win32;
using Octgn.Library.Utils;

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

                    var endTime = DateTime.Now.AddSeconds(10);
                    while (GameManager.Instance.Games.Any(x => x.HostUri.Port == port)
                        || NetworkHelper.IsPortAvailable(port) == false
                        || port >= 20000)
                    {
                        port++;
                        if (port >= 20000)
                            port = 10000;
                        if (DateTime.Now > endTime) throw new TimeoutException("Took longer than 10 seconds to get a port");
                    }

                    SetCurrentValue(port);
                    return port;
                }
            }
        }

        private static int GetCurrentValue()
        {
            var ret = (int)InstanceHandler.Root.GetValue("CurrentPort", 10000);
            return ret;
        }

        private static void SetCurrentValue(int port)
        {
            InstanceHandler.Root.SetValue("CurrentPort", port, RegistryValueKind.DWord);
        }

        private static DateTime _expireExternalIp = DateTime.Now;
        private static IPAddress _externalIp = null;

        public static IPAddress ExternalIp
        {
            get
            {
                try
                {
                    if (_externalIp == null || DateTime.Now > _expireExternalIp)
                    {
                        const string Dyndns = "http://checkip.dyndns.org";
                        using (var wc = new WebClient())
                        {
                            var utf8 = new System.Text.UTF8Encoding();
                            var requestHtml = "";
                            var ipAddress = "";
                            requestHtml = utf8.GetString(wc.DownloadData(Dyndns));
                            var fullStr = requestHtml.Split(':');
                            ipAddress = fullStr[1].Remove(fullStr[1].IndexOf('<')).Trim();
                            var externalIp = IPAddress.Parse(ipAddress);
                            _externalIp = externalIp;
                            _expireExternalIp = DateTime.Now.AddMinutes(5);
                            return _externalIp;
                        }
                    }
                    return _externalIp;
                }
                catch (Exception e)
                {
                    Log.Error("ExternalIp Error",e);
                }
                return _externalIp ?? IPAddress.Parse("96.31.76.45");
            }
        }
    }
}