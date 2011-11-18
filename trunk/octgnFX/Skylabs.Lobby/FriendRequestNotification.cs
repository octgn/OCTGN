using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.Net;

namespace Skylabs.Lobby
{
    public class FriendRequestNotification : Notification
    {
        public User User { get; private set; }
        public FriendRequestNotification(User u,LobbyClient lc) :base(lc)
        {
            User = u;
        }
        public void Accept()
        {
            SocketMessage sm = new SocketMessage("acceptfriend");
            sm.AddData("uid", User.Uid);
            sm.AddData("accept", true);
            LobbyClient.WriteMessage(sm);
            Dismissed = true;
        }
        public void Decline()
        {
            SocketMessage sm = new SocketMessage("acceptfriend");
            sm.AddData("uid", User.Uid);
            sm.AddData("accept", false);
            LobbyClient.WriteMessage(sm);
            Dismissed = true;
        }
    }
}
