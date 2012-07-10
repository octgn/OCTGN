using System.IO;
using System.Net;
using System.Threading;
using Mono.WebServer;
using Octgn.Common;

namespace Octgn
{
	public class WebHoster
	{
		public int Port { get; private set; }
		public DirectoryInfo WebRoot { get; private set; }

		private static bool _keepAppRunning = true;
		private readonly XSPWebSource _webSource;
		private readonly ApplicationServer _appServer;

		public WebHoster(int port , DirectoryInfo webRoot)
		{
			Port = port;
			WebRoot = webRoot;
#if(DEBUG)
			var debugPath = Path.Combine(webRoot.Parent.Parent.FullName,"Octgn.App");
			WebRoot = new DirectoryInfo(debugPath);
#endif
			//TODO Figure out how to deal with this obsolete issue.
			_webSource = new XSPWebSource(IPAddress.Any , Port);
			_appServer = new ApplicationServer(_webSource);
			
			_appServer.AddApplication("",Port,"/",WebRoot.FullName);
			Common.Log.L("Web Root: {0}",WebRoot.FullName);
		}

		public void Start()
		{
			Log.L("Starting the new AppDomain to host our xsp runtime system...");
			_appServer.Start(true);

			while(_keepAppRunning)
			{
				Thread.Sleep(100);
			}
		}

		public void Stop() { _keepAppRunning = false; }
	}

}
