﻿namespace Octgn.Online.SASManagerService
{
    using System;
    using System.Reflection;
    using System.Security.Principal;
    using System.ServiceProcess;
    using System.Threading;

    using Octgn.Online.Library.UpdateManager;

    using log4net;

    internal class Program
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static bool KeepRunning;
        internal static SASManagerService Service;
        internal static void Main()
        {
            Log.Info("Starting Octgn.Online.SASManagerService");
            if (!IsAdmin()) return;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            if (UpdateManager.GetContext().Update()) return;
            UpdateManager.GetContext().OnUpdateDetected += UpdateManagerOnOnUpdateDetected;
            UpdateManager.GetContext().Start();
#if(DEBUG)

            StartServiceCommandLine();
            Console.WriteLine("==DONE==");
            Console.ReadLine();
#else
            StartService();
#endif
        }

        internal static void StartServiceCommandLine()
        {
            Log.Info("Starting in CommandLine mode");
            using (Service = new SASManagerService())
            {
                KeepRunning = true;
                Service.Start();
                Service.OnServiceStop += (sender, args) => Stop(false);
                Console.WriteLine("Press 'q' to quit");
                while (KeepRunning)
                {
                    if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Q) break;
                    Thread.Sleep(100);
                }
                Stop();
            }
        }

        internal static void StartService()
        {
            Log.Info("Starting in Service mode");
            Service = new SASManagerService();
            Service.OnServiceStop += (sender, args) => Stop(false);
            var services = new ServiceBase[] { Service };
            ServiceBase.Run(services);
        }

        internal static void Stop(bool stopService = true)
        {
            KeepRunning = false;
            if(Service != null && stopService)
                Service.Stop();
            UpdateManager.GetContext().Stop();
        }

        private static void UpdateManagerOnOnUpdateDetected(object sender, EventArgs eventArgs)
        {
            Stop();
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Log.Fatal("Unhandled Exception",ex??new Exception("Unknown Exception"));
        }

        private static bool IsAdmin()
        {
            Log.Info("Check if running as admin(required)");
            var identity = WindowsIdentity.GetCurrent();
            if (identity == null || !new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator))
            {
                Log.Fatal("Not running as Admin, Admin mode required. Exiting.");
                return false;
            }
            Log.Info("Running as admin, good...good");
            return true;
        }
    }
}
