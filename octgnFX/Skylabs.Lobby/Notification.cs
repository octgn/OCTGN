using System;
using System.Collections.Generic;

namespace Skylabs.Lobby
{
    public class Notification : IEqualityComparer<Notification>
    {
        public Notification(LobbyClient lc, int id)
        {
            ID = id;
            Time = DateTime.Now;
            LobbyClient = lc;
            Dismissed = false;
        }

        public DateTime Time { get; set; }
        public LobbyClient LobbyClient { get; set; }
        public bool Dismissed { get; set; }
        public int ID { get; private set; }

        #region IEqualityComparer<Notification> Members

        public bool Equals(Notification x, Notification y)
        {
            return x.ID == y.ID;
        }

        public int GetHashCode(Notification obj)
        {
            return obj.ID;
        }

        #endregion
    }
}