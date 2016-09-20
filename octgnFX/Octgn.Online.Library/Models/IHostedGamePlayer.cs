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
    }
}