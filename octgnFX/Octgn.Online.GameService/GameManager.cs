namespace Octgn.Online.GameService
{
    using System;

    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using Microsoft.Owin.Hosting;

    using Octgn.Online.GameService.Hubs;

    using Owin;

    using System.Configuration;
    using System.Reflection;

    using log4net;
    public class GameManager
    {
        #region singleton
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static GameManager current;
        private static readonly object Locker = new object();
        public static GameManager GetContext()
        {
            lock (Locker)
            {
                return current ?? (current = new GameManager());
            }
        }
        #endregion

        internal IDisposable Host;

        public GameManager()
        {
            Log.Info("Creating");
            Log.Info("Created");
        }

        public void Start()
        {
            Log.Info("Starting");
            Host = WebApplication.Start<GameManager>(ConfigurationManager.AppSettings["GameManagerHost"]);
            Log.Info("Started");
        }

        public void Stop()
        {
            Log.Info("Stopping");
            if (Host != null)
                Host.Dispose();
            Log.Info("Stopped");
        }

        public void Configuration(IAppBuilder app)
        {
            app.MapHubs("/GameManager", new HubConfiguration());
        }
    }
}
