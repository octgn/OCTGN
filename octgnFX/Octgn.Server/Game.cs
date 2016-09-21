using Octgn.Online.Library.Enums;
using Octgn.Online.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Octgn.Server
{
    public class Game : IHostedGameState
    {
        #region IHostedGameState
        public int DBId { get; set; }
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

        public IList<IHostedGamePlayer> Players { get; set; }

        public int CurrentTurnPlayer { get; set; }
        public int CurrentTurnNumber { get; set; }

        public bool Spectators { get; set; }

        public string GameIconUrl { get; set; }

        public string HostUserIconUrl { get; set; }
        public bool AcceptingPlayers { get; set; }
        public HashSet<ulong> TurnStopPlayers { get; set; }
        public HashSet<Tuple<ulong, byte>> PhaseStopPlayers { get; set; }

        public IList<IHostedGamePlayer> KickedPlayers { get; set; }

        public IList<IHostedGamePlayer> DisconnectedPlayers { get; set; }

        public bool MuteSpectators { get; set; }

        public bool HideBoard { get; set; }
        #endregion IHostedGameState

        public Player GetPlayer(ulong fp) {
            return (Player)Players.FirstOrDefault(x => x.Id == fp);
        }
    }
}
