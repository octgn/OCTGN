/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Configuration;

namespace Octgn.Online.GameService
{
    public class AppConfig
    {
        #region Singleton

        internal static AppConfig SingletonContext { get; set; }

        private static readonly object AppConfigSingletonLocker = new object();

        public static AppConfig Instance {
            get {
                if (SingletonContext == null) {
                    lock (AppConfigSingletonLocker) {
                        if (SingletonContext == null) {
                            SingletonContext = new AppConfig();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        public string ComUrl => ConfigurationManager.AppSettings[nameof(ComUrl)];
        public string ComUsername => ConfigurationManager.AppSettings[nameof(ComUsername)];
        public string ComPassword => ConfigurationManager.AppSettings[nameof(ComPassword)];
        public string ComDeviceId => ConfigurationManager.AppSettings[nameof(ComDeviceId)];
        public int GameBroadcastPort => int.Parse(ConfigurationManager.AppSettings[nameof(GameBroadcastPort)]);
        public string ApiKey => ConfigurationManager.AppSettings[nameof(ApiKey)];
        public string ApiUrl => ConfigurationManager.AppSettings[nameof(ApiUrl)];
        public string HostName => ConfigurationManager.AppSettings[nameof(HostName)];
    }
}