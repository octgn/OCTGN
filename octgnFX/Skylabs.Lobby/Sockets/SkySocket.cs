using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Skylabs.Net
{
    public enum DisconnectReason { PingTimeout, RemoteHostDropped, CleanDisconnect, MalformedData };
}

namespace Skylabs.Net.Sockets
{
    public class StateObject
    {
        public TcpClient WorkSocket;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
    }

    public abstract class SkySocket
    {
        /// <summary>
        /// Underlying TcpClient
        /// </summary>
        public TcpClient Sock { get; private set; }

        public EndPoint RemoteEndPoint
        {
            get
            {
                lock (SocketLocker)
                {
                    try
                    {
                        if (Sock != null && Sock.Client != null && Sock.Client.RemoteEndPoint != null)
                            return Sock.Client.RemoteEndPoint;
                        return null;
                    }
                    catch(Exception)
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Is this connected to the remote socket
        /// </summary>
        public bool Connected { get; private set; }

        private List<byte> _buffer= new List<byte>();

        private Thread _thread;

        private DateTime _lastPingReceived;

        private DisconnectReason _dr;

        private object SocketLocker = new object();

        /// <summary>
        /// Creates new SkySocket that isn't connected. You must call Connect to connect.
        /// </summary>
        protected SkySocket()
        {
            Connected = false;
            Sock = null;
            _buffer = new List<Byte>();
            _thread = new Thread(Run);
        }

        /// <summary>
        /// Creates new SkySocket using an already made connection.
        /// </summary>
        /// <param name="client">Connected TcpClient</param>
        protected SkySocket(TcpClient client)
        {
            _Connect(client);
        }

        /// <summary>
        /// Connect to a remote host.
        /// </summary>
        /// <param name="host">Host Name</param>
        /// <param name="port">Port</param>
        /// <returns>True if connected, false if not.</returns>
        public bool Connect(string host, int port)
        {
            if(!Connected)
            {
                TcpClient c = new TcpClient();
                try
                {
                    c.Connect(host, port);
                    _Connect(c);
                    return true;
                }
                catch(SocketException)
                {
                    return false;
                }
            }
            return false;
        }

        private void _Connect(TcpClient c)
        {
            lock (SocketLocker)
            {
                Connected = true;
                Sock = c;
                _buffer = new List<byte>();
                _thread = new Thread(Run);
                _lastPingReceived = DateTime.Now;
                _thread.Start();
                Recieve();
            }
            
        }

        /// <summary>
        /// Message received
        /// </summary>
        /// <param name="sm">Socket Message received</param>
        public abstract void OnMessageReceived(SocketMessage sm);

        /// <summary>
        /// When this client disconnects
        /// </summary>
        /// <param name="reason">Reason why</param>
        public abstract void OnDisconnect(DisconnectReason reason);

        private void Run()
        {
            while(true)
            {
                lock (SocketLocker)
                {
                    DateTime temp = _lastPingReceived.AddSeconds(30);
                    if (DateTime.Now >= temp)
                    {
                        Close(DisconnectReason.PingTimeout);
                    }
                    else
                        WriteMessage(new SocketMessage("ping"));
                    if (!Connected)
                        break;
                }
                Thread.Sleep(5000);
            }
        }

        private void Recieve()
        {
            lock (SocketLocker)
            {
                StateObject state = new StateObject { WorkSocket = Sock };
                Sock.Client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCallback, state);
            }
        }

        /// <summary>
        /// Close the client.
        /// </summary>
        /// <param name="reason">Reason why</param>
        public void Close(DisconnectReason reason)
        {
            lock (SocketLocker)
            {
                _dr = reason;
                if (Sock != null && Sock.Client != null)
                    Sock.Client.BeginDisconnect(false, DisconnectCallback, Sock.Client);
                else
                {
                    Thread t = new Thread(()=>DisconnectCallback(null));
                    t.Start();
                }
            }
        }

        private void DisconnectCallback(IAsyncResult ar)
        {
            lock (SocketLocker)
            {
                if (ar != null)
                    Sock.Client.EndDisconnect(ar);
                Connected = false;
                Thread t = new Thread(() => OnDisconnect(_dr));
                t.Start();
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            lock (SocketLocker)
            {
                try
                {
                    StateObject state = (StateObject)ar.AsyncState;
                    TcpClient client = state.WorkSocket;

                    // Read data from the remote device.
                    int bytesRead = client.Client.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        // There might be more data, so store the data received so far.
                        for (int i = 0; i < bytesRead; i++)
                            _buffer.Add(state.Buffer[i]);
                        Thread t = new Thread(() => { HandleInput(); Recieve(); });
                        t.Start();
                    }
                    else
                    {
                        // The network input stream is closed
                        // Handle any remaining data
                        Thread t = new Thread(()=>HandleInput());
                        t.Start();
                    }
                }
                catch (SocketException)
                {
                    //Skylabs.ConsoleHelper.ConsoleWriter.writeLine(e.ToString(), false);
                    //if(System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                    Thread t = new Thread(()=>Close(DisconnectReason.RemoteHostDropped));
                    t.Start();
                }
            }
        }

        private void HandleInput()
        {
            lock (SocketLocker)
            {
                if (_buffer.Count > 8)
                {
                    byte[] mlength = new byte[8];
                    _buffer.CopyTo(0, mlength, 0, 8);
                    long count = BitConverter.ToInt64(mlength, 0);
                    if (_buffer.Count >= count + 8)
                    {
                        byte[] mdata = new byte[count];
                        _buffer.CopyTo(8, mdata, 0, (int)count);
                        SocketMessage sm = null;
                        try
                        {
                            sm = SocketMessage.Deserialize(mdata);
                        }
                        catch (Exception)
                        {
#if(DEBUG)
                        if(System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
#endif
                            Thread t = new Thread(() => Close(DisconnectReason.MalformedData));
                            t.Start();
                        }
                        if (sm != null)
                        {
                            if (sm.Header.ToLower() == "ping")
                            {
                                _lastPingReceived = DateTime.Now;
                            }
                            else
                            {
                                Thread t = new Thread(() => OnMessageReceived(sm));
                                t.Start();
                            }
                        }
                        _buffer.RemoveRange(0, (int)count + 8);
                    }
                }
            }
        }

        /// <summary>
        /// Send a SocketMessage.
        /// </summary>
        /// <param name="message">Message to send.</param>
        public void WriteMessage(SocketMessage message)
        {
            lock (SocketLocker)
            {
                byte[] data = SocketMessage.Serialize(message);
                byte[] messagesize = BitConverter.GetBytes(data.LongLength);
                try
                {
                    Sock.Client.Send(messagesize);
                    Sock.Client.Send(data);
                    Sock.GetStream().Flush();
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode == 10058)
                        return;
#if(!DEBUG)
                    if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
#else
                if(System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
#endif
                    Thread t = new Thread(()=>Close(DisconnectReason.RemoteHostDropped));
                    t.Start();
                }
                catch (ObjectDisposedException)
                {
                    Thread t = new Thread(() => Close(DisconnectReason.RemoteHostDropped));
                    t.Start();
                }
            }
        }
    }
}