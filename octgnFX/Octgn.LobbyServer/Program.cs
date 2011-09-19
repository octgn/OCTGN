using System.Net;
using System.Net.Sockets;
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

        public static string MySqlConnectionString
        {
            get
            {
                MySql.Data.MySqlClient.MySqlConnectionStringBuilder sb = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder();
                sb.Database = Settings.db;
                sb.UserID = Settings.dbUser;
                sb.Password = Settings.dbPass;
                sb.Server = Settings.dbHost;
                return sb.ToString();
            }
        }

        private static void Main(string[] args)
        {
            ConsoleEventLog.eAddEvent += new ConsoleEventLog.EventEventDelegate(ConsoleEventLog_eAddEvent);
            ConsoleWriter.CommandText = "LobbyServer: ";
            ConsoleReader.eConsoleInput += new ConsoleReader.ConsoleInputDelegate(ConsoleReader_eConsoleInput);
            //Start_Server();

            ConsoleReader.Start();
            ConsoleWriter.writeCT();
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
                case "host":
                    Start_Server();
                    ConsoleWriter.writeLine("Hosting", true);
                    break;
                case "connect":
                    Client c = new Client(new TcpClient("localhost", Settings.BindPort), 1, server);
                    c.WriteMessage(new Skylabs.Net.SocketMessage("Hello"));
                    c.Stop();
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