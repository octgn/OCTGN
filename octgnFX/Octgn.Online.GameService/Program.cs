using System;
using System.Reflection;
using System.Threading;
using log4net;
using Octgn.Library;

namespace Octgn.Online.GameService
{
    class Program
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static bool _gotCheckBack;
        private static bool _running = true;
        private static DateTime _startTime;
        static void Main(string[] args)
        {
            try
            {
                GameBot.Instance.Start();
                GameManager.Instance.Start();
                _startTime = DateTime.Now;
                _gotCheckBack = true;
                GameBot.Instance.OnCheckRecieved += OnCheckRecieved;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
                AppDomain.CurrentDomain.ProcessExit += CurrentDomainProcessExit;
                Run();
            }
            catch (Exception e)
            {
                Log.Fatal("Fatal Main Error", e);
            }
            finally
            {
                Quit();
            }
        }

        static void Run()
        {
            DateTime dt = DateTime.Now;
            while (_running)
            {
                if (!_running) return;
                Thread.Sleep(100);
                if (new TimeSpan(DateTime.Now.Ticks - dt.Ticks).Seconds > 30 && _gotCheckBack == false)
                {
                    Log.Error("[Status]Bot must have died. Remaking.");
                    GameBot.Instance.RemakeXmpp();
                    _gotCheckBack = true;
                }
                if (new TimeSpan(DateTime.Now.Ticks - dt.Ticks).Minutes > 1)
                {
                    dt = DateTime.Now;
                    var ts = new TimeSpan(dt.Ticks - _startTime.Ticks);
                    Log.InfoFormat("[Running For]: {0} Days, {1} Hours, {2} Minutes", ts.Days, ts.Hours, ts.Minutes);
                    GameBot.Instance.CheckBotStatus();
                    Log.Info("[Status]Bot Checking...");
                    _gotCheckBack = false;
                }

            }
        }

        private static void OnCheckRecieved(object sender)
        {
            _gotCheckBack = true;
            Log.Info("[Status]Bot Alive.");
        }

        private static void CurrentDomainProcessExit(object sender, EventArgs e)
        {
            Quit();
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            Log.Fatal("Unhandled Exception",ex);
            Quit();
        }

        private static void Quit()
        {
            X.Instance.Try(GameBot.Instance.Dispose);
            X.Instance.Try(GameManager.Instance.Dispose);
            X.Instance.Try(()=>GameBot.Instance.OnCheckRecieved -= OnCheckRecieved);
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomainUnhandledException;
            AppDomain.CurrentDomain.ProcessExit -= CurrentDomainProcessExit;
            _running = false;
            Log.Fatal("###PROCESS QUIT####");
        }
    }
}
