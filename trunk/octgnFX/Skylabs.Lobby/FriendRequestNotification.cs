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
            for (int i = 0; i < LobbyClient.Notifications.Count; i++)
            {
                FriendRequestNotification fr = LobbyClient.Notifications[i] as FriendRequestNotification;
                if (fr != null)
                {
                    if (fr.User.Uid == User.Uid)
                    { 
                        LobbyClient.Notifications.RemoveAt(i);
                        break;
                    }
                }
            }
            Dismissed = true;
        }
        public void Decline()
        {
            SocketMessage sm = new SocketMessage("acceptfriend");
            sm.AddData("uid", User.Uid);
            sm.AddData("accept", false);
            LobbyClient.WriteMessage(sm);
            for (int i = 0; i < LobbyClient.Notifications.Count; i++)
            {
                FriendRequestNotification fr = LobbyClient.Notifications[i] as FriendRequestNotification;
                if (fr != null)
                {
                    if (fr.User.Uid == User.Uid)
                    {
                        LobbyClient.Notifications.RemoveAt(i);
                        break;
                    }
                }
            }
            Dismissed = true;
        }
    }
}
