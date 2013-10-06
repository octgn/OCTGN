namespace Octgn.Core.Networking
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;

    using log4net;

    using Octgn.Core.ExtensionMethods;

    public abstract class SocketBase : ISocket
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
        public SocketStatus Status { get; internal set; }
        public IPEndPoint EndPoint { get; internal set; }
        public ISocketMessageProcessor MessageProcessor { get; internal set; }

        internal bool FirstConnection = true;
        internal TcpClient Client;

        public void Setup(IPEndPoint ep, ISocketMessageProcessor processor)
        {
            lock (this)
            {
                if (ep == null) throw new ArgumentNullException("ep");
                if (processor == null) throw new ArgumentNullException("processor");
                if (this.Status != SocketStatus.Disconnected) throw new InvalidOperationException("You can't setup a socket if it isn't disconnected.");
                Log.DebugFormat("Setup {0}",ep);
                this.EndPoint = ep;
                if (this.Client != null)
                {
                    try{this.Client.Close();}
                    catch{}
                }
                this.Client = new TcpClient();
            }
        }

        public void Connect()
        {
            lock (this)
            {
                if (this.EndPoint == null) throw new InvalidOperationException("EndPoint must be set.");
                if (this.Status != SocketStatus.Disconnected) throw new InvalidOperationException("You can't connect if the socket isn't disconnected");
                Log.Debug("Connect");
                if (this.Client.IsDisposed())
                {
                    this.Client = new TcpClient();
                }
				this.Client.Connect(this.EndPoint);
				this.CallOnConnectionEvent(this.FirstConnection ? SocketConnectionEvent.Connected : SocketConnectionEvent.Reconnected);
                this.FirstConnection = false;
                var bundle = new SocketReceiveBundle(this.Client);
                this.Client.Client.BeginReceive(
                    bundle.Buffer,
                    0,
                    SocketReceiveBundle.BufferSize,
                    SocketFlags.None,
                    this.EndReceive,
                    bundle);
            }
        }

        public void Disconnect()
        {
            lock (this)
            {
                if (this.Status == SocketStatus.Disconnected) return;
                this.Status = SocketStatus.Disconnected;
            }
            Log.Debug("OnDisconnect");
            try { this.Client.Close(); }
            catch { }
            this.Client = null;
            this.MessageProcessor.Clear();
            this.CallOnConnectionEvent(SocketConnectionEvent.Disconnected);
        }

        internal void CallOnConnectionEvent(SocketConnectionEvent args)
        {
            try
            {
                Log.DebugFormat("CallOnConnectionEvent {0}", args);
                this.OnConnectionEvent(this, args);
            }
            catch (Exception e)
            {
                Log.Error("CallOnConnectionEvent Error", e);
            }
        }

        internal void CallOnDataReceived(byte[] data)
        {
			if (data == null) throw new ArgumentNullException("data");
            try
            {
                Log.DebugFormat("CallOnDataReceived {0} bytes", data.Length);
                this.OnDataReceived(this, data);
            }
            catch (Exception e)
            {
                Log.Error("CallOnDataReceived Error", e);
            }
        }

        internal void EndReceive(IAsyncResult res)
        {
            try
            {
                var state = res.AsyncState as SocketReceiveBundle;
                var count = state.Client.Client.EndReceive(res);

                if (count <= 0)
                {
                    this.Disconnect();
                    return;
                }

				this.MessageProcessor.AddData(state.Buffer.Take(count).ToArray());
                
                while (true)
                {
                    var buff = this.MessageProcessor.PopMessage();
                    if (buff == null) break;
					this.CallOnDataReceived(buff);
                }

                var bundle = new SocketReceiveBundle(this.Client);
                this.Client.Client.BeginReceive(
                    bundle.Buffer,
                    0,
                    SocketReceiveBundle.BufferSize,
                    SocketFlags.None,
                    this.EndReceive,
                    bundle);
            }
            catch (SocketException e)
            {
				Log.Error("EndReceive",e);
                this.Disconnect();
            }
            catch (ObjectDisposedException e)
            {
				Log.Error("EndReceive",e);
                this.Disconnect();
            }
            catch (Exception e)
            {
				Log.Error("EndReceive",e);
            }
        }

        public abstract void OnConnectionEvent(object sender, SocketConnectionEvent e);

        public abstract void OnDataReceived(object sender, byte[] data);

        ~SocketBase()
        {
            try
            {
                this.Client.Close();
            }
            catch
            {
                
            }
            this.Client = null;
            this.EndPoint = null;
        }
    }
}