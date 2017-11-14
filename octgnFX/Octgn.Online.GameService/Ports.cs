/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using log4net;
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
                    var usedPorts = HostedGames.UsedPorts.ToArray();

                    for(var port = 10000;port <= 20000; port++) {
                        if (usedPorts.Contains(port)) continue;
                        if (!NetworkHelper.IsPortAvailable(port)) continue;

                        return port;
                    }

                    return -1;
                }
            }
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