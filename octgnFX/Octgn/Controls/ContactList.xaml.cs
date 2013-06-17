using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Skylabs.Lobby;

namespace Octgn.Controls
{
    /// <summary>
    /// Interaction logic for ContactList.xaml
    /// </summary>
    public partial class ContactList : UserControl, IDisposable
    {
        public ContactList()
        {
            InitializeComponent();
            Program.LobbyClient.OnDataReceived += LobbyClientOnOnDataRecieved;
        }

        private void LobbyClientOnOnDataRecieved(object sender, DataRecType type, object data)
        {
            if (type == DataRecType.FriendList)
            {
                RefreshList();
            }
        }

        public void RefreshList()
        {
            //User[] flist = Program.LobbyClient.Friends.OrderByDescending(x => x.Status == UserStatus.Online).ThenBy(x => x.UserName).ToArray();
            //Dispatcher.Invoke(new Action(() =>
            //                                 {
            //                                     foreach (
            //                                         var f in stackPanel1.Children.OfType<FriendListItem>().ToArray())
            //                                     {
            //                                         f.MouseDoubleClick -= FMouseDoubleClick;
            //                                         f.Dispose();
            //                                     }
            //                                     foreach (
            //                                         var f in stackPanel1.Children.OfType<GroupChatListItem>().ToArray())
            //                                     {
            //                                         f.MouseDoubleClick -= GiMouseDoubleClick;
            //                                         f.Dispose();
            //                                     }
            //                                     stackPanel1.Children.Clear();
            //                                     foreach (FriendListItem f in flist.Select(u => new FriendListItem
            //                                                                                     {
            //                                                                                         ThisUser = u,
            //                                                                                         HorizontalAlignment
            //                                                                                             =
            //                                                                                             HorizontalAlignment
            //                                                                                             .
            //                                                                                             Stretch
            //                                                                                     }))
            //                                     {
            //                                         f.MouseDoubleClick += FMouseDoubleClick;
            //                                         stackPanel1.Children.Add(f);
            //                                     }
            //                                     foreach (var g in Program.LobbyClient.Chatting.Rooms.Where(x => x.IsGroupChat))
            //                                     {
            //                                         var gc = new GroupChatListItem(g);
            //                                         gc.MouseDoubleClick += GiMouseDoubleClick;
            //                                         stackPanel1.Children.Add(gc);
            //                                     }
            //                                 }));
        }


        private static void GiMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //var fi = sender as GroupChatListItem;
            //if (fi == null) return;
            //var room = Program.LobbyClient.Chatting.GetRoom(fi.ThisRoom.GroupUser, true);
        }


        private static void FMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //var fi = sender as FriendListItem;
            //if (fi == null) return;
            //var room = Program.LobbyClient.Chatting.GetRoom(fi.ThisUser);
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Program.LobbyClient.OnDataReceived -= LobbyClientOnOnDataRecieved;
            Dispatcher.Invoke(
                new Action(
                    () =>
                    {
                        foreach (var f in stackPanel1.Children.OfType<FriendListItem>().ToArray())
                        {
                            try
                            {
                                f.MouseDoubleClick -= FMouseDoubleClick;
                                f.Dispose();

                            }
                            catch
                            {

                            }
                        }
                        foreach (var f in stackPanel1.Children.OfType<GroupChatListItem>().ToArray())
                        {
                            try
                            {
                                f.MouseDoubleClick -= GiMouseDoubleClick;
                                f.Dispose();
                            }
                            catch
                            {

                            }
                        }
                    }));
        }

        #endregion
    }
}
