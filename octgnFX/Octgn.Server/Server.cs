using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Octgn.Online.Library.Models;
namespace Octgn.Server
{
    using System.Reflection;

    using Octgn.Online.Library;
    using Octgn.Online.Library.Enums;

    using log4net;

    public sealed class Server
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Private fields

        private readonly List<Connection> _clients = new List<Connection>(); // List of all the connected clients		
        private readonly Thread _connectionChecker;
        internal readonly Handler _handler; // Message handler
        private readonly TcpListener _tcp; // Underlying windows socket
        private readonly Timer _disconnectedPlayerTimer;

        private bool _closed;
        private Thread _serverThread;
        public event EventHandler OnStop;

        private TcpClient _hostClient;

        #endregion

        #region Public interface

        // Creates and starts a new server
        public Server(IGameStateEngine stateEngine)
        {
            GameStateEngine.Set(stateEngine);
            Log.InfoFormat("Creating server {0}", stateEngine.Game.HostUri);
            _tcp = new TcpListener(IPAddress.Any, stateEngine.Game.HostUri.Port);
            _handler = new Handler(stateEngine.Game.GameId, stateEngine.Game.GameVersion, stateEngine.Game.Password);
            _connectionChecker = new Thread(CheckConnections);
            _connectionChecker.Start();
            _disconnectedPlayerTimer = new Timer(CheckDisconnectedPlayers, null, 1000, 5000);
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
            catch (Exception e)
            {
                if (Debugger.IsAttached) Debugger.Break();
            }
            // Close all open connections
            _clients.ForEach(x => x.Disconnect());
            _clients.Clear();
            if (OnStop != null)
                OnStop.Invoke(this, null);
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
            GameStateEngine.Get().SetStatus(EnumHostedGameStatus.GameReady);
            started.WaitOne();
        }

        private void CheckDisconnectedPlayers(object state)
        {
            lock (_handler)
                lock (_clients)
                {
                    foreach (var c in this._handler.Players.ToArray())
                    {
                        if (c.Value.Connected) continue;
                        if (new TimeSpan(DateTime.Now.Ticks - c.Value.TimeDisconnected.Ticks).TotalMinutes >= 1)
                        {
                            var con = this._clients.FirstOrDefault(x => x.Client == c.Key);
                            _handler.SetupHandler(c.Key, con);
                            _handler.Leave(c.Value.Id);
                        }
                    }
                }
        }

        private void CheckConnections()
        {
            while (!_closed)
            {
                Thread.Sleep(120000);
                lock (_clients)
                {
                    if (_clients.Count == 0)
                    {
                        Stop();
                        break;
                    }
                    if (_hostClient == null)
                        _hostClient = _handler.Players.FirstOrDefault(x => x.Value.Id == 1).Key;
                    if (_hostClient == null && _handler.GameStarted == false)
                    {
                        Stop();
                        break;
                    }
                    _clients.FindAll(x => x.Disposed || !x.Client.Connected || new TimeSpan(DateTime.Now.Ticks - x.LastPingTime.Ticks).TotalSeconds > 240).ForEach(me => me.Disconnect());
                    _clients.RemoveAll(x => x.Disposed || !x.Client.Connected || new TimeSpan(DateTime.Now.Ticks - x.LastPingTime.Ticks).TotalSeconds > 240);
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
                    var sc = new Connection(this, _tcp.AcceptTcpClient());
                    lock (_clients) _clients.Add(sc);
                    _hostClient = _handler.Players.FirstOrDefault(x => x.Value.Id == 1).Key;
                    if (_connectionChecker == null)
                    {
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