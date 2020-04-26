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
using System.Collections.Generic;
using System.IO;

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

        private static NetworkHelper _networkHelper;

        public static void Init() {
            _networkHelper = new NetworkHelper(10000, 20000, "GameService");
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

        public static async Task<Guid> HostGame(HostedGame req) {
            // Try to kill every other game this asshole started before this one.
            if (req.HostUser.Id != "26950") {
                var others = _gameListener.Games.Where(x => x.HostUser.Equals(req.HostUser))
                    .ToArray();

                foreach (var g in others) {
                    try {
                        g.KillGame();
                    } catch (InvalidOperationException ex) {
                        Log.Error($"{nameof(HostGame)}: Error killing game. See inner exception for more details.", ex);
                    }
                }
            }

            req.Id = Guid.NewGuid();
            req.HostAddress = AppConfig.Instance.HostName + ":" + _networkHelper.NextPort.ToString();

            var waitTask = _gameListener.WaitForGame(req.Id);

            StartHostedGameProcess(req);

            await waitTask;

            return req.Id;
        }

        private static void StartHostedGameProcess(HostedGame req) {
            var broadcastPort = AppConfig.Instance.GameBroadcastPort;

            var args = HostedGameProcess.CreateArguments(req, broadcastPort, false);

            var argString = string.Join(Environment.NewLine, args);

            var fileName = Path.Combine(Path.GetTempPath(), "Octgn", "GameService", "StartSASRequests");

            if (!Directory.Exists(fileName)) {
                Directory.CreateDirectory(fileName);
            }

            fileName = Path.Combine(fileName, req.Id.ToString() + ".startrequest");

            using (var stream = File.Open(fileName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream)) {
                writer.WriteLine(req.OctgnVersion);
                writer.Write(argString);

                writer.Flush();
            }
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