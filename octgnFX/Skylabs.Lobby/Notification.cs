using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skylabs.Lobby
{
    public class Notification
    {
        public DateTime Time { get; set; }
        public LobbyClient LobbyClient { get; set; }
        public bool Dismissed { get; set; }
        public Notification(LobbyClient lc)
        {
            Time = DateTime.Now;
            LobbyClient = lc;
            Dismissed = false;
        }
    }
}
