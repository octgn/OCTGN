using System;
using Octgn.Online.Library.Enums;

namespace Octgn.Online.Library.Models
{
    public interface IHostedGamePlayer
    {
        ulong Id { get; set; }
        string Name { get; set; }
        ulong PublicKey { get; set; }
        Enums.EnumPlayerState State { get; set; }
        ConnectionState ConnectionState { get; set; }
        bool IsMod { get; set; }
        bool Kicked { get; set; }
        bool InvertedTable { get; set; }
        bool SaidHello { get; set; }
        bool Disconnected { get; set; }
        DateTime DisconnectedDate { get; set; }
    }

    public class HostedGamePlayer : IHostedGamePlayer
    {
        public ConnectionState ConnectionState { get; set; }

        public bool Disconnected { get; set; }

        public DateTime DisconnectedDate { get; set; }

        public ulong Id { get; set; }

        public bool InvertedTable { get; set; }

        public bool IsMod { get; set; }

        public bool Kicked { get; set; }

        public string Name { get; set; }

        public ulong PublicKey { get; set; }

        public bool SaidHello { get; set; }

        public EnumPlayerState State { get; set; }
    }
}