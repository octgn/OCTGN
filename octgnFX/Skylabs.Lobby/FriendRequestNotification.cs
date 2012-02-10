using System.Linq;
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
            foreach (
                Notification t in
                    from t in ns
                    let fr = t as FriendRequestNotification
                    where fr != null
                    where fr.User.Uid == User.Uid
                    select t)
            {
                LobbyClient.RemoveNotification(t);
                break;
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
            foreach (
                Notification t in
                    from t in ns
                    let fr = t as FriendRequestNotification
                    where fr != null
                    where fr.User.Uid == User.Uid
                    select t)
            {
                LobbyClient.RemoveNotification(t);
                break;
            }
            Dismissed = true;
        }
    }
}