using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;

namespace Octgn
{
	public static class Program
	{
        public static string Tester = "test1";
		public const int Port = 8123;
		public static AssemblyName AssName = Assembly.GetExecutingAssembly().GetName();
		public static WebHoster Hoster;
        private static ServiceHost _host;

		public static DirectoryInfo WebRoot { get { return new DirectoryInfo(Directory.GetCurrentDirectory()); } }

		private static void Main(string[] args)
		{


            //named pipe initialize
            _host = new ServiceHost(typeof(BaseClass), new[] { new Uri("net.pipe://localhost") });
            _host.AddServiceEndpoint(typeof(IBaseInterface), new NetNamedPipeBinding(), "PipeBase");
            _host.Open();

            
            //host initialize
			Hoster = new WebHoster(Port,WebRoot);
			Hoster.Start();
            if (Console.KeyAvailable)
            {
                Hoster.Stop();
                _host.Close();
            }
		}
	}
}
