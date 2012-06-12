using System;
using System.Collections;
using System.Diagnostics;
using agsXMPP;
using agsXMPP.protocol.client;

namespace Octgn.Lobby
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
    public class NewUser : IEquatable<NewUser>,IEqualityComparer
    { 
        public Jid User { get; private set; }

        public UserStatus Status { get; private set; }

        public string CustomStatus { get; set; }

        public string Email { get; set; }

        public NewUser(Jid user)
        {
            User = user.Bare;
            Status = UserStatus.Offline;
            CustomStatus = "";
            Email = "";
        }

        public void SetStatus(Presence p)
        {
            Trace.WriteLine("[SetStatus]For: " + p.From);
            Status = PresenceToStatus(p);
            Trace.WriteLine("[SetStatus]Status: " + Status.ToString());
        }
        public void SetStatus(UserStatus status) { Status = status; }
        public static UserStatus PresenceToStatus(Presence p)
        {
            if(p.From == null)return UserStatus.Unknown;
            Trace.WriteLine("[p2s]From: " + p.From);
            Trace.WriteLine("[p2s]Type: " + p.Type.ToString());
            Trace.WriteLine("[p2s]Show: " + p.Show.ToString());
            var status = UserStatus.Unknown;
            if(p.Type == PresenceType.unavailable)
                status = UserStatus.Offline;
            else if(p.Type == PresenceType.available && p.Show == ShowType.NONE)
                status = UserStatus.Online;
            else
            {
                switch(p.Show)
                {
                    case ShowType.NONE:
                        status = UserStatus.Offline;
                        break;
                    case ShowType.away:
                        status = UserStatus.Away;
                        break;
                    case ShowType.chat:
                        status = UserStatus.Online;
                        break;
                    case ShowType.dnd:
                        status = UserStatus.DoNotDisturb;
                        break;
                    case ShowType.xa:
                        status = UserStatus.DoNotDisturb;
                        break;
                }
            }
            System.Diagnostics.Trace.WriteLine("[p2s]Result: " + status.ToString());
            return status;
        }
        public bool Equals(NewUser other)
        {
            return other.User.Bare == User.Bare;
        }
        public static bool operator ==(NewUser a, NewUser b)
        {
            string rid1 = null;
            string rid2 = null;
            try
            {
                rid1 = a.User.Bare;
            }
            catch { }
            try
            {
                rid2 = b.User.Bare;
            }
            catch { }

            if (rid1 == null && rid2 == null) return true;
            return rid1 == rid2;
        }
        public static bool operator !=(NewUser a , NewUser b) { return !(a == b); }
        public override string ToString() { return User.User; }
        public new bool Equals(object x, object y) { return x == y; }
        public int GetHashCode(object obj) { return obj.GetHashCode(); }
        public override int GetHashCode() { return User.GetHashCode(); }
        public override bool Equals(object obj) { return obj.GetHashCode() == GetHashCode(); }
    }
}
