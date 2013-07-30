using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Octgn.Windows
{
    using System.Collections.Concurrent;
    using System.ComponentModel;

    using Octgn.Controls;
    using Octgn.Extentions;

    using Skylabs.Lobby;

    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : IDisposable
    {
        private bool realClose = false;

        internal ChatRoom Room { get; set; }

        public ChatWindow(ChatRoom room)
        {
            Room = room;
            this.Activated += OnActivated;
            this.Closing += OnClosing;
            InitializeComponent();
            this.Title = Room.IsGroupChat
                 ? Room.GroupUser.UserName
                 : Room.Users.First(x => x != Program.LobbyClient.Me).UserName;
            Room.OnUserListChange += RoomOnOnUserListChange;
            ChatControl.SetRoom(Room);
        }

        public ChatWindow(ChatControl control)
        {
            Room = control.Room;
            this.Activated += OnActivated;
            this.Closing += OnClosing;
            InitializeComponent();
            this.Title = Room.IsGroupChat
                 ? Room.GroupUser.UserName
                 : Room.Users.First(x => x != Program.LobbyClient.Me).UserName;
            Room.OnUserListChange += RoomOnOnUserListChange;
            var chatParent = this.ChatControl.Parent as Panel;
            chatParent.Children.Remove(this.ChatControl);
            ChatControl = control;
            ChatControl.Width = Double.NaN;
            ChatControl.Height = Double.NaN;
            Grid.SetRow(ChatControl,1);
            ChatControl.VerticalAlignment = VerticalAlignment.Stretch;
            ChatControl.HorizontalAlignment = HorizontalAlignment.Stretch;
            chatParent.Children.Add(ChatControl);
        }

        public void CloseDown()
        {
            this.realClose = true;
            this.Close();
        }

        private void MenuItemLeaveChatClick(object sender, RoutedEventArgs e)
        {
            this.realClose = true;
            this.Close();
            Program.LobbyClient.Chatting.LeaveRoom(Room);
        }

        private void MenuItemDockClick(object sender, RoutedEventArgs e)
        {
            this.realClose = true;
            var cc = ChatControl;
            (ChatControl.Parent as Panel).Children.Remove(cc);
            this.Close();
            Application.Current.Dispatcher.BeginInvoke(new Action(()=>ChatManager.Get().MoveToChatBar(cc)));
        }

        private void RoomOnOnUserListChange(object sender, List<User> users)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => this.RoomOnOnUserListChange(sender, users)));
                return;
            }
            this.Title = Room.IsGroupChat
                 ? Room.GroupUser.UserName
                 : Room.Users.First(x => x != Program.LobbyClient.Me).UserName;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (!realClose)
            {
                cancelEventArgs.Cancel = true;
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                var nl = WindowManager.ChatWindows.ToList();
                nl.Remove(this);
                WindowManager.ChatWindows = new ConcurrentBag<ChatWindow>(nl);
            }
        }

        private void OnActivated(object sender, EventArgs eventArgs)
        {
            this.StopFlashingWindow();
        }

        public new void Dispose()
        {
            this.Activated -= this.OnActivated;
            this.Closing -= OnClosing;
            base.Dispose();
        }
    }
}
