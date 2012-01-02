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
            Program.lobbyClient.Chatting.eChatEvent += new Chatting.ChatEventDelegate(Chatting_eChatEvent);
        }

        void Chatting_eChatEvent(ChatRoom cr, Chatting.ChatEvent e, User u, object data)
        {
            if (e != Chatting.ChatEvent.ChatMessage)
            {
                RefreshList();
            }
        }
        public void RefreshList()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                stackPanel1.Children.Clear();
                foreach (User u in Program.lobbyClient.FriendList)
                {
                    FriendListItem f = new FriendListItem();
                    f.ThisUser = u;
                    f.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    f.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(f_MouseDoubleClick);
                    stackPanel1.Children.Add(f);
                }
                foreach (ChatRoom cr in Program.lobbyClient.Chatting.Rooms)
                {
                    if (cr.ID == 0 || (cr.Users.Count > 2))
                    {
                        GroupChatListItem gi = new GroupChatListItem();
                        gi.ThisRoom = cr;
                        gi.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        gi.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(gi_MouseDoubleClick);
                        stackPanel1.Children.Add(gi);
                    }
                }
            }));
        }
        private void lobbyClient_OnDataRecieved(DataRecType type, object e)
        {
            if(type == DataRecType.FriendList)
            {
                RefreshList();
            }
        }

        void gi_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GroupChatListItem gi = sender as GroupChatListItem;
            if (gi != null)
            {
                foreach (ChatWindow cw in Program.ChatWindows)
                {
                    if (gi.ThisRoom.ID == cw.ID)
                    {
                        cw.Show();
                        return;
                    }
                }
                if (gi.ThisRoom.ID == 0)
                {
                    ChatWindow cw = new ChatWindow(0);
                    cw.Show();
                }
            }
        }


        void f_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FriendListItem fi = sender as FriendListItem;
            if (fi != null)
            {
                foreach (ChatWindow cw in Program.ChatWindows)
                {
                    long rid = cw.ID;
                    ChatRoom cr = Program.lobbyClient.Chatting.GetChatRoomFromRID(rid);
                    if (rid != null)
                    {
                        if (cr.ID == 0)
                            continue;
                        if (cr.Users.Count == 2 && cr.Users.Contains(Program.lobbyClient.Me) && cr.Users.Contains(fi.ThisUser))
                        {
                            if (cw.Visibility != System.Windows.Visibility.Visible)
                            {
                                cw.Show();
                                return;
                            }
                        }
                    }
                }
                Program.lobbyClient.Chatting.CreateChatRoom(fi.ThisUser);
            }
                
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void lobbyClient_OnUserStatusChanged(Skylabs.Lobby.UserStatus eve, Skylabs.Lobby.User u)
        {
            RefreshList();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Program.lobbyClient.OnUserStatusChanged -= lobbyClient_OnUserStatusChanged;
            Program.lobbyClient.OnDataRecieved -= lobbyClient_OnDataRecieved;
        }
    }
}