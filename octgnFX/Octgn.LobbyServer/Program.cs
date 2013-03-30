using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Skylabs.Net;
using agsXMPP;

namespace Skylabs.LobbyServer
{
    public static class Program
    {
        private static readonly Thread RunThread = new Thread(Runner);
        private static bool _running = true;
        private static DateTime _startTime;
        private static bool _gotCheckBack;
        private static void Main()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            Trace.WriteLine(String.Format("[LobbyServer]: V{0}\nStart Time: {1} {2}", Assembly.GetExecutingAssembly().GetName().Version,DateTime.Now.ToShortDateString(),DateTime.Now.ToShortTimeString()));
            //Console.WriteLine(String.Format("[LobbyServer] V{0}",Assembly.GetExecutingAssembly().GetName().Version.ToString()));
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainProcessExit;
            RunThread.Name = "Main";
            RunThread.Start();            
            GameBot.Init();
            _startTime = DateTime.Now;
            _gotCheckBack = true;
            GameBot.OnCheckRecieved += OnCheckRecieved;
        }

        private static void OnCheckRecieved(object sender)
        {
            _gotCheckBack = true; 
            Trace.WriteLine("[Status]Bot Alive.");
        }

        private static void Runner()
        {
            DateTime dt = DateTime.Now;
            while (_running)
            {
                if(!_running) return;
                Thread.Sleep(100);
                if(new TimeSpan(DateTime.Now.Ticks - dt.Ticks).Seconds > 30 && _gotCheckBack == false)
                {
                    Trace.WriteLine("[Status]Bot must have died. Remaking.");
                    GameBot.RemakeXmpp();
                    _gotCheckBack = true;
                }
                if(new TimeSpan(DateTime.Now.Ticks - dt.Ticks).Minutes > 1)
                {
                    dt = DateTime.Now;
                    var ts = new TimeSpan(dt.Ticks - _startTime.Ticks);
                    Trace.WriteLine(String.Format("[Running For]: {0} Days, {1} Hours, {2} Minutes",ts.Days,ts.Hours, ts.Minutes));
                    GameBot.CheckBotStatus();
                    Trace.WriteLine("[Status]Bot Checking...");
                    _gotCheckBack = false;
                }
                
            }
        }
        public static void KillServerInTime(int minutes)
        {
            if (minutes == 0)
            {
                //_sentMinuteWarning = false;
                //_killTime = new DateTime(0);
                return;
            }
            //_killTime = DateTime.Now.AddMinutes(minutes);
            var sm = new SocketMessage("servermessage");
            sm.AddData("message",
                       "Server will be shutting down in " + minutes.ToString(CultureInfo.InvariantCulture) + " minutes");
        }

        private static void CurrentDomainProcessExit(object sender, EventArgs e)
        {
            Quit();
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Trace.WriteLine("------UNHANDLED EXCEPTION-------");
            var ex = (Exception) e.ExceptionObject;
            Trace.WriteLine(ex.Message);
            Trace.WriteLine(ex.StackTrace);
            Trace.WriteLine("--------------------------------");
            Quit();
        }

        private static void Quit()
        {
            try
            {
                Gaming.Stop();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (Debugger.IsAttached) Debugger.Break();
            }
            _running = false;
            Trace.WriteLine("###PROCESS QUIT####");
        }
    }
}