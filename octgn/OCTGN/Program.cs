using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using MonoTorrent;
using MonoTorrent.Common;
using MonoTorrent.PeerSwarm;

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
			Console.CancelKeyPress += delegate { Shutdown(); };
			AppDomain.CurrentDomain.ProcessExit += delegate { Shutdown(); };
			//AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) { Debug.WriteLine(e.ExceptionObject); Shutdown(); };
			//Thread.GetDomain().UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) { Debug.WriteLine(e.ExceptionObject); Shutdown(); };

			//StartServiceHost();
			StartWebHoster();
		}

		public static void Shutdown()
		{
			//_host.Close();
			Hoster.Stop();
		}

		private static void StartServiceHost()
		{
			_host = new ServiceHost(typeof(BaseClass), new[] { new Uri("net.pipe://localhost") });
			_host.AddServiceEndpoint(typeof(IBaseInterface), new NetNamedPipeBinding(), "PipeBase");
			_host.Open();
		}

		private static void StartWebHoster()
		{
			Hoster = new WebHoster(Port, WebRoot);
			Hoster.Start();
			if (Console.KeyAvailable)
			{
				Hoster.Stop();
				_host.Close();
			}
		}
	}
}
