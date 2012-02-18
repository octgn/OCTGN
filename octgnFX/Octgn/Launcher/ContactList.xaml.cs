using System;
using System.Linq;
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
            Program.LobbyClient.Chatting.EChatEvent += ChattingEChatEvent;
        }

        private void ChattingEChatEvent(ChatRoom cr, Chatting.ChatEvent e, User u, object data)
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
                                                 foreach (FriendListItem f in flist.Select(u => new FriendListItem
                                                                                                    {
                                                                                                        ThisUser = u,
                                                                                                        HorizontalAlignment
                                                                                                            =
                                                                                                            HorizontalAlignment
                                                                                                            .
                                                                                                            Stretch
                                                                                                    }))
                                                 {
                                                     f.MouseDoubleClick += FMouseDoubleClick;
                                                     stackPanel1.Children.Add(f);
                                                 }
                                                 foreach (
                                                     GroupChatListItem gi in
                                                         from cr in Program.LobbyClient.Chatting.Rooms
                                                         where cr.Id == 0 || (cr.UserCount > 2)
                                                         select new GroupChatListItem
                                                                    {
                                                                        ThisRoom = cr,
                                                                        HorizontalAlignment =
                                                                            HorizontalAlignment.Stretch
                                                                    })
                                                 {
                                                     gi.MouseDoubleClick += GiMouseDoubleClick;
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

        private static void GiMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var gi = sender as GroupChatListItem;
            if (gi == null) return;
            foreach (ChatWindow cw in Program.ChatWindows.Where(cw => gi.ThisRoom.Id == cw.Id))
            {
                cw.Show();
                return;
            }
            if (gi.ThisRoom.Id != 0) return;
            var cw2 = new ChatWindow(0);
            Program.ChatWindows.Add(cw2);
            cw2.Show();
        }


        private static void FMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fi = sender as FriendListItem;
            if (fi == null) return;
            foreach (ChatWindow cw in from cw in Program.ChatWindows
                                      let rid = cw.Id
                                      let cr = Program.LobbyClient.Chatting.GetChatRoomFromRID(rid)
                                      where cr != null
                                      where cr.Id != 0
                                      where
                                          cr.UserCount == 2 && cr.ContainsUser(Program.LobbyClient.Me) &&
                                          cr.ContainsUser(fi.ThisUser)
                                      where cw.Visibility != Visibility.Visible
                                      select cw)
            {
                cw.Show();
                return;
            }
            Program.LobbyClient.Chatting.CreateChatRoom(fi.ThisUser);
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void lobbyClient_OnUserStatusChanged(UserStatus eve, User u)
        {
            RefreshList();
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            Program.LobbyClient.OnUserStatusChanged -= lobbyClient_OnUserStatusChanged;
            Program.LobbyClient.OnDataRecieved -= lobbyClient_OnDataRecieved;
            Program.LobbyClient.Chatting.EChatEvent -= ChattingEChatEvent;
        }
    }
}