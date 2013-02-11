namespace Octgn.Online.GameService
{
    using System;
    using System.Reflection;
    using System.ServiceProcess;

    using Octgn.Online.GameService.Api;

    using log4net;

    public partial class GameService : ServiceBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public event EventHandler OnServiceStop;
        public GameService()
        {
            Log.Info("Created");
            InitializeComponent();
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
            ApiManager.GetContext().Start();
            GameManager.GetContext().Start();
            Log.Info("OnStart Completed");
        }

        protected override void OnStop()
        {
            Log.Info("OnStop Called");
            ApiManager.GetContext().Stop();
            GameManager.GetContext().Stop();
            this.FireOnServiceStop();
            Log.Info("OnStop Completed");
        }
    }
}
