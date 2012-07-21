using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;

namespace Octgn.App
{
	//TODO probubly poinless, should maybe just remove.
	[Serializable]
	public class PeerMessage
	{
		public long ID { get; set; }
		public string Header { get; set; }
		public Dictionary<string,object> Items { get; set; }
		public PeerMessage(string header) 
		{ 
			Header = header;
			Items = new Dictionary<string , object>();
			ID = DateTime.UtcNow.ToFileTimeUtc();
		}
		public void Add(string name, object value)
		{
			Items[name] = value;
		}
		public static byte[] Serialize(PeerMessage message)
		{
			using (var ms = new MemoryStream())
			{
				try
				{
					var bf = new BinaryFormatter();
					bf.Serialize(ms, message);
					ms.Flush();
					return ms.ToArray();
				}
				catch (Exception e)
				{
					Trace.TraceError("sm1:" + e.Message, e);
				}
			}
			return null;
		}

		public static PeerMessage Deserialize(byte[] data)
		{
			using (var ms = new MemoryStream(data))
			{
				var bf = new BinaryFormatter();
				try
				{
					var sm = bf.Deserialize(ms) as PeerMessage;
					return sm;
				}
				catch (Exception e)
				{
					Trace.TraceError("sm0:" + e.Message, e);
					return null;
				}
			}
		}
	}
}