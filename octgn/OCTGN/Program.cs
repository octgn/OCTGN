using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.ServiceModel;

namespace Octgn
{
	public static class Program
	{
        public static string tester = "test1";
		public const int Port = 8123;
		public static AssemblyName AssName = Assembly.GetExecutingAssembly().GetName();
		public static WebHoster Hoster;
        private static ServiceHost host;

		public static DirectoryInfo WebRoot { get { return new DirectoryInfo(Directory.GetCurrentDirectory()); } }

		private static void Main(string[] args)
		{


            //named pipe initialize
            host = new ServiceHost(typeof(BaseClass), new Uri[] { new Uri("net.pipe://localhost") });
            host.AddServiceEndpoint(typeof(IBaseInterface), new NetNamedPipeBinding(), "PipeBase");
            host.Open();

            
            //host initialize
			Hoster = new WebHoster(Port,WebRoot);
			Hoster.Start();
            if (Console.KeyAvailable)
            {
                Hoster.Stop();
                host.Close();
            }
		}
	}
}
