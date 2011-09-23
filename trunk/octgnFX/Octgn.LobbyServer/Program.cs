using System;
using System.Net;
using Octgn.LobbyServer;
using Skylabs.ConsoleHelper;

namespace Skylabs.LobbyServer
{
    public class Program
    {
        public static Server server;
#if(DEBUG)
        public static serverdebug Settings = serverdebug.Default;
#else
        public static server Settings = server.Default;
#endif

        private static void Main(string[] args)
        {
            ConsoleEventLog.eAddEvent += new ConsoleEventLog.EventEventDelegate(ConsoleEventLog_eAddEvent);
            ConsoleWriter.CommandText = "LobbyServer: ";
            ConsoleReader.eConsoleInput += new ConsoleReader.ConsoleInputDelegate(ConsoleReader_eConsoleInput);
            AppDomain.CurrentDomain.FirstChanceException += new EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs>(CurrentDomain_FirstChanceException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            Start_Server();

            ConsoleReader.Start();
            ConsoleWriter.writeCT();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            ConsoleEventLog.SerializeEvents("log.xml");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            ConsoleEventLog.addEvent(new ConsoleEventError(ex.Message, ex), false);
        }

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            ConsoleEventLog.addEvent(new ConsoleEventError(e.Exception.Message, e.Exception), false);
        }

        private static void ConsoleEventLog_eAddEvent(ConsoleEvent e)
        {
#if(DEBUG)
            System.Diagnostics.Debugger.Break();

#endif
        }

        private static void Start_Server()
        {
            IPAddress cto;
            if(Settings.BindTo == "*")
                cto = IPAddress.Any;
            else
                cto = IPAddress.Parse(Settings.BindTo);
            server = new Server(cto, Settings.BindPort);
            server.Start();
        }

        private static void Quit()
        {
            ConsoleReader.Stop();
        }

        private static void Tester()
        {
        }

        private static void ConsoleReader_eConsoleInput(ConsoleMessage input)
        {
            switch(input.Header.ToLower())
            {
                case "test":
                    Tester();
                    break;
                case "start":
                    Start_Server();
                    ConsoleWriter.writeLine("Hosting", true);
                    break;
                case "stop":
                    server.Stop();
                    ConsoleWriter.writeLine("Hosting Stopped.", true);
                    break;
                case "quit":
                    Quit();
                    break;
                default:
                    ConsoleWriter.writeLine("", true);
                    break;
            }
        }
    }
}