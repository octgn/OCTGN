using System;

namespace Skylabs.Lobby
{
    public class HostGameRequest
    {
        public HostGameRequest()
        {

        }

        public HostGameRequest(Guid gameguid, Version gameversion, string name
            , string gameName, string gameIconurl, string password, Version sasVersion, bool spectators)
        {
            RequestId = Guid.NewGuid();
            GameGuid = gameguid;
            GameVersion = gameversion;
            Name = name;
            GameName = gameName;
            Password = password;
            SasVersion = sasVersion;
            Spectators = spectators;
            GameIconUrl = gameIconurl;
        }

        public Guid GameGuid { get; set; }

        public String GameIconUrl { get; set; }

        public Version GameVersion { get; set; }

        public string GameName { get; set; }

        public String Name { get; set; }

        public String Password { get; set; }

        public Guid RequestId { get; set; }

        public Version SasVersion { get; set; }

        public bool Spectators { get; set; }
    }
}