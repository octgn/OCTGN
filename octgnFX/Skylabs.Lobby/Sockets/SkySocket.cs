using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Skylabs.Lobby.Threading;
using System.Diagnostics;
using Skylabs.Net;

namespace Skylabs.Lobby.Sockets
{
    public sealed class SkySocket : IDisposable
    {
        public delegate void MessageReceived(SkySocket socket, SocketMessage message);

        public delegate void ConnectionClosed(SkySocket socket);

        public event MessageReceived OnMessageReceived;

        public event ConnectionClosed OnConnectionClosed;

        public bool Connected { get;private set ;}

        public bool IsDisposed {get;private set;}

        public const long MaxReceiveSize = 5242880;

        private TcpClient Sock { get; set; }

        public EndPoint RemoteEndPoint { get; private set; }

        private object SocketLocker = new object();

        private Thread SocketThread;

        private bool Stopping;

        public SkySocket()
        {
            lock(SocketLocker)
            {
                IsDisposed = false;
                Connected = false;
                Stopping = false;
                Sock = new TcpClient();
                SocketThread = new Thread(ReadThreadRunner);
                SocketThread.Name = "SkySocket Read Thread";
            }
        }
        public SkySocket(TcpClient c)
        {
            lock (SocketLocker)
            {
                IsDisposed = false;
                Stopping = false;
                Sock = c;
                SocketThread = new Thread(ReadThreadRunner);
                SocketThread.Name = "SkySocket Read Thread";
                RemoteEndPoint = Sock.Client.RemoteEndPoint;
                SocketThread.Start();
                Connected = true;
            }
        }
        public bool Connect(string host, int port)
        {
            lock (SocketLocker)
            {
                if (!Connected)
                {
                    try
                    {
                        Sock.Connect(host, port);
                        RemoteEndPoint = Sock.Client.RemoteEndPoint;
                        SocketThread.Start();
                        Connected = true;
                        return true;
                    }
                    catch (SocketException)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public void Stop()
        {
            lock (SocketLocker)
            {
                Stopping = true;
            }
        }

        public void WriteMessage(SocketMessage message)
        {
            lock (SocketLocker)
            {
                if (IsDisposed)
                    return;
                byte[] data = SocketMessage.Serialize(message);
                byte[] messagesize = BitConverter.GetBytes(data.LongLength);
                try
                {
                    Sock.Client.Send(messagesize);
                    Sock.Client.Send(data);
                    //Sock.GetStream().Flush();
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode == 10058)
                        return;
                    LazyAsync.Invoke(() => Stop());
                }
                catch (ObjectDisposedException)
                {
                    LazyAsync.Invoke(() => Stop());
                }
                catch (NullReferenceException)
                {
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                }
            }
        }

        private void ReadThreadRunner()
        {
            List<byte> sizebuffer = new List<byte>();
            List<byte> messagebuffer = new List<byte>();
            int messagesize = -1;
            while (true)
            {
                lock (SocketLocker)
                {
                    
                    if (Stopping)
                        break;
                    try
                    {
                        NetworkStream ns = Sock.GetStream();
                        if (ns.DataAvailable)
                        {
                            int ib = ns.ReadByte();
                            if (ib == -1)
                                break;
                            byte b = (byte)ib;
                            if (sizebuffer.Count < 8)
                            {
                                sizebuffer.Add(b);
                            }
                            else
                            {
                                messagebuffer.Add(b);
                            }
                            if (sizebuffer.Count == 8 && messagesize == -1)
                            {
                                messagesize = (int)BitConverter.ToInt64(sizebuffer.ToArray(), 0);
                            }
                            if (messagebuffer.Count == messagesize)
                            {
                                SocketMessage sm = SocketMessage.Deserialize(messagebuffer.ToArray());
                                sizebuffer = new List<byte>();
                                messagebuffer = new List<byte>();
                                messagesize = -1;
                                if (sm != null)
                                {
                                    if (OnMessageReceived != null)
                                        OnMessageReceived.BeginInvoke(this, (SocketMessage)sm.Clone(), null, null);
                                }
                            }

                        }
                        else
                            Thread.Sleep(10);
                    }
                    catch (SocketException se)
                    {
                        Trace.TraceError("ss0:" + se.Message, se);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("ss1" + e.Message, e);
                    }
                }
            }
            LazyAsync.Invoke(()=>{ if (OnConnectionClosed != null)OnConnectionClosed.Invoke(this); });
            //Call disconnection bullhonkey here.
        }
    
        public void Dispose()
        {
            lock(SocketLocker)
            {
                if(!IsDisposed)
                {
                    Sock.Close();
                    IsDisposed = true;
                }
            }
        }
    }
}
