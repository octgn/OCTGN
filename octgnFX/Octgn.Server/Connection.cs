//using System;
//using System.Diagnostics;
//using System.Net.Sockets;
//using System.Threading;
//using Octgn.Core.Networking;

//namespace Octgn.Server
//{
//    public class Connection2
//    {
//        internal readonly TcpClient Client; // The underlying Windows socket            
//        private readonly byte[] _buffer = new byte[512]; // Buffer to receive data
//        private readonly Thread _pingThread;
//        private readonly Server _server; // The containing server
//        public bool Disposed; // Indicates if the connection has already been disposed
//        private DateTime _lastPing = DateTime.Now;
//        private byte[] _packet = new byte[512]; // Buffer where received data is processed in packets
//        private int _packetPos; // Current position in the packet buffer

//        // C'tor
//        internal Connection2(Server server, TcpClient client)
//        {
//            // Init fields
//            _server = server;
//            Client = client;
//            //Start ping thread
//            _pingThread = new Thread(DoPing);
//            _lastPing = DateTime.Now;
//            _pingThread.Start();
//            // Start reading
//            client.GetStream().BeginRead(_buffer, 0, 512, Receive, null);
//        }

//        public DateTime LastPingTime
//        {
//            get { return (_lastPing); }
//        }

//        public void PingReceived()
//        {
//            _lastPing = DateTime.Now;
//        }

//        private void DoPing()
//        {
//            while (!Disposed)
//            {
//                //lock (this)
//                //{
//                var ts = new TimeSpan(DateTime.Now.Ticks - _lastPing.Ticks);
//                lock (_server._handler)
//                {
//                    if (_server._handler.Players.ContainsKey(this.Client))
//                    {
//                        if (ts.TotalSeconds > 5)
//                        {
//                            Disconnected("Ping timeout");
//                        }
//                    }
//                    else
//                    {
//                        if (ts.TotalSeconds > 60)
//                        {
//                            Disconnected("Ping timeout");
//                        }
//                    }
//                }
//                if (Disposed) return;
//                //}
//                Thread.Sleep(500);
//            }
//        }

//        // Callback when data is received
//        private void Receive(IAsyncResult ar)
//        {
//            try
//            {
//                // Get how many bytes were received
//                int count = Client.GetStream().EndRead(ar);
//                // 0 or less mean we were disconnected, or an error happened
//                if (count < 1)
//                {
//                    Disconnected("End of stream.");
//                    return;
//                }
//                // Copy the new data in the packet buffer. Make the buffer larger if necessary.
//                if ((_packetPos + count) > _packet.Length)
//                {
//                    var newPacket = new byte[_packetPos + count];
//                    Array.Copy(_packet, newPacket, _packetPos);
//                    _packet = newPacket;
//                }
//                Array.Copy(_buffer, 0, _packet, _packetPos, count);

//                // Handle the received data, either as Binary or xml, depending on current status
//                BinaryReceive(count);
//                // Check if the connection is still alive (might be refused by handler)
//                if (Client.Connected)
//                {
//                    // Wait for new data
//                    Client.GetStream().BeginRead(_buffer, 0, 512, Receive, null);
//                }
//                else
//                {
//                    Disconnected("Received message but client not connected.");
//                }
//            }
//            catch (Exception e)
//            {
//                // If an unexpected error arose during processing, log it
//                if (!(e is SocketException) && !(e is ObjectDisposedException))
//                {
//                    Debug.WriteLine("Unexpected exception in Server.Receive:");
//                    Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
//                }
//                // Disconnect the client
//                Disconnected(e.Message);
//            }
//        }

//        // Handle the received data as a Binary packet 
//        private void BinaryReceive(int count)
//        {
//            // Adjust the current packet position with the new data
//            _packetPos += count;
//            // Packet starts with size as a 32 bits int.
//            while (_packetPos > 4)
//            {
//                int length = _packet[0] | _packet[1] << 8 | _packet[2] << 16 | _packet[3] << 24;
//                if (_packetPos < length)
//                    break;
//                // Copy the packet data in an array
//                var data = new byte[length - 4];
//                Array.Copy(_packet, 4, data, 0, length - 4);


//                // Lock the handler, because it is not thread-safe
//                //lock (_server._handler)
//                //    _server._handler.ReceiveMessage(data, Client, this);

//                // Adjust the packet pos and contents
//                _packetPos -= length;
//                Array.Copy(_packet, length, _packet, 0, _packetPos);
//            }
//        }

//        // Disconnect the client
//        internal void Disconnect(string message = "")
//        {
//            // Lock the disposed field
//            Console.WriteLine("Disconnect called for client : {0}", message);
//            //Console.WriteLine(Resource1.Connection_Disconnect_Client_Disconnected_);
//            // Quit if this client is already disposed
//            if (Disposed) return;
//            // Mark as disposed
//            Disposed = true;
//            // If it is connected, close it
//            if (Client.Connected)
//            {
//                try
//                {
//                    Client.GetStream().Close();
//                    Client.Close();
//                }
//                catch (Exception e)
//                {
//                    Debug.WriteLine(e);
//                    if (Debugger.IsAttached) Debugger.Break();
//                }
//            }
//            // Remove it from the list
//        }

//        // Notify that the client was unexpectedly disconnected
//        internal void Disconnected(string message = "")
//        {
//            // Lock the disposed field
//            //lock (this)
//            //{
//            // Quit if the client is already disposed
//            if (Disposed) return;
//            // Disconnect the client
//            Disconnect();
//            // Notify the event
//            _server._handler.Disconnected(Client);
//            Console.WriteLine("Disconnected: {0}", message);
//            //}

//        }
//    }
//}