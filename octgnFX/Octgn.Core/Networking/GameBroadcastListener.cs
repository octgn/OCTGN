using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using log4net;

namespace Octgn.Core.Networking
{
    public class GameBroadcastListener : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsListening { get; internal set; }

        internal UdpClient Client { get; set; }
        internal ISocketMessageProcessor MessageProcessor { get; set; }

        public GameBroadcastListener()
        {
            IsListening = false;
        }

        public void StartListening()
        {
            lock (this)
            {
                if (IsListening)
                {
                    return;
                }
                try
                {
                    if (Client == null)
                    {
                        Client = new UdpClient(new IPEndPoint(IPAddress.Any, 9999));
                        Client.Client.ReceiveTimeout = 1000;
                    }

                    Receive();

                    IsListening = true;
                }
                catch (Exception e)
                {
                    Log.Error("Error listening", e);
                }
            }
        }

        public void StopListening()
        {
            lock (this)
            {
                if (!IsListening)
                    return;

                try
                {
                    Client.Close();
                }
                catch
                {
                }

                Client = null;
                IsListening = false;
            }
        }

        private void Receive()
        {
            var bundle = new SocketReceiveBundle(this.Client);
            this.Client.Client.BeginReceive(
                bundle.Buffer,
                0,
                SocketReceiveBundle.BufferSize,
                SocketFlags.None,
                this.EndReceive,
                bundle);
        }

        private void EndReceive(IAsyncResult res)
        {
            try
            {
                var state = res.AsyncState as SocketReceiveBundle;
                var count = state.TcpClient.Client.EndReceive(res);

                if (count <= 0)
                {
                    return;
                }

                this.MessageProcessor.AddData(state.Buffer.Take(count).ToArray());

                while (true)
                {
                    var buff = this.MessageProcessor.PopMessage();
                    if (buff == null) break;
                    this.OnDataRecieved(buff);
                }

                if (IsListening)
                {
                    Receive();
                }
            }
            catch (Exception e)
            {
                Log.Error("EndReceive", e);
            }
        }

        private void OnDataRecieved(byte[] data)
        {
            // Need message processor
            // Need data parsing here
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                StopListening();
            }
            catch{}
        }

        #endregion
    }
}