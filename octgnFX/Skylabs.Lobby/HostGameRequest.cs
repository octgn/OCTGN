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

        public HostGameRequest(Guid gameguid, Version gameversion, string name, string gameName, string password)
            : base("hostgamerequest", "hostgamerequest", "octgn:hostgamerequest")
        {
            RequestId = Guid.NewGuid();
            GameGuid = gameguid;
            GameVersion = gameversion;
            Name = name;
            GameName = gameName;
            Password = password;
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
    }
}