using System;
using agsXMPP;

namespace Octgn.Online.MatchmakingService
{
    public class QueueUser : IComparable, IEquatable<Jid>, IEquatable<QueueUser>
    {
        public Jid JidUser { get; set; }

        public bool IsReady { get; set; }

        public bool IsInReadyQueue { get; set; }

        public int FailedReadyCount { get; set; }

        public QueueUser(Jid user)
        {
            JidUser = new Jid(user.User, user.Server, null);
        }

        public int CompareTo(object obj)
        {
            if (obj is QueueUser)
            {
                return JidUser.CompareTo((obj as QueueUser).JidUser);
            }
            if (obj is Jid)
            {
                var jid = new Jid((obj as Jid).User, (obj as Jid).Server, null);
                return JidUser.CompareTo(jid);
            }
            return JidUser.CompareTo(obj);
        }

        public bool Equals(Jid other)
        {
            var o2 = new Jid(other.User, other.Server, null);
            return o2.Equals(JidUser);
        }

        public bool Equals(QueueUser other)
        {
            return JidUser.Equals(other.JidUser);
        }

        public static bool operator ==(QueueUser a, QueueUser b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if ((null == a) || (null == b))
            {
                return false;
            }

            // Return true if the fields match:
            return a.JidUser.Equals(b.JidUser);
        }

        public static bool operator !=(QueueUser a, QueueUser b)
        {
            return !(a == b);
        }

        static public implicit operator QueueUser(Jid value)
        {
            return new QueueUser(value);
        }

        static public implicit operator Jid(QueueUser user)
        {
            return user.JidUser;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QueueUser)obj);
        }

        public override int GetHashCode()
        {
            return (JidUser != null ? JidUser.GetHashCode() : 0);
        }
    }
}