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

        public bool Connected { get; private set; }

        public bool IsDisposed { get; private set; }

        public const long MaxReceiveSize = 5242880;

        private TcpClient Sock { get; set; }

        public EndPoint RemoteEndPoint { get; private set; }

        private object SocketLocker = new object();

        //private Thread SocketThread;

        private bool Stopping;

        private SocketMessageBuilder Builder;

        public SkySocket()
        {
            lock (SocketLocker)
            {
                IsDisposed = false;
                Connected = false;
                Stopping = false;
                Builder = new SocketMessageBuilder();
                Sock = new TcpClient();
                //SocketThread = new Thread(ReadThreadRunner);
                //SocketThread.Name = "SkySocket Read Thread";
            }
        }
        public SkySocket(TcpClient c)
        {
            lock (SocketLocker)
            {
                IsDisposed = false;
                Stopping = false;
                Sock = c;
                Builder = new SocketMessageBuilder();
                //SocketThread = new Thread(ReadThreadRunner);
                //SocketThread.Name = "SkySocket Read Thread";
                RemoteEndPoint = Sock.Client.RemoteEndPoint;
                //SocketThread.Start();
                Connected = true;
                LazyAsync.Invoke(AsyncRead);
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
                        Sock = new TcpClient();
                        Sock.Connect(host, port);
                        RemoteEndPoint = Sock.Client.RemoteEndPoint;
                        //SocketThread.Start();
                        Connected = true;
                        LazyAsync.Invoke(AsyncRead);
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
                LazyAsync.Invoke(() => { if (OnConnectionClosed != null)OnConnectionClosed.Invoke(this); Dispose(); });
            }
        }
        private void AsyncRead()
        {
            lock (SocketLocker)
            {
                if (Stopping)
                    return;
                try
                {
                    byte[] buffer = new byte[256];
                    Sock.Client.BeginReceive(buffer, 0, 256, SocketFlags.None, AsyncReadDone, buffer);
                }
                catch (SocketException se)
                {
                    #region "SocketErrors"
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
                                break;
                            }
                        default:
                            {
                                Trace.WriteLine("ss0:" + se.Message);
                                Trace.WriteLine(se.StackTrace);
                                Thread.Sleep(10);
                                break;
                            }
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    Trace.WriteLine("ss1:" + e.Message);
                    Trace.WriteLine(e.StackTrace);
                    Thread.Sleep(10);
                }
            }
        }
        private void AsyncReadDone(IAsyncResult ar)
        {
            lock (SocketLocker)
            {
                try
                {
                    byte[] buff = (byte[])ar.AsyncState;
                    if (buff == null)
                        return;
                    if (Sock.Client == null)
                        return;
                    int rin = Sock.Client.EndReceive(ar);
                    if (rin > 0)
                    {
                        if (rin != 256)
                            Array.Resize(ref buff, rin);
                        Builder.AddBytes(buff);
                    }
                    else
                    {
                        LazyAsync.Invoke(Stop);
                    }
                }
                catch (SocketException se)
                {
                    #region "SocketErrors"
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
                                break;
                            }
                        default:
                            {
                                Trace.WriteLine("ss2:" + se.Message);
                                Trace.WriteLine(se.StackTrace);
                                break;
                            }
                    }
                    #endregion
                }
                catch (ObjectDisposedException oe)
                {
                    Trace.WriteLine("ss5:" + oe.Message);
                    Trace.WriteLine(oe.StackTrace);
                }
                catch (NullReferenceException ne)
                {
                    Trace.WriteLine("ss3:" + ne.Message);
                    Trace.WriteLine(ne.StackTrace);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("ss4:" + e.Message);
                    Trace.WriteLine(e.StackTrace);
                }
                while (Builder.SMQueue.Count > 0)
                {
                    SocketMessage sm = Builder.SMQueue.Dequeue();
                    if (sm != null)
                    {
                        if (OnMessageReceived != null)
                            OnMessageReceived.BeginInvoke(this, (SocketMessage)sm.Clone(), null, null);
                    }
                }
                LazyAsync.Invoke(AsyncRead);
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

        public void Dispose()
        {
            lock (SocketLocker)
            {
                if (!IsDisposed)
                {
                    Sock.Close();
                    IsDisposed = true;
                }
            }
        }
    }
}
