using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Octgn.Server
{
    public sealed class Server
    {
        #region Private fields

        private readonly Thread ConnectionChecker;
        private readonly List<Connection> clients = new List<Connection>(); // List of all the connected clients		
        private readonly Handler handler; // Message handler
        private readonly TcpListener tcp; // Underlying windows socket

        private Thread ServerThread;

        private Timer checkConnectionDrops;
        private bool closed;
        public event EventHandler OnStop;

        #endregion

        #region Public interface

        // Creates and starts a new server
        public Server(int port, Guid gameId, Version gameVersion)
        {
            tcp = new TcpListener(IPAddress.Any, port);
            handler = new Handler(gameId, gameVersion);
            ConnectionChecker = new Thread(CheckConnections);
            ConnectionChecker.Start();
            Start();
        }

        // Stop the server
        public void Stop()
        {
            // Stop the server and release resources
            closed = true;

            //drop connection timer cleanup
            checkConnectionDrops.Dispose();

            try
            {
                tcp.Server.Close();
                tcp.Stop();
            }
            catch
            {
            }
            // Close all open connections
            while (true)
            {
                Connection client = null;
                lock (clients)
                {
                    if (clients.Count > 0)
                        client = clients[0];
                }
                if (client != null)
                    client.Disconnect();
                else
                    break;
            }
            if (OnStop != null)
                OnStop.Invoke(this, null);
        }

        #endregion

        #region Private implementation

        // Start the server
        private void Start()
        {
            // Creates a new thread for the server
            ServerThread = new Thread(Listen);
            ServerThread.Name = "OCTGN.net Server";
            // Flag used to wait until the server is really started
            var started = new ManualResetEvent(false);

            //start checking for dropped connections every 60 seconds
            TimerCallback tcb = CheckForDroppedConnection;
            checkConnectionDrops = new Timer(tcb, null, 0, 60000);


            // Start the server
            ServerThread.Start(started);
            started.WaitOne();
        }

        // Called when a client gets disconnected
        public bool Disconnected(TcpClient lost)
        {
            bool ret = false;
            lock (clients)
            {
                // Search the client
                foreach (Connection t in clients)
                {
                    if (t.client == lost)
                    {
                        // Remove it
                        t.Disconnected();
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }

        private void CheckConnections()
        {
            while (!closed)
            {
                Thread.Sleep(10000);
                lock (clients)
                {
                    if (clients.Count == 0)
                    {
                        Stop();
                    }
                    DoAgain:
                    for (int i = 0; i < clients.Count; i++)
                    {
                        if (clients[i].disposed)
                        {
                            clients.RemoveAt(i);
                            goto DoAgain;
                        }
                    }
                }
            }
        }

        private void CheckForDroppedConnection(object stateInfo)
        {
            if (!closed)
            {
                Connection[] connections = clients.ToArray();
                for (int i = 0; i < connections.Length; i++)
                {
                    var ts = new TimeSpan(DateTime.Now.Ticks - connections[i].LastPingTime.Ticks);
                    if (ts.Seconds > 60)
                    {
                        connections[i].Disconnected();
                    }
                }
            }
        }

        // Main thread function: waits and accept incoming connections
        private void Listen(object o)
        {
            // Retrieve the parameter
            var started = (ManualResetEvent) o;
            // Start the server and signal it
            tcp.Start();
            started.Set();
            try
            {
                while (true)
                {
                    // Accept new connections
                    var sc = new Connection(this, tcp.AcceptTcpClient());
                    lock (clients) clients.Add(sc);
                    if (ConnectionChecker == null)
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
            private readonly Thread PingThread;
            private readonly byte[] buffer = new byte[512]; // Buffer to receive data
            internal readonly TcpClient client; // The underlying Windows socket            
            private readonly Server server; // The containing server
            private bool binary; // Receives binary data ?
            public bool disposed; // Indicates if the connection has already been disposed
            private DateTime lastPing = DateTime.Now;
            private byte[] packet = new byte[512]; // Buffer where received data is processed in packets
            private int packetPos; // Current position in the packet buffer

            // C'tor
            internal Connection(Server server, TcpClient client)
            {
                // Init fields
                this.server = server;
                this.client = client;
                //Start ping thread
                PingThread = new Thread(DoPing);
                lastPing = DateTime.Now;
                PingThread.Start();
                // Start reading
                client.GetStream().BeginRead(buffer, 0, 512, Receive, null);
            }

            public DateTime LastPingTime
            {
                get { return (lastPing); }
            }

            public void PingReceived()
            {
                lastPing = DateTime.Now;
            }

            private void DoPing()
            {
                while (!disposed)
                {
                    lock (this)
                    {
                        var ts = new TimeSpan(DateTime.Now.Ticks - lastPing.Ticks);
                        if (ts.TotalSeconds > 20)
                            server.Disconnected(client);
                                //TODO We want to disconnect, but we also want to inform the server to lock the game until a rejoin, or a vote to kick happens.
                        if (disposed) return;
                    }
                    Thread.Sleep(1000);
                }
            }

            // Callback when data is received
            private void Receive(IAsyncResult ar)
            {
                try
                {
                    // Get how many bytes were received
                    int count = client.GetStream().EndRead(ar);
                    // 0 or less mean we were disconnected, or an error happened
                    if (count < 1)
                    {
                        Disconnected();
                        return;
                    }
                    // Copy the new data in the packet buffer. Make the buffer larger if necessary.
                    if ((packetPos + count) > packet.Length)
                    {
                        var newPacket = new byte[packetPos + count];
                        Array.Copy(packet, newPacket, packetPos);
                        packet = newPacket;
                    }
                    Array.Copy(buffer, 0, packet, packetPos, count);

                    // Handle the received data, either as binary or xml, depending on current status
                    if (binary)
                        BinaryReceive(count);
                    else
                        XmlReceive(count);
                    // Check if the connection is still alive (might be refused by handler)
                    if (client.Connected)
                    {
                        // Wait for new data
                        client.GetStream().BeginRead(buffer, 0, 512, Receive, null);
                    }
                    else
                    {
                        lock (server.clients) server.clients.Remove(this);
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
                    Disconnected();
                }
            }

            // Handle the received data as a binary packet 
            private void BinaryReceive(int count)
            {
                // Adjust the current packet position with the new data
                packetPos += count;
                // Packet starts with size as a 32 bits int.
                while (packetPos > 4)
                {
                    int length = packet[0] | packet[1] << 8 | packet[2] << 16 | packet[3] << 24;
                    if (packetPos >= length)
                    {
                        // Copy the packet data in an array
                        var data = new byte[length - 4];
                        Array.Copy(packet, 4, data, 0, length - 4);


                        // Lock the handler, because it is not thread-safe
                        lock (server.handler)
                            server.handler.ReceiveMessage(data, client, this);

                        // Adjust the packet pos and contents
                        packetPos -= length;
                        Array.Copy(packet, length, packet, 0, packetPos);
                    }
                    else
                        break;
                }
            }

            // Handle the received data as an xml packet
            private void XmlReceive(int count)
            {
                // Look for a 0 at the end of the packet
                for (int i = packetPos; i < packetPos + count; i++)
                {
                    if (packet[i] == 0)
                    {
                        // Get the message as xml
                        string xml = Encoding.UTF8.GetString(packet, 0, i);
                        // Check if it's a request to switch to binary mode
                        if (xml == "<Binary />")
                        {
                            binary = true;
                        }
                        else
                        {
                            // Lock the handler, because it is not thread-safe
                            lock (server.handler) server.handler.ReceiveMessage(xml, client, this);
                        }
                        // Adjust the packet position and contents
                        count += packetPos - i - 1;
                        packetPos = 0;
                        Array.Copy(packet, i + 1, packet, 0, count);
                        // Continue the loop
                        i = -1;
                    }
                }
                // Ajust packet position
                packetPos += count;
            }

            // Disconnect the client
            internal void Disconnect()
            {
                // Lock the disposed field
                lock (this)
                {
                    Console.WriteLine("Client Disconnected.");
                    // Quit if this client is already disposed
                    if (disposed) return;
                    // Mark as disposed
                    disposed = true;
                }
                // If it is connected, close it
                if (client.Connected)
                    try
                    {
                        client.GetStream().Close();
                        client.Close();
                    }
                    catch
                    {
                    }
                // Remove it from the list
                lock (server.clients)
                    server.clients.Remove(this);
            }

            // Notify that the client was unexpectedly disconnected
            internal void Disconnected()
            {
                // Lock the disposed field
                lock (this)
                {
                    // Quit if the client is already disposed
                    if (disposed) return;
                    // Disconnect the client
                    Disconnect();
                    // Notify the event
                    server.handler.Disconnected(client);
                }
            }
        }

        #endregion
    }
}