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
using Skylabs.Lobby.Threading;

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
                    Action a = new Action(() => Stop());
                    a.BeginInvoke(null, null);
                }
                catch (ObjectDisposedException)
                {
                    Action a = new Action(() => Stop());
                    a.BeginInvoke(null, null);
                }
                catch (NullReferenceException)
                {
                }
                catch (Exception)
                {
                    if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                }
            }
        }

        private void ReadThreadRunner()
        {
            while (true)
            {
                lock (SocketLocker)
                {
                    if (Stopping)
                        break;
                    byte[] buffer = new byte[8];
                    SocketError err;
                    try
                    {
                        if (Sock.Client.Available >= 8)
                        {
                            Sock.Client.Receive(buffer, 0, 8, SocketFlags.None, out err);
                            if (err != SocketError.Success)
                            {
                                Trace.TraceError(err.ToString());
                                break;
                            }
                            long count = BitConverter.ToInt64(buffer, 0);
                            if (count > MaxReceiveSize)
                            {
                                Trace.TraceError("Tried to receive a message that was greater than {0} from endpoint {1}", MaxReceiveSize, RemoteEndPoint.ToString());
                                break;
                            }
                            buffer = new byte[count];
                            Sock.Client.Receive(buffer, 0, (int)count, SocketFlags.None, out err);
                            if (err != SocketError.Success)
                            {
                                Trace.TraceError(err.ToString());
                                break;
                            }
                            SocketMessage sm = SocketMessage.Deserialize(buffer);
                            if (sm != null)
                            {
                                if (OnMessageReceived != null)
                                    OnMessageReceived.BeginInvoke(this, (SocketMessage)sm.Clone(),null,null); 
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("Error(ReadThreadRunner):{0}\n{1}", e.Message, e.StackTrace);
                        break;
                    }

                }
                Thread.Sleep(10);
            }
            Action a = new Action(()=>{ if (OnConnectionClosed != null)OnConnectionClosed.Invoke(this); });
            a.BeginInvoke(null,null);
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