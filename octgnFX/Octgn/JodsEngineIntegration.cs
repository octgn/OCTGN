/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using log4net;
using Octgn.Core;
using Octgn.Core.DataManagers;
using Octgn.Library;
using Octgn.Online.Hosting;
using Octgn.Tabs.GameHistory;
using Octgn.Tabs.Play;
using Octgn.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Octgn
{
    public class JodsEngineIntegration
    {
        private readonly static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Task<bool> HostGame(HostedGame hostedGame, string username) {
            new HostedGameProcess(
                hostedGame,
                X.Instance.Debug,
                true
            ).Start();

            var args = "-h ";
            args += $"-u \"{username}\" ";
            args += "-k \"" + HostedGame.Serialize(hostedGame) + "\"";

            return LaunchJodsEngine(args);
        }

        public void HostGame(int? hostPort, Guid? gameId) {
            throw new NotImplementedException();
        }

        public Task<bool> LaunchDeckEditor(string deckPath = null) {
            if (string.IsNullOrWhiteSpace(deckPath)) {
                return LaunchJodsEngine("-e");
            } else {
                return LaunchJodsEngine($"-e -d \"{deckPath}\"");
            }
        }

        public Task<bool> JoinGame(DataNew.Entities.Game game, HostedGame hostedGame, string password) {
            var username = Program.LobbyClient.User.DisplayName;

            var host = hostedGame.HostAddress;

            //foreach(var address in Dns.GetHostAddresses(AppConfig.GameServerPath)) {
            //    try {
            //        if (address == IPAddress.IPv6Loopback) continue;

            //        // Should use gameData.IpAddress sometime.
            //        Log.Info($"{nameof(StartOnlineGame)}: Trying to connect to {address}:{result.Port}");

            //        Program.Client = new ClientSocket(address, result.Port);
            //        await Program.Client.Connect();
            //        SuccessfulHost = true;
            //        return;
            //    } catch (Exception ex) {
            //        Log.Error($"{nameof(StartOnlineGame)}: Couldn't connect to address {address}:{result.Port}", ex);
            //    }
            //}
            //throw new InvalidOperationException($"Unable to connect to {AppConfig.GameServerPath}.{result.Port}");

            throw new NotImplementedException();
        }

        public void JoinGame(HostedGameViewModel hostedGame) {
            var spectate = hostedGame.Status == HostedGameStatus.GameInProgress && hostedGame.Spectator;
            Program.IsHost = false;
            var password = "";
            if (hostedGame.HasPassword) {
                //TODO: Password Dialog
                //var dlg = new InputDlg("Password", "Please enter this games password", "");
                //password = dlg.GetString();
                throw new NotImplementedException();
            }
            var username = (Program.LobbyClient.IsConnected == false
                || Program.LobbyClient.User == null
                || Program.LobbyClient.User.DisplayName == null) ? Prefs.Nickname : Program.LobbyClient.User.DisplayName;

            var game = GameManager.Get().GetById(hostedGame.GameId);

            throw new NotImplementedException();
        }

        internal void LaunchReplay(GameHistoryViewModel history) => throw new NotImplementedException();
        internal void JoinOfflineGame() => throw new NotImplementedException();
        internal void HostGame() => throw new NotImplementedException();

        private async Task<bool> LaunchJodsEngine(string args) {
            var engineDirectory = "jodsengine";
            if (X.Instance.Debug) {
                engineDirectory = "..\\..\\..\\Octgn.JodsEngine\\bin\\Debug\\netcoreapp3.1";
            }

            engineDirectory = Path.GetFullPath(engineDirectory);

            var enginePath = Path.Combine(engineDirectory, "octgn.exe");

            var psi = new ProcessStartInfo(enginePath, args);
            psi.UseShellExecute = true;
            psi.WorkingDirectory = engineDirectory;

            var proc = Process.Start(psi);

            try {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10))) {
                    await Task.Run(async () => {
                        while (proc.MainWindowHandle == IntPtr.Zero) {
                            await Task.Delay(2000, cts.Token);
                            if (proc.HasExited) {
                                break;
                            }
                        }
                    }, cts.Token);
                }
            } catch (OperationCanceledException) {
                Log.Warn("Engine did not show UI withing alloted time.");

                MessageBox.Show("Engine appears to be frozen, please try again. If this continues to happen, let us know.", "Frozen Engine", MessageBoxButton.OK, MessageBoxImage.Error);

                try {
                    proc.Kill();
                } catch (Exception ex) {
                    Log.Warn($"Error killing proc: {ex.Message}", ex);
                }

                return false;
            }

            if (proc.HasExited) {
                MessageBox.Show("Engine prematurely shutdown, please try again. If this continues to happen, let us know.", "Engine Shutdown", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }

            return true;
        }

        internal void JoinGame(DataGameViewModel game, IPAddress host, int port, string username, string password) => throw new NotImplementedException();
    }
}
