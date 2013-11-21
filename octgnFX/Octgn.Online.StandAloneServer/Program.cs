namespace Octgn.Online.StandAloneServer
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.ServiceProcess;
    using System.Threading;

    using Octgn.Online.Library.Enums;
    using Octgn.Online.Library.Models;
    using Octgn.StandAloneServer;

    using log4net;

    class Program
    {
        internal static HostedGameSASModel HostedGame = new HostedGameSASModel();
        internal static bool KeepRunning;
        internal static bool Debug;
        internal static bool Local;
        internal static OptionSet Options;
        internal static Service Service;
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static int BroadcastPort = 21234;
        static void Main(string[] args)
        {
            Log.InfoFormat("Starting {0}", Assembly.GetEntryAssembly().GetName().Name);
#if(DEBUG)
            Debug = true;
            Log.Debug("Debug mode enabled.");
#else
            //if (UpdateManager.GetContext().Update()) return;
            //UpdateManager.GetContext().OnUpdateDetected += OnOnUpdateDetected;
            //UpdateManager.GetContext().Start();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
#endif
            if (HandleArguments(args))
            {
                // arguments didn't fail, so do stuff
                // Setup game state engine
                GameStateEngine.SetContext(HostedGame.ToHostedGameState(EnumHostedGameStatus.Booting),Local);
                //if (Debug || Local) 
                //else StartService();
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
            using (Service = new Service())
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

        internal static void StartService()
        {
            Log.Info("Starting in Service mode");
            Service = new Service();
            Service.OnServiceStop += (sender, args) => Stop(false);
            var services = new ServiceBase[] { Service };
            ServiceBase.Run(services);
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
#if(DEBUG)
            if (args == null || args.Length == 0)
            {
                var atemp = new List<string>();
                atemp.Add("-id=" + Guid.NewGuid());
                atemp.Add("-name=" + "Name");
                atemp.Add("-hostusername=" + "test");
                atemp.Add("-gamename=" + "cardgame");
                atemp.Add("-gameid=" + Guid.Parse("844d5fe3-bdb5-4ad2-ba83-88c2c2db6d88"));
                atemp.Add("-gameversion=" + new Version(1, 3, 3, 7));
                atemp.Add("-local");
                atemp.Add("-debug");
                atemp.Add("-bind=" + "0.0.0.0:9999");
                atemp.Add("-broadcastport=" + "21234");
                args = atemp.ToArray();
            }

#endif
            Options = new OptionSet()
                .Add("id=", "Id of the HostedGame.", x => HostedGame.Id = Guid.Parse(x))
                .Add("name=", "Name of the HostedGame", x => HostedGame.Name = x)
                .Add("hostusername=", "Username of user hosting the HostedGame", x => HostedGame.HostUserName = x)
                .Add("gamename=", "Name of the Octgn Game", x => HostedGame.GameName = x)
                .Add("gameid=", "Id of the Octgn Game", x => HostedGame.GameId = Guid.Parse(x))
                .Add("gameversion=", "Version of the Octgn Game", x => HostedGame.GameVersion = Version.Parse(x))
                .Add("debug", "Little more verbose", x => Debug = true)
                .Add("local","Is this a local game",x=> Local = true)
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
                .Add("broadcastport=","Port it broadcasts on",x=>BroadcastPort = int.Parse(x));

            try
            {
                Options.Parse(args);
                // Validate inputs. All other inputs get parsed, so if they fail they'll throw exceptions themselves.
                if (String.IsNullOrWhiteSpace(HostedGame.Name)) throw new Exception("Must enter name");
                if (String.IsNullOrWhiteSpace(HostedGame.HostUserName)) throw new Exception("Must enter hostusername");
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
