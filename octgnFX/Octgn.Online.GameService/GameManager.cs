/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using log4net;
using Octgn.Library.Networking;
using Octgn.Site.Api;
using System.Threading.Tasks;
using Octgn.Online.Hosting;
using Octgn.Library;

namespace Octgn.Online.GameService
{
    public class GameManager : IDisposable
    {
        #region Singleton

        internal static GameManager SingletonContext { get; set; }

        private static readonly object GameManagerSingletonLocker = new object();

        public static GameManager Instance {
            get {
                if (SingletonContext == null)
                {
                    lock (GameManagerSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new GameManager();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal GameBroadcastListener GameListener;
        internal System.Timers.Timer UpdateWebsiteTimer;

        public void Start()
        {
            GameListener = new GameBroadcastListener(AppConfig.Instance.BroadcastPort);
            GameListener.StartListening();
            UpdateWebsiteTimer = new System.Timers.Timer(10000);
            UpdateWebsiteTimer.Elapsed += UpdateWebsiteTimerOnElapsed;
            if (!AppConfig.Instance.TestMode)
                UpdateWebsiteTimer.Start();
        }

        public IEnumerable<HostedGame> Games {
            get {
                return GameListener.Games
                    .ToArray();
            }
        }

        public async Task<Guid> HostGame(HostedGame req, User u)
        {
            // Try to kill every other game this asshole started before this one.
            var others = GameListener.Games.Where(x => x.HostUserId.Equals(u.UserId, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
            foreach (var g in others)
            {
                try {
                    g.KillGame();
                } catch (InvalidOperationException ex) {
                    Log.Error($"{nameof(HostGame)}: Error killing game. See inner exception for more details.", ex);
                }
            }

            var bport = AppConfig.Instance.BroadcastPort;

            req.Id = Guid.NewGuid();
            req.HostAddress = AppConfig.Instance.Host + ":" + Ports.NextPort.ToString();

            var waitTask = GameListener.WaitForGame(req.Id);

            var gameProcess = new HostedGameProcess(req, Service.IsDebug, false, AppConfig.Instance.BroadcastPort);

            gameProcess.Start();

            await waitTask;

            return req.Id;
        }

        private void UpdateWebsiteTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                UpdateWebsiteTimer.Enabled = false;

                var client = new ApiClient();

                if (!client.SetGameList(AppConfig.Instance.ApiKey, this.Games.ToArray()))
                {
                    Log.Warn("UpdateWebsiteTimerOnElapsed: Couldn't set the game list, some kinda error.");
                }
            }
            catch (Exception e)
            {
                Log.Error("UpdateWebsiteTimerOnElapsedError", e);
            }
            finally
            {
                UpdateWebsiteTimer.Enabled = true;
            }
        }

        public void Dispose()
        {
            Log.Info("GameManager Disposed");
            if (GameListener != null)
                GameListener.Dispose();
            UpdateWebsiteTimer.Dispose();
        }
    }
}