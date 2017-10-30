using Exceptionless;
using log4net;
using Octgn.ChatService.Data;
using Octgn.Communication;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Net;
using System.ServiceProcess;

namespace Octgn
{
    class Program
    {
        private readonly static ILog Log = LogManager.GetLogger(typeof(Program));

        public static Service Service { get; private set; }

        public static bool IsDebug {
            get {
#if (DEBUG)
                return true;
#else
                return false;
#endif
            }
        }

        static void Main(string[] args) {
            try {
                Log.Info("Startup");

                var exapikey = ConfigurationManager.AppSettings["Exceptionless:ApiKey"];
                var port = int.Parse(ConfigurationManager.AppSettings["port"]);
                var hostIp = IPAddress.Parse(ConfigurationManager.AppSettings["hostip"]);
                var gameServerName = ConfigurationManager.AppSettings["gameservername"];
                var apiPath = ConfigurationManager.AppSettings["apiurl"];

                ExceptionlessClient.Default.Startup(exapikey);

                Octgn.Site.Api.ApiClient.DefaultUrl = new Uri(apiPath);

                ExceptionlessClient.Default.Configuration.UseSessions();
                ExceptionlessClient.Default.SubmitSessionStart();

                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                LoggerFactory.DefaultMethod = CreateLogger;

                Log.Info("Updating database...");
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<DataContext, Octgn.Migrations.Configuration>());
                Log.Info("Database updated.");

                Service = new Service(hostIp, port, gameServerName);

                if (IsDebug && Environment.UserInteractive) {
                    Service.Start();
                    Console.ReadKey();
                } else {
                    var services = new ServiceBase[] { Service };
                    ServiceBase.Run(services);
                }
            } catch (Exception ex) {
                Log.Fatal($"{nameof(Main)}", ex);
            } finally {
                Log.Info($"Shutting down...");
            }

            Log.Info($"Waiting for finalizers");

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Log.Info($"Done waiting for finalizers");

            if (Signal.Exceptions.Count > 0) {
                while (Signal.Exceptions.Count > 0) {
                    if (Signal.Exceptions.TryDequeue(out var result)) {
                        Log.Error($"{nameof(Signal)} Exception: {result.Message}", result.Exception);
                    }
                }
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {

            var ex = (Exception)e.ExceptionObject;

            if (e.IsTerminating) Log.Fatal($"{nameof(CurrentDomain_UnhandledException)}", ex);
            else Log.Error($"{nameof(CurrentDomain_UnhandledException)}", ex);
        }

        private static ILogger CreateLogger(LoggerFactory.Context arg) {
            return new Log4NetLogger(arg.Name);
        }
    }
}
