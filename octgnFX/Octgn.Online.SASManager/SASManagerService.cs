namespace Octgn.Online.SASManagerService
{
    using System;
    using System.ServiceProcess;
    partial class SASManagerService : ServiceBase
    {
        public event EventHandler OnServiceStop;
        public SASManagerService()
        {
            InitializeComponent();
        }

        public void Start()
        {
            this.OnStart(null);
        }

        internal void FireOnServiceStop()
        {
            if(OnServiceStop != null)
                OnServiceStop(this,new EventArgs());
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
            this.FireOnServiceStop();
        }
    }
}
