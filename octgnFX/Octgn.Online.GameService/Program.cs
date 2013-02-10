namespace Octgn.Online.GameService
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Security.Principal;
    using System.ServiceProcess;
    using System.Threading;

    using Octgn.Online.Library.UpdateManager;

    using log4net;

    internal static class Program
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static GameService Service;
        internal static bool KeepRunning;
        internal static void Main()
        {
            if (!IsAdmin()) return;
            Log.Info("Starting Octgn.Online.GameService");
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

        private static bool IsAdmin()
        {
            Log.Info("Check if running as admin(required)");
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Log.Fatal("Not running as Admin, Admin mode required. Exiting.");
                //+ Probably don't want to uncomment the blow shitz
                //var newP = new Process();
                //newP.EnableRaisingEvents = true;
                //var info = new ProcessStartInfo(Assembly.GetEntryAssembly().Location);
                //info.Verb = "runas";
                //newP.StartInfo = info;
                //newP.Start();
                //newP.WaitForExit();
                return false;
            }
            else
            {
                Log.Info("Running as admin, good...good");
            }
            return true;
        }

        private static void UpdateManagerOnOnUpdateDetected(object sender, EventArgs eventArgs)
        {
            Stop();
        }

        internal static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Log.Fatal("Unhandled Exception",ex??new Exception("Unknown Exception"));
            Stop();
        }

        internal static void StartServiceCommandLine()
        {
            Log.Info("Starting in CommandLine mode");
            using (Service = new GameService())
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
            Service = new GameService();
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
    }
}
