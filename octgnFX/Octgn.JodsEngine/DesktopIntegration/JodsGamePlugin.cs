using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using Octgn.Communication;
using Octgn.Core;
using Octgn.DataNew;
using Octgn.DataNew.Entities;
using Octgn.Library;
using Octgn.Loaders;
using Octgn.Online.Hosting;
using Octgn.Sdk.Extensibility;

namespace Octgn.DesktopIntegration
{
    [PluginDetails(TypeName)]
    public class JodsGamePlugin : GamePlugin, ISinglePlayerGameMode
    {
        private readonly static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const string TypeName = "octgn.jodsengine.gameplugin";

        public override void Initialize(Package package) {
            base.Initialize(package);
        }

        public async Task StartSinglePlayer() {
            DbContext.SetContext(new DesktopDbContext());

            var configLoader = new ConfigLoader();

            await configLoader.Load(null);

            var octgnVersion = typeof(Server.Server).Assembly.GetName().Version;

            var username = Prefs.Username;
            if (string.IsNullOrWhiteSpace(username)) {
                username = Library.Randomness.GrabRandomNounWord() + Library.Randomness.GrabRandomJargonWord();
            }

            var user = new User(Guid.NewGuid().ToString(), username);

            var name = Prefs.LastRoomName;

            if (string.IsNullOrWhiteSpace(name)) {
                name = Library.Randomness.RandomRoomName();
            }

            var gameId = GetGameId();

            var gameName = Details.Name;
            var gameVersion = Package.Record.Version;

            var hostedGame = new HostedGame() {
                Id = Guid.NewGuid(),
                Name = name,
                HostUser = user,
                GameName = gameName,
                GameId = gameId,
                GameVersion = gameVersion,
                HostAddress = $"0.0.0.0:{Prefs.LastLocalHostedGamePort}",
                OctgnVersion = octgnVersion.ToString(),
                Spectators = true,
            };

            var args = "-h ";

            new HostedGameProcess(
                hostedGame,
                X.Instance.Debug,
                true
            ).Start();

            args += $"-u \"{username}\" ";
            args += "-p ";
            args += "-k \"" + HostedGame.Serialize(hostedGame) + "\"";

            await LaunchEngine(args);
        }

        private Guid GetGameId() {
            var xmlPath = Details.Path;

            var serializer = new GameSerializer();

            var game = (Game)serializer.Deserialize(xmlPath);

            return game.Id;
        }

        private async Task<bool> LaunchEngine(string args) {
            var engineDirectory = "jodsengine";
            if (X.Instance.Debug) {
                engineDirectory = "..\\..\\..\\..\\Octgn.JodsEngine\\bin\\Debug\\netcoreapp3.1";
            }

            engineDirectory = Path.GetFullPath(engineDirectory);

            var enginePath = Path.Combine(engineDirectory, "Octgn.JodsEngine.exe");

            var psi = new ProcessStartInfo(enginePath, args);
            psi.UseShellExecute = true;
            psi.WorkingDirectory = engineDirectory;

            var proc = Process.Start(psi);

            try {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10))) {
                    await Task.Run(async () => {
                        while (proc.MainWindowHandle == IntPtr.Zero) {
                            await Task.Delay(1000, cts.Token);
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
    }
}
