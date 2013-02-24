namespace Octgn.Online.StandAloneServer
{
    using System;
    using System.Reflection;
    using System.ServiceProcess;

    using Octgn.Online.StandAloneServer.Clients;

    using log4net;

    public class Service : ServiceBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public event EventHandler OnServiceStop;
        public Service()
        {
            Log.Info("Created");
        }

        public void Start()
        {
            Log.Info("Starting");
            this.OnStart(null);
            SasManagerServiceClient.GetContext().Start();
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
            Log.Info("OnStop Called");
            SasManagerServiceClient.GetContext().Stop();
            this.FireOnServiceStop();
            Log.Info("OnStop Completed");
        }
    }
}