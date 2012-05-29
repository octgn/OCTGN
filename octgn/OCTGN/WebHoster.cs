using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Mono.WebServer;

namespace Octgn
{
	public class WebHoster
	{
		public int Port { get; private set; }
		public DirectoryInfo WebRoot { get; private set; }

		private static bool _keepAppRunning = true;
		private XSPWebSource webSource;
		private ApplicationServer appServer;

		public WebHoster(int port , DirectoryInfo webRoot)
		{
			Port = port;
			WebRoot = webRoot;

			webSource = new XSPWebSource(IPAddress.Any , Port);
			appServer = new ApplicationServer(webSource);

			string cmdLine=Port+":/:"+WebRoot.FullName;
			cmdLine = Port + ":/:" + "F:\\Programming\\OCTGN\\octgn\\Octgn.App";
			appServer.AddApplication("",Port,"/",WebRoot.FullName);
			//appServer.AddApplicationsFromCommandLine(cmdLine);
		}

		public void Start()
		{

			Console.WriteLine("Starting the new AppDomain to host our xsp runtime system...");
			appServer.Start(true);

			while(_keepAppRunning)
			{
				Thread.Sleep(100);
			}

			// Close down our thread and listener if required.
		}

		public void Stop() { _keepAppRunning = false; }
	}

}
