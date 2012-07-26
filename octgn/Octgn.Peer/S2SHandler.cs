using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Octgn.Common;
using Octgn.Common.Sockets;

namespace Octgn.Peer
{
	public class S2SHandler
	{
		public delegate void MessageReceivedHandler(SocketMessage message , EndPoint from);

		public event MessageReceivedHandler MessageReceived;
		private readonly UdpClient _listener;
		private readonly int _port;

		public S2SHandler(int port)
		{
			try
			{
				_port = port;
				_listener = new UdpClient(new IPEndPoint(IPAddress.Any , _port));
				//_listener.AllowNatTraversal(true);
				BeginReceive();
				RLog("Started Listening on Port {0}" , _port);
			}
			catch(Exception e)
			{
				LogError(0 , e.Message);
			}
		}
		public void Stop()
		{
			try
			{
				_listener.Close();
			}
			catch(Exception e)
			{
				LogError(1 , e.Message);
			}
		}
		public void MessagePeer(SocketMessage message, Uri p)
		{
			try
			{
				var mb = SocketMessage.Serialize(message);
				_listener.Send(mb , mb.Length , p.Host , p.Port);
			}
			catch(Exception e)
			{
				LogError(2 , e.Message);
			}
		}
		public void MessagePeers(SocketMessage message, Uri[] peers)
		{
			foreach (var u in peers) MessagePeer(message , u);
		}
		private void BeginReceive(UdpState state = null)
		{
			try
			{
				if(state == null)
				{
					state = new UdpState
					{
						Listener = _listener.Client
					};
				}
				_listener.Client.BeginReceiveFrom(state.Bytes , 0 , UdpState.BufferSize , SocketFlags.None ,
				                                  ref state.RemoteEndPoint , EndReceive , state);
			}
			catch(Exception e)
			{
				LogError(3 , e.Message);
			}
		}
		private void EndReceive(IAsyncResult res)
		{
			try
			{

				var state = (UdpState) res.AsyncState;
				var mess = SocketMessage.Deserialize(state.Bytes);
				//TODO Using PLINQ and some fanciness should be able to invoke all at the same time
				if(mess != null && MessageReceived != null) MessageReceived.Invoke(mess , state.RemoteEndPoint);
				BeginReceive(state);
			}
			catch(Exception e)
			{
				LogError(4 , e.Message);
			}
		}
		private void LogError(int num, string format, params object[] args){ Log.L("[S2SHandler:Error{0}] {1}" , num , String.Format(format,args)); }
		private void DLog(string format, params object[] args) { Log.D("[S2SHandler] {0}" ,String.Format(format,args)); }
		private void RLog(string format, params object[] args) { Log.L("[S2SHandler] {0}" ,String.Format(format,args)); }
	}
	public class UdpState
	{
		public Socket Listener;
		public byte[] Bytes = new byte[BufferSize];
		public const int BufferSize = 1024*10;
		public EndPoint RemoteEndPoint = new IPEndPoint(IPAddress.Any,0);
	}
}
