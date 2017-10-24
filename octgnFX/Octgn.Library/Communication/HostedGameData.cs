using System;
using System.Net;

namespace Octgn.Library.Communication
{
    public class HostedGameData : IHostedGameData
    {
        public HostedGameData()
        {

        }

        public HostedGameData(Guid id, Guid gameguid, Version gameversion, int port, string name, string userId,
                          DateTime startTime, string gameName, string gameIconUrl, bool hasPassword, IPAddress ipAddress, HostedGameSource source, EHostedGame status, bool spectator)
        {
            ProcessId = -1;
            Id = id;
            GameGuid = gameguid;
            GameVersion = gameversion;
            Port = port;
            Name = name;
            UserId = userId;
            GameStatus = status;
            TimeStarted = startTime;
            HasPassword = hasPassword;
            GameName = gameName;
            IpAddress = ipAddress;
            Source = source;
            Spectator = spectator;
            GameIconUrl = gameIconUrl;
            if (UserIconUrl == null)
                UserIconUrl = "";
            if (GameIconUrl == null)
                GameIconUrl = "";
        }

        public Guid Id { get; set; }

        public Guid GameGuid { get; set; }
        public Version GameVersion { get; set; }
        public int Port { get; set; }
        public String Name { get; set; }

        public string GameName { get; set; }

        public String GameIconUrl { get; set; }

        public string UserId { get; set; }

        public String UserIconUrl { get; set; }

        public bool HasPassword { get; set; }
        public EHostedGame GameStatus { get; set; }
        public DateTimeOffset TimeStarted { get; set; }

        public IPAddress IpAddress { get; set; }
        public HostedGameSource Source { get; set; }
        public int ProcessId { get; set; }
        public bool Spectator { get; set; }
    }
}