/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Configuration;
using System.Reflection;

using log4net;

namespace Octgn
{
    public static class AppConfig
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly string WebsitePath;
        public static readonly string ChatServerHost;
        public static readonly string GameServerPath;
        public static readonly string UpdateInfoPath;
        public static readonly string GameFeed;
        public static readonly bool UseGamePackageManagement;
        public static readonly string StaticWebsitePath;

        static AppConfig()
        {
            Log.Info("Setting AppConfig");
            if (Program.IsReleaseTest == false
                && Octgn.Library.X.Instance.Debug == false)
            {
                WebsitePath = ConfigurationManager.AppSettings["WebsitePath"];
            }
            else
            {
                WebsitePath = ConfigurationManager.AppSettings["WebsitePathTest"];
            }
            StaticWebsitePath = ConfigurationManager.AppSettings[nameof(StaticWebsitePath)];
            ChatServerHost = ConfigurationManager.AppSettings["ChatServerHost"];
            GameServerPath = ConfigurationManager.AppSettings["GameServerPath"];
            GameFeed = ConfigurationManager.AppSettings["GameFeed"];
            UseGamePackageManagement = bool.Parse(ConfigurationManager.AppSettings["UseGamePackageManagement"]);
            if (Program.IsReleaseTest)
                UpdateInfoPath = ConfigurationManager.AppSettings["UpdateCheckPathTest"];
            else
                UpdateInfoPath = ConfigurationManager.AppSettings["UpdateCheckPath"];

            Log.Info("Set AppConfig");
        }
    }
}