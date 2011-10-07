using System;
using System.Windows;
using System.Windows.Controls;
using Octgn.Controls;
using Skylabs.Lobby;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for ContactList.xaml
    /// </summary>
    public partial class ContactList : Page
    {
        public ContactList()
        {
            InitializeComponent();
            Program.lobbyClient.OnUserStatusChanged += new Skylabs.Lobby.LobbyClient.UserStatusChanged(lobbyClient_OnUserStatusChanged);
            Program.lobbyClient.OnDataRecieved += new LobbyClient.DataRecieved(lobbyClient_OnDataRecieved);
        }

        private void lobbyClient_OnDataRecieved(DataRecType type, object e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if(type == DataRecType.FriendList)
                {
                    stackPanel1.Children.Clear();
                    foreach(User u in Program.lobbyClient.FriendList)
                    {
                        FriendListItem f = new FriendListItem();
                        f.ThisUser = u;
                        f.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        stackPanel1.Children.Add(f);
                    }
                }
            }));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            foreach(User u in Program.lobbyClient.FriendList)
            {
                FriendListItem f = new FriendListItem();
                f.ThisUser = u;
                stackPanel1.Children.Add(f);
            }
        }

        private void lobbyClient_OnUserStatusChanged(Skylabs.Lobby.UserStatus eve, Skylabs.Lobby.User u)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                foreach(UIElement ui in stackPanel1.Children)
                {
                    FriendListItem f = ui as FriendListItem;
                    if(ui != null)
                    {
                        if(f.ThisUser.Equals(u))
                        {
                            f.ThisUser = u;
                            break;
                        }
                    }
                }
            }));
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Program.lobbyClient.OnUserStatusChanged -= lobbyClient_OnUserStatusChanged;
            Program.lobbyClient.OnDataRecieved -= lobbyClient_OnDataRecieved;
        }
    }
}