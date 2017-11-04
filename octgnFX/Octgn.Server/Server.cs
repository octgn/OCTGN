using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Octgn.Site.Api;
using Octgn.Site.Api.Models;

namespace Octgn.Server
{

    public sealed class Server
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(nameof(Server));

        private readonly Thread _connectionChecker;
        private readonly TcpListener _tcp; // Underlying windows socket
        private readonly Timer _disconnectedPlayerTimer;
        private readonly Timer _pingTimer;

        private bool _closed;
        private Thread _serverThread;
        public event EventHandler OnStop;

        private GameBroadcaster _broadcaster;

        public State State { get; }

        #region Public interface

        // Creates and starts a new server
        public Server(State state, int broadcastPort)
        {
            State = state;
            Log.InfoFormat("Creating server {0}", State.Game.HostAddress);
            _tcp = new TcpListener(IPAddress.Any, State.Game.Port);
            _connectionChecker = new Thread(CheckConnections);
            _connectionChecker.Start();
            _disconnectedPlayerTimer = new Timer(CheckDisconnectedPlayers, null, 1000, 1500);
            _broadcaster = new GameBroadcaster(State, broadcastPort);
            _pingTimer = new Timer(PingPlayers, null, 5000, 2000);
            Start();
        }

        // Stop the server
        public void Stop()
        {
            // Stop the server and release resources
            _closed = true;

            try
            {
                _tcp.Stop();
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(Stop)}", ex);
            }
            _broadcaster.StopBroadcasting();

            // Submit end game report
            try
            {
                var c = new ApiClient();
                var dcUsers = State.DcUsers.ToArray();
                var req = new PutGameCompleteReq(State.ApiKey, State.Game.Id.ToString(), dcUsers);
                c.CompleteGameHistory(req);
            }
            catch (Exception e)
            {
                Log.Error("Disconnect Error reporting disconnect", e);
            }
            // Close all open connections
            foreach (var c in State.Clients)
            {
                try
                {
                    c.Disconnect(false);
                }
                catch(Exception ex) {
                    Log.Error($"{nameof(Stop)}", ex);
                }
            }
            State.RemoveAllClients();
            try
            {
                if (OnStop != null)
                    OnStop.Invoke(this, null);

            }
            catch(Exception ex) {
                Log.Error($"{nameof(Stop)}", ex);
            }
        }

        #endregion

        #region Private implementation

        // Start the server
        private void Start()
        {
            // Creates a new thread for the server
            _serverThread = new Thread(Listen) { Name = "OCTGN.net Server" };
            // Flag used to wait until the server is really started
            var started = new ManualResetEvent(false);


            // Start the server
            _serverThread.Start(started);
            _broadcaster.StartBroadcasting();
            started.WaitOne();
        }

        private void CheckDisconnectedPlayers(object state)
        {
            foreach (var c in State.Players)
            {
                if (c.Connected)
                {
                    var timeoutSeconds = this.State.IsDebug ? 240 : 12;
                    if (new TimeSpan(DateTime.Now.Ticks - c.Socket.LastPingTime.Ticks).TotalSeconds >= timeoutSeconds && c.SaidHello)
                    {
                        Log.InfoFormat("Player {0} timed out after {1} seconds. Last ping was {2}. Total pings received was {3}", c.Nick, timeoutSeconds, c.Socket.LastPingTime, c.Socket.PingsReceived);
                        c.Disconnect(true);
                    }
                    continue;
                }
                if (new TimeSpan(DateTime.Now.Ticks - c.TimeDisconnected.Ticks).TotalMinutes >= 10)
                {
                    State.Handler.SetupHandler(c.Socket);
                    State.Handler.Leave(c.Id);
                }
            }
        }

        private void PingPlayers(object state)
        {
            foreach (var c in State.Players)
            {
                if (!c.Connected) continue;
                c.Rpc.Ping();
            }
        }

        private void CheckConnections()
        {
            var startTime = DateTime.Now;
            while (!_closed)
            {
                Thread.Sleep(1000);

                if (State.HasSomeoneJoined)
                {
                    if (State.Players.Length == 0)
                    {
                        Stop();
                        break;
                    }
                    if (State.Players.All(x => x.Connected == false))
                    {
                        Stop();
                        break;
                    }
                }
                else
                {
                    if (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).Seconds >= 60)
                    {
                        Stop();
                        break;
                    }
                }
            }
        }

        // Main thread function: waits and accept incoming connections
        private void Listen(object o)
        {
            // Retrieve the parameter
            var started = (ManualResetEvent)o;
            // Start the server and signal it
            _tcp.Start();
            started.Set();
            try
            {
                while (!_closed)
                {
                    // Accept new connections
                    var con = _tcp.AcceptTcpClient();
                    if (con != null)
                    {
                        Log.InfoFormat("New Connection {0}", con.Client.RemoteEndPoint);
                        var sc = new ServerSocket(con, this);
                        State.AddClient(sc);
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
    }
}