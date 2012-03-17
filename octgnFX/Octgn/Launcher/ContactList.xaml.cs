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
            Program.LClient.OnDataRecieved += LClientOnOnDataRecieved;
            Program.LClient.Chatting.OnCreateRoom += ChattingOnOnCreateRoom;
        }

        private void ChattingOnOnCreateRoom(object sender , NewChatRoom room)
        {
            RefreshList();
        }

        private void LClientOnOnDataRecieved(object sender, Client.DataRecType type, object data)
        {
            if (type == Client.DataRecType.FriendList)
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
                foreach( var g in Program.LClient.Chatting.Rooms.Where(x=>x.IsGroupChat))
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
            var room = Program.LClient.Chatting.GetRoom(fi.ThisRoom.GroupUser,true);
            var cw = Program.ChatWindows.SingleOrDefault(x => x.Id == room.RID);
            if(cw == null)
            {
                cw = new ChatWindow(room);
                Program.ChatWindows.Add(cw);
            }
            cw.Show();
        }


        private static void FMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fi = sender as FriendListItem;
            if (fi == null) return;
            var room = Program.LClient.Chatting.GetRoom(fi.ThisUser);
            var cw = Program.ChatWindows.SingleOrDefault(x => x.Id == room.RID);
            if(cw == null)
            {
                cw = new ChatWindow(room);
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
            Program.LClient.OnDataRecieved -= LClientOnOnDataRecieved;
        }
    }
}