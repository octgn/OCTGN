namespace Octgn.Online.GameService
{
    using System;
    using System.Reflection;
    using System.ServiceProcess;
    using System.Threading;

    using log4net;

    internal static class Program
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static GameService Service;
        internal static bool KeepRunning;
        internal static void Main()
        {
            Log.Info("Starting Octgn.Online.GameService");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            if (UpdateManager.Update()) return;
            UpdateManager.OnUpdateDetected += UpdateManagerOnOnUpdateDetected;
            UpdateManager.Start();
#if(DEBUG)

            StartServiceCommandLine();
            Console.WriteLine("==DONE==");
            Console.ReadLine();
#else
            StartService();
#endif

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
            UpdateManager.Stop();
        }
    }
}
