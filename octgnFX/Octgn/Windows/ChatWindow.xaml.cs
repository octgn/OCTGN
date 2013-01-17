using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Octgn.Launcher;
using Skylabs.Lobby;
using agsXMPP;
using Uri = System.Uri;

namespace Octgn.Windows
{
    /// <summary>
    ///   Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow
    {
        private bool _realClose;
        public NewChatRoom Room;
        public long Id { get { return Room.Rid; } }
        public bool IsLobbyChat { get; private set; }

        public ChatWindow(NewChatRoom room)
        {
            InitializeComponent();
            Room = room;
            chatControl.SetRoom(room);
        }

        private void WindowDrop(object sender, DragEventArgs e)
        {
            var s = e.Data.GetData(typeof (String)) as String;
            if (s == null) return;
            Room.AddUser(new NewUser(new Jid(s)));
            if (Room.IsGroupChat && Room.GroupUser.UserName != "lobby")
            {
                miLeaveChat.IsEnabled = true;
                var cl = Program.MainWindow.frame1.Content as ContactList;
                if(cl != null)
                    cl.RefreshList();
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            this.Title = (Room.IsGroupChat)
                             ? Room.GroupUser.UserName
                             : "Chat with: "
                               + Room.Users.FirstOrDefault(x => x.FullUserName != Program.LobbyClient.Me.FullUserName).UserName;
        }

        private void WindowUnloaded(object sender, RoutedEventArgs e)
        {
            Program.ChatWindows.RemoveAll(r => r.Id == Id);
            var cl = Program.MainWindow.frame1.Content as ContactList;
            Room.LeaveRoom();
            if (cl != null)
                cl.RefreshList();
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (_realClose) return;
            e.Cancel = true;
            Hide();
        }

        public void CloseChatWindow()
        {
            Dispatcher.Invoke(new Action(() =>
                                         {
                                            _realClose = true;
                                            Close();                                             
                                         }));

        }

        private void MiLeaveChatClick(object sender, RoutedEventArgs e)
        {
            _realClose = true;
            Close();
        }
    }
}