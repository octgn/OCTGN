using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (args.Length == 0)
            {
                args = new string[3]
                           {
                               "-p=8088",
                               "-g=A6C8D2E8-7CD8-11DD-8F94-E62B56D89593",
                               "-v=2.0.7"
                           };
            }

        OptionSet set = new OptionSet()
                .Add("p=|port=", "Port for the server to host on.", (v) => int.TryParse(v, out Port))
                .Add("g=|guid=", "GUID of the game being played.", (v) => Guid.TryParse(v, out GameGuid))
                .Add("v=|version=", "Game version.", (v) => GameVersion = Version.TryParse(v, out GameVersion) == true?GameVersion : null);
            if(!HandleArgs(args,set) || Port == 0 || GameGuid.Equals(Guid.Empty) || GameVersion == null)
            {
                set.WriteOptionDescriptions(Console.Out);
                return;
            }


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
            Server = new Server.Server(Port,GameGuid,GameVersion);
            Server.OnStop += new EventHandler(Server_OnStop);
            Console.WriteLine("Starting server on port " + Port);
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
            Console.WriteLine(ex.StackTrace);
        }

    }
}
