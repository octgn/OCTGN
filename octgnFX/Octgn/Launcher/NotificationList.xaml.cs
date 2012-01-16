using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Octgn.Controls;
using System.IO;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for GameList.xaml
    /// </summary>
    public partial class NotificationList : Page
    {
        public NotificationList()
        {
            InitializeComponent();

        }
        private void Reload_List()
        {
            Skylabs.Lobby.Notification[] nlist = Program.lobbyClient.GetNotificationList();
            foreach (Skylabs.Lobby.Notification n in nlist)
            {
                if (n.GetType() == typeof(Skylabs.Lobby.FriendRequestNotification))
                {
                    Skylabs.Lobby.FriendRequestNotification fr = n as Skylabs.Lobby.FriendRequestNotification;
                    FriendRequestNotification fi = new FriendRequestNotification();
                    fi.Notification = fr;
                    fi.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    fi.OnDismiss += NotificationDismissed;
                    stackPanel1.Children.Add(fi);
                }
            }

        }
        private void NotificationDismissed(object sender, EventArgs e)
        {
            UIElement u = sender as UIElement;
            if(u != null)
                stackPanel1.Children.Remove(u);
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Reload_List();
            Program.lobbyClient.OnFriendRequest += new Skylabs.Lobby.LobbyClient.FriendRequest(lobbyClient_OnFriendRequest);
        }

        void lobbyClient_OnFriendRequest(Skylabs.Lobby.User u)
        {
            //Reload_List();
            Dispatcher.Invoke(new Action(Reload_List));
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Program.lobbyClient.OnFriendRequest -= lobbyClient_OnFriendRequest;
        }
    }
}
