using System;
using System.Collections.Generic;
using System.Reflection;

using Octgn.StandAloneServer;

using log4net;
using Octgn.Online.Hosting;
using Octgn.Server;
using System.Configuration;
using Octgn.ServiceUtilities;
using Octgn.Communication;
using Octgn.Utils;

namespace Octgn.Online.StandAloneServer
{
    public class Service : OctgnServiceBase
    {
        internal static HostedGame HostedGame = new HostedGame();

        internal static bool Local;
        internal static OptionSet Options;
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static int BroadcastPort = 21234;

        internal static State State;
        static void Main(string[] args) {
            try {
                LoggerFactory.DefaultMethod = (con) => new Log4NetLogger(con.Name);
                Signal.OnException += Signal_OnException;
                Log.Info("Startup");

                HandleArguments(IsDebug, args);

                State = new State(HostedGame, Local);
                State.ApiKey = ConfigurationManager.AppSettings["SiteApiKey"];
                var handler = new Handler(State);
                State.Handler = handler;

                using (var service = new Service()) {
                    service.Run(args);
                }
            } catch (Exception ex) {
                Log.Error($"{nameof(Main)}", ex);
            } finally {
                Log.Info("Shutting down");
            }

            LogManager.Shutdown();
        }

        private Server.Server _server;
        protected override void OnStart(string[] args) {
            base.OnStart(args);
            _server = new Octgn.Server.Server(State, BroadcastPort);
            _server.OnStop += Server_OnStop;
            _server.OnStop += (sender, _) => {
            };
        }

        protected override void OnStop() {
            base.OnStop();
            var server = _server;
            _server = null;
            server?.Stop();
        }

        private void Server_OnStop(object sender, EventArgs e) {
            try {
                _server = null;
                var server = sender as Server.Server;
                server.OnStop -= Server_OnStop;
                Stop();
            } catch (Exception ex) {
                Log.Error($"{nameof(Server)}.{nameof(OnStop)}", ex);
            }
        }

        private static void Signal_OnException(object sender, ExceptionEventArgs args) {
            Log.Fatal($"Signal_OnException: {args.Message}", args.Exception);
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
                .Add("hostuserid=", "Username of user hosting the HostedGame", x => HostedGame.HostUserId = x)
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
                if (String.IsNullOrWhiteSpace(HostedGame.HostUserId)) throw new Exception("Must enter hostuserid");
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
