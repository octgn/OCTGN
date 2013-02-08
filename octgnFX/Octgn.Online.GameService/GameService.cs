namespace Octgn.Online.GameService
{
    using System;
    using System.ServiceProcess;

    using Octgn.Online.GameService.Api;

    public partial class GameService : ServiceBase
    {
        public event EventHandler OnServiceStop;
        public GameService()
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
            ApiManager.GetContext().Start();
            GameManager.GetContext().Start();
        }

        protected override void OnStop()
        {
            ApiManager.GetContext().Stop();
            GameManager.GetContext().Stop();
            this.FireOnServiceStop();
        }
    }
}
