using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Octgn.Data.Properties;

namespace Octgn.Server
{
    public sealed class Server
    {
        #region Private fields

        private readonly List<Connection> _clients = new List<Connection>(); // List of all the connected clients		
        private readonly Thread _connectionChecker;
        private readonly Handler _handler; // Message handler
        private readonly TcpListener _tcp; // Underlying windows socket

        private bool _closed;
        private Thread _serverThread;
        public event EventHandler OnStop;

        private TcpClient _hostClient;

        #endregion

        #region Public interface

        // Creates and starts a new server
        public Server(int port, Guid gameId, Version gameVersion)
        {
            _tcp = new TcpListener(IPAddress.Any, port);
            _handler = new Handler(gameId, gameVersion);
            _connectionChecker = new Thread(CheckConnections);
            _connectionChecker.Start();
            Start();
        }

        // Stop the server
        public void Stop()
        {
            // Stop the server and release resources
            _closed = true;

            try
            {
                _tcp.Server.Close();
                _tcp.Stop();
            }
            catch (Exception e)
            {
                if (Debugger.IsAttached) Debugger.Break();
            }
            // Close all open connections
            _clients.ForEach(x=>x.Disconnect());
            _clients.Clear();
            if (OnStop != null)
                OnStop.Invoke(this, null);
        }

        #endregion

        #region Private implementation

        // Start the server
        private void Start()
        {
            // Creates a new thread for the server
            _serverThread = new Thread(Listen) {Name = "OCTGN.net Server"};
            // Flag used to wait until the server is really started
            var started = new ManualResetEvent(false);


            // Start the server
            _serverThread.Start(started);
            started.WaitOne();
        }

        private void CheckConnections()
        {
            while (!_closed)
            {
                Thread.Sleep(20000);
                lock (_clients)
                {
                    if (_clients.Count == 0 )
                    {
                        Stop();
                        break;
                    }
                    if(_hostClient == null)
                        _hostClient = _handler.Players.FirstOrDefault(x => x.Value.Id == 1).Key;
                    if (_hostClient == null && _handler.GameStarted == false)
                    {
                        Stop();
                        break;
                    }
                    _clients.FindAll(x=> x.Disposed || !x.Client.Connected || new TimeSpan(DateTime.Now.Ticks - x.LastPingTime.Ticks).TotalSeconds > 60).ForEach(me=>me.Disconnect());
                    _clients.RemoveAll(x => x.Disposed || !x.Client.Connected || new TimeSpan(DateTime.Now.Ticks - x.LastPingTime.Ticks).TotalSeconds > 60);
                }
            }
        }

        // Main thread function: waits and accept incoming connections
        private void Listen(object o)
        {
            // Retrieve the parameter
            var started = (ManualResetEvent) o;
            // Start the server and signal it
            _tcp.Start();
            started.Set();
            try
            {
                while (!_closed)
                {
                    // Accept new connections
                    var sc = new Connection(this, _tcp.AcceptTcpClient());
                    lock (_clients) _clients.Add(sc);
                    _hostClient = _handler.Players.FirstOrDefault(x => x.Value.Id == 1).Key;
                    if (_connectionChecker == null)
                    {
                    }
                }
            }
            catch (SocketException e)
            {
                // Interrupted is expected when the server gets stopped
                if (e.SocketErrorCode != SocketError.Interrupted) throw;
            }
        }

        #endregion

        #region Nested type: Connection

        public class Connection
        {
            internal readonly TcpClient Client; // The underlying Windows socket            
            private readonly byte[] _buffer = new byte[512]; // Buffer to receive data
            private readonly Thread _pingThread;
            private readonly Server _server; // The containing server
            public bool Disposed; // Indicates if the connection has already been disposed
            private DateTime _lastPing = DateTime.Now;
            private byte[] _packet = new byte[512]; // Buffer where received data is processed in packets
            private int _packetPos; // Current position in the packet buffer

            // C'tor
            internal Connection(Server server, TcpClient client)
            {
                // Init fields
                _server = server;
                Client = client;
                //Start ping thread
                _pingThread = new Thread(DoPing);
                _lastPing = DateTime.Now;
                _pingThread.Start();
                // Start reading
                client.GetStream().BeginRead(_buffer, 0, 512, Receive, null);
            }

