namespace Octgn
{
    using System.Configuration;
    using System.Reflection;

    using log4net;

    public static class AppConfig
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly string WebsitePath;
        public static readonly string ChatServerPath;
        public static readonly string GameServerPath;
        public static readonly string UpdateInfoPath;
        public static readonly string GameFeed;
        public static readonly bool UseGamePackageManagement;

        static AppConfig()
        {
            Log.Info("Setting AppConfig");
            WebsitePath = ConfigurationManager.AppSettings["WebsitePath"];
            ChatServerPath = ConfigurationManager.AppSettings["ChatServerPath"];
            GameServerPath = ConfigurationManager.AppSettings["GameServerPath"];
            GameFeed = ConfigurationManager.AppSettings["GameFeed"];
            UseGamePackageManagement = bool.Parse(ConfigurationManager.AppSettings["UseGamePackageManagement"]);
#if(Release_Test)
            UpdateInfoPath = ConfigurationManager.AppSettings["UpdateCheckPathTest"];
#else
            UpdateInfoPath = ConfigurationManager.AppSettings["UpdateCheckPath"];
#endif
            Log.Info("Set AppConfig");
        }
    }
}