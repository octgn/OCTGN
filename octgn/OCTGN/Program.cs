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
		public static PeerSwarmManager Swarm;
		private static ServiceHost _host;

		public static DirectoryInfo WebRoot { get { return new DirectoryInfo(Directory.GetCurrentDirectory()); } }

		private static void Main(string[] args) 
		{
			Console.CancelKeyPress += delegate { Shutdown(); };
			AppDomain.CurrentDomain.ProcessExit += delegate { Shutdown(); };
			//AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) { Debug.WriteLine(e.ExceptionObject); Shutdown(); };
			//Thread.GetDomain().UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) { Debug.WriteLine(e.ExceptionObject); Shutdown(); };

			//StartServiceHost();
			StartPeerSwarm();
			StartWebHoster();
		}

		private static void Shutdown()
		{
			_host.Close();
			Hoster.Stop();
			Swarm.Stop();
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

		private static void StartPeerSwarm()
		{
			var sha = new SHA1CryptoServiceProvider();
			var hash = new InfoHash(sha.ComputeHash(Encoding.ASCII.GetBytes("Octgn")));
			var param = new MonoTorrent.Client.Tracker.AnnounceParameters
			{
				InfoHash = hash,
				BytesDownloaded = 0,
				BytesLeft = 0,
				BytesUploaded = 0,
				PeerId = "Octgn",
				Ipaddress = IPAddress.Any.ToString(),
				Port = Port,
				RequireEncryption = false,
				SupportsEncryption = true,
				ClientEvent = new TorrentEvent()
			};

			Swarm = new PeerSwarmManager(Port, param, hash, Path.Combine(Environment.CurrentDirectory, "DHTNodes.txt"));
			Swarm.AddTracker("udp://tracker.openbittorrent.com:80/announce");
			Swarm.AddTracker("udp://tracker.publicbt.com:80/announce");
			Swarm.AddTracker("udp://tracker.ccc.de:80/announce");
			Swarm.AddTracker("udp://tracker.istole.it:80/announce");
			Swarm.AddTracker("http://announce.torrentsmd.com:6969/announce");
			Swarm.PeersFound += SwarmPeersFound;
			Swarm.LogOutput += LogOutput;
		}

		private static void LogOutput(object sender , LogEventArgs e)
		{
			if (!e.DebugLog)
				Common.Log.L(e.Message);
			else Common.Log.D(e.Message);
		}

		private static void SwarmPeersFound(object sender , PeersFoundEventArgs e)
		{
			 
		}
	}
}
