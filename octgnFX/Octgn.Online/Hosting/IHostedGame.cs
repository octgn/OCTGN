using System;

namespace Octgn.Online.Hosting
{
    public interface IHostedGame {
        Guid Id { get; set; }

        string Name { get; set; }

        string HostUserId { get; set; }

        string GameName { get; set; }

        Guid GameId { get; set; }

        Version GameVersion { get; set; }

        string Password { get; set; }

        bool HasPassword { get; set; }

        bool Spectators { get; set; }

        string GameIconUrl { get; set; }

        string HostUserIconUrl { get; set; }
        Uri HostUri { get; set; }
        HostedGameStatus Status { get; set; }
        HostedGameSource Source { get; set; }
        DateTimeOffset DateCreated { get; set; }
        DateTimeOffset? DateStarted { get; set; }
    }
}