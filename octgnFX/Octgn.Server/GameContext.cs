using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Octgn.Data;
using Octgn.Online.Hosting;

namespace Octgn.Server
{
    public class GameContext
    {
        public HostedGame Game { get; set; }
        internal Broadcaster Broadcaster { get; }
        public GameSettings GameSettings { get; }
        public Config Config { get; }
        public PlayerCollection Players { get; }

        public int TurnNumber { get; set; }
        public byte PhaseNumber { get; set; }

        /// <summary>
        /// The current job id that's muted.
        /// </summary>
        public int IsMuted { get; set; }
        public byte NextPlayerId => (byte)Interlocked.Increment(ref _nextPlayerId);

        private int _nextPlayerId = 0;

        public readonly HashSet<byte> TurnStopPlayers = new HashSet<byte>();
        public readonly HashSet<Tuple<byte, byte>> PhaseStops = new HashSet<Tuple<byte, byte>>();

        public GameContext(HostedGame game, Config config) {
            Game = game ?? throw new ArgumentNullException(nameof(game));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Players = new PlayerCollection(this);

            Broadcaster = new Broadcaster(Players);
            GameSettings = new GameSettings() {
                AllowSpectators = Game.Spectators
            };
        }

        /// <summary>
        /// Reset the game state and broadcast the reset to other players
        /// </summary>
        /// <param name="playerId">The player initiating the reset</param>
        public void Reset(byte playerId) {
            TurnNumber = 0;
            PhaseNumber = 0;
            TurnStopPlayers.Clear();
            PhaseStops.Clear();
            Broadcaster.Reset(playerId);
        }

        private Task _dispatcherTask = Task.FromResult<object>(null);

        /// <summary>
        /// Queues up an action to be run synchronusly
        /// </summary>
        /// <param name="action"></param>
        /// <returns>Task that runs the action</returns>
        public Task Run(Action action) {
            return _dispatcherTask = _dispatcherTask.ContinueWith((task) => action());
        }

        /// <summary>
        /// Queues up an action to be run synchronusly
        /// </summary>
        /// <param name="action"></param>
        /// <returns>Task that runs the action</returns>
        public Task Run(Func<Task> action) {
            return _dispatcherTask = _dispatcherTask.ContinueWith((task) => action()).Unwrap();
        }
    }
}