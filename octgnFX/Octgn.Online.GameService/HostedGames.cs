/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Linq;
using System.Reflection;
using System.Timers;
using log4net;
using Octgn.Library.Networking;
using Octgn.Site.Api;
using System.Threading.Tasks;
using Octgn.Online.Hosting;
using Octgn.Library;
using Octgn.Communication;
using System.Collections.Generic;

namespace Octgn.Online.GameService
{
    public static class HostedGames
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly GameBroadcastListener _gameListener = new GameBroadcastListener(AppConfig.Instance.GameBroadcastPort);

        private static readonly Timer _updateWebsiteTimer = new Timer(10000);

        public static IEnumerable<int> UsedPorts => _gameListener.Games.Select(game => game.Port);

        public static HostedGame Get(Guid id) {
            return _gameListener.Games.FirstOrDefault(x => x.Id == id);
        }

        private static bool EnableUpdateTimer => !string.IsNullOrWhiteSpace(AppConfig.Instance.ApiKey);

        public static void Start() {
            _gameListener.StartListening();
            _updateWebsiteTimer.Elapsed += UpdateWebsiteTimerOnElapsed;
            if (EnableUpdateTimer)
                _updateWebsiteTimer.Start();
        }

        public static void Stop() {
            _gameListener.StopListening();
            _updateWebsiteTimer.Elapsed -= UpdateWebsiteTimerOnElapsed;
            if (EnableUpdateTimer)
                _updateWebsiteTimer.Stop();
        }

        public static async Task<Guid> HostGame(HostedGame req, User u) {
            // Try to kill every other game this asshole started before this one.
            var others = _gameListener.Games.Where(x => x.HostUser.Equals(u))
                .ToArray();

            foreach (var g in others) {
                try {
                    g.KillGame();
                } catch (InvalidOperationException ex) {
                    Log.Error($"{nameof(HostGame)}: Error killing game. See inner exception for more details.", ex);
                }
            }

            var bport = AppConfig.Instance.GameBroadcastPort;

            req.Id = Guid.NewGuid();
            req.HostAddress = AppConfig.Instance.HostName + ":" + Ports.NextPort.ToString();

            var waitTask = _gameListener.WaitForGame(req.Id);

            var gameProcess = new HostedGameProcess(req, Service.IsDebug, false, AppConfig.Instance.GameBroadcastPort);

            gameProcess.Start();

            await waitTask;

            return req.Id;
        }

        private static async void UpdateWebsiteTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            try {
                _updateWebsiteTimer.Enabled = false;

                var client = new ApiClient();

                await client.SetGameList(AppConfig.Instance.ApiKey, _gameListener.Games.ToArray());
            } catch (Exception ex) {
                Log.Error(nameof(UpdateWebsiteTimerOnElapsed), ex);
            } finally {
                _updateWebsiteTimer.Enabled = true;
            }
        }
    }
}