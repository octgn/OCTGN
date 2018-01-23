using Octgn.Communication;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Octgn.WindowsDesktopUtilities
{
    public class OctgnProgram : IDisposable
    {
        private static ILogger Log = LoggerFactory.Create(nameof(OctgnProgram));

        public static bool IsDebug => _debug;
#if (DEBUG)
        public static bool _debug = true;
#else
        public static bool _debug = false;
#endif

        public static string AssemblyName => _assemblyName;
        private static string _assemblyName = Assembly.GetEntryAssembly().GetName().Name + "v" + Assembly.GetEntryAssembly().GetName().Version;

        protected OctgnProgram() {
            IEnumerable<string> GetProperties() {
                yield return $"{nameof(AssemblyName)}={AssemblyName}";
                yield return $"{nameof(ConsoleUtils.IsConsoleWindowVisible)}={ConsoleUtils.IsConsoleWindowVisible}";
            };

            var joined = string.Join(": ", GetProperties());

            Log.Info($"{nameof(OctgnProgram)}: {joined}");

            Signal.OnException += (sender, args)=> {
                Log.Error($"Signal Exception: {args.Message}", args.Exception);
                Task.Run(()=>Stop());
            };
        }

        public async Task Run(string[] args) {
            Log.Info($"Calling  OnStart");
            await OnStart(args);

            if(!ConsoleUtils.TryWaitForKey()) {
                Log.Info($"{nameof(Run)}: There's NO console window, waiting for the service to end.");
                try {
                    _programStoppedEvent.WaitOne();
                } catch (ObjectDisposedException) { }
            }
        }

        protected virtual Task OnStart(string[] args) {
            return Task.CompletedTask;
        }

        private AutoResetEvent _programStoppedEvent = new AutoResetEvent(false);

        public void Stop() {
            Log.Info($"{nameof(Stop)}");
            OnStop();
        }

        protected virtual void OnStop() {
            Log.Info($"{nameof(OnStop)}");

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
                _programStoppedEvent.Set();
            } catch (ObjectDisposedException) { }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _programStoppedEvent.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose() {
            Log.Info($"{nameof(Dispose)}");
            Dispose(true);
        }
        #endregion
    }
}