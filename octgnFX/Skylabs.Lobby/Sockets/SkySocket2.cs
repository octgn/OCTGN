using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Skylabs.Threading;
using System.Diagnostics;
using Skylabs.Net;
using Skylabs.Lobby.Threading;

namespace Skylabs.Lobby.Sockets
{
    public sealed class SkySocket2 : IDisposable
    {
        public delegate void MessageReceived(SkySocket2 socket, SocketMessage message);

        public event MessageReceived OnMessageReceived;

        public bool Connected { get;private set ;}

        public bool IsDisposed {get;private set;}

        public const long MaxReceiveSize = 5242880;

        private TcpClient Sock { get; set; }

        private EndPoint RemoteEndPoint;

        private object SocketLocker = new object();

        private Conductor Conductor;

        private Thread SocketThread;

        private bool Stopping;

        public SkySocket2()
        {
            lock(SocketLocker)
            {
                Conductor = new Threading.Conductor();
                IsDisposed = false;
                Connected = false;
                Stopping = false;
                Sock = new TcpClient();
                SocketThread = new Thread(ReadThreadRunner);
                SocketThread.Name = "SkySocket Read Thread";
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
                        SocketThread.Start();
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

        private void ReadThreadRunner()
        {
            while (true)
            {
                lock (SockerLocker)
                {
                    if (Stopping)
                        break;
                    byte[] buffer = new byte[8];
                    SocketError err;
                    try
                    {
                        if (Sock.Client.Available >= 8)
                        {
                            Sock.Client.Receive(buffer, 0, 8, SocketFlags.None, err);
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
                            Sock.Client.Receive(buffer, 0, count, SocketFlags.None, err);
                            if (err != SocketError.Success)
                            {
                                Trace.TraceError(err.ToString());
                                break;
                            }
                            SocketMessage sm = SocketMessage.Deserialize(buffer);
                            if (sm != null)
                            {
                                this.Conductor.Add(() => { if (OnMessageReceived != null)OnMessageReceived.Invoke(this, (SocketMessage)sm.Clone()); });
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
                    Conductor.Dispose();
                }
            }
        }
    }
}