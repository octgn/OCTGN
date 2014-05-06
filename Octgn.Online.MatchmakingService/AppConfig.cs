/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Configuration;

namespace Octgn.Online.MatchmakingService
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

        public bool TestMode { get { return bool.Parse(ConfigurationManager.AppSettings["TestMode"]); } }
    }
}