using Octgn.Communication;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;

namespace Octgn.WindowsDesktopUtilities
{
    public class OctgnServiceBase : ServiceBase
    {
        private static ILogger Log = LoggerFactory.Create(nameof(OctgnServiceBase));

        public static bool IsDebug => OctgnProgram.IsDebug;
        public static string AssemblyName => OctgnProgram.AssemblyName;
        public static bool IsService => !Environment.UserInteractive;

        protected OctgnServiceBase() {
            IEnumerable<string> GetProperties() {
                yield return $"{nameof(AssemblyName)}={AssemblyName}";
                yield return $"{nameof(ConsoleUtils.IsConsoleWindowVisible)}={ConsoleUtils.IsConsoleWindowVisible}";
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

            if(!ConsoleUtils.TryWaitForKey()) {
                Log.Info($"{nameof(Run)}: There's NO console window, waiting for the service to end.");
                try {
                    ServiceStoppedEvent.WaitOne();
                } catch (ObjectDisposedException) { }
            }
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
