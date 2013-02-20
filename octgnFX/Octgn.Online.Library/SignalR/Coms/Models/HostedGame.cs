namespace Octgn.Online.Library.SignalR.Coms.Models
{
    using System;

    public class HostedGame
    {
        public string Name { get; set; }

        public string HostName { get; set; }

        public string GameName { get; set; }

        public Guid Id { get; set; }

        public Guid GameId { get; set; }

        public Uri Host { get; set; }
    }
}