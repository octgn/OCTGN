using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Octgn.Controls;
using Skylabs.Lobby;

namespace Octgn.Launcher
{
    /// <summary>
    ///   Interaction logic for ContactList.xaml
    /// </summary>
    public partial class ContactList : Page
    {
        public ContactList()
        {
            InitializeComponent();
            Program.lobbyClient.OnUserStatusChanged += lobbyClient_OnUserStatusChanged;
            Program.lobbyClient.OnDataRecieved += lobbyClient_OnDataRecieved;
            Program.lobbyClient.Chatting.eChatEvent += Chatting_eChatEvent;
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
                                                 User[] flist = Program.lobbyClient.GetFriendsList();
                                                 foreach (User u in flist)
                                                 {
                                                     var f = new FriendListItem();
                                                     f.ThisUser = u;
                                                     f.HorizontalAlignment = HorizontalAlignment.Stretch;
                                                     f.MouseDoubleClick += f_MouseDoubleClick;
                                                     stackPanel1.Children.Add(f);
                                                 }
                                                 foreach (ChatRoom cr in Program.lobbyClient.Chatting.Rooms)
                                                 {
                                                     if (cr.ID == 0 || (cr.UserCount > 2))
                                                     {
                                                         var gi = new GroupChatListItem();
                                                         gi.ThisRoom = cr;
                                                         gi.HorizontalAlignment = HorizontalAlignment.Stretch;
                                                         gi.MouseDoubleClick += gi_MouseDoubleClick;
                                                         stackPanel1.Children.Add(gi);
                                                     }
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

        private void gi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var gi = sender as GroupChatListItem;
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
                    var cw = new ChatWindow(0);
                    Program.ChatWindows.Add(cw);
                    cw.Show();
                }
            }
        }


        private void f_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fi = sender as FriendListItem;
            if (fi != null)
            {
                foreach (ChatWindow cw in Program.ChatWindows)
                {
                    long rid = cw.ID;
                    ChatRoom cr = Program.lobbyClient.Chatting.GetChatRoomFromRID(rid);
                    if (cr != null)
                    {
                        if (cr.ID == 0)
                            continue;
                        if (cr.UserCount == 2 && cr.ContainsUser(Program.lobbyClient.Me) && cr.ContainsUser(fi.ThisUser))
                        {
                            if (cw.Visibility != Visibility.Visible)
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

        private void lobbyClient_OnUserStatusChanged(UserStatus eve, User u)
        {
            RefreshList();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Program.lobbyClient.OnUserStatusChanged -= lobbyClient_OnUserStatusChanged;
            Program.lobbyClient.OnDataRecieved -= lobbyClient_OnDataRecieved;
            Program.lobbyClient.Chatting.eChatEvent -= Chatting_eChatEvent;
        }
    }
}