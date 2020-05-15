using Octgn.Communication;
using Octgn.Library;
using Octgn.Utils;
using Octgn.WindowsDesktopUtilities;
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
        private static ILogger Log;

        private static CancellationTokenSource _cancellationTokenSource;
        private static bool _isDebug;

        static async Task Main(string[] args) {
            using (_cancellationTokenSource = new CancellationTokenSource()) {
                try {
                    LoggerFactory.DefaultMethod = (con) => new Log4NetLogger(con.Name);
                    Log = LoggerFactory.Create(nameof(Program));

                    AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
                    AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                    Signal.OnException += Signal_OnException;

                    Console.CancelKeyPress += (_, __) => {
                        _cancellationTokenSource.Cancel();
                    };

                    _isDebug = OctgnProgram.IsDebug;

                    Log.Info("Running...");

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
            _requestPath = "F:\\SASRequests";

            if (!Directory.Exists(_requestPath)) {
                Directory.CreateDirectory(_requestPath);
            }
        }

        async Task Run(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                var request = GetNextRequest(out var filePath);

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

                if (!Version.TryParse(octgnVersionString, out var sasVersion)) throw new InvalidOperationException($"Octgn version string in request is invalid in file {filePath}");

                try {
                    await LaunchGameProcess(sasVersion, args, _isDebug);
                } catch (FileNotFoundException ex) {
                    Log.Error($"Error launching game {filePath}: SAS file not found: {ex.FileName}");
                }
            }
        }

        string GetNextRequest(out string filePath) {
            var files = Directory.GetFiles(_requestPath, "*.startrequest");

            foreach (var file in files) {
                var newPath = file + ".started";

                try {
                    File.Move(file, newPath);
                } catch (FileNotFoundException) {
                } catch (IOException) {
                    Log.Warn($"Unable to start request, it was already started: {file}");

                    continue;
                }

                var contents = File.ReadAllText(newPath);

                filePath = newPath;

                return contents;
            }

            filePath = null;
            return null;
        }

        async Task<Process> LaunchGameProcess(Version sasVersion, string[] args, bool isDebug) {
            var path = HostedGameProcess.GetSASPath(sasVersion, false, isDebug);
            var pathFileInfo = new FileInfo(path);

            var completePath = Path.Combine(pathFileInfo.Directory.FullName, "complete");

            // wait for file to exist
            {
                var waitCount = 0;
                while (!File.Exists(path) && !File.Exists(completePath)) {
                    if (waitCount >= 10) {
                        if (!File.Exists(path)) {
                            throw new FileNotFoundException($"File Not Found: {path}", path);
                        } else if (!File.Exists(completePath)) {
                            throw new FileNotFoundException($"File Not Found: {completePath}", completePath);
                        } else break; // file found last minute, #tyjezus
                    }

                    await Task.Delay(1000, _cancellationTokenSource.Token);

                    waitCount++;
                }
            }

            var process = new Process();
            process.StartInfo.Arguments = string.Join(" ", args);
            process.StartInfo.FileName = path;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            process.Start();

            return process;
        }
    }
}
