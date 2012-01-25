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
            SocketMessageBuilder Builder = new SocketMessageBuilder();
            while (true)
            {
                lock (SocketLocker)
                {
                    
                    if (Stopping)
                        break;
                    try
                    {
                        int av = Sock.Client.Available;
                        if (av > 0)
                        {
                            byte[] buf = new byte[av];
                            int rec = Sock.Client.Receive(buf, 0, av, SocketFlags.None);
                            if (rec != av)
                                Array.Resize(ref buf, rec);
                            Builder.AddBytes(buf);
                        }
                        if(Builder.SMQueue.Count > 0)
                        {
                            SocketMessage sm = Builder.SMQueue.Dequeue();
                            if (sm != null)
                            {
                                if (OnMessageReceived != null)
                                    OnMessageReceived.BeginInvoke(this, (SocketMessage)sm.Clone(), null, null);
                            }
                        }
                        Thread.Sleep(3);
                    }
                    catch (SocketException se)
                    {
                        switch (se.SocketErrorCode)
                        {
                            case SocketError.ConnectionAborted:
                                {
                                    LazyAsync.Invoke(Stop);
                                    break;
                                }
                            case SocketError.ConnectionReset:
                                {
                                    LazyAsync.Invoke(Stop);
                                    break;
                                }
                            case SocketError.Disconnecting:
                                {
                                    LazyAsync.Invoke(Stop);
                                    break;
                                }
                            case SocketError.NetworkDown:
                                {
                                    LazyAsync.Invoke(Stop);
                                    break;
                                }
                            case SocketError.NetworkReset:
                                {
                                    LazyAsync.Invoke(Stop);
                                    break;
                                }
                            case SocketError.NoRecovery:
                                {
                                    LazyAsync.Invoke(Stop);
                                    break;
                                }
                            case SocketError.NotConnected:
                                {
                                    LazyAsync.Invoke(Stop);
                                    break;
                                }
                            case SocketError.OperationAborted:
                                {
                                    LazyAsync.Invoke(Stop);
                                    break;
                                }
                            case SocketError.Shutdown:
                                {
                                    LazyAsync.Invoke(Stop);
                                    break;
                                }
                            case SocketError.SocketError:
                                {
                                    LazyAsync.Invoke(Stop);
                                    break;
                                }
                            case SocketError.TimedOut:
                                {
                                    LazyAsync.Invoke(Stop);
                                    break;
                                }
                        }
                        Trace.TraceError("ss0:" + se.Message, se);
                        Thread.Sleep(10);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("ss1" + e.Message, e);
                        Thread.Sleep(10);
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
