﻿using log4net;
using Octgn.ChatService.Data;
using Octgn.Communication;
using Octgn.Communication.Modules;
using Octgn.Communication.Serializers;
using Octgn.Communication.Tcp;
using Octgn.Online;
using Octgn.Online.Hosting;
using Octgn.WindowsDesktopUtilities;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Net;
using System.Reflection;

namespace Octgn
{
    public class Service : OctgnServiceBase
    {
        private readonly static ILog Log = LogManager.GetLogger(typeof(Service));

        private static Service _service;

        static void Main(string[] args) {
            try {
                Log.Info("Startup");

                var port = int.Parse(ConfigurationManager.AppSettings["port"]);
                var hostIp = IPAddress.Parse(ConfigurationManager.AppSettings["hostip"]);
                var gameServerUserId = ConfigurationManager.AppSettings["gameserveruserid"];
                var apiPath = ConfigurationManager.AppSettings["apiurl"];

                Octgn.Site.Api.ApiClient.DefaultUrl = new Uri(apiPath);

                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                Signal.OnException += Signal_OnException;

                LoggerFactory.DefaultMethod = CreateLogger;

                Log.Info("Updating database...");
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<DataContext, Octgn.Migrations.Configuration>());
                Log.Info("Database updated.");

                using (_service = new Service(hostIp, port, gameServerUserId)) {
                    _service.Run(args);
                }
            } catch (Exception ex) {
                Log.Fatal($"{nameof(Main)}", ex);
                throw;
            } finally {
                Log.Info($"Shutting down...");
            }
        }

        public Server Server => _server;

        private readonly int _port;
        private readonly Server _server;
        private readonly DefaultHandshaker _handshaker = new DefaultHandshaker();

        public Service(IPAddress hostIp, int port, string gameServerUserId)
        {
            _port = port;
            var endpoint = new IPEndPoint(hostIp, _port);
            _server = new Server(new TcpListener(endpoint, _handshaker), new XmlSerializer());
            _server.Attach(new Hosting(_server, gameServerUserId));
            _server.InitializeStatsModule();
        }

        protected override void OnStart(string[] args)
        {
            _server.Initialize();
        }

        protected override void OnStop()
        {
            _server.Dispose();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {

            var ex = (Exception)e.ExceptionObject;

            if (e.IsTerminating) Log.Fatal($"{nameof(CurrentDomain_UnhandledException)}", ex);
            else Log.Error($"{nameof(CurrentDomain_UnhandledException)}", ex);
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

        private static void Signal_OnException(object sender, ExceptionEventArgs args) {
            Log.Fatal($"Signal_OnException: {args.Message}", args.Exception);
            try {
                _service.ExitCode = 2;
                _service.Stop();
            } catch (Exception ex) {
                Log.Error($"Signal_OnException: Could not stop service", ex);
            }
        }

        private static ILogger CreateLogger(LoggerFactory.Context arg) {
            return new Log4NetLogger(arg.Name);
        }
    }
}
