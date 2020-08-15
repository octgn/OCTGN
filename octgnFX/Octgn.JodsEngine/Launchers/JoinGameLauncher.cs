using System.Windows;
using System;
using System.Threading.Tasks;
using Octgn.Online.Hosting;
using Octgn.Scripting.Controls;
using Octgn.Core.DataManagers;
using Octgn.Networking;
using Octgn.Play;
using System.Net;
using System.Collections.Generic;
using Octgn.Windows;
using Octgn.Library.Exceptions;
using System.Windows.Threading;

namespace Octgn.Launchers
{
    public class JoinGameLauncher : LauncherBase
    {
        public override string Name => "Game Table";

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

        protected override async Task<Window> Load(ILoadingView loadingView) {
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

                loadingView.UpdateStatus("Loading game");
                var gm = GameManager.Get();

                var game = GameManager.Get().GetById(hostedGame.GameId);

                if (game == null) {
                    var msg = $"Game {hostedGame.GameName}({hostedGame.Id}) could not be found.";
                    throw new UserMessageException(UserMessageExceptionMode.Blocking, msg);
                }

                loadingView.UpdateStatus("Building engine");
                Program.GameEngine = new GameEngine(game, _username, _spectate, password);

                loadingView.UpdateStatus($"Connecting to {hostedGame.HostAddress}");
                await Task.Delay(100);
                Program.Client = await Connect(hostedGame.Host, hostedGame.Port);

                if (Program.Client == null) {
                    var msg = $"Unable to connect to {hostedGame.Name} at {hostedGame.HostAddress}";

                    throw new UserMessageException(UserMessageExceptionMode.Blocking, msg);
                }

                Window window = null;
                await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                    window = WindowManager.PlayWindow = new PlayWindow();

                    window.Closed += PlayWindow_Closed;

                    window.Show();
                }, DispatcherPriority.Background);

                return window;
            } catch (Exception e) {
                var msg = $"Error joining game {hostedGame.Name}: {e.Message}";

                Log.Warn(msg, e);

                throw new UserMessageException(UserMessageExceptionMode.Blocking, msg, e);
            }
        }

        private async Task<IClient> Connect(string host, int port) {
            foreach (var address in Resolve(host)) {
                try {
                    // Should use gameData.IpAddress sometime.
                    Log.Info($"{nameof(JoinGameLauncher)}: Trying to connect to {address}:{port}");

                    var client = new ClientSocket(address, port);

                    await Task.Run(client.Connect);

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

        private void PlayWindow_Closed(object sender, EventArgs e) {
            Log.Info("Play window closed, shutting down");

            Program.Exit();
        }
    }
}