using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.ConsoleHelper;
using Mono.Options;
using System.Threading;

namespace Octgn.StandAloneServer
{
    public class Program
    {
        public static Octgn.Server.Server Server;
        public static int Port;
        public static Guid GameGuid;
        public static Version GameVersion;
        private static bool KeepRunning = true;
        static void Main(string[] args)
        {
            OptionSet set = new OptionSet()
                .Add("p=|port=", "Port for the server to host on.", (v) => int.TryParse(v, out Port))
                .Add("g=|guid=", "GUID of the game being played.", (v) => Guid.TryParse(v, out GameGuid))
                .Add("v=|version=", "Game version.", (v) => GameVersion = Version.TryParse(v, out GameVersion) == true?GameVersion : null);
            if(!HandleArgs(args,set) || Port == 0 || GameGuid.Equals(Guid.Empty) || GameVersion == null)
            {
                set.WriteOptionDescriptions(Console.Out);
                return;
            }


            ConsoleEventLog.EAddEvent += ConsoleEventLogEAddEvent;
            ConsoleWriter.CommandText = "StandAloneServer: ";
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainFirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainProcessExit;
            
            StartServer();
        }
        private static bool HandleArgs(string[] args, OptionSet set)
        {
            if (args.Length < 2)
                return false;
            set.Parse(args);
            return true;
        }
        private static void StartServer()
        {
            Server = new Server.Server(Port,false,GameGuid,GameVersion);
            Server.OnStop += new EventHandler(Server_OnStop);
            while (KeepRunning)
            {
                Thread.Sleep(1000);
            }
        }

        static void Server_OnStop(object sender, EventArgs e)
        {
            Server = null;
            KeepRunning = false;
        }

        private static void CurrentDomainProcessExit(object sender, EventArgs e)
        {
            
            //ConsoleEventLog.SerializeEvents("sologs/"+DateTime.Now.ToFileTime().ToString() + "SOServer.xml");
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            ConsoleEventLog.AddEvent(new ConsoleEventError(ex.Message, ex), false);
            Console.WriteLine(ex.StackTrace);
        }

        private static void CurrentDomainFirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            ConsoleEventLog.AddEvent(new ConsoleEventError(e.Exception.Message, e.Exception), false);
        }

        private static void ConsoleEventLogEAddEvent(ConsoleEvent e)
        {
#if(DEBUG)
            //System.Diagnostics.Debugger.Break();
            //TODO Serialize and save to a file before release.
            //Console.WriteLine(e.);

#endif
        }
    }
}
