using System;
using System.Linq;
using System.Windows;
using Skylabs.Lobby;

namespace Octgn.Launcher
{
    /// <summary>
    ///   Interaction logic for GameList.xaml
    /// </summary>
    public partial class NotificationList
    {
        public NotificationList()
        {
            InitializeComponent();
        }

        private void ReloadList()
        {
            Notification[] nlist = Program.LobbyClient.GetNotificationList();
            foreach (var fi in from n in nlist
                               where n.GetType() == typeof (FriendRequestNotification)
                               select n as FriendRequestNotification
                               into fr select new Controls.FriendRequestNotification
                                                  {Notification = fr, HorizontalAlignment = HorizontalAlignment.Stretch}
                )
            {
                fi.OnDismiss += NotificationDismissed;
                stackPanel1.Children.Add(fi);
            }
        }

        private void NotificationDismissed(object sender, EventArgs e)
        {
            var u = sender as UIElement;
            if (u != null)
                stackPanel1.Children.Remove(u);
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            ReloadList();
            Program.LobbyClient.OnFriendRequest += lobbyClient_OnFriendRequest;
        }

        private void lobbyClient_OnFriendRequest(User u)
        {
            //Reload_List();
            Dispatcher.Invoke(new Action(ReloadList));
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            Program.LobbyClient.OnFriendRequest -= lobbyClient_OnFriendRequest;
        }
    }
}