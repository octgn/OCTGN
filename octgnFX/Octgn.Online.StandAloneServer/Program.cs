namespace Octgn.Online.StandAloneServer
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.ServiceProcess;
    using System.Threading;

    using Octgn.Online.Library.Models;
    using Octgn.Online.Library.UpdateManager;
    using Octgn.StandAloneServer;

    using log4net;

    class Program
    {
        internal static HostedGameSASModel HostedGame = new HostedGameSASModel();
        internal static bool KeepRunning;
        internal static bool Debug;
        internal static OptionSet Options;
        internal static Service Service;
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            Log.InfoFormat("Starting {0}", Assembly.GetEntryAssembly().GetName().Name);
#if(DEBUG)
            Debug = true;
            Log.Debug("Debug mode enabled.");
#endif
            if (UpdateManager.GetContext().Update()) return;
            UpdateManager.GetContext().OnUpdateDetected += OnOnUpdateDetected;
            UpdateManager.GetContext().Start();
#if(!DEBUG)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
#endif
            if (HandleArguments(args))
            {
                // arguments didn't fail, so do stuff
#if(DEBUG)

                StartServiceCommandLine();
                Console.WriteLine("==DONE==");
                Console.ReadLine();
#else
            StartService();
#endif
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
                    if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Q) break;
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
            UpdateManager.GetContext().Stop();
        }

        private static void OnOnUpdateDetected(object sender, EventArgs eventArgs)
        {
            Stop();
        }

        private static bool HandleArguments(string[] args)
        {
#if(DEBUG)
            var atemp = new List<string>();
            atemp.Add("-id=" + Guid.NewGuid());
            atemp.Add("-name=" + "Name");
            atemp.Add("-hostusername=" + "test");
            atemp.Add("-gamename=" + "cardgame");
            atemp.Add("-gameid=" + Guid.Empty);
            atemp.Add("-gameversion=" + new Version(1,2,3,4));
            atemp.Add("-debug");
            atemp.Add("-bind=" + "0.0.0.0:9999");
            args = atemp.ToArray();

#endif
            Options = new OptionSet()
                .Add("id=", "Id of the HostedGame.", x => HostedGame.Id = Guid.Parse(x))
                .Add("name=", "Name of the HostedGame", x => HostedGame.Name = x)
                .Add("hostusername=", "Username of user hosting the HostedGame", x => HostedGame.HostUserName = x)
                .Add("gamename=", "Name of the Octgn Game", x => HostedGame.GameName = x)
                .Add("gameid=", "Id of the Octgn Game", x => HostedGame.GameId = Guid.Parse(x))
                .Add("gameversion=", "Version of the Octgn Game", x => HostedGame.GameVersion = Version.Parse(x))
                .Add("debug", "Little more verbose", x => Debug = true)
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
                .Add("bind=", "Address to listen to, 0.0.0.0:12 for all on port 12", x => HostedGame.HostUri = new Uri("http://" + x));

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
            if (Debug)
            {
                Console.WriteLine();
                Console.WriteLine("Any key to exit");
                Console.ReadKey();
            }
        }
    }
}
