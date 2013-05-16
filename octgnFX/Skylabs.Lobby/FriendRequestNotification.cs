using System.Linq;
using Skylabs.Net;
using agsXMPP;

namespace Skylabs.Lobby
{
    public class FriendRequestNotification : Notification
    {
        public FriendRequestNotification(Jid u, Client lc, int id) : base(lc, id)
        {
            User = u;
        }

        public Jid User { get; private set; }

        public void Accept()
        {
            LobbyClient.AcceptFriendship(User);
            LobbyClient.Notifications.Remove(this);
            Dismissed = true;
        }

        public void Decline()
        {
            LobbyClient.DeclineFriendship(User);
            LobbyClient.Notifications.Remove(this);
            Dismissed = true;
        }
    }
}