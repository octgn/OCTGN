using System;
using System.Collections.Generic;

namespace Octgn.Online.Library.Models
{

    public class HostedGameState : IHostedGameState
    {
        public Uri HostUri { get; set; }

        public string Password { get; set; }

        public Enums.EnumHostedGameStatus Status { get; set; }

        public IList<IHostedGamePlayer> Players { get; set; }
        public IList<IHostedGamePlayer> KickedPlayers { get; set; }
        public IList<IHostedGamePlayer> DisconnectedPlayers { get; set; }

        public int CurrentTurnPlayer { get; set; }
        public int CurrentTurnNumber { get; set; }
        public HashSet<Guid> TurnStopPlayers { get; set; }
        public HashSet<Tuple<Guid, byte>> PhaseStopPlayers { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string HostUserName { get; set; }

        public Guid HostId { get; set; }

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

        public HostedGameState() {

        }

        public HostedGameState(HostedGameRequest game, Uri hostUri) {
            this.AcceptingPlayers = game.AcceptingPlayers;
            this.GameIconUrl = game.GameIconUrl;
            this.GameId = game.GameId;
            this.GameName = game.GameName;
            this.GameVersion = Version.Parse(game.GameVersion);
            this.HasPassword = game.HasPassword;
            this.HideBoard = game.HideBoard;
            this.HostUserIconUrl = game.HostUserIconUrl;
            this.HostUserName = game.HostUserName;
            this.HostId = game.HostId;
            this.Id = game.Id;
            this.MuteSpectators = game.MuteSpectators;
            this.Name = game.Name;
            this.Spectators = game.Spectators;
            this.TwoSidedTable = game.TwoSidedTable;
            this.Password = game.Password;

            this.HostUri = hostUri;

            this.Status = Enums.EnumHostedGameStatus.Unknown;
            this.Players = new List<IHostedGamePlayer>();
            this.KickedPlayers = new List<IHostedGamePlayer>();
            this.DisconnectedPlayers = new List<IHostedGamePlayer>();
            this.CurrentTurnPlayer = 0;
            this.CurrentTurnNumber = 0;
            this.TurnStopPlayers = new HashSet<Guid>();
            this.PhaseStopPlayers = new HashSet<Tuple<Guid, byte>>();
        }
    }

}