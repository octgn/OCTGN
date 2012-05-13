using System;
using System.Collections.Generic;
using System.Threading;

namespace Octgn.StandAloneServer
{
    public class Program
    {
        public static Server.Server Server;
        public static int Port;
        public static Guid GameGuid;
        public static Version GameVersion;
        private static bool _keepRunning = true;

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new[]
                           {
                               "-p=8088",
                               "-g=A6C8D2E8-7CD8-11DD-8F94-E62B56D89593",
                               "-v=2.0.7"
                           };
            }

            OptionSet set = new OptionSet()
                .Add("p=|port=", "Port for the server to host on.", v => int.TryParse(v, out Port))
                .Add("g=|guid=", "GUID of the game being played.", v => Guid.TryParse(v, out GameGuid))
                .Add("v=|version=", "Game version.",
                     v => GameVersion = Version.TryParse(v, out GameVersion) ? GameVersion : null);
            if (!HandleArgs(args, set) || Port == 0 || GameGuid.Equals(Guid.Empty) || GameVersion == null)
            {
                set.WriteOptionDescriptions(Console.Out);
                return;
            }


            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainProcessExit;

            StartServer();
        }

        private static bool HandleArgs(ICollection<string> args, OptionSet set)
        {
            if (args.Count < 2)
                return false;
            set.Parse(args);
            return true;
        }

        private static void StartServer()
        {
            Server = new Server.Server(Port, GameGuid, GameVersion);
            Server.OnStop += Server_OnStop;
            Console.WriteLine("Starting server on port " + Port);
            while (_keepRunning)
            {
                Thread.Sleep(1000);
            }
        }

        private static void Server_OnStop(object sender, EventArgs e)
        {
            Server = null;
            _keepRunning = false;
        }

        private static void CurrentDomainProcessExit(object sender, EventArgs e)
        {
            //ConsoleEventLog.SerializeEvents("sologs/"+DateTime.Now.ToFileTime().ToString() + "SOServer.xml");
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception) e.ExceptionObject;
            Console.WriteLine(ex.StackTrace);
        }
    }
}