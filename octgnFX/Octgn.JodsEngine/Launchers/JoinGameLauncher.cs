using System.Windows;
using MessageBox = System.Windows.MessageBox;
using System;
using System.Threading.Tasks;
using Octgn.Online.Hosting;
using Octgn.Scripting.Controls;
using Octgn.Core.DataManagers;
using Octgn.Networking;
using Octgn.Play;
using System.Windows.Threading;
using System.Net;
using System.Collections.Generic;

namespace Octgn.Launchers
{
    public class JoinGameLauncher : LauncherBase
    {
        private readonly string _username;
        private readonly HostedGame _game;
        private readonly bool _spectate;
        private readonly bool _isHost;

        public JoinGameLauncher(
            HostedGame game,
            string username,
            bool isHost,
            bool spectate
        ) {
            _username = username;
            _spectate = spectate;
            _isHost = isHost;
            _game = game ?? throw new ArgumentNullException(nameof(game));

            Validate();
        }

        private void Validate() {
            //if (this.gameId == null) {
            //    MessageBox.Show("You must supply a GameId with -g=GUID on the command line.", "Error",
            //        MessageBoxButton.OK, MessageBoxImage.Error);
            //    this.Shutdown = true;
            //}
        }

        protected override async Task Load() {
            var hostedGame = _game;

            try {
                var password = string.Empty;
                if (Program.IsHost = _isHost) {
                    password = hostedGame.Password;
                    Program.CurrentHostedGame = hostedGame;
                } else {
                    if (hostedGame.HasPassword) {
                        var dlg = new InputDlg("Password", "Please enter this games password", "");

                        password = dlg.GetString();
                    }
                }

                if (hostedGame.Source == HostedGameSource.Online) {
                    Program.CurrentOnlineGameName = hostedGame.Name;
                }

                var game = GameManager.Get().GetById(hostedGame.GameId);

                Program.GameEngine = new GameEngine(game, _username, _spectate, password);

                var hostUrl = hostedGame.Host;

                Program.Client = await Connect(hostUrl, hostedGame.Port);

                if (Program.Client == null) {
                    MessageBox.Show(
                        $"Unable to connect to {hostedGame.Name} at {hostedGame.HostAddress}",
                        "Unable to Join Game",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    this.Shutdown = true;

                    Program.Exit();
                }

                Log.Info($"{nameof(Loaded)}: Launching {nameof(PlayWindow)}");
            } catch (Exception e) {
                this.Log.Warn($"Couldn't join game: {e.Message}", e);

                MessageBox.Show(
                    $"Error joining game {hostedGame.Name}: {e.Message}",
                    "Error Joining Game",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                this.Shutdown = true;

                Program.Exit();
            }
        }

        protected override Task Loaded() {
            LaunchPlayWindow();

            return Task.CompletedTask;
        }

        private async Task<IClient> Connect(string host, int port) {
            foreach (var address in Resolve(host)) {
                try {
                    // Should use gameData.IpAddress sometime.
                    Log.Info($"{nameof(JoinGameLauncher)}: Trying to connect to {address}:{port}");

                    var client = new ClientSocket(address, port);

                    await client.Connect();

                    return client;
                } catch (Exception ex) {
                    Log.Error($"{nameof(JoinGameLauncher)}: Couldn't connect to address {address}:{port}", ex);
                }
            }

            return null;
        }

        private IEnumerable<IPAddress> Resolve(string host) {
            if (host == "0.0.0.0") {
                yield return IPAddress.Loopback;

                yield break;
            }

            foreach (var address in Dns.GetHostAddresses(host)) {
                if (address == IPAddress.IPv6Loopback) continue;

                yield return address;
            }
        }

        private void LaunchPlayWindow() {
            System.Windows.Application.Current.Dispatcher.VerifyAccess();

            if (WindowManager.PlayWindow != null) throw new InvalidOperationException($"Can't run more than one game at a time.");

            System.Windows.Application.Current.Dispatcher
                .InvokeAsync(async () => {
                    await Dispatcher.Yield(DispatcherPriority.Background);

                    Application.Current.MainWindow
                        = WindowManager.PlayWindow
                        = new PlayWindow();

                    WindowManager.PlayWindow.Closed += PlayWindow_Closed;

                    WindowManager.PlayWindow.Show();
                    WindowManager.PlayWindow.Activate();
                });
        }

        private void PlayWindow_Closed(object sender, EventArgs e) {
            Log.Info("Play window closed, shutting down");

            Program.Exit();
        }
    }
}