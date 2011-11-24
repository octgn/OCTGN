using System;
using System.Net;
using Octgn.LobbyServer;
using Skylabs.ConsoleHelper;

namespace Skylabs.LobbyServer
{
    public class Program
    {
        public static Server Server;
#if(DEBUG)
        public static serverdebug Settings = serverdebug.Default;
#else
        public static server Settings = server.Default;
#endif

        private static void Main(string[] args)
        {
            ConsoleEventLog.EAddEvent += ConsoleEventLogEAddEvent;
            ConsoleWriter.CommandText = "LobbyServer: ";
            ConsoleReader.EConsoleInput += ConsoleReaderEConsoleInput;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainFirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainProcessExit;
            StartServer();

            ConsoleReader.Start();
            ConsoleWriter.WriteCt();
        }

        private static void CurrentDomainProcessExit(object sender, EventArgs e)
        {
            ConsoleEventLog.SerializeEvents("log.xml");
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            ConsoleEventLog.AddEvent(new ConsoleEventError(ex.Message, ex), false);
        }

        private static void CurrentDomainFirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            ConsoleEventLog.AddEvent(new ConsoleEventError(e.Exception.Message, e.Exception), false);
        }

        private static void ConsoleEventLogEAddEvent(ConsoleEvent e)
        {
#if(DEBUG)
            ConsoleEventError er = e as ConsoleEventError;
            if (er != null)
            {
                if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                else
                {
                    Console.WriteLine(er.Exception.StackTrace);
                    Console.WriteLine(e.GetConsoleString());
                    Console.ReadKey();
                }
            }
            
#else
            
#endif
        }

        private static void StartServer()
        {
            IPAddress cto = Settings.BindTo == "*" ? IPAddress.Any : IPAddress.Parse(Settings.BindTo);
            Server = new Server(cto, Settings.BindPort);
            Server.Start();
        }

        private static void Quit()
        {
            ConsoleReader.Stop();
        }

        private static void Tester()
        {
        }

        private static void ConsoleReaderEConsoleInput(ConsoleMessage input)
        {
            switch(input.Header.ToLower())
            {
                case "test":
                    Tester();
                    break;
                case "start":
                    StartServer();
                    ConsoleWriter.WriteLine("Hosting", true);
                    break;
                case "stop":
                    Server.Stop();
                    ConsoleWriter.WriteLine("Hosting Stopped.", true);
                    break;
                case "quit":
                    Quit();
                    break;
                default:
                    ConsoleWriter.WriteLine("", true);
                    break;
            }
        }
    }
}