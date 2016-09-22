using System;
using System.Linq;
using System.Collections.Generic;

namespace Octgn.Online.Library.Models
{

    public class HostedGameState : IHostedGameState
    {
        public int DBId { get; set; }

        public Uri HostUri { get; }

        public string Password { get; }

        public Enums.EnumHostedGameStatus Status { get; }

        public IList<IHostedGamePlayer> Players { get; }
        public IList<IHostedGamePlayer> KickedPlayers { get; }
        public IList<IHostedGamePlayer> DisconnectedPlayers { get; }

        public int CurrentTurnPlayer { get; }
        public int CurrentTurnNumber { get; set; }
        public HashSet<ulong> TurnStopPlayers { get; }
        public HashSet<Tuple<ulong, byte>> PhaseStopPlayers { get; }

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