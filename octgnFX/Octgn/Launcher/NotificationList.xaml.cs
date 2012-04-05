using System;
using System.Linq;
using System.Windows;
using Skylabs.Lobby;
using FriendRequestNotification = Octgn.Controls.FriendRequestNotification;

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
            foreach (FriendRequestNotification fi in from n in nlist
                                                     where
                                                         n.GetType() == typeof (Skylabs.Lobby.FriendRequestNotification)
                                                     select n as Skylabs.Lobby.FriendRequestNotification
                                                     into fr select new FriendRequestNotification
                                                                        {
                                                                            Notification = fr,
                                                                            HorizontalAlignment =
                                                                                HorizontalAlignment.Stretch
                                                                        }
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
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}