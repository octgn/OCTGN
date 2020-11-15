using System;
using Octgn.Online.Hosting;

namespace Octgn.Server
{
    public class GameContext
    {
        public Broadcaster Broadcaster { get; }
        public Config Config { get; }
        public GameState State { get; }

        public HostedGame Game => State.Game;

        public GameContext(HostedGame game, Config config) {
            Config = config ?? throw new ArgumentNullException(nameof(config));

            State = new GameState(game, this);

            Broadcaster = new Broadcaster(State.Players);
        }

        /// <summary>
        /// Reset the game state and broadcast the reset to other players
        /// </summary>
        /// <param name="playerId">The player initiating the reset</param>
        public void Reset(byte playerId, bool isSoft) {
            State.TurnNumber = 0;
            State.PhaseNumber = 0;
            State.TurnStopPlayers.Clear();
            State.PhaseStops.Clear();
            Broadcaster.Reset(playerId, isSoft);
        }

        private readonly object _dispatcherLock = new object();

        /// <summary>
        /// Queues up an action to be run synchronusly
        /// </summary>
        /// <param name="action"></param>
        /// <returns>Task that runs the action</returns>
        public void Run(Action action) {
            lock (_dispatcherLock) {
                action();
            }
        }
    }
}