using System;
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

        private void Reload_List()
        {
            Notification[] nlist = Program.LobbyClient.GetNotificationList();
            foreach (Notification n in nlist)
            {
                if (n.GetType() != typeof (FriendRequestNotification)) continue;
                var fr = n as FriendRequestNotification;
                var fi = new Controls.FriendRequestNotification
                             {Notification = fr, HorizontalAlignment = HorizontalAlignment.Stretch};
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Reload_List();
            Program.LobbyClient.OnFriendRequest += lobbyClient_OnFriendRequest;
        }

        private void lobbyClient_OnFriendRequest(User u)
        {
            //Reload_List();
            Dispatcher.Invoke(new Action(Reload_List));
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Program.LobbyClient.OnFriendRequest -= lobbyClient_OnFriendRequest;
        }
    }
}