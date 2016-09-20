using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Octgn.Site.Api;
using Octgn.Site.Api.Models;
using System.Reflection;

using Octgn.Online.Library;
using Octgn.Online.Library.Enums;

using log4net;

namespace Octgn.Server
{

    public sealed class Server
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Private fields

        private readonly Thread _connectionChecker;
        private readonly Timer _disconnectedPlayerTimer;
        private readonly Timer _pingTimer;

        private bool _closed;
        private Thread _serverThread;
        public event EventHandler OnStop;

        //private TcpClient _hostClient;

        private GameBroadcaster _broadcaster;

        #endregion

        #region Public interface

        // Creates and starts a new server
        public Server(IGameStateEngine stateEngine, int broadcastPort)
        {
            _state.Engine = stateEngine;
            Log.InfoFormat("Creating server {0}", stateEngine.Game.HostUri);
            _tcp = new TcpListener(IPAddress.Any, stateEngine.Game.HostUri.Port);
            _connectionChecker = new Thread(CheckConnections);
            _connectionChecker.Start();
            _disconnectedPlayerTimer = new Timer(CheckDisconnectedPlayers, null, 1000, 1500);
            _broadcaster = new GameBroadcaster(broadcastPort);
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
                _tcp.Server.Close();
                _tcp.Stop();
            }
            catch (Exception)
            {
                if (Debugger.IsAttached) Debugger.Break();
            }
            try { _broadcaster.StopBroadcasting(); }
            catch (Exception) { }

            // Submit end game report
            try
            {
                var c = new ApiClient();
                var dcUsers = _state.DcUsers.ToArray();
                var req = new PutGameCompleteReq(_state.Engine.ApiKey, _state.Engine.Game.Id.ToString(), dcUsers);
                c.CompleteGameHistory(req);
            }
            catch (Exception e)
            {
                Log.Error("Disconnect Error reporting disconnect", e);
            }
            // Close all open connections
            foreach (var c in _state.Clients)
            {
                try
                {
                    c.Disconnect(false);
                }
                catch { }
            }
            _state.RemoveAllClients();
            try
            {
                if (OnStop != null)
                    OnStop.Invoke(this, null);

            }
            catch { }
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
            _state.Engine.SetStatus(EnumHostedGameStatus.GameReady);
            _broadcaster.StartBroadcasting();
            started.WaitOne();
        }

        private void CheckDisconnectedPlayers(object state)
        {
            foreach (var c in _state.Players)
            {
                if (c.Connected)
                {
                    if (new TimeSpan(DateTime.Now.Ticks - c.Socket.LastPingTime.Ticks).TotalSeconds >= 12 && c.SaidHello)
                    {
                        Log.InfoFormat("Player {0} timed out", c.Nick);
                        c.Disconnect(true);
                    }
                    continue;
                }
                if (new TimeSpan(DateTime.Now.Ticks - c.TimeDisconnected.Ticks).TotalMinutes >= 10)
                {
                    var player = _state.GetPlayer(c.Socket);
                    var req = new RequestHandler(null, this._state.Engine.Game, this._state);
                    req.HandleRequest(null, player);
                    req.Leave(c.Id);
                }
            }
        }

        private void PingPlayers(object state)
        {
            foreach (var c in _state.Players)
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

                if (_state.HasSomeoneJoined)
                {
                    if (_state.Players.Length == 0)
                    {
                        Stop();
                        break;
                    }
                    if (_state.Players.All(x => x.Connected == false))
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
                        _state.AddClient(sc);
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