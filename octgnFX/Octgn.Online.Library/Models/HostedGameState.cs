namespace Octgn.Online.Library.Models
{
    using System;
    using System.Collections.Generic;

    public class HostedGameState : IHostedGame
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

        public Enums.EnumHostedGameStatus Status { get; set; }

        public List<HostedGamePlayer> Players { get; set; }

        public int CurrentTurnPlayer { get; set; }
    }
}