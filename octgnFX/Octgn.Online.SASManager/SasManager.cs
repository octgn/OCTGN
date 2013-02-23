namespace Octgn.Online.SASManagerService
{
    using System;
    using System.Configuration;
    using System.Reflection;

    using Microsoft.AspNet.SignalR;
    using Microsoft.Owin.Hosting;

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
            Log.Info("Creating");
            Log.Info("Created");
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

        public void Configuration(IAppBuilder app)
        {
            app.MapHubs("/SasManager", new HubConfiguration());
        }
    }
}