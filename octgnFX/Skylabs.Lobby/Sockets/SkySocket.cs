using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Skylabs.Lobby.Threading;
using Skylabs.Net;

namespace Skylabs.Lobby.Sockets
{
    public sealed class SkySocket : IDisposable
    {
        #region Delegates

        public delegate void ConnectionClosed(SkySocket socket);

        public delegate void MessageReceived(SkySocket socket, SocketMessage message);

        #endregion

        public const long MaxReceiveSize = 5242880;
        private readonly SocketMessageBuilder _builder;

        private readonly object _socketLocker = new object();

        //private Thread SocketThread;

        private bool _stopping;

        public SkySocket()
        {
            lock (_socketLocker)
            {
                IsDisposed = false;
                Connected = false;
                _stopping = false;
                _builder = new SocketMessageBuilder();
                Sock = new TcpClient();
                //SocketThread = new Thread(ReadThreadRunner);
                //SocketThread.Name = "SkySocket Read Thread";
            }
        }

        public SkySocket(TcpClient c)
        {
            lock (_socketLocker)
            {
                IsDisposed = false;
                _stopping = false;
                Sock = c;
                _builder = new SocketMessageBuilder();
                //SocketThread = new Thread(ReadThreadRunner);
                //SocketThread.Name = "SkySocket Read Thread";
                RemoteEndPoint = Sock.Client.RemoteEndPoint;
                //SocketThread.Start();
                Connected = true;
                LazyAsync.Invoke(AsyncRead);
            }
        }

        public bool Connected { get; private set; }

        public bool IsDisposed { get; private set; }
        private TcpClient Sock { get; set; }

        public EndPoint RemoteEndPoint { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            lock (_socketLocker)
            {
                if (IsDisposed) return;
                Sock.Close();
                IsDisposed = true;
            }
        }

        #endregion

        public event MessageReceived OnMessageReceived;

        public event ConnectionClosed OnConnectionClosed;

        public bool Connect(string host, int port)
        {
            lock (_socketLocker)
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
            lock (_socketLocker)
            {
                _stopping = true;
                LazyAsync.Invoke(() =>
                                     {
                                         if (OnConnectionClosed != null) OnConnectionClosed.Invoke(this);
                                         Dispose();
                                     });
            }
        }

        private void AsyncRead()
        {
            lock (_socketLocker)
            {
                if (_stopping)
                    return;
                try
                {
                    var buffer = new byte[256];
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
            lock (_socketLocker)
            {
                try
                {
                    var buff = (byte[]) ar.AsyncState;
                    if (buff == null)
                        return;
                    if (Sock.Client == null)
                        return;
                    int rin = Sock.Client.EndReceive(ar);
                    if (rin > 0)
                    {
                        if (rin != 256)
                            Array.Resize(ref buff, rin);
                        _builder.AddBytes(buff);
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
                while (_builder.SmQueue.Count > 0)
                {
                    SocketMessage sm = _builder.SmQueue.Dequeue();
                    if (sm == null) continue;
                    if (OnMessageReceived != null)
                        OnMessageReceived.BeginInvoke(this, (SocketMessage) sm.Clone(), null, null);
                }
                LazyAsync.Invoke(AsyncRead);
            }
        }

        public void WriteMessage(SocketMessage message)
        {
            lock (_socketLocker)
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
                    LazyAsync.Invoke(Stop);
                }
                catch (ObjectDisposedException)
                {
                    LazyAsync.Invoke(Stop);
                }
                catch (NullReferenceException)
                {
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    if (Debugger.IsAttached) Debugger.Break();
                }
            }
        }
    }
}