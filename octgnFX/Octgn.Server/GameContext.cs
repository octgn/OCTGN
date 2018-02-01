using System;
using System.Threading;
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
        public byte NextPlayerId => (byte)Interlocked.Increment(ref _nextPlayerId);

        private int _nextPlayerId = 0;


        public GameContext(HostedGame game, Config config) {
            Game = game ?? throw new ArgumentNullException(nameof(game));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Players = new PlayerCollection(this);

            Broadcaster = new Broadcaster(Players);
            GameSettings = new GameSettings() {
                AllowSpectators = Game.Spectators
            };
        }
    }
}