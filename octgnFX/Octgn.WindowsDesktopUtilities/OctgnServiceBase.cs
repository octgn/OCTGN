using Octgn.Communication;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;

namespace Octgn.ServiceUtilities
{
    public class OctgnServiceBase : ServiceBase
    {
        private static ILogger Log = LoggerFactory.Create(nameof(OctgnServiceBase));

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

#if (DEBUG)
        internal static bool _debug = true;
#else
        internal static bool _debug = false;
#endif

        private static string _assemblyName = Assembly.GetEntryAssembly().GetName().Name + "v" + Assembly.GetEntryAssembly().GetName().Version;

        public string AssemblyName => _assemblyName;
        public static bool IsConsoleWindowVisible => GetConsoleWindow() != IntPtr.Zero;
        public static bool IsService => !Environment.UserInteractive;
        public static bool IsDebug => _debug;

        protected OctgnServiceBase() {
            IEnumerable<string> GetProperties() {
                yield return $"{nameof(AssemblyName)}={AssemblyName}";
                yield return $"{nameof(IsConsoleWindowVisible)}={IsConsoleWindowVisible}";
                yield return $"{nameof(IsService)}={IsService}";
            };

            var joined = string.Join(": ", GetProperties());

            Log.Info($"{nameof(OctgnServiceBase)}: {joined}");
        }

        public void Run(string[] args) {
            if (IsService) {
                Log.Info($"{nameof(Run)}: It's a service, using the ServiceBase");
                var services = new ServiceBase[] { this };
                ServiceBase.Run(services);
                return;
            }

            Log.Info($"{nameof(Run)}: Not a service. Firing {nameof(OnStart)}");
            OnStart(args);

            if(!TryWaitForKey()) {
                Log.Info($"{nameof(Run)}: There's NO console window, waiting for the service to end.");
                try {
                    ServiceStoppedEvent.WaitOne();
                } catch (ObjectDisposedException) { }
            }
        }

        /// <summary>
        /// Try and wait for a key from the console. If the application is in a state where it
        /// can't receive input from the console, this returns false and doesn't wait.
        /// </summary>
        /// <returns>True if it waited for a key, otherwise false</returns>
        protected static bool TryWaitForKey() {
            if (!IsConsoleWindowVisible) return false;
            Log.Info($"{nameof(TryWaitForKey)}: There's a console window, waiting for a key stroke to exit.");
            Console.WriteLine("Waiting for key...");
            Console.ReadKey();
            return true;
        }

        private AutoResetEvent ServiceStoppedEvent = new AutoResetEvent(false);

        protected override void OnStop() {
            Log.Info($"{nameof(OnStop)}");
            base.OnStop();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (Signal.Exceptions.Count > 0) {
                while (Signal.Exceptions.Count > 0) {
                    if (Signal.Exceptions.TryDequeue(out var result)) {
                        Log.Error($"{nameof(Signal)} Exception: {result.Message}", result.Exception);
                    }
                }
            }

            try {
                ServiceStoppedEvent.Set();
            } catch (ObjectDisposedException) { }
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (!disposing) return;

            ServiceStoppedEvent.Dispose();
        }
    }
}
