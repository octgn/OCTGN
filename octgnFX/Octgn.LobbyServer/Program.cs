using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Skylabs.ConsoleHelper;

namespace Skylabs.LobbyServer
{
    public static class Program
    {
        public static Dictionary<string, string> Settings; 
        public static WebServer WebServer;
        private static void Main(string[] args)
        {
            if (!LoadSettings())
                return;
            ConsoleEventLog.EAddEvent += ConsoleEventLogEAddEvent;
            ConsoleWriter.CommandText = "LobbyServer: ";
            ConsoleReader.EConsoleInput += ConsoleReaderEConsoleInput;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainProcessExit;
            StartServer();
            WebServer = new WebServer();
            WebServer.Start();

            ConsoleReader.Start();
            ConsoleWriter.WriteCt();
        }
        private static bool LoadSettings()
        {
            Settings = new Dictionary<string, string>();
#if(DEBUG)
            string sname = "serversettingsdebug.ini";
#else
            string sname = "serversettings.ini";
#endif
            if(!File.Exists(sname))
            {
                Console.WriteLine("Can't find settings file.");
                return false;
            }
            foreach(string l in File.ReadLines(sname))
            {
                string s = l.Trim();
                if(s[0] == '#')
                    continue;
                String[] parts = s.Split(new char[1] {':'}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    continue;
                parts[0] = parts[0].Trim();
                parts[1] = parts[1].Trim();
                if(Settings.ContainsKey(parts[0]))
                    Settings[parts[0]] = parts[1];
                else
                    Settings.Add(parts[0],parts[1]);
            }
            return true;
        }
        private static void CurrentDomainProcessExit(object sender, EventArgs e)
        {
            WebServer.Stop();
            ConsoleEventLog.SerializeEvents("log.xml");
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            ConsoleEventLog.AddEvent(new ConsoleEventError(ex.Message, ex), false);
        }

        private static void ConsoleEventLogEAddEvent(ConsoleEvent e)
        {
            ConsoleEventError er = e as ConsoleEventError;
#if(DEBUG)
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
            if (er != null)
            {
                Console.WriteLine(er.Exception.StackTrace);
                Console.WriteLine(e.GetConsoleString());
            }
#endif
        }

        private static void StartServer()
        {
            IPAddress cto = Settings["BindTo"] == "*" ? IPAddress.Any : IPAddress.Parse(Settings["BindTo"]);
            Server.Start(cto, Int32.Parse(Settings["BindPort"]));
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