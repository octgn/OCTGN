using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly Timer _pingTimer;
        private readonly Timer _disconnectedPlayerTimer;

        private readonly GameContext _context;

        public PlayerCollection(GameContext context) {
            _context = _context = context ?? throw new ArgumentNullException(nameof(context));
            _pingTimer = new Timer(PingTimer_Tick, this, 5000, 2000);
            _disconnectedPlayerTimer = new Timer(DisconnectedPlayerTimer_Tick, this, 2000, 2000);
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
        /// <summary>
        /// The DateTime that all of the players were found to be disconnected. If a player connects again, this will become null.
        /// </summary>
        private DateTime? _allPlayersDisconnectedTime = null;

        private static void DisconnectedPlayerTimer_Tick(object state) {
            var players = (PlayerCollection)state;

            if (players.TotalPlayersAdded == 0) return;

            bool anyConnected = false;
            foreach (var player in players.Players)
            {
                anyConnected |= player.Connected;
                if (player.Connected && player.IsTimedOut && player.SaidHello)
                {
                    Log.Info($"{player} timed out. Time since last ping is {player.TimeSinceLastPing}. Total pings received {player.TotalPingsReceived}");
                    player.Disconnect(PlayerDisconnectedEventArgs.TimeoutReason, $"Timed out after {players._context.Config.PlayerTimeoutSeconds} seconds.");
                }
            }

            if (anyConnected) {
                players._allPlayersDisconnectedTime = null;
                return;
            }

            // Nobody is connected anymore

            if(players._allPlayersDisconnectedTime == null) {
                players._allPlayersDisconnectedTime = DateTime.Now;
                return;
            }

            // Wait for X amount of time after everyone disconnects.
            // This gives players a chance to connect back to the game

            var timeSinceAllDisconnected = DateTime.Now - players._allPlayersDisconnectedTime.Value;
            var timeToWait = players._context.Config.IsLocal
                ? TimeSpan.FromSeconds(15)
                : TimeSpan.FromMinutes(2);

            if(timeSinceAllDisconnected < timeToWait) {
                // Not enough time has passed to fire the event.
                return;
            }

            // Enough time has passed to fire the event.

            bool alreadyFiredEvent
                = Interlocked.CompareExchange(ref players._firedAllPlayersDisconnectedEvent, 1, 0) == 1;


            if (!alreadyFiredEvent) {
                // We haven't fired the event yet, go ahead and fire it.
                Log.Info($"Firing {players.AllPlayersDisconnected}");
                players.AllPlayersDisconnected?.Invoke(players, new EventArgs());
            }
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
}