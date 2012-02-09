using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using Skylabs.Net;

namespace Skylabs.LobbyServer
{
    public static class Program
    {
        public static Dictionary<string, string> Settings;
        public static WebServer WebServer;
        private static DateTime _killTime;
        private static Timer _killTimer;
        private static bool _sentMinuteWarning;
        private static readonly Thread RunThread = new Thread(Runner);
        private static bool _running = true;

        private static void Main()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            Trace.WriteLine(String.Format("[LobbyServer] V{0}", Assembly.GetExecutingAssembly().GetName().Version));
            RunThread.Start();
            if (!LoadSettings())
                return;
            //Console.WriteLine(String.Format("[LobbyServer] V{0}",Assembly.GetExecutingAssembly().GetName().Version.ToString()));
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainProcessExit;
            StartServer();
            WebServer = new WebServer();
            WebServer.Start();
            _killTimer = new Timer(CheckKillTime, _killTime, 1000, 1000);
        }

        private static void Runner()
        {
            while (_running)
            {
                if (!_running)
                    return;
                Thread.Sleep(100);
            }
        }

        private static bool LoadSettings()
        {
            Settings = new Dictionary<string, string>();
#if(DEBUG)
            const string sname = "serversettingsdebug.ini";
#else
            const string sname = "serversettings.ini";
#endif
            if (!File.Exists(sname))
            {
                Console.WriteLine("Can't find settings file.");
                return false;
            }
            foreach (string l in File.ReadLines(sname))
            {
                string s = l.Trim();
                if (s[0] == '#')
                    continue;
                String[] parts = s.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    continue;
                parts[0] = parts[0].Trim();
                parts[1] = parts[1].Trim();
                if (Settings.ContainsKey(parts[0]))
                    Settings[parts[0]] = parts[1];
                else
                    Settings.Add(parts[0], parts[1]);
            }
            return true;
        }

        public static void KillServerInTime(int minutes)
        {
            if (minutes == 0)
            {
                _sentMinuteWarning = false;
                _killTime = new DateTime(0);
                return;
            }
            _killTime = DateTime.Now.AddMinutes(minutes);
            var sm = new SocketMessage("servermessage");
            sm.AddData("message",
                       "Server will be shutting down in " + minutes.ToString(CultureInfo.InvariantCulture) + " minutes");
            Server.AllUserMessage(sm);
        }

        private static void CheckKillTime(Object stateInfo)
        {
            if (_killTime == new DateTime(0))
                return;
            if (_killTime.Ticks > DateTime.Now.Ticks)
            {
                if (new TimeSpan(_killTime.Ticks - DateTime.Now.Ticks).TotalMinutes <= 1)
                {
                    if (!_sentMinuteWarning)
                    {
                        var sm = new SocketMessage("servermessage");
                        sm.AddData("message", "Server will be shutting down in about a minute.");
                        Server.AllUserMessage(sm);
                        _sentMinuteWarning = true;
                    }
                }
                return;
            }
            Quit();
        }

        private static void CurrentDomainProcessExit(object sender, EventArgs e)
        {
            Quit();
            Console.WriteLine(String.Format("TotalRunTime: {0}", Server.ServerRunTime));
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("------UNHANDLED EXCEPTION-------");
            var ex = (Exception) e.ExceptionObject;
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine(String.Format("TotalRunTime: {0}", Server.ServerRunTime));
            Console.WriteLine("--------------------------------");
            Quit();
        }

        private static void StartServer()
        {
            IPAddress cto = Settings["BindTo"] == "*" ? IPAddress.Any : IPAddress.Parse(Settings["BindTo"]);
            Server.Start(cto, Int32.Parse(Settings["BindPort"]));
        }

        private static void Quit()
        {
            try
            {
                Gaming.Stop();
            }
            catch (Exception e) { Debug.WriteLine(e); if (Debugger.IsAttached) Debugger.Break(); }
            try
            {
                Server.Stop();
            }
            catch (Exception e) { Debug.WriteLine(e); if (Debugger.IsAttached) Debugger.Break(); }
            try
            {
                _killTimer.Dispose();
            }
            catch (Exception e) { Debug.WriteLine(e); if (Debugger.IsAttached) Debugger.Break(); }
            _running = false;
        }
    }
}