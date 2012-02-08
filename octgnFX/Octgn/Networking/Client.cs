using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Octgn.Server;

namespace Octgn.Networking
{
    public sealed class Client
    {
        #region Private fields

        private readonly IPAddress _address; // Address to connect to
        private readonly byte[] _buffer = new byte[1024]; // Receive buffer
        private readonly Handler _handler; // Message handler
        private readonly int _port; // Port number to connect to
        private readonly TcpClient _tcp; // Underlying windows socket        
        private Thread _pingThread;
        private BinaryReceiveDelegate _binHandler; // Receive delegate when in binary mode
        private bool _disposed; // True when the client has been closed
        private byte[] _packet = new byte[1024]; // Packet buffer (gets the receive buffer's content)
        private int _packetPos; // Position in the packet buffer
        private XmlReceiveDelegate _xmlHandler; // Receive delegate when in xml mode

        // Delegates definitions

        #region Nested type: BinaryReceiveDelegate

        private delegate void BinaryReceiveDelegate(byte[] data);

        #endregion

        #region Nested type: XmlReceiveDelegate

        private delegate void XmlReceiveDelegate(string xmlMsg);

        #endregion

        #endregion

        #region Public interface

        // Indicates if this client is connected
        public Client(IPAddress address, int port)
        {
            // Init fields
            this._port = port;
            this._address = address;
            _tcp = new TcpClient(address.AddressFamily);
            _handler = new Handler();
            _xmlHandler = _handler.ReceiveMessage;
            // Create a remote call interface
            Rpc = new XmlSenderStub(_tcp);
        }

        public bool IsConnected
        {
            get { return _tcp.Client != null && _tcp.Connected; }
        }

        // Used to send messages to the server
        internal IServerCalls Rpc { get; private set; }

        public int Muted { get; set; }

        // Gets the underlying windows socket
        //public TcpClient Socket
        //{ get { return tcp; } }

        // C'tor

        // Try to connect the client to the server
        public void Connect()
        {
            _packetPos = 0;
            // Connect to the give address
            _tcp.Connect(_address, _port);
            _disposed = false;
            // Start waiting for incoming data
            _tcp.GetStream().BeginRead(_buffer, 0, 1024, Receive, null);
        }

        private void DoPings()
        {
            while (!_disposed)
            {
                lock (this)
                {
                    if (_disposed)
                        return;
                    Rpc.Ping();
                }
                Thread.Sleep(2000);
            }
        }

        public void StartPings()
        {
            _pingThread = new Thread(DoPings);
            _pingThread.Start();
        }

        public void BeginConnect(EventHandler<ConnectedEventArgs> callback)
        {
            _packetPos = 0;
            _tcp.BeginConnect(_address, _port,
                             delegate(IAsyncResult ar)
                                 {
                                     try
                                     {
                                         lock (this)
                                         {
                                             if (_tcp.Client == null) return; // was cancelled
                                             _tcp.EndConnect(ar);
                                             _tcp.GetStream().BeginRead(_buffer, 0, 1024, Receive, null);
                                         }
                                         callback(this, new ConnectedEventArgs());
                                     }
                                     catch (SocketException se)
                                     {
                                         callback(this, new ConnectedEventArgs(se));
                                     }
                                 }, null);
        }

        public void CancelConnect()
        {
            lock (this)
            {
                if (_tcp.Client == null) return;
                if (_tcp.Connected)
                    _tcp.GetStream().Close();
                _tcp.Close();
            }
        }

        // Disconnect the client
        public void Disconnect()
        {
            // Lock the disposed field
            lock (this)
            {
                // Quits if this client has already been disposed
                if (_disposed) return;
                // Close the connection
                if (_tcp.Connected)
                    try
                    {
                        _tcp.GetStream().Close();
                        _tcp.Close();
                    }
                    catch
                    {
                    }
                // Set disposed to 0
                _disposed = true;
            }
        }

        public void Binary()
        {
            Rpc.Binary();
            Rpc = new BinarySenderStub(_tcp);
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
                if (_disposed) return;
                // Disconnect
                Disconnect();
            }

            if (Program.Dispatcher != null)
                Program.Dispatcher.Invoke(new Action<string>(Program.TraceWarning),
                                          "You have been disconnected from server.");
            else
                Program.TraceWarning("You have been disconnected from server.");
        }

        // Receive data
        private void Receive(IAsyncResult ar)
        {
            try
            {
                int count = _tcp.GetStream().EndRead(ar);
                // If count <= 0 the connection has been closed, or there was an error
                if (count < 1)
                {
                    Disconnected();
                    return;
                }
                // Copy the new data to the packet buffer (make it bigger if needed)
                if ((_packetPos + count) > _packet.Length)
                {
                    var newPacket = new byte[_packetPos + count];
                    Array.Copy(_packet, newPacket, _packetPos);
                    _packet = newPacket;
                }
                Array.Copy(_buffer, 0, _packet, _packetPos, count);
                // Handle the message
                if (_binHandler != null)
                    BinaryReceive(count);
                else
                    XmlReceive(count);
                // Wait for new data
                _tcp.GetStream().BeginRead(_buffer, 0, 1024, Receive, null);
            }
            catch (Exception e)
            {
                // Log the error if it was not socket related
                if (!(e is SocketException) && !(e is ObjectDisposedException))
                {
                    Debug.WriteLine("Unexpected exception in Client.Receive:");
                    Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
                // Disconnect the client if something went wrong
                Disconnected();
            }
        }

        // Handle a binary packet
        private void BinaryReceive(int count)
        {
            // Adjust the packet position with the new data
            _packetPos += count;
            // Packet starts with Int32 size
            while (_packetPos > 4)
            {
                int length = _packet[0] | _packet[1] << 8 | _packet[2] << 16 | _packet[3] << 24;
                if (_packetPos < length)
                    break;
                // Copy the message into a new array
                var data = new byte[length - 4];
                Array.Copy(_packet, 4, data, 0, length - 4);
                // Invoke the handler
                Program.Dispatcher.BeginInvoke(DispatcherPriority.Normal, _binHandler, data);
                // Adjust packet buffer contents
                _packetPos -= length;
                Array.Copy(_packet, length, _packet, 0, _packetPos);
            }
        }

        // Handle an xml packet
        private void XmlReceive(int count)
        {
            // Look for a 0 at the end.
            for (int i = _packetPos; i < _packetPos + count; i++)
            {
                if (_packet[i] != 0) continue;
                // Extract the xml
                string xml = Encoding.UTF8.GetString(_packet, 0, i);
                // Invoke the handler                                        
                Program.Dispatcher.BeginInvoke(DispatcherPriority.Normal, _xmlHandler, xml);
                // Switch to a binary handler if the message asked for it
                if (xml == "<Binary />")
                {
                    _binHandler = _handler.ReceiveMessage;
                    _xmlHandler = null;
                }
                // Adjust the packet buffer
                count += _packetPos - i - 1;
                _packetPos = 0;
                Array.Copy(_packet, i + 1, _packet, 0, count);
                i = -1;
            }
            // Adjust the position in the packet buffer
            _packetPos += count;
        }

        #endregion
    }

    public class ConnectedEventArgs : EventArgs
    {
        public Exception Exception; // null for success

        public ConnectedEventArgs()
        {
            Exception = null;
        }

        public ConnectedEventArgs(Exception error)
        {
            Exception = error;
        }
    }
}