using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Octgn.Communication;
using Octgn.Library.Localization;
using Octgn.Library.Networking;
using Octgn.Online.Hosting;

namespace Octgn.Server
{

    public sealed class Server : IDisposable
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(nameof(Server));

        private readonly TcpListener _tcp; // Underlying windows socket

        public event EventHandler OnStop;

        private GameBroadcaster _broadcaster;

        public GameContext Context { get; }

        public Server(Config config, HostedGame game, int broadcastPort) {
            Context = new GameContext(game, config);

            Context.Players.PlayerDisconnected += Players_PlayerDisconnected;
            Context.Players.AllPlayersDisconnected += Players_AllPlayersDisconnected;

            Log.InfoFormat("Creating server {0}", Context.Game.HostAddress);

            _tcp = new TcpListener(IPAddress.Any, Context.Game.Port);
            _broadcaster = new GameBroadcaster(Context.Game, broadcastPort);
        }

        private Task _listenTask;

        public async Task Start() {
            if (_listenTask != null) throw new InvalidOperationException("Server already started.");

            _broadcaster.StartBroadcasting();

            _tcp.Start();

            Log.Info("Waiting for first connection");
            using (var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(1))) {
                cancellation.Token.Register(() => _tcp.Stop());
                await ListenSingle();
                Log.Info("Got first connection");
            }

            _listenTask = Listen();
        }

        private void Players_PlayerDisconnected(object sender, PlayerDisconnectedEventArgs e) {
            if (!e.Player.SaidHello) return;
            switch (e.Reason) {
                case PlayerDisconnectedEventArgs.KickedReason: {
                        Context.Broadcaster.Error(string.Format(L.D.ServerMessage__PlayerKicked, e.Player.Nick, e.Details));
                        Context.Broadcaster.Leave(e.Player.Id);
                        Context.Players.RemoveClient(e.Player);
                        break;
                    }

                case PlayerDisconnectedEventArgs.ConnectionReplacedReason: {
                        Context.Players.RemoveClient(e.Player);
                        return;
                    }
                case PlayerDisconnectedEventArgs.DisconnectedReason: {
                        Context.Broadcaster.PlayerDisconnect(e.Player.Id);
                        break;
                    }
                case PlayerDisconnectedEventArgs.ShutdownReason: {
                        Context.Broadcaster.PlayerDisconnect(e.Player.Id);
                        Context.Players.RemoveClient(e.Player);
                        break;
                    }
                case PlayerDisconnectedEventArgs.TimeoutReason: {
                        Context.Broadcaster.PlayerDisconnect(e.Player.Id);
                        break;
                    }
                case PlayerDisconnectedEventArgs.LeaveReason: {
                        Context.Broadcaster.Leave(e.Player.Id);
                        Context.Players.RemoveClient(e.Player);
                        break;
                    }
                default: {
                        Context.Broadcaster.PlayerDisconnect(e.Player.Id);
                        break;
                    }
            }
        }

        private void Players_AllPlayersDisconnected(object sender, EventArgs e) {
            Log.Info(nameof(Players_AllPlayersDisconnected));
            Shutdown();
        }

        private bool _calledShutdown;
        private void Shutdown() {
            try {
                if (_calledShutdown) return;
                _calledShutdown = true;

                Log.Info(nameof(Shutdown));

                _tcp.Stop();

                _broadcaster.StopBroadcasting();

                _listenTask.Wait();

                OnStop?.Invoke(this, null);
            } catch (Exception ex) {
                Log.Error($"{nameof(Shutdown)}", ex);
            }
        }

        private async Task Listen() {
            try {
                while (!disposedValue) {
                    await ListenSingle();
                }
            } catch (Exception ex) when (!_calledShutdown) {
                Signal.Exception(ex);
                Shutdown();
            } catch (Exception ex) {
                Log.Warn($"{nameof(Listen)}", ex);
            }
        }

        private async Task ListenSingle() {
            try {
                TcpClient client = null;
                try {
                    client = await _tcp.AcceptTcpClientAsync();
                } catch (ObjectDisposedException) {
                    return;
                }
                if (client != null) {
                    Log.InfoFormat("New Connection {0}", client.Client.RemoteEndPoint);
                    var sc = new ServerSocket(client, this);
                    Context.Players.AddClient(sc);
                    return;
                }
                throw new NotImplementedException();
            } catch (SocketException e) {
                if (e.SocketErrorCode != SocketError.Interrupted) throw;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    Shutdown();
                }

                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(true);
        }
        #endregion
    }
}