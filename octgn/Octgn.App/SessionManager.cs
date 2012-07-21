using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Octgn.Data;
using Octgn.Peer;

namespace Octgn.App
{
	public static class SessionManager
	{
		private static Dictionary<Guid , Session> Sessions;
		static SessionManager()
		{
			Sessions = new Dictionary<Guid , Session>();
		}
		/// <summary>
		/// Makes sure static classes are initialized
		/// </summary>
		public static void StartSession()
		{
			//Need to trace some values, that way this code doesn't get
			//optimized away.
			Trace.WriteLine("PeerCount: " + PeerHandler.Swarm.Peers.Count);
			Trace.WriteLine("DBCount: " + Database.DbServer.Ext().ClientCount());
		}
	}
}