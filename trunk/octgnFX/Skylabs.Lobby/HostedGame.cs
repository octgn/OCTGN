using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Skylabs.Net;

namespace Skylabs.Lobby
{
    [Serializable]
    public class HostedGame : IEquatable<HostedGame>
    {
        [Serializable]
        public enum eHostedGame
        {
            StartedHosting,
            GameInProgress,
            StoppedHosting
        };
        public Guid GameGuid { get; private set; }
        public Version GameVersion { get; private set; }
        public int Port { get; private set; }
        public String Name { get; private set; }
        public bool PasswordRequired { get; private set; }
        public User UserHosting { get; private set; }
        public eHostedGame GameStatus { get; set; }
        public HostedGame(Guid gameguid, Version gameversion, int port, string name, bool passreq, User huser)
        {
            GameGuid = gameguid;
            GameVersion = gameversion;
            Port = port;
            Name = name;
            PasswordRequired = passreq;
            UserHosting = huser;
            GameStatus = eHostedGame.StartedHosting;
        }
        public HostedGame(SocketMessage sm)
        {
            GameGuid = (Guid) sm["guid"];
            GameVersion = (Version) sm["version"];
            Port = (int) sm["port"];
            Name = (string) sm["name"];
            PasswordRequired = (bool) sm["passrequired"];
            UserHosting = (User) sm["hoster"];
            GameStatus = eHostedGame.StartedHosting;
        }

        public bool Equals(HostedGame other)
        {
            return other.Port == Port;
        }
    }
}
