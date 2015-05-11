using System;
using System.Collections.Generic;

namespace Skylabs.Lobby
{
    public class Notification : IEqualityComparer<Notification>
    {
        public Notification(Client lc, int id)
        {
            Id = id;
            Time = DateTime.Now;
            LobbyClient = lc;
            Dismissed = false;
        }

        public DateTime Time { get; set; }
        public Client LobbyClient { get; set; }
        public bool Dismissed { get; set; }
        public int Id { get; private set; }

        #region IEqualityComparer<Notification> Members

        public bool Equals(Notification x, Notification y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Notification obj)
        {
            return obj.Id;
        }

        #endregion
    }
}