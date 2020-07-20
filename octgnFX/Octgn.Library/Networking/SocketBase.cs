using System;
using System.Linq;
using System.Net.Sockets;
using log4net;
using Octgn.Library.ExtensionMethods;
using System.Threading.Tasks;
using System.Net;
using System.Runtime.InteropServices;

namespace Octgn.Library.Networking
{
    public abstract class SocketBase : ISocket
    {
        private static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SocketStatus Status { get; internal set; }
        public IPEndPoint EndPoint { get; internal set; }
        public ISocketMessageProcessor MessageProcessor { get; internal set; }
        public TcpClient Client { get; private set; }

        internal bool FirstConnection = true;

        protected SocketBase()
        {
        }

        public void Setup(IPEndPoint ep, ISocketMessageProcessor processor)
        {
            lock (this)
            {
                if (ep == null) throw new ArgumentNullException("ep");
                if (processor == null) throw new ArgumentNullException("processor");
                if (this.Status != SocketStatus.Disconnected) throw new InvalidOperationException("You can't setup a socket if it isn't disconnected.");
                Log.DebugFormat("Setup {0}", ep);
                this.EndPoint = ep;
                if (this.Client != null)
                {
                    try { this.Client.Close(); }
                    catch { }
                }
                Client = new MyBBTcpClientOhMi();
                this.MessageProcessor = processor;
            }
        }

        public void Setup(TcpClient client, ISocketMessageProcessor processor)
        {
            lock (this)
            {
                if(client == null)throw new ArgumentNullException("client");
                if (processor == null) throw new ArgumentNullException("processor");
                if (this.Status != SocketStatus.Disconnected) throw new InvalidOperationException("You can't setup a socket if it isn't disconnected.");
				Log.DebugFormat("Setup {0}",client.Client.RemoteEndPoint);
                this.EndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                this.MessageProcessor = processor;
                Client = client;
                this.Status = SocketStatus.Connected;
            }
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

        public async Task Connect()
        {
            if (this.EndPoint == null) throw new InvalidOperationException("EndPoint must be set.");
            if (this.Status != SocketStatus.Disconnected) throw new InvalidOperationException("You can't connect if the socket isn't disconnected");
            Log.Debug($"Connect to {EndPoint.Address}:{EndPoint.Port}");
            if(Client is MyBBTcpClientOhMi bb && bb.IsDisposed) {
                Client = new MyBBTcpClientOhMi();
            }
            await this.Client.ConnectAsync(this.EndPoint.Address, this.EndPoint.Port);
            Log.Debug("Connected");

            this.Status = SocketStatus.Connected;

            this.CallOnConnectionEvent(this.FirstConnection ? SocketConnectionEvent.Connected : SocketConnectionEvent.Reconnected);
            this.FirstConnection = false;

            Log.Debug("Receiving");
            var bundle = new SocketReceiveBundle(this.Client);
            this.Client.Client.BeginReceive(
                bundle.Buffer,
                0,
                SocketReceiveBundle.BufferSize,
                SocketFlags.None,
                this.EndReceive,
                bundle);
        }

        public void Disconnect()
        {
            lock (this)
            {
                if (this.Status == SocketStatus.Disconnected) return;
                this.Status = SocketStatus.Disconnected;
                System.Threading.Thread.Sleep(200);
            }
            Log.Debug("OnDisconnect");
            try { this.Client.Close(); }
            catch { }
            //this.Client = null;
            this.MessageProcessor.Clear();
            this.CallOnConnectionEvent(SocketConnectionEvent.Disconnected);
        }

        public void Send(byte[] data)
        {
            try
            {
                Client.Client.Send(data);
            }
            catch (Exception e)
            {
                Log.Warn("Send", e);
                Disconnect();
            }
        }

        internal void CallOnConnectionEvent(SocketConnectionEvent args)
        {
            try
            {
                Log.DebugFormat("CallOnConnectionEvent {0}", args);
                this.OnConnectionEvent(this, args);
                this.ConnectionChanged?.Invoke(this, new ConnectionChangedEventArgs { Event = args });
            }
            catch (Exception e)
            {
                Log.Error("CallOnConnectionEvent Error", e);
            }
        }

        internal void EndReceive(IAsyncResult res)
        {
            try
            {
                var state = res.AsyncState as SocketReceiveBundle;
                var count = state.TcpClient.Client?.EndReceive(res);

                var receivedData = count > 0;

                if (!receivedData)
                {
                    this.Disconnect();
                    return;
                }

                this.MessageProcessor.AddData(state.Buffer.Take(count.Value).ToArray());

                while (true)
                {
                    var buff = this.MessageProcessor.PopMessage();
                    if (buff == null) break;
                    this.OnDataReceived(this, buff);
                    this.DataReceived?.Invoke(this, new DataReceivedEventArgs {
                        Data = buff
                    });
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
                Log.Error("EndReceive", e);
                this.Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                Log.Warn("EndReceive", e);
                this.Disconnect();
            }
            catch (Exception e)
            {
                Log.Error("EndReceive", e);
            }
        }

        public event EventHandler<ConnectionChangedEventArgs> ConnectionChanged;
        public virtual void OnConnectionEvent(object sender, SocketConnectionEvent e) {

        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public virtual void OnDataReceived(object sender, byte[] data) {

        }

        ~SocketBase()
        {
            try
            {
                this.Client.Close();
            }
            catch
            {

            }
            Client = null;
            this.EndPoint = null;
        }

        private class MyBBTcpClientOhMi : TcpClient
        {
            public bool IsDisposed { get; private set; }

            public MyBBTcpClientOhMi() { }

            public MyBBTcpClientOhMi(TcpClient other) {
            }

            protected override void Dispose(bool disposing) {
                IsDisposed = true;

                base.Dispose(disposing);
            }
        }
    }

    public class ConnectionChangedEventArgs : EventArgs
    {
        public SocketConnectionEvent Event { get; set; }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }
}