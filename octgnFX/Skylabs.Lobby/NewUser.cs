using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using agsXMPP;
using agsXMPP.protocol.client;

namespace Skylabs.Lobby
{
    public enum UserStatus
    {
        Unknown = 0,
        Offline = 1,
        Online = 2,
        Away = 3,
        DoNotDisturb = 4,
        Invisible = 5
    };
    public class NewUser : IEquatable<NewUser>
    { 
        public Jid User { get; private set; }

        public UserStatus Status { get; set; }

        public string CustomStatus { get; set; }

        public NewUser(Jid user)
        {
            User = user;
            Status = UserStatus.Offline;
            CustomStatus = "";
        }

        public void SetStatus(Presence p)
        {
            Status = NewUser.PresenceToStatus(p);
        }
        public static UserStatus PresenceToStatus(Presence p)
        {
            UserStatus Status = UserStatus.Unknown;
            if(p.Type == PresenceType.unavailable)
                Status = UserStatus.Offline;
            else if(p.Type == PresenceType.available && p.Show == ShowType.NONE)
                Status = UserStatus.Online;
            else
            {
                switch(p.Show)
                {
                    case ShowType.NONE:
                        Status = UserStatus.Offline;
                        break;
                    case ShowType.away:
                        Status = UserStatus.Away;
                        break;
                    case ShowType.chat:
                        Status = UserStatus.Online;
                        break;
                    case ShowType.dnd:
                        Status = UserStatus.DoNotDisturb;
                        break;
                    case ShowType.xa:
                        Status = UserStatus.DoNotDisturb;
                        break;
                }
            }
            return Status;
        }
        public bool Equals(NewUser other)
        {
            return other.User.User == User.User;
        }
    }
}
