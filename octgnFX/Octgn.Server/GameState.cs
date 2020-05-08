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

        public byte LastPlayerId { get; set; }

        public GameSettings Settings { get; set; } = new GameSettings();

        public PlayerCollection Players { get; set; }

        public HashSet<byte> TurnStopPlayers { get; set; } = new HashSet<byte>();

        public HashSet<Tuple<byte, byte>> PhaseStops { get; set; } = new HashSet<Tuple<byte, byte>>();

        public GameState(HostedGame game, GameContext context) {
            Players = new PlayerCollection(context);
            Game = game;
            Settings.AllowSpectators = game.Spectators;
        }

        public byte NextPlayerId() => ++LastPlayerId;
    }
}
