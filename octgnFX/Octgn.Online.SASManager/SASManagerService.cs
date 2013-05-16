namespace Octgn.Online.SASManagerService
{
    using System;
    using System.Reflection;
    using System.ServiceProcess;

    using Octgn.Online.SASManagerService.Clients;

    using log4net;

    partial class SASManagerService : ServiceBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public event EventHandler OnServiceStop;
        public SASManagerService()
        {
            Log.Info("Created");
            InitializeComponent();
        }

        public void Start()
        {
            Log.Info("Starting");
            this.OnStart(null);
            GameServiceClient.GetContext().Start();
            SasManager.GetContext().Start();
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
            GameServiceClient.GetContext().Stop();
            SasManager.GetContext().Stop();
            this.FireOnServiceStop();
            Log.Info("OnStop Completed");
        }
    }
}
