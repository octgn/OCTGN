namespace Octgn.Online.StandAloneServer
{
    using System;
    using System.Reflection;

    using Octgn.Online.Library.Models;
    using Octgn.StandAloneServer;

    using log4net;

    class Program
    {
        internal static HostedGameSASModel HostedGame = new HostedGameSASModel();
        internal static bool Debug;
        internal static OptionSet Options;
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Options = new OptionSet().Add("id=", "Id of the HostedGame.", x => HostedGame.Id = Guid.Parse(x))
                                           .Add("name=", "Name of the HostedGame", x => HostedGame.Name = x)
                                           .Add("hostusername=", "Username of user hosting the HostedGame", x => HostedGame.HostUserName = x)
                                           .Add("gamename=", "Name of the Octgn Game", x => HostedGame.GameName = x)
                                           .Add("gameid=", "Id of the Octgn Game", x => HostedGame.GameId = Guid.Parse(x))
                                           .Add("gameversion=", "Version of the Octgn Game", x => HostedGame.GameVersion = Version.Parse(x))
                                           .Add("debug","Little more verbose",x=>Debug = true)
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

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}",e.Message);
                Console.WriteLine();
                Options.WriteOptionDescriptions(Console.Out);
            }

            if (Debug)
            {
                Console.WriteLine();
                Console.WriteLine("Any key to exit");
                Console.ReadKey();
            }

        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Log.Fatal("Unhandled Exception",unhandledExceptionEventArgs.ExceptionObject as Exception);
            if (Debug)
            {
                Console.WriteLine();
                Console.WriteLine("Any key to exit");
                Console.ReadKey();
            }
        }
    }
}
