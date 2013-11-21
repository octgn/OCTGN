using System;
using System.Net;
using Octgn.Library;
using agsXMPP.Xml.Dom;

namespace Skylabs.Lobby
{
    using System.Globalization;

    public class HostedGameData : Element, IHostedGameData
    {
        public HostedGameData()
            : base("gameitem", "gameitem", "octgn:gameitem")
        {

        }

        public HostedGameData(Guid id,Guid gameguid, Version gameversion, int port, string name, User huser,
                          DateTime startTime, string gameName, bool hasPassword, IPAddress ipAddress, HostedGameSource source, EHostedGame status)
            : base("gameitem", "gameitem", "octgn:gameitem")
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
        }

        public Guid Id
        {
            get
            {
                Guid ret = Guid.Empty;
                Guid.TryParse(GetTag("Id"), out ret);
                return ret;
            }
            set { SetTag("Id", value.ToString()); }
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
        public int Port
        {
            get { return GetTagInt("port"); }
            set { SetTag("port", value); }
        }
        public String Name
        {
            get { return GetTag("name"); }
            set { SetTag("name", value); }
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

        public string Username {
            get { return GetTag("username"); }
            set { SetTag("username",value);}
        }
        public bool HasPassword
        {
            get { return this.GetTagBool("haspassword"); }
            set{SetTag("haspassword",value);}
        }
        public EHostedGame GameStatus
        {
            get
            {
                EHostedGame ret = EHostedGame.StoppedHosting;
                Enum.TryParse(GetTag("gamestatus"), out ret);
                return ret;
            }
            set { SetTag("gamestatus", value.ToString()); }
        }
        public DateTime TimeStarted
        {
            get
            {
                string[] formats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt", 
                   "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss", 
                   "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt", 
                   "M/d/yyyy h:mm", "M/d/yyyy h:mm", 
                   "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm"};
                DateTime ret = DateTime.Now;
                var temp = GetTag("timestarted").Trim();
                if (DateTime.TryParseExact(
                    temp, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out ret))
                {
                    ret = ret.ToLocalTime();
                }
                return ret;
            }
            set { SetTag("timestarted", value.ToString()); }
        }

        public IPAddress IpAddress
        {
            get
            {
                return IPAddress.Parse(GetTag("ipaddress"));
            }
            set
            {
                SetTag("ipaddress",value.ToString());
            }
        }
        public HostedGameSource Source
        {
            get
            {
                return (HostedGameSource)Enum.Parse(typeof(HostedGameSource),GetTag("source"));
            }
            set { SetTag("source", value.ToString()); }
        }
        public int ProcessId
        {
            get { return GetTagInt("processid"); }
            set { SetTag("processid", value); }
        }
    }
}