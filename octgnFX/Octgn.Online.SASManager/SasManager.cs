namespace Octgn.Online.SASManagerService
{
    using System;
    using System.Configuration;
    using System.Reflection;

    using Microsoft.AspNet.SignalR;
    using Microsoft.Owin.Hosting;

    using Octgn.Online.Library.Enums;
    using Octgn.Online.Library.Models;
    using Octgn.Online.Library.Net;

    using Owin;

    using log4net;

    public class SasManager
    {
        #region singleton
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static SasManager current;
        private static readonly object Locker = new object();
        public static SasManager GetContext()
        {
            lock (Locker)
            {
                return current ?? (current = new SasManager());
            }
        }
        #endregion

        internal IDisposable Host;

        public SasManager()
        {
            Log.Debug("Creating");
            Log.Debug("Created");
        }

        public void Start()
        {
            Log.Info("Starting");
            Host = WebApplication.Start<SasManager>(ConfigurationManager.AppSettings["SASManagerHost"]);
            Log.Info("Started");
        }

        public void Stop()
        {
            Log.Info("Stopping");
            if (Host != null)
                Host.Dispose();
            Log.Info("Stopped");
        }

        public void StartGame(HostedGameSASRequest request)
        {
            var hostName = Tools.HostName;
            var port = Tools.GetNextPort();
            var uri = new Uri("http://" + hostName + ":" + port);
            var model = request.ToHostedGameSasModel(uri);

            var state = model.ToHostedGameState(EnumHostedGameStatus.BootRequested);
            state.Engine().Register().LaunchProcess();
        }

        public void Configuration(IAppBuilder app)
        {
            app.MapHubs("/SasManager", new HubConfiguration());
        }
    }
}