using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Octgn.Server
{
    public class PlayerCollection : IDisposable
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(nameof(PlayerCollection));

        public event EventHandler<PlayerDisconnectedEventArgs> PlayerDisconnected;

        public int TotalPlayersAdded { get; private set; }

        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private readonly List<Player> _players = new List<Player>();

        private readonly List<ulong> _kickedPlayers = new List<ulong>();

        public Player[] Clients {
            get {
                try {
                    _locker.EnterReadLock();
                    return _players.ToArray();
                } finally {
                    _locker.ExitReadLock();
                }
            }
        }

        public Player[] Players {
            get {
                try {
                    _locker.EnterReadLock();
                    return _players.Where(x => x.SaidHello).ToArray();
                } finally {
                    _locker.ExitReadLock();
                }
            }
        }

        public ulong[] KickedPlayers {
            get {
                try {
                    _locker.EnterReadLock();
                    return _kickedPlayers.ToArray();
                } finally {
                    _locker.ExitReadLock();
                }
            }
        }

        public void AddClient(ServerSocket socket) {
            try {
                _locker.EnterWriteLock();
                var player = new Player(socket, _context);
                _players.Add(player);
                player.Disconnected += Player_Disconnected;
                TotalPlayersAdded++;
            } finally {
                _locker.ExitWriteLock();
            }
        }

        public void RemoveClient(Player player) {
            try {
                _locker.EnterWriteLock();
                _players.Remove(player);
                player.Disconnected -= Player_Disconnected;
            } finally {
                _locker.ExitWriteLock();
            }
        }

        private void Player_Disconnected(object sender, PlayerDisconnectedEventArgs e) {
            PlayerDisconnected?.Invoke(sender, e);
        }

        public Player GetPlayer(byte id) {
            return Players.FirstOrDefault(x => x.Id == id);
        }
        public void AddKickedPlayer(Player pinfo) {
            try {
                _locker.EnterWriteLock();
                _kickedPlayers.Add(pinfo.Pkey);
            } finally {
                _locker.ExitWriteLock();
            }
        }

        public bool SaidHello(ServerSocket socket) {
            return SaidHello(socket.Client);
        }

        public bool SaidHello(TcpClient client) {
            try {
                _locker.EnterReadLock();
                var p = _players.FirstOrDefault(x => x.Equals(client));
                if (p == null) return false;
                return p.SaidHello;
            } finally {
                _locker.ExitReadLock();
            }
        }

        private readonly Timer _pingTimer;
        private readonly Timer _disconnectedPlayerTimer;

        private readonly GameContext _context;

        public PlayerCollection(GameContext context) {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _pingTimer = new Timer(PingTimer_Tick, this, 5000, 2000);
            _disconnectedPlayerTimer = new Timer(DisconnectedPlayerTimer_Tick, this, 1000, 1500);
        }

        private static void PingTimer_Tick(object state) {
            var playerCollection = (PlayerCollection)state;

            foreach (var player in playerCollection.Players) {
                if (!player.Connected) continue;

                player.Ping();
            }
        }

        public event EventHandler AllPlayersDisconnected;

        /// <summary>
        /// Indicates if we've fired <see cref="AllPlayersDisconnected"/> event. 0 means no, 1 means yes. All other values are invalid.
        /// </summary>
        private int _firedAllPlayersDisconnectedEvent = 0;

        private void DisconnectedPlayerTimer_Tick(object state) {
            var playerCollection = (PlayerCollection)state;

            bool anyConnected = false;
            foreach (var player in playerCollection.Players)
            {
                anyConnected = player.Connected || anyConnected;
                if (player.Connected && player.IsTimedOut && player.SaidHello)
                {
                    Log.Info($"{player} timed out. Time since last ping is {player.TimeSinceLastPing}. Total pings received {player.TotalPingsReceived}");
                    player.Disconnect(PlayerDisconnectedEventArgs.TimeoutReason, $"Timed out after {_context.Config.PlayerTimeoutSeconds} seconds.");
                }
            }

            bool alreadyFiredAllPlayersDisconnectedEvent
                = Interlocked.CompareExchange(ref _firedAllPlayersDisconnectedEvent, 1, 0) == 1;

            if (!anyConnected && !alreadyFiredAllPlayersDisconnectedEvent)
                AllPlayersDisconnected?.Invoke(this, new EventArgs());
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _pingTimer.Dispose();
                    _disconnectedPlayerTimer.Dispose();

                    foreach(var client in Clients) {
                        try {
                            client.Disconnect(PlayerDisconnectedEventArgs.ShutdownReason, string.Empty);
                        } catch (Exception ex) {
                            Log.Warn($"{nameof(Dispose)}", ex);
                        }
                        _players.Remove(client);
                    }

                    _locker.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(true);
        }
        #endregion
    }

    public class PlayerDisconnectedEventArgs : EventArgs
    {
        public const string TimeoutReason = "TimedOut";
        public const string DisconnectedReason = "Disconnected";
        public const string KickedReason = "Kicked";
        public const string ShutdownReason = "Shutdown";
        public const string ConnectionReplacedReason = "ConnectionReplaced";
        public const string LeaveReason = "Leave";

        public Player Player { get; set; }
        public string Reason { get; set; }
        public string Details { get; set; }

        public PlayerDisconnectedEventArgs(Player player, string reason, string details) {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            Details = details;
        }
    }
}