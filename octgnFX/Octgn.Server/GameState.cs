using Octgn.Data;
using Octgn.Online.Hosting;
using System;
using System.Collections.Generic;

namespace Octgn.Server
{
    public class GameState
    {
        public HostedGame Game { get; set; }

        public int TurnNumber { get; set; }

        public byte PhaseNumber { get; set; }

        public int IsMuted { get; set; }

        public byte NextPlayerId { get; set; }

        public GameSettings Settings { get; set; } = new GameSettings();

        public PlayerCollection Players { get; set; } = new PlayerCollection();

        public HashSet<byte> TurnStopPlayers { get; set; } = new HashSet<byte>();

        public HashSet<Tuple<byte, byte>> PhaseStops { get; set; } = new HashSet<Tuple<byte, byte>>();

        public GameState() { }

        public static GameState New(HostedGame game) {
            var state = new GameState() {
                Game = game,
            };

            state.Settings.AllowSpectators = state.Game.Spectators;

            return state;
        }
    }
}
