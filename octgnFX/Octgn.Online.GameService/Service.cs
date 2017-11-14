/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using log4net;
using Octgn.Utils;
using Octgn.Communication;
using Octgn.Site.Api;
using Octgn.ServiceUtilities;

namespace Octgn.Online.GameService
{
    public class Service : OctgnServiceBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static GameServiceClient Client { get; set; }

        private static DateTime _startTime;
        static void Main(string[] args) {
            try {
                LoggerFactory.DefaultMethod = (con) => new Log4NetLogger(con.Name);
                Signal.OnException += Signal_OnException;

                ApiClient.DefaultUrl = new Uri(AppConfig.Instance.ApiUrl);

                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

                Client = new GameServiceClient();

                using (var service = new Service()) {
                    service.Run(args);
                }
            } catch (Exception e) {
                Log.Fatal("Fatal Main Error", e);
            }
        }

        protected override void OnStart(string[] args) {
            base.OnStart(args);
            Client.Start().Wait();
            HostedGames.Start();
            SasUpdater.Instance.Start();
            _startTime = DateTime.Now;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            Client.Dispose();
            HostedGames.Stop();
            SasUpdater.Instance.Dispose();
        }

        private static void Signal_OnException(object sender, ExceptionEventArgs args) {
            Log.Fatal($"Signal_OnException: {args.Message}", args.Exception);
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            var ex = (Exception)e.ExceptionObject;
            Log.Fatal("Unhandled Exception", ex);
        }
    }
}
