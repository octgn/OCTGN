using Octgn.Communication;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;

namespace Octgn.ServiceUtilities
{
    public abstract class OctgnServiceBase : ServiceBase
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
            try {
                if (!IsService) {
                    Log.Info($"{nameof(Run)}: Not a service. Firing {nameof(OnStart)}");
                    OnStart(args);
                    if (IsConsoleWindowVisible) {
                        Log.Info($"{nameof(Run)}: There's a console window, waiting for a key stroke to exit.");
                        Console.ReadKey();
                    } else {
                        Log.Info($"{nameof(Run)}: There's NO console window, waiting for the service to end.");
                        try {
                            ServiceStoppedEvent.WaitOne();
                        } catch (ObjectDisposedException) { }
                    }
                } else {
                    Log.Info($"{nameof(Run)}: It's a service, using the ServiceBase");
                    var services = new ServiceBase[] { this };
                    ServiceBase.Run(services);
                }
            } finally {
                Log.Info($"{nameof(Run)}: Complete");
            }
        }

        private AutoResetEvent ServiceStoppedEvent = new AutoResetEvent(false);

        protected override void OnStop() {
            Log.Info($"{nameof(OnStop)}");
            base.OnStop();
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
