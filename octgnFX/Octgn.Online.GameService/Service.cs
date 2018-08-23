/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using log4net;
using Octgn.Utils;
using Octgn.Communication;
using Octgn.Site.Api;
using Octgn.WindowsDesktopUtilities;

namespace Octgn.Online.GameService
{
    public class Service : OctgnServiceBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static GameServiceClient Client { get; set; }

        private static DateTime _startTime;

        private static Service _service;

        static void Main(string[] args) {
            try {
                LoggerFactory.DefaultMethod = (con) => new Log4NetLogger(con.Name);
                Signal.OnException += Signal_OnException;

                ApiClient.DefaultUrl = new Uri(AppConfig.Instance.ApiUrl);

                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
                AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                HostedGames.Init();

                Client = new GameServiceClient();

                using (_service = new Service()) {
                    _service.Run(args);
                }
            } catch (Exception e) {
                Log.Fatal("Fatal Main Error", e);
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
            Log.Fatal($"Signal_OnException: {args.Message}", args.Exception);
            try {
                _service.Stop();
            } catch (Exception ex) {
                Log.Error($"Signal_OnException: Could not stop service", ex);
            }
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            var ex = (Exception)e.ExceptionObject;
            Log.Fatal("Unhandled Exception", ex);
        }
    }
}
