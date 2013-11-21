using System.Configuration;

namespace Octgn.Online.GameService
{
    public class AppConfig
    {
        #region Singleton

        internal static AppConfig SingletonContext { get; set; }

        private static readonly object AppConfigSingletonLocker = new object();

        public static AppConfig Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (AppConfigSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new AppConfig();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        public string ServerPath { get { return ConfigurationManager.AppSettings["ServerPath"]; } }

        public string XmppUsername { get { return ConfigurationManager.AppSettings["XmppUsername"]; } }

        public string XmppPassword { get { return ConfigurationManager.AppSettings["XmppPassword"]; } }

        public bool Test { get { return bool.Parse(ConfigurationManager.AppSettings["TestMode"]); } }
    }
}