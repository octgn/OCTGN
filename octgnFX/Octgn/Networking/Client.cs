using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using Octgn.Play;
using System.Threading;


namespace Octgn.Networking
{
    public sealed class Client
    {
        #region Private fields

        private IPAddress address;                  // Address to connect to
        private int port;                           // Port number to connect to
        private TcpClient tcp;                      // Underlying windows socket        
        private byte[] buffer = new byte[1024];     // Receive buffer
        private byte[] packet = new byte[1024];     // Packet buffer (gets the receive buffer's content)
        private int packetPos;                      // Position in the packet buffer
        private Handler handler;                    // Message handler
        private BinaryReceiveDelegate binHandler;   // Receive delegate when in binary mode
        private XmlReceiveDelegate xmlHandler;      // Receive delegate when in xml mode
        private Server.IServerCalls rpc;
        private bool disposed = false;              // True when the client has been closed

        private Thread PingThread;

        // Delegates definitions
        private delegate void BinaryReceiveDelegate(byte[] data);
        private delegate void XmlReceiveDelegate(string xmlMsg);

        #endregion

        #region Public interface

        // Indicates if this client is connected
        public bool IsConnected
        { get { return tcp.Client != null && tcp.Connected; } }

        // Used to send messages to the server
        internal Server.IServerCalls Rpc
        { get { return rpc; } }

        public int Muted
        { get; set; }

        // Gets the underlying windows socket
        //public TcpClient Socket
        //{ get { return tcp; } }

        // C'tor
        public Client(IPAddress address, int port)
        {
            // Init fields
            this.port = port;
            this.address = address;
            tcp = new TcpClient(address.AddressFamily);
            handler = new Handler();
            xmlHandler = new XmlReceiveDelegate(handler.ReceiveMessage);
            // Create a remote call interface
            rpc = new XmlSenderStub(tcp);
        }

        // Try to connect the client to the server
        public void Connect()
        {
            packetPos = 0;
            // Connect to the give address
            tcp.Connect(address, port);
            disposed = false;
            // Start waiting for incoming data
            tcp.GetStream().BeginRead(buffer, 0, 1024, Receive, null);
        }
        private void DoPings()
        {
            while (!disposed)
            {
                lock (this)
                {
                    if (disposed)
                        return;
                    this.Rpc.Ping();
                }
                Thread.Sleep(2000);
            }
        }
        public void StartPings()
        {
            PingThread = new Thread(DoPings);
            PingThread.Start();
        }
        public void BeginConnect(EventHandler<ConnectedEventArgs> callback)
        {
            packetPos = 0;
            tcp.BeginConnect(address, port,
                delegate(IAsyncResult ar)
                {
                    try
                    {
                        lock (this)
                        {
                            if (tcp.Client == null) return; // was cancelled
                            tcp.EndConnect(ar);
                            tcp.GetStream().BeginRead(buffer, 0, 1024, Receive, null);
                        }
                        callback(this, new ConnectedEventArgs());
                    }
                    catch (System.Net.Sockets.SocketException se)
                    {
                        callback(this, new ConnectedEventArgs(se));
                    }
                }, null);
        }

        public void CancelConnect()
        {
            lock (this)
            {
                if (tcp.Client == null) return;
                if (tcp.Connected)
                    tcp.GetStream().Close();
                tcp.Close();
            }
        }

        // Disconnect the client
        public void Disconnect()
        {
            // Lock the disposed field
            lock (this)
            {
                // Quits if this client has already been disposed
                if (disposed) return;
                // Close the connection
                if (tcp.Connected)
                    try
                    {
                        tcp.GetStream().Close();
                        tcp.Close();
                    }
                    catch
                    { }
                // Set disposed to 0
                disposed = true;
            }
        }

        public void Binary()
        {
            Rpc.Binary();
            rpc = new BinarySenderStub(tcp);
        }

        #endregion

        #region Private implementation

        // Called when the client is unexpectedly disconnected
        internal void Disconnected()
        {
            // Lock the disposed field
            lock (this)
            {
                // Quits if the client is already disconnected
                if (disposed) return;
                // Disconnect
                Disconnect();
            }

            if (Program.Dispatcher != null)
                Program.Dispatcher.Invoke(new Action<string>(Program.TraceWarning), "You have been disconnected from server.");
            else
                Program.TraceWarning("You have been disconnected from server.");
        }

        // Receive data
        private void Receive(IAsyncResult ar)
        {
            try
            {
                int count = tcp.GetStream().EndRead(ar);
                // If count <= 0 the connection has been closed, or there was an error
                if (count < 1)
                { Disconnected(); return; }
                // Copy the new data to the packet buffer (make it bigger if needed)
                if ((packetPos + count) > packet.Length)
                {
                    byte[] newPacket = new byte[packetPos + count];
                    Array.Copy(packet, newPacket, packetPos); packet = newPacket;
                }
                Array.Copy(buffer, 0, packet, packetPos, count);
                // Handle the message
                if (binHandler != null)
                    BinaryReceive(count);
                else
                    XmlReceive(count);
                // Wait for new data
                tcp.GetStream().BeginRead(buffer, 0, 1024, Receive, null);
            }
            catch (Exception e)
            {
                // Log the error if it was not socket related
                if (!(e is SocketException) && !(e is ObjectDisposedException))
                {
                    System.Diagnostics.Debug.WriteLine("Unexpected exception in Client.Receive:");
                    System.Diagnostics.Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
                // Disconnect the client if something went wrong
                Disconnected();
            }
        }

        // Handle a binary packet
        private void BinaryReceive(int count)
        {
            // Adjust the packet position with the new data
            packetPos += count;
            // Packet starts with Int32 size
            while (packetPos > 4)
            {
                int length = packet[0] | (int)packet[1] << 8 | (int)packet[2] << 16 | (int)packet[3] << 24;
                if (packetPos >= length)
                {
                    // Copy the message into a new array
                    byte[] data = new byte[length - 4]; Array.Copy(packet, 4, data, 0, length - 4);
                    // Invoke the handler
                    Program.Dispatcher.BeginInvoke(DispatcherPriority.Normal, binHandler, data);
                    // Adjust packet buffer contents
                    packetPos -= length;
                    Array.Copy(packet, length, packet, 0, packetPos);
                }
                else
                    break;
            }
        }

        // Handle an xml packet
        private void XmlReceive(int count)
        {
            // Look for a 0 at the end.
            for (int i = packetPos; i < packetPos + count; i++)
            {
                if (packet[i] == 0)
                {
                    // Extract the xml
                    string xml = System.Text.Encoding.UTF8.GetString(packet, 0, i);
                    // Invoke the handler                                        
                    Program.Dispatcher.BeginInvoke(DispatcherPriority.Normal, xmlHandler, xml);
                    // Switch to a binary handler if the message asked for it
                    if (xml == "<Binary />")
                    {
                        binHandler = new BinaryReceiveDelegate(handler.ReceiveMessage);
                        xmlHandler = null;
                    }
                    // Adjust the packet buffer
                    count += packetPos - i - 1; packetPos = 0;
                    Array.Copy(packet, i + 1, packet, 0, count);
                    i = -1; continue;
                }
            }
            // Adjust the position in the packet buffer
            packetPos += count;
        }

        #endregion
    }

    public class ConnectedEventArgs : EventArgs
    {
        public Exception exception;    // null for success

        public ConnectedEventArgs()
        { exception = null; }

        public ConnectedEventArgs(Exception error)
        { exception = error; }
    }
}