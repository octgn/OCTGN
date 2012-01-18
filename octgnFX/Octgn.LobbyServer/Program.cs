using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Skylabs.Net;
using System.Reflection;

namespace Skylabs.LobbyServer
{
    public static class Program
    {
        public static Dictionary<string, string> Settings; 
        public static WebServer WebServer;
        private static DateTime _killTime;
        private static Timer _killTimer;
        private static bool _sentMinuteWarning =false;
        private static Thread _runThread = new Thread(runner);
        private static bool _running = true;
        private static void Main(string[] args)
        {
            _runThread.Start();
            if (!LoadSettings())
                return;
            Console.WriteLine(String.Format("[LobbyServer] V{0}",Assembly.GetExecutingAssembly().GetName().Version.ToString()));
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainProcessExit;
            StartServer();
            WebServer = new WebServer();
            WebServer.Start();
            _killTimer = new Timer(CheckKillTime, _killTime, 1000, 1000);
        }
        private static void runner()
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
        public static void KillServerInTime(int minutes)
        {
            if(minutes == 0)
            {
                _sentMinuteWarning = false;
                _killTime = new DateTime(0);
                return;
            }
            _killTime = DateTime.Now.AddMinutes((int)minutes);
            SocketMessage sm = new SocketMessage("servermessage");
            sm.AddData("message", "Server will be shutting down in " + minutes.ToString() + " minutes");
            Server.AllUserMessage(sm);
        }
        private static void CheckKillTime(Object stateInfo)
        {
            if (_killTime == null)
                return;
            if (_killTime == new DateTime(0))
                return;
            if (_killTime.Ticks > DateTime.Now.Ticks)
            {
                if(new TimeSpan(_killTime.Ticks - DateTime.Now.Ticks).TotalMinutes <= 1)
                {
                    if(!_sentMinuteWarning)
                    {
                        SocketMessage sm = new SocketMessage("servermessage");
                        sm.AddData("message","Server will be shutting down in about a minute.");
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
            Console.WriteLine(String.Format("TotalRunTime: {0}", Server.ServerRunTime.ToString()));
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine(String.Format("TotalRunTime: {0}", Server.ServerRunTime.ToString()));
            Quit();
        }

        private static void StartServer()
        {
            IPAddress cto = Settings["BindTo"] == "*" ? IPAddress.Any : IPAddress.Parse(Settings["BindTo"]);
            Server.Start(cto, Int32.Parse(Settings["BindPort"]));
        }

        private static void Quit()
        {
            try { Gaming.Stop(); }
            catch { }
            try {Server.Stop();}
            catch { }
            try { _killTimer.Dispose(); }
            catch { }
            _running = false;
        }

        private static void Tester()
        {
        }

    }
}