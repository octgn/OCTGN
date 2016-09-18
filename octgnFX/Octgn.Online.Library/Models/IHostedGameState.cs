namespace Octgn.Online.Library.Models
{
    using System;
    using System.Collections.Generic;

    using Octgn.Online.Library.Enums;

    public interface IHostedGameState : IHostedGame
    {
        Uri HostUri { get; }

        string Password { get; }

        Enums.EnumHostedGameStatus Status { get; }

        List<HostedGamePlayer> Players { get; }

        int CurrentTurnPlayer { get; }
        int CurrentTurnNumber { get; set; }
        byte NextPlayerId { get; set; }
        HashSet<byte> TurnStopPlayers { get; }
        HashSet<Tuple<byte, byte>> PhaseStopPlayers { get; }
    }

    public class HostedGameState : IHostedGameState
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string HostUserName { get; set; }

        public string GameName { get; set; }

        public Guid GameId { get; set; }

        public Version GameVersion { get; set; }

        public bool HasPassword { get; set; }

        public bool TwoSidedTable { get; set; }

        public Uri HostUri { get; set; }

        public string Password { get; set; }

        public EnumHostedGameStatus Status { get; set; }

        public List<HostedGamePlayer> Players { get; set; }

        public int CurrentTurnPlayer { get; set; }
        public int CurrentTurnNumber { get; set; }

        public bool Spectators { get; set; }

        public string GameIconUrl { get; set; }

        public string HostUserIconUrl { get; set; }
        public byte NextPlayerId { get; set; }
        public bool AcceptingPlayers { get; set; }
        public HashSet<byte> TurnStopPlayers { get; set; }
        public HashSet<Tuple<byte, byte>> PhaseStopPlayers { get; set; }
    }
}