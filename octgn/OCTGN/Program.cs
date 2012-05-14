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

namespace Octgn
{
	public static class Program
	{
		public static string tester = "test1";
		public const int Port = 8080;
		public static AssemblyName AssName = Assembly.GetExecutingAssembly().GetName();
		public static WebHoster Hoster;

#if(DEBUG)
		public static DirectoryInfo WebRoot { get { return new DirectoryInfo("../../../Octgn.App/"); } }
#endif

		private static void Main(string[] args)
		{
			Hoster = new WebHoster(Port,WebRoot);
			Hoster.Start();
			if (Console.KeyAvailable)
				Hoster.Stop();
		}
	}
}
