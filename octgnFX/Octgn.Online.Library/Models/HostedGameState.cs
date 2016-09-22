using System;
using System.Collections.Generic;

namespace Octgn.Online.Library.Models
{

    public class HostedGameState : IHostedGameState
    {
        public int DBId { get; set; }

        public Uri HostUri { get; set; }

        public string Password { get; set; }

        public Enums.EnumHostedGameStatus Status { get; set; }

        public IList<IHostedGamePlayer> Players { get; set; }
        public IList<IHostedGamePlayer> KickedPlayers { get; set; }
        public IList<IHostedGamePlayer> DisconnectedPlayers { get; set; }

        public int CurrentTurnPlayer { get; set; }
        public int CurrentTurnNumber { get; set; }
        public HashSet<ulong> TurnStopPlayers { get; set; }
        public HashSet<Tuple<ulong, byte>> PhaseStopPlayers { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string HostUserName { get; set; }

        public string GameName { get; set; }

        public Guid GameId { get; set; }

        public Version GameVersion { get; set; }

        public bool HasPassword { get; set; }

        public bool TwoSidedTable { get; set; }

        public bool Spectators { get; set; }

        public bool MuteSpectators { get; set; }

        public bool HideBoard { get; set; }

        public string GameIconUrl { get; set; }

        public string HostUserIconUrl { get; set; }

        public bool AcceptingPlayers { get; set; }
    }

}