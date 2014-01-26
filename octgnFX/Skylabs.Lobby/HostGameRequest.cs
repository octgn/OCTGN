using System;
using agsXMPP.Xml.Dom;

namespace Skylabs.Lobby
{
    public class HostGameRequest: Element
    {
        public HostGameRequest()
            : base("hostgamerequest", "hostgamerequest", "octgn:hostgamerequest")
        {

        }

        public HostGameRequest(Guid gameguid, Version gameversion, string name
            , string gameName, string password, Version sasVersion, bool spectators)
            : base("hostgamerequest", "hostgamerequest", "octgn:hostgamerequest")
        {
            RequestId = Guid.NewGuid();
            GameGuid = gameguid;
            GameVersion = gameversion;
            Name = name;
            GameName = gameName;
            Password = password;
            SasVersion = sasVersion;
            Spectators = spectators;
        }

        public Guid GameGuid
        {
            get
            {
                Guid ret = Guid.Empty;
                Guid.TryParse(GetTag("guid"), out ret);
                return ret;
            }
            set { SetTag("guid", value.ToString()); }
        }

        public Version GameVersion
        {
            get
            {
                Version v = new Version(0, 0);
                Version.TryParse(GetTag("version"), out v);
                return v;
            }
            set
            {
                SetTag("version", value.ToString());
            }
        }

        public string GameName
        {
            get
            {
                return GetTag("gamename");
            }
            set
            {
                SetTag("gamename", value);
            }
        }

        public String Name
        {
            get { return GetTag("name"); }
            set { SetTag("name", value); }
        }

        public String Password
        {
            get { return GetTag("password"); }
            set { SetTag("password", String.IsNullOrWhiteSpace(value) ? "" : value); }
        }

        public Guid RequestId
        {
            get
            {
                Guid ret;
                Guid.TryParse(GetTag("RequestId"), out ret);
                return ret;
            }
            set { SetTag("RequestId", value.ToString()); }
        }

        public Version SasVersion
        {
            get
            {
                Version v = new Version(0, 0);
                Version.TryParse(GetTag("SasVersion"), out v);
                return v;
            }
            set
            {
                SetTag("SasVersion", value.ToString());
            }
        }

        public bool Spectators
        {
            get
            {
                bool v = false;
                bool.TryParse(GetTag("Spectators"), out v);
                return v;
            }
            set
            {
                SetTag("Spectators", value.ToString());
            }
        }
    }
}