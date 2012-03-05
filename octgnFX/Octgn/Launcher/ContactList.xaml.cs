using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Octgn.Controls;
using Skylabs.Lobby;
using agsXMPP;

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

            Program.LClient.OnDataRecieved += LClientOnOnDataRecieved;
        }

        private void LClientOnOnDataRecieved(object sender, Client.DataRecType type, object data)
        {
            if (type == Client.DataRecType.FriendList)
            {
                RefreshList();
            }
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
                NewUser[] flist = Program.LClient.Friends.ToArray();
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
                        where cr.IsGroupChat
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
            //foreach (var cw in from r in Program.LobbyClient.Chatting.Rooms where r.ContainsUser(Program.LobbyClient.Me) && r.ContainsUser(fi.ThisUser) && !r.IsGroupChat && r.Id != 0 select (ChatWindow) (Program.ChatWindows.FirstOrDefault(c => c.Id == r.Id) ?? new ChatWindow(r.Id)))
            //{
             //   cw.Show();
              //  return;
            //}
            //Program.LobbyClient.Chatting.CreateChatRoom(fi.ThisUser);
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
            Program.LClient.OnDataRecieved -= LClientOnOnDataRecieved;
        }
    }
}