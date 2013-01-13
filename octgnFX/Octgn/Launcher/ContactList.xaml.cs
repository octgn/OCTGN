using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Octgn.Controls;
using Skylabs.Lobby;
using Skylabs.Lobby.Threading;
using agsXMPP;
using System.Collections;

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
            Program.LobbyClient.OnDataReceived += LobbyClientOnOnDataRecieved;
            Program.LobbyClient.Chatting.OnCreateRoom += ChattingOnOnCreateRoom;
        }

        private void ChattingOnOnCreateRoom(object sender , NewChatRoom room)
        {
            LazyAsync.Invoke(RefreshList);
        }

        private void LobbyClientOnOnDataRecieved(object sender,DataRecType type, object data)
        {
            if (type ==DataRecType.FriendList)
            {
                RefreshList();
            }
        }

        public void RefreshList()
        {
            Dispatcher.Invoke(new Action(() =>
                                             {
                stackPanel1.Children.Clear();
                NewUser[] flist = Program.LobbyClient.Friends.OrderByDescending(x => x.Status == UserStatus.Online).ThenBy(x => x.UserName).ToArray();
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
                foreach( var g in Program.LobbyClient.Chatting.Rooms.Where(x=>x.IsGroupChat))
                {
                    var gc = new GroupChatListItem()
                    {
                        ThisRoom = g ,
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };
                    gc.MouseDoubleClick += GiMouseDoubleClick;
                    stackPanel1.Children.Add(gc);
                }
                                             }));
        }


        private static void GiMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fi = sender as GroupChatListItem;
            if (fi == null) return;
            var room = Program.LobbyClient.Chatting.GetRoom(fi.ThisRoom.GroupUser,true);
            var cw = Program.ChatWindows.FirstOrDefault(x => x.Id == room.Rid);
            if(cw == null)
            {
                cw = new Windows.ChatWindow(room);
                Program.ChatWindows.Add(cw);
            }
            cw.Show();
            cw.richTextBox1.ScrollToEnd();
        }


        private static void FMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fi = sender as FriendListItem;
            if (fi == null) return;
            var room = Program.LobbyClient.Chatting.GetRoom(fi.ThisUser);
            var cw = Program.ChatWindows.FirstOrDefault(x => x.Id == room.Rid);
            if(cw == null)
            {
                cw = new Windows.ChatWindow(room);
                Program.ChatWindows.Add(cw);
            }
            cw.Show();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }


        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            Program.LobbyClient.OnDataReceived -= LobbyClientOnOnDataRecieved;
        }
    }
}