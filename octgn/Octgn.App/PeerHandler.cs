// 
//  PeerHandler.cs
//  
//  Author:
//       Kelly Elton <kelly.elton@skylabsonline.com>
// 
//  Copyright (c) 2012 Kelly Elton - Skylabs Corporation
//  All Rights Reserved.
using System;
using System.Net.Sockets;
using MonoTorrent.PeerSwarm;
using MonoTorrent;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using MonoTorrent.Common;
using System.IO;
using Octgn.Common.Sockets;

namespace Octgn.App
{
	public static class PeerHandler
	{
		public static PeerSwarmManager Swarm;
		public static void MessagePeers(SocketMessage message)
		{
			var client = new UdpClient();
			foreach(var s in Swarm.Peers)
			{
				var u = s.ConnectionUri;
				var mb =SocketMessage.Serialize(message);
				client.Send(mb , mb.Length , u.Host , u.Port + 1);
			}
		}
		static PeerHandler ()
		{
			StartPeerSwarm();
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
				Port = Octgn.Program.Port,
				RequireEncryption = false,
				SupportsEncryption = true,
				ClientEvent = new TorrentEvent()
			};

			Swarm = new PeerSwarmManager(Octgn.Program.Port, param, hash, Path.Combine(Environment.CurrentDirectory, "DHTNodes.txt"));
			Swarm.AddTracker("udp://tracker.openbittorrent.com:80/announce");
			Swarm.AddTracker("udp://tracker.publicbt.com:80/announce");
			Swarm.AddTracker("udp://tracker.ccc.de:80/announce");
			Swarm.AddTracker("udp://tracker.istole.it:80/announce");
			Swarm.AddTracker("http://announce.torrentsmd.com:6969/announce");
			Swarm.PeersFound += SwarmPeersFound;
			Swarm.LogOutput += LogOutput;
			Swarm.Start();
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

