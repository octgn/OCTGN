namespace Octgn.Online.Library.Models
{
    using System;

    public class HostedGamePlayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid Key { get; set; }
        public Enums.EnumPlayerState State { get; set; }
        public Microsoft.AspNet.SignalR.Client.ConnectionState ConnectionState { get; set; }
        public bool IsMod { get; set; }
        public bool Kicked { get; set; }
        public bool InvertedTable { get; set; }
    }
}