namespace Octgn.Hosting.Services
{
    using System.ServiceProcess;

    internal static class Program
    {
        internal static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new CentralHostingService() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