            public DateTime LastPingTime
            {
                get { return (_lastPing); }
            }

            public void PingReceived()
            {
                _lastPing = DateTime.Now;
            }

            private void DoPing()
            {
                while (!Disposed)
                {
                    //lock (this)
                    //{
                        var ts = new TimeSpan(DateTime.Now.Ticks - _lastPing.Ticks);
                        if (ts.TotalSeconds > 20)
                            Disconnect("Ping timeout");
                        if (Disposed) return;
                    //}
                    Thread.Sleep(1000);
                }
            }

            // Callback when data is received
            private void Receive(IAsyncResult ar)
            {
                try
                {
                    // Get how many bytes were received
                    int count = Client.GetStream().EndRead(ar);
                    // 0 or less mean we were disconnected, or an error happened
                    if (count < 1)
                    {
                        Disconnected("End of stream.");
                        return;
                    }
                    // Copy the new data in the packet buffer. Make the buffer larger if necessary.
                    if ((_packetPos + count) > _packet.Length)
                    {
                        var newPacket = new byte[_packetPos + count];
                        Array.Copy(_packet, newPacket, _packetPos);
                        _packet = newPacket;
                    }
                    Array.Copy(_buffer, 0, _packet, _packetPos, count);

                    // Handle the received data, either as Binary or xml, depending on current status
                    BinaryReceive(count);
                    // Check if the connection is still alive (might be refused by handler)
                    if (Client.Connected)
                    {
                        // Wait for new data
                        Client.GetStream().BeginRead(_buffer, 0, 512, Receive, null);
                    }
                    else
                    {
                        Disconnected("Receieved message but client not connected.");
                    }
                }
                catch (Exception e)
                {
                    // If an unexpected error arose during processing, log it
                    if (!(e is SocketException) && !(e is ObjectDisposedException))
                    {
                        Debug.WriteLine("Unexpected exception in Server.Receive:");
                        Debug.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                    }
                    // Disconnect the client
                    Disconnected(e.Message);
                }
            }

            // Handle the received data as a Binary packet 
            private void BinaryReceive(int count)
            {
                // Adjust the current packet position with the new data
                _packetPos += count;
                // Packet starts with size as a 32 bits int.
                while (_packetPos > 4)
                {
                    int length = _packet[0] | _packet[1] << 8 | _packet[2] << 16 | _packet[3] << 24;
                    if (_packetPos < length)
                        break;
                    // Copy the packet data in an array
                    var data = new byte[length - 4];
                    Array.Copy(_packet, 4, data, 0, length - 4);


                    // Lock the handler, because it is not thread-safe
                    lock (_server._handler)
                        _server._handler.ReceiveMessage(data, Client, this);

                    // Adjust the packet pos and contents
                    _packetPos -= length;
                    Array.Copy(_packet, length, _packet, 0, _packetPos);
                }
            }

            // Disconnect the client
            internal void Disconnect(string message = "")
            {
                // Lock the disposed field
                    Console.WriteLine("Disconnect called for client : {0}",message);
                    Console.WriteLine(Resource1.Connection_Disconnect_Client_Disconnected_);
                    // Quit if this client is already disposed
                    if (Disposed) return;
                    // Mark as disposed
                    Disposed = true;
                // If it is connected, close it
                if (Client.Connected)
                {
                    try
                    {
                        Client.GetStream().Close();
                        Client.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        if (Debugger.IsAttached) Debugger.Break();
                    }
                }
                // Remove it from the list
            }

            // Notify that the client was unexpectedly disconnected
            internal void Disconnected(string message ="")
            {
                // Lock the disposed field
                //lock (this)
                //{
                    // Quit if the client is already disposed
                    if (Disposed) return;
                    // Disconnect the client
                    Disconnect();
                    // Notify the event
                    _server._handler.Disconnected(Client);
                    Console.WriteLine("Disconnected: {0}",message);
                //}

            }
        }

        #endregion
    }
}