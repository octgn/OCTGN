using Skylabs.Net;

namespace Skylabs.Lobby
{
    public class FriendRequestNotification : Notification
    {
        public FriendRequestNotification(User u, LobbyClient lc, int id) : base(lc, id)
        {
            User = u;
        }

        public User User { get; private set; }

        public void Accept()
        {
            var sm = new SocketMessage("acceptfriend");
            sm.AddData("uid", User.Uid);
            sm.AddData("accept", true);
            LobbyClient.WriteMessage(sm);
            Notification[] ns = LobbyClient.GetNotificationList();
            for (int i = 0; i < ns.Length; i++)
            {
                var fr = ns[i] as FriendRequestNotification;
                if (fr != null)
                {
                    if (fr.User.Uid == User.Uid)
                    {
                        LobbyClient.RemoveNotification(ns[i]);
                        break;
                    }
                }
            }
            Dismissed = true;
        }

        public void Decline()
        {
            var sm = new SocketMessage("acceptfriend");
            sm.AddData("uid", User.Uid);
            sm.AddData("accept", false);
            LobbyClient.WriteMessage(sm);
            Notification[] ns = LobbyClient.GetNotificationList();
            for (int i = 0; i < ns.Length; i++)
            {
                var fr = ns[i] as FriendRequestNotification;
                if (fr != null)
                {
                    if (fr.User.Uid == User.Uid)
                    {
                        LobbyClient.RemoveNotification(ns[i]);
                        break;
                    }
                }
            }
            Dismissed = true;
        }
    }
}