/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using Octgn.Utils;
using Octgn.Communication;
using Octgn.Site.Api;
using Octgn.WindowsDesktopUtilities;
using Octgn.Communication.Tcp;

namespace Octgn.Online.GameService
{
    public class Service : OctgnServiceBase
    {
        private static ILogger Log;

        public static GameServiceClient Client { get; set; }

        private static DateTime _startTime;

        private static Service _service;

        static void Main(string[] args) {
            Environment.ExitCode = -69;

            LoggerFactory.DefaultMethod = (con) => new Log4NetLogger(con.Name);
            Log = LoggerFactory.Create(nameof(Service));

            //This will catch any exceptions that happen after this line
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            Signal.OnException += Signal_OnException;

            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            ApiClient.DefaultUrl = new Uri(AppConfig.Instance.ApiUrl);

            HostedGames.Init();

            var handshaker = new DefaultHandshaker();
            var connectionCreator = new TcpConnectionCreator(handshaker);

            Client = new GameServiceClient(connectionCreator);
            Client.ReconnectRetryCount = int.MaxValue; //retry forever basically

            using (_service = new Service()) {
                _service.Run(args);
            }

            ConsoleUtils.TryWaitForKey();
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

        protected override void OnStart(string[] args) {
            try {
                Log.Info($"{nameof(OnStart)}");
                Client.Start().Wait();
                Log.Info($"{nameof(OnStart)}: Starting Hosted Games");
                HostedGames.Start();
                Log.Info($"{nameof(OnStart)}: Starting Sas Updater");
                SasUpdater.Instance.Start();
                _startTime = DateTime.Now;
                Log.Info($"{nameof(OnStart)}: Done");
            } catch (Exception ex) {
                Signal.Exception(ex);
                throw;
            }
        }

        protected override void Dispose(bool disposing) {
            Log.Info($"{nameof(Dispose)}");
            base.Dispose(disposing);
            Log.Info($"{nameof(Dispose)}: Client");
            Client.Dispose();
            Log.Info($"{nameof(Dispose)}: HostedGames");
            HostedGames.Stop();
            Log.Info($"{nameof(Dispose)}: SasUpdater");
            SasUpdater.Instance.Dispose();
            Log.Info($"{nameof(Dispose)}: Done");
        }

        private static void Signal_OnException(object sender, ExceptionEventArgs args) {
            Environment.ExitCode = 20;

            Log.Error($"Signal_OnException: {args.Message}", args.Exception);

            try {
                _service.Stop();
            } catch (Exception ex) {
                Log.Error($"Signal_OnException: Could not stop service", ex);
            }
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Environment.ExitCode = 21;

            var ex = (Exception)e.ExceptionObject;

            Log.Error("Unhandled Exception", ex);
        }
    }
}
