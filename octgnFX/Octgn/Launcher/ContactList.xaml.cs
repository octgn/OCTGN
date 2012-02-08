using System;
using System.Windows;
using System.Windows.Input;
using Octgn.Controls;
using Skylabs.Lobby;

namespace Octgn.Launcher
{
    /// <summary>
    ///   Interaction logic for ContactList.xaml
    /// </summary>
    public partial class ContactList
    {
        public ContactList()
        {
            InitializeComponent();
            Program.LobbyClient.OnUserStatusChanged += lobbyClient_OnUserStatusChanged;
            Program.LobbyClient.OnDataRecieved += lobbyClient_OnDataRecieved;
            Program.LobbyClient.Chatting.EChatEvent += Chatting_eChatEvent;
        }

        private void Chatting_eChatEvent(ChatRoom cr, Chatting.ChatEvent e, User u, object data)
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
                                                 User[] flist = Program.LobbyClient.GetFriendsList();
                                                 foreach (User u in flist)
                                                 {
                                                     var f = new FriendListItem
                                                                 {
                                                                     ThisUser = u,
                                                                     HorizontalAlignment = HorizontalAlignment.Stretch
                                                                 };
                                                     f.MouseDoubleClick += f_MouseDoubleClick;
                                                     stackPanel1.Children.Add(f);
                                                 }
                                                 foreach (ChatRoom cr in Program.LobbyClient.Chatting.Rooms)
                                                 {
                                                     if (cr.Id != 0 && (cr.UserCount <= 2)) continue;
                                                     var gi = new GroupChatListItem
                                                                  {
                                                                      ThisRoom = cr,
                                                                      HorizontalAlignment = HorizontalAlignment.Stretch
                                                                  };
                                                     gi.MouseDoubleClick += gi_MouseDoubleClick;
                                                     stackPanel1.Children.Add(gi);
                                                 }
                                             }));
        }

        private void lobbyClient_OnDataRecieved(DataRecType type, object e)
        {
            if (type == DataRecType.FriendList)
            {
                RefreshList();
            }
        }

        private static void gi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var gi = sender as GroupChatListItem;
            if (gi == null) return;
            foreach (ChatWindow cw in Program.ChatWindows)
            {
                if (gi.ThisRoom.Id != cw.ID) continue;
                cw.Show();
                return;
            }
            if (gi.ThisRoom.Id == 0)
            {
                var cw = new ChatWindow(0);
                Program.ChatWindows.Add(cw);
                cw.Show();
            }
        }


        private static void f_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fi = sender as FriendListItem;
            if (fi == null) return;
            foreach (ChatWindow cw in Program.ChatWindows)
            {
                long rid = cw.ID;
                ChatRoom cr = Program.LobbyClient.Chatting.GetChatRoomFromRID(rid);
                if (cr == null) continue;
                if (cr.Id == 0)
                    continue;
                if (cr.UserCount == 2 && cr.ContainsUser(Program.LobbyClient.Me) && cr.ContainsUser(fi.ThisUser))
                {
                    if (cw.Visibility != Visibility.Visible)
                    {
                        cw.Show();
                        return;
                    }
                }
            }
            Program.LobbyClient.Chatting.CreateChatRoom(fi.ThisUser);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void lobbyClient_OnUserStatusChanged(UserStatus eve, User u)
        {
            RefreshList();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Program.LobbyClient.OnUserStatusChanged -= lobbyClient_OnUserStatusChanged;
            Program.LobbyClient.OnDataRecieved -= lobbyClient_OnDataRecieved;
            Program.LobbyClient.Chatting.EChatEvent -= Chatting_eChatEvent;
        }
    }
}