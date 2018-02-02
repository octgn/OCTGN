using System;
using System.Collections.Generic;
using System.Reflection;

using Octgn.StandAloneServer;

using log4net;
using Octgn.Online.Hosting;
using Octgn.Server;
using System.Configuration;
using Octgn.Communication;
using Octgn.Utils;
using System.Threading.Tasks;
using Octgn.WindowsDesktopUtilities;

namespace Octgn.Online.StandAloneServer
{
    public class Program : OctgnProgram
    {
        internal static HostedGame HostedGame = new HostedGame() {
            HostUser = new User(),
            DateCreated = DateTimeOffset.Now
        };

        internal static bool Local;
        internal static OptionSet Options;
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static int BroadcastPort = 21234;

        private static Config _config;
        static void Main(string[] args) {
            bool waitForKeyOnExit = false;

            try {
                LoggerFactory.DefaultMethod = (con) => new Log4NetLogger(con.Name);
                Log.Info("Startup");

                waitForKeyOnExit = bool.Parse(ConfigurationManager.AppSettings["WaitForKeyOnExit"]);

                HandleArguments(IsDebug, args);

                _config = new Config();
                _config.IsLocal = Local;
                _config.IsDebug = IsDebug;
                _config.ApiKey = ConfigurationManager.AppSettings["SiteApiKey"];
                _config.PlayerTimeoutSeconds = int.Parse(ConfigurationManager.AppSettings["PlayerTimeoutSeconds"]);

                Log.Info("Starting program");
                using (var program = new Program()) {
                    program.Run(args).Wait();
                }
            } catch (Exception ex) {
                Log.Error($"{nameof(Main)}", ex);
            } finally {
                Log.Info("Shutting down");
            }

            if (waitForKeyOnExit) {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }

            LogManager.Shutdown();
        }

        private Server.Server _server;
        protected override async Task OnStart(string[] args) {
            _server = new Octgn.Server.Server(_config, HostedGame, BroadcastPort);
            _server.OnStop += Server_OnStop;

            await _server.Start();
            await base.OnStart(args);
        }

        protected override void OnStop() {
            var server = _server;
            _server = null;
            server?.Dispose();
            base.OnStop();
        }

        private void Server_OnStop(object sender, EventArgs e) {
            try {
                Log.Info($"The server stopped. Stopping the service.");
                _server = null;
                var server = sender as Server.Server;
                server.OnStop -= Server_OnStop;
                Stop();
            } catch (Exception ex) {
                Log.Error($"{nameof(Server)}.{nameof(OnStop)}", ex);
            }
        }

        private static void HandleArguments(bool debug, string[] args) {
            if (debug) {
                if (args == null || args.Length == 0) {
                    var atemp = new List<string>();
                    atemp.Add("-id=" + Guid.NewGuid());
                    atemp.Add("-name=" + "Name");
                    atemp.Add("-hostuserid=" + "test");
                    atemp.Add("-gamename=" + "cardgame");
                    atemp.Add("-gameid=" + Guid.Parse("844d5fe3-bdb5-4ad2-ba83-88c2c2db6d88"));
                    atemp.Add("-gameversion=" + new Version(1, 3, 3, 7));
                    atemp.Add("-local");
                    atemp.Add("-debug");
                    atemp.Add("-bind=" + "0.0.0.0:9999");
                    atemp.Add("-broadcastport=" + "21234");
                    args = atemp.ToArray();
                }
            }
            Options = new OptionSet()
                .Add("id=", "Id of the HostedGame.", x => HostedGame.Id = Guid.Parse(x))
                .Add("name=", "Name of the HostedGame", x => HostedGame.Name = x)
                .Add("hostuserid=", "UserId of user hosting the HostedGame", x => HostedGame.HostUser.Id = x)
                .Add("hostusername=", "Username of user hosting the HostedGame", x => HostedGame.HostUser.DisplayName = x)
                .Add("gamename=", "Name of the Octgn Game", x => HostedGame.GameName = x)
                .Add("gameid=", "Id of the Octgn Game", x => HostedGame.GameId = Guid.Parse(x))
                .Add("gameversion=", "Version of the Octgn Game", x => HostedGame.GameVersion = Version.Parse(x).ToString())
                .Add("local", "Is this a local game", x => Local = true)
                .Add("gameiconurl=", "Games Icon Url", x => HostedGame.GameIconUrl = x)
                .Add("usericonurl=", "Users Icon Url", x => HostedGame.HostUserIconUrl = x)
                .Add(
                    "password=",
                    "Password of the HostedGame",
                    x => {
                        if (!String.IsNullOrWhiteSpace(x)) {
                            HostedGame.Password = x;
                            HostedGame.HasPassword = true;
                        }
                    })
                .Add("bind=", "Address to listen to, 0.0.0.0:12 for all on port 12", x => HostedGame.HostAddress = x)
                .Add("broadcastport=", "Port it broadcasts on", x => BroadcastPort = int.Parse(x))
                .Add("spectators", "Allow spectators?", x => HostedGame.Spectators = true);

            try {
                Options.Parse(args);
                // Validate inputs. All other inputs get parsed, so if they fail they'll throw exceptions themselves.
                if (String.IsNullOrWhiteSpace(HostedGame.Name)) throw new Exception("Must enter name");
                if (String.IsNullOrWhiteSpace(HostedGame.HostUser.Id)) throw new Exception("Must enter hostuserid");
                if (String.IsNullOrWhiteSpace(HostedGame.HostUser.DisplayName)) throw new Exception("Must enter hostusername");
                if (String.IsNullOrWhiteSpace(HostedGame.GameName)) throw new Exception("Must enter a gamename");

            } catch (Exception e) {
                Console.WriteLine("Error: {0}", e.Message);
                Console.WriteLine();
                Options.WriteOptionDescriptions(Console.Out);
                throw;
            }
        }
    }
}
