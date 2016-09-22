namespace Octgn.Online.StandAloneServer
{
    using System;
    using System.Reflection;
    using System.ServiceProcess;

    using Octgn.Server;

    using log4net;

    public class Service : ServiceBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal bool StopCalled { get; set; }
        public event EventHandler OnServiceStop;
        public Service()
        {
            Log.Info("Created");
        }

        public void Start()
        {
            Log.Info("Starting");
            this.OnStart(null);
            Log.Info("Started");
        }

        internal void FireOnServiceStop()
        {
            Log.Info("FireOnServiceStop Called");
            if (OnServiceStop != null)
            {
                Log.Info("OnServiceStop Event Called");
                OnServiceStop(this, new EventArgs());
            }
            Log.Info("FireOnServiceStop Completed");
        }

        protected override void OnStart(string[] args)
        {
            Log.Info("OnStart Called");
            Log.Info("OnStart Completed");
        }

        protected override void OnStop()
        {
            StopCalled = true;
            Log.Info("OnStop Called");
            this.FireOnServiceStop();
            Log.Info("OnStop Completed");
            LogManager.Shutdown();
        }
    }
}