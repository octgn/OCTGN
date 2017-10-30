using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using Octgn.StandAloneServer;

using log4net;
using Octgn.Online.Hosting;
using Octgn.Server;
using System.Configuration;

namespace Octgn.Online.StandAloneServer
{
    class Program
    {
        internal static HostedGame HostedGame = new HostedGame();
        internal static bool KeepRunning;
#if(DEBUG)
        internal static bool _debug = true;
#else
        internal static bool _debug = false;
#endif

        internal static bool Debug
        {
            get { return _debug; }
            set { _debug = value; }
        }

        internal static bool Local;
        internal static OptionSet Options;
        internal static Service Service;
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static int BroadcastPort = 21234;
        static void Main(string[] args)
        {
            Log.InfoFormat("Starting {0}", Assembly.GetEntryAssembly().GetName().Name);
            if (Debug)
            {
                Log.Debug("Debug mode enabled.");
            }
            else
            {
                //if (UpdateManager.GetContext().Update()) return;
                //UpdateManager.GetContext().OnUpdateDetected += OnOnUpdateDetected;
                //UpdateManager.GetContext().Start();
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            }
            if (HandleArguments(args))
            {
                StartServiceCommandLine();
            }
            if (Debug)
            {
                Console.WriteLine();
                Console.WriteLine("Any key to exit");
                Console.ReadKey();
            }

        }

        internal static void StartServiceCommandLine()
        {
            Log.Info("Starting in CommandLine mode");
            var state = new State(Program.HostedGame, Program.Local);

            state.ApiKey = ConfigurationManager.AppSettings["SiteApiKey"];

            using (Service = new Service(state))
            {
                KeepRunning = true;
                Service.Start();
                Service.OnServiceStop += (sender, args) => Stop(false);
                Console.WriteLine("Press 'q' to quit");
                while (KeepRunning)
                {
                    if (!Local && Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Q) break;
                    Thread.Sleep(100);
                }
                Stop();
            }
        }

        internal static void Stop(bool stopService = true)
        {
            KeepRunning = false;
            if (Service != null && stopService)
                Service.Stop();
            // If this is uncommented, make sure it has a context for local games.
            //UpdateManager.GetContext().Stop();
        }

        private static void OnOnUpdateDetected(object sender, EventArgs eventArgs)
        {
            Stop();
        }

        private static bool HandleArguments(string[] args)
        {
            if (Debug)
            {
                if (args == null || args.Length == 0)
                {
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
                .Add("gameversion=", "Version of the Octgn Game", x => HostedGame.GameVersion = Version.Parse(x))
                .Add("debug", "Little more verbose", x => Debug = true)
                .Add("local", "Is this a local game", x => Local = true)
				.Add("gameiconurl=","Games Icon Url", x=>HostedGame.GameIconUrl = x)
				.Add("usericonurl=","Users Icon Url", x=>HostedGame.HostUserIconUrl = x)
                .Add(
                    "password=",
                    "Password of the HostedGame",
                    x =>
                    {
                        if (!String.IsNullOrWhiteSpace(x))
                        {
                            HostedGame.Password = x;
                            HostedGame.HasPassword = true;
                        }
                    })
                .Add("bind=", "Address to listen to, 0.0.0.0:12 for all on port 12", x => HostedGame.HostUri = new Uri("http://" + x))
                .Add("broadcastport=", "Port it broadcasts on", x => BroadcastPort = int.Parse(x))
                .Add("spectators", "Allow spectators?", x => HostedGame.Spectators = true);

            try
            {
                Options.Parse(args);
                // Validate inputs. All other inputs get parsed, so if they fail they'll throw exceptions themselves.
                if (String.IsNullOrWhiteSpace(HostedGame.Name)) throw new Exception("Must enter name");
                if (String.IsNullOrWhiteSpace(HostedGame.HostUserId)) throw new Exception("Must enter hostuserid");
                if (String.IsNullOrWhiteSpace(HostedGame.GameName)) throw new Exception("Must enter a gamename");
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                Console.WriteLine();
                Options.WriteOptionDescriptions(Console.Out);
            }
            return false;
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Log.Fatal("Unhandled Exception", unhandledExceptionEventArgs.ExceptionObject as Exception);
            LogManager.Shutdown();
            if (Debug)
            {
                Console.WriteLine();
                Console.WriteLine("Any key to exit");
                Console.ReadKey();
            }
        }
    }
}
