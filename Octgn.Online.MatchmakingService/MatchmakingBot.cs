/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Reflection;
using log4net;
using Skylabs.Lobby.Messages;

namespace Octgn.Online.MatchmakingService
{
    public class MatchmakingBot : Messanger
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Singleton

        internal static MatchmakingBot SingletonContext { get; set; }

        private static readonly object GameBotSingletonLocker = new object();

        public static MatchmakingBot Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (GameBotSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new MatchmakingBot();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        public MatchmakingBot() 
            : base(AppConfig.Instance.ServerPath, AppConfig.Instance.XmppUsername, AppConfig.Instance.XmppPassword)
        {
        }
    }
}
