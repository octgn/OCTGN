namespace Octgn.Hosting.Services
{
    using System.ServiceProcess;

    public partial class CentralHostingService : ServiceBase
    {
        public CentralHostingService()
        {
            this.InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
