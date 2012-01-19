//Copyright 2012 Skylabs
//In order to use this software, in any manor, you must first contact Skylabs.
//Website: http://www.skylabsonline.com
//Email:   skylabsonline@gmail.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skylabs.Lobby
{
    public class Notification : IEqualityComparer<Notification>
    {
        public DateTime Time { get; set; }
        public LobbyClient LobbyClient { get; set; }
        public bool Dismissed { get; set; }
        public int ID { get; private set; }
        public Notification(LobbyClient lc, int id)
        {
            ID = id;
            Time = DateTime.Now;
            LobbyClient = lc;
            Dismissed = false;
        }

        public bool Equals(Notification x, Notification y)
        {
            return x.ID == y.ID;
        }

        public int GetHashCode(Notification obj)
        {
            return obj.ID;
        }
    }
}
