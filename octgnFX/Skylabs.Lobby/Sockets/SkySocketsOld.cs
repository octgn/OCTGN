using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Skylabs.Net
{
    public enum DisconnectReason
    {
        PingTimeout,
        RemoteHostDropped,
        CleanDisconnect,
        MalformedData
    };
}

namespace Skylabs.Net.Sockets
{
    public class StateObject
    {
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
        public TcpClient WorkSocket;
    }

    public abstract class SkySocketOld
    {
        private readonly ConcurrentQueue<Action> DelegateQueue = new ConcurrentQueue<Action>();
        private readonly object SocketLocker = new object();

        private Timer DelegateTimer;

        private List<byte> _buffer = new List<byte>();

        private DisconnectReason _dr;
        private DateTime _lastPingReceived;
        private Thread _thread;

        /// <summary>
        /// Creates new SkySocket that isn't connected. You must call Connect to connect.
        /// </summary>
        protected SkySocketOld()
        {
            DelegateQueue = new ConcurrentQueue<Action>();
            Connected = false;
            Sock = null;
            _buffer = new List<Byte>();
            _thread = new Thread(Run);
            DelegateTimer = new Timer(DelegateTimerTick, null, 5, 5);
        }

        /// <summary>
        /// Creates new SkySocket using an already made connection.
        /// </summary>
        /// <param name="client">Connected TcpClient</param>
        protected SkySocketOld(TcpClient client)
        {
            _Connect(client);
        }

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
                    catch (Exception)
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

        /// <summary>
        /// Connect to a remote host.
        /// </summary>
        /// <param name="host">Host Name</param>
        /// <param name="port">Port</param>
        /// <returns>True if connected, false if not.</returns>
        public bool Connect(string host, int port)
        {
            if (!Connected)
            {
                var c = new TcpClient();
                try
                {
                    c.Connect(host, port);
                    _Connect(c);
                    return true;
                }
                catch (SocketException)
                {
                    return false;
                }
            }
            return false;
        }

        private void DelegateTimerTick(object state)
        {
            try
            {
                Action o = null;
                if (DelegateQueue.TryDequeue(out o))
                    o.Invoke();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (Debugger.IsAttached) Debugger.Break();
            }
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
            while (true)
            {
                lock (SocketLocker)
                {
                    DateTime temp = _lastPingReceived.AddSeconds(30);
                    if (DateTime.Now >= temp)
                    {
                        DelegateQueue.Enqueue(() => Close(DisconnectReason.PingTimeout));
                    }
                    else
                        DelegateQueue.Enqueue(() => WriteMessage(new SocketMessage("ping")));
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
                var state = new StateObject {WorkSocket = Sock};
                Sock.Client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCallback,
                                         state);
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
                    DelegateQueue.Enqueue(() => DisconnectCallback(null));
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
                DelegateQueue.Enqueue(() => OnDisconnect(_dr));
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            lock (SocketLocker)
            {
                try
                {
                    var state = (StateObject) ar.AsyncState;
                    TcpClient client = state.WorkSocket;

                    // Read data from the remote device.
                    int bytesRead = client.Client.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        // There might be more data, so store the data received so far.
                        for (int i = 0; i < bytesRead; i++)
                            _buffer.Add(state.Buffer[i]);
                        DelegateQueue.Enqueue(() =>
                                                  {
                                                      HandleInput();
                                                      Recieve();
                                                  });
                    }
                    else
                    {
                        // The network input stream is closed
                        // Handle any remaining data
                        DelegateQueue.Enqueue(() => HandleInput());
                    }
                }
                catch (SocketException)
                {
                    //Skylabs.ConsoleHelper.ConsoleWriter.writeLine(e.ToString(), false);
                    //if(System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                    DelegateQueue.Enqueue(() => Close(DisconnectReason.RemoteHostDropped));
                }
            }
        }

        private void HandleInput()
        {
            lock (SocketLocker)
            {
                if (_buffer.Count > 8)
                {
                    var mlength = new byte[8];
                    _buffer.CopyTo(0, mlength, 0, 8);
                    long count = BitConverter.ToInt64(mlength, 0);
                    if (_buffer.Count >= count + 8)
                    {
                        var mdata = new byte[count];
                        _buffer.CopyTo(8, mdata, 0, (int) count);
                        SocketMessage sm = null;
                        try
                        {
                            sm = SocketMessage.Deserialize(mdata);
                        }
                        catch (Exception)
                        {
#if(DEBUG)
                            if (Debugger.IsAttached) Debugger.Break();
#endif
                            DelegateQueue.Enqueue(() => Close(DisconnectReason.MalformedData));
                        }
                        if (sm != null)
                        {
                            if (sm.Header.ToLower() == "ping")
                            {
                                _lastPingReceived = DateTime.Now;
                            }
                            else
                            {
                                DelegateQueue.Enqueue(() => OnMessageReceived(sm));
                            }
                        }
                        _buffer.RemoveRange(0, (int) count + 8);
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
                if (message.Header.ToLower() != "ping")
                    Debug.WriteLine("Writing message({0}):", message.Header, message.Data);
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
                    DelegateQueue.Enqueue(() => Close(DisconnectReason.RemoteHostDropped));
                }
                catch (ObjectDisposedException)
                {
                    DelegateQueue.Enqueue(() => Close(DisconnectReason.RemoteHostDropped));
                }
                catch (NullReferenceException)
                {
                }
                catch (Exception)
                {
                    if (Debugger.IsAttached) Debugger.Break();
                }
            }
        }
    }
}