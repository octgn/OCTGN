using System;
using System.Collections.Generic;
using Skylabs.Net;
using agsXMPP;
using agsXMPP.protocol.component;

namespace Skylabs.Lobby
{
    [Serializable]
    public class HostedGameData :IQ, IEquatable<HostedGameData>, IEqualityComparer<HostedGameData>
    {
        #region EHostedGame enum

        [Serializable]
        public enum EHostedGame
        {
            StartedHosting,
            GameInProgress,
            StoppedHosting
        };

        #endregion

        public HostedGameData(Guid gameguid, Version gameversion, int port, string name, NewUser huser,
                          DateTime startTime)
        {
            GameGuid = gameguid;
            GameVersion = gameversion;
            Port = port;
            Name = name;
            UserHosting = huser;
            GameStatus = EHostedGame.StartedHosting;
            TimeStarted = startTime;
        }

        public HostedGameData(SocketMessage sm)
        {
            GameGuid = (Guid) sm["guid"];
            GameVersion = (Version) sm["version"];
            Port = (int) sm["port"];
            Name = (string) sm["name"];
            UserHosting = ((NewUser) sm["hoster"]);
            GameStatus = EHostedGame.StartedHosting;
            TimeStarted = new DateTime(DateTime.Now.ToUniversalTime().Ticks);
        }

        public Guid GameGuid
        {
            get
            {
                Guid ret = Guid.Empty;
                Guid.TryParse(GetTag("guid"),out ret);
                return ret;
            } 
            private set{SetTag("guid",value.ToString());}
        }
        public Version GameVersion
        {
            get
            {
                Version v = new Version(0,0);
                Version.TryParse(GetTag("version") , out v);
                return v;
            } 
            private set
            {
                SetTag("version",value.ToString());
            }
        }
        public int Port
        {
            get { return GetTagInt("port"); }
            set{SetTag("port",value);}
        }
        public String Name
        {
            get { return GetTag("name"); }
            private set{SetTag("name",value);}
        }
        public NewUser UserHosting
        {
            get { return new NewUser(GetTagJid("userhosting")); }
            private set{SetTag("userhosting",value.User.Bare);}
        }
        public EHostedGame GameStatus
        {
            get
            {
                EHostedGame ret = EHostedGame.StoppedHosting;
                Enum.TryParse(GetTag("gamestatus") , out ret);
                return ret;
            }
            set{SetTag("gamestatus",value.ToString());}
        }
        public DateTime TimeStarted
        {
            get
            {
                DateTime ret = DateTime.Now;
                DateTime.TryParse(GetTag("timestarted") , out ret);
                return ret;
            }
            private set{SetTag("timestarted",value.ToString());}
        }

        #region IEqualityComparer<HostedGame> Members

        public bool Equals(HostedGameData x, HostedGameData y)
        {
            return x.Port == y.Port;
        }

        public int GetHashCode(HostedGameData obj)
        {
            return obj.Port;
        }

        #endregion

        #region IEquatable<HostedGame> Members

        public bool Equals(HostedGameData other)
        {
            return other.Port == Port;
        }

        #endregion
    }
}