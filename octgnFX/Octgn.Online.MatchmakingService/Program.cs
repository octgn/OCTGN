/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Threading;
using log4net;
using Octgn.Library;

namespace Octgn.Online.MatchmakingService
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
				Log.Info("Starting application");
                if (args.Length == 1 && args[0].Equals("kill"))
                {
                    Log.Info("Kill mode active...");
                    if (InstanceHandler.Instance.OtherExists())
                    {
                        Log.Info("Other instance exists...Killing");
                        InstanceHandler.Instance.KillOther();
                    }
                    else
                    {
                        Log.Info("No other instance running.");
                    }
					Log.Info("Done killing");
                    return;
                }

				Log.Info("Setting up values");
                InstanceHandler.Instance.SetupValues();

                MatchmakingBot.Instance.Connect();
                MatchmakingBot.Instance.OnLogin((x) =>
                {
                });

                _startTime = DateTime.Now;
                _gotCheckBack = true;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
                AppDomain.CurrentDomain.ProcessExit += CurrentDomainProcessExit;
				Log.Info("Run called");
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
			Log.Info("Running");
            DateTime dt = DateTime.Now;
            while (_running)
            {
                if (!_running) return;
                Thread.Sleep(2000);
                if (new TimeSpan(DateTime.Now.Ticks - dt.Ticks).Seconds > 30 && _gotCheckBack == false)
                {
                    Log.Error("[Status]Bot must have died. Remaking.");
                    MatchmakingBot.Instance.Reset();
                    _gotCheckBack = true;
                }
                if (new TimeSpan(DateTime.Now.Ticks - dt.Ticks).Minutes > 1)
                {
                    dt = DateTime.Now;
                    var ts = new TimeSpan(dt.Ticks - _startTime.Ticks);
                    Log.InfoFormat("[Running For]: {0} Days, {1} Hours, {2} Minutes", ts.Days, ts.Hours, ts.Minutes);
                    MatchmakingBot.Instance.CheckStatus().ContinueWith(x =>
                    {
                        if (x.IsFaulted == false && x.IsCanceled == false && x.Result)
                        {
                            _gotCheckBack = true;
                            Log.Info("[Status]Bot Alive.");
                        }
                        else
                        {
                            Log.Info("Bot must have died...");
                        }
                    });
                    Log.Info("[Status]Bot Checking...");
                    _gotCheckBack = false;
                }
                if (InstanceHandler.Instance.KillMe)
                {
                    Log.Info("This program wants to die...");
                    _running = false;
					X.Instance.Try(MatchmakingBot.Instance.Dispose);
                }

            }
        }

        private static void CurrentDomainProcessExit(object sender, EventArgs e)
        {
            Quit();
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            Log.Fatal("Unhandled Exception", ex);
            Quit();
        }

        private static void Quit()
        {
            X.Instance.Try(MatchmakingBot.Instance.Dispose);
            //X.Instance.Try(GameManager.Instance.Dispose);
            //X.Instance.Try(SasUpdater.Instance.Dispose);
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomainUnhandledException;
            AppDomain.CurrentDomain.ProcessExit -= CurrentDomainProcessExit;
            _running = false;
            Log.Fatal("###PROCESS QUIT####");
        }
    }
}
