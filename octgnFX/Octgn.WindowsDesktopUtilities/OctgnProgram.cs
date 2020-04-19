using Octgn.Communication;
using System;
using System.Collections.Generic;
using System.Reflection;
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

            await _programStoppedEvent.Task;
        }

        protected virtual Task OnStart(string[] args) {
            return Task.CompletedTask;
        }

        private TaskCompletionSource<object> _programStoppedEvent = new TaskCompletionSource<object>();

        public void Stop() {
            Log.Info($"{nameof(Stop)}");
            OnStop();
        }

        protected virtual void OnStop() {
            Log.Info($"{nameof(OnStop)}");

            GC.Collect();
            GC.WaitForPendingFinalizers();

            _programStoppedEvent.TrySetResult(null);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _programStoppedEvent.TrySetCanceled();
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