using Octgn.Communication;
using Octgn.Library;
using Octgn.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Octgn.Online.GameService.ServerLauncher
{
    class Program
    {
        private static ILogger Log = LoggerFactory.Create(nameof(Program));

        private static CancellationTokenSource _cancellationTokenSource;
        private static bool _isDebug;

        static async Task Main(string[] args) {
            using (_cancellationTokenSource = new CancellationTokenSource()) {
                try {
                    Console.CancelKeyPress += (_, __) => {
                        _cancellationTokenSource.Cancel();
                    };


                    LoggerFactory.DefaultMethod = (con) => new Log4NetLogger(con.Name);
                    Signal.OnException += Signal_OnException;
                    AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
                    AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                    throw new NotImplementedException("IS DEBUG NOT SET");

                    var proggy = new Program();

                    await proggy.Run(_cancellationTokenSource.Token);
                } catch (Exception ex) {
                    Log.Error($"FATAL ERROR: {ex}");
                }
            }
        }

        private static void Signal_OnException(object sender, ExceptionEventArgs args) {
            Log.Error($"Signal_OnException: {args.Message}", args.Exception);

            Task.Run(() => {
                _cancellationTokenSource.Cancel();
            });
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            var ex = (Exception)e.ExceptionObject;

            Log.Error("Unhandled Exception", ex);
        }
        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args) {
            Log.Info($"Loaded Assembly: {args.LoadedAssembly.FullName}");
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            Log.Warn($"Assembly Failed To Load: {args.Name}: {args.RequestingAssembly.FullName}");

            // Return Assembly if found, or null if assembly can't be resolved.
            // We're not trying to resolve anything, just log the fact that it can't be resolved.
            return null;
        }

        private readonly string _requestPath;

        public Program() {
            _requestPath = Path.Combine(Path.GetTempPath(), "Octgn", "GameService", "StartSASRequests");

            if (!Directory.Exists(_requestPath)) {
                Directory.CreateDirectory(_requestPath);
            }
        }

        async Task Run(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                // check for game
                var request = GetNextRequest();

                if (request == null) {
                    await Task.Delay(1000, cancellationToken);

                    continue;
                }

                string[] args;
                string octgnVersionString;
                using (var reader = new StringReader(request)) {
                    octgnVersionString = reader.ReadLine();

                    var argList = new List<string>();

                    var argLine = reader.ReadLine();

                    while (argLine != null) {
                        argList.Add(argLine);

                        argLine = reader.ReadLine();
                    }

                    args = argList.ToArray();
                }

                //TODO: Should log file name that has bad data
                if (!Version.TryParse(octgnVersionString, out var sasVersion)) throw new InvalidOperationException($"Octgn version string in request is invalid.");

                // launch game
                LaunchGameProcess(sasVersion, args, _isDebug);
            }

            throw new NotImplementedException();
        }

        string GetNextRequest() {
            var files = Directory.GetFiles(_requestPath, "*.startrequest");

            foreach (var file in files) {
                try {
                    var contents = File.ReadAllText(file);

                    File.Move(file, file + ".started");

                    return contents;
                } catch (Exception ex) {
                    Log.Warn($"Unable to open {file}: {ex}");
                }
            }

            return null;
        }

        Process LaunchGameProcess(Version sasVersion, string[] args, bool isDebug) {
            var path = HostedGameProcess.GetSASPath(sasVersion, false, isDebug);

            var process = new Process();

            process.StartInfo.Arguments = string.Join(" ", args);
            process.StartInfo.FileName = path;

            process.Start();

            return process;
        }
    }
}
