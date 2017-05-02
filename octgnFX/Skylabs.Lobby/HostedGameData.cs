using System;
using System.Net;
using Octgn.Library;

namespace Skylabs.Lobby
{
    using System.Globalization;

    public class HostedGameData : IHostedGameData
    {
        public HostedGameData()
        {

        }

        public HostedGameData(Guid id, Guid gameguid, Version gameversion, int port, string name, User huser,
                          DateTime startTime, string gameName, string gameIconUrl, bool hasPassword, IPAddress ipAddress, HostedGameSource source, EHostedGame status, bool spectator)
        {
            ProcessId = -1;
            Id = id;
            GameGuid = gameguid;
            GameVersion = gameversion;
            Port = port;
            Name = name;
            Username = huser.UserName;
            GameStatus = status;
            TimeStarted = startTime;
            HasPassword = hasPassword;
            GameName = gameName;
            IpAddress = ipAddress;
            Source = source;
            Spectator = spectator;
            GameIconUrl = gameIconUrl;
            if (huser.ApiUser != null)
                UserIconUrl = huser.ApiUser.IconUrl;
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

        public string Username { get; set; }

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