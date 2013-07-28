/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octgn.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;

    using CodeBits;

    using Octgn.Controls.ControlTemplates;
    using Octgn.Extentions;
    using Octgn.Site.Api.Models;
    using Octgn.Utils;
    using Octgn.Windows;

    using Skylabs.Lobby;

    using agsXMPP;

    using log4net;

    /// <summary>
    /// Interaction logic for ChatControl
    /// </summary>
    public partial class ChatControl : UserControl,INotifyPropertyChanged,IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #region Privates

        /// <summary>
        /// The sent message cache
        /// </summary>
        private readonly List<string> messageCache;

        /// <summary>
        /// The chat room.
        /// </summary>
        private ChatRoom room;

        /// <summary>
        /// Is the shift key down
        /// </summary>
        private bool shiftDown;

        /// <summary>
        /// The cur message cache item.
        /// </summary>
        private int curMessageCacheItem;

        /// <summary>
        /// The user refresh timer.
        /// </summary>
        private Timer userRefreshTimer;

        /// <summary>
        /// Just scrolled to bottom.
        /// </summary>
        private bool justScrolledToBottom;

        private bool isLightTheme;

        private bool showChatInputHint = true;

        private ChatRoomState roomState;

        #endregion Privates

        public OrderedObservableCollection<ChatUserListItem> UserListItems { get; set; }

        public OrderedObservableCollection<FriendListItem> FriendListItems { get; set; } 

        public bool IsAdmin
        {
            get
            {
                if (this.Room == null) return false;
                return this.Room.AdminList.Any(x => x == Program.LobbyClient.Me);
            }
        }

        public bool IsModerator
        {
            get
            {
                if (this.Room == null) return false;
                return this.Room.ModeratorList.Any(x => x == Program.LobbyClient.Me);
            }
        }

        public bool BanMenuVisible
        {
            get
            {
                return IsAdmin || IsModerator;
            }
        }

        public bool ShowChatInputHint
        {
            get
            {
                return this.showChatInputHint;
            }
            set
            {
                if (this.showChatInputHint == value) return;
                this.showChatInputHint = value;
                OnPropertyChanged("ShowChatInputHint");
            }
        }

        public bool IsLightTheme
        {
            get
            {
                return this.isLightTheme;
            }
            set
            {
                if (value == this.isLightTheme) return;
                this.isLightTheme = value;
                OnPropertyChanged("IsLightTheme");
            }
        }

        public bool IsOffline
        {
            get
            {
                return this.isOffline;
            }
            set
            {
                if (value == this.isOffline) return;
                this.isOffline = value;
                OnPropertyChanged("IsOffline");
            }
        }

        public bool IsLoadingUsers
        {
            get
            {
                return this.RoomState == ChatRoomState.GettingUsers || this.RoomState == ChatRoomState.Connecting || this.RoomState == ChatRoomState.Disconnected;
            }
        }

        public bool IsLoadingHistory
        {
            get
            {
                return this.RoomState == ChatRoomState.GettingUsers || this.RoomState == ChatRoomState.Connecting || this.RoomState == ChatRoomState.Disconnected || this.RoomState == ChatRoomState.GettingHistory;
            }
        }

        public bool IsChatLoaded
        {
            get
            {
                return this.RoomState == ChatRoomState.Connected;
            }
        }

        public ChatRoomState RoomState
        {
            get
            {
                return roomState;
            }
            set
            {
                if (roomState == value) return;
                roomState = value;
                OnPropertyChanged("RoomState");
                OnPropertyChanged("IsLoadingUsers");
                OnPropertyChanged("IsLoadingHistory");
                OnPropertyChanged("IsChatLoaded");
            }
        }

        public ContextMenu UserContextMenu { get; set; }

        public ContextMenu FriendContextMenu { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatControl"/> class.
        /// </summary>
        public ChatControl()
        {
            this.UserListItems = new OrderedObservableCollection<ChatUserListItem>();
            this.FriendListItems = new OrderedObservableCollection<FriendListItem>();
            if (!this.IsInDesignMode())
            {
                if (Program.LobbyClient != null && Program.LobbyClient.IsConnected)
                {
                    IsOffline = false;
                }
            }
            this.InitializeComponent();
            this.messageCache = new List<string>();
            this.DataContext = UserListItems;

            if (this.IsInDesignMode())
            {
                return;
            }
            justScrolledToBottom = true;
            this.CreateUserContextMenu();
            Program.OnOptionsChanged += ProgramOnOnOptionsChanged;
            Program.LobbyClient.OnDataReceived += LobbyClientOnDataReceived;
            Program.LobbyClient.OnLoginComplete += LobbyClientOnOnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClientOnOnDisconnect;
            this.userRefreshTimer = new Timer(this.OnRefreshTimerTick, this, 100, 7000);
            this.Loaded += OnLoaded;
        }

        private void LobbyClientOnOnDisconnect(object sender, EventArgs eventArgs)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => this.LobbyClientOnOnDisconnect(sender, eventArgs)));
                return;
            }
            this.IsEnabled = false;
            IsOffline = true;
        }

        private void LobbyClientOnOnLoginComplete(object sender, LoginResults results)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => this.LobbyClientOnOnLoginComplete(sender, results)));
                return;
            }
            if (results == LoginResults.Success)
            {
                this.IsEnabled = true;
                IsOffline = false;
            }
        }

        private void LobbyClientOnDataReceived(object sender, DataRecType type, object data)
        {
            if (type == DataRecType.FriendList)
            {
                this.InvokeFriendList();
            }
        }

        private MenuItem whisperContextMenuItem;
        private MenuItem profileContextMenuItem;
        private MenuItem addFriendContextMenuItem;
        private MenuItem banContextMenuItem;
        private MenuItem friendWhisperContextMenuItem;
        private MenuItem friendProfileContextMenuItem;
        private MenuItem removeFriendContextMenuItem;

        private void CreateUserContextMenu()
        {
            // UserContextMenu
            UserContextMenu = new ContextMenu();
            whisperContextMenuItem = new MenuItem();
            whisperContextMenuItem.Header = "Whisper";
            whisperContextMenuItem.Click += WhisperOnClick;
            UserContextMenu.Items.Add(whisperContextMenuItem);

            addFriendContextMenuItem = new MenuItem();
            addFriendContextMenuItem.Header = "Add Friend";
            addFriendContextMenuItem.Click += AddFriendOnClick;
            UserContextMenu.Items.Add(addFriendContextMenuItem);

            banContextMenuItem = new MenuItem();
            banContextMenuItem.Header = "Ban";
            banContextMenuItem.Click += BanOnClick;
            //UserContextMenu.Items.Add(banContextMenuItem);

            profileContextMenuItem = new MenuItem();
            profileContextMenuItem.Header = "Profile";
            profileContextMenuItem.Click += ProfileOnClick;
            UserContextMenu.Items.Add(profileContextMenuItem);

            var binding = new System.Windows.Data.Binding();
            binding.Mode = System.Windows.Data.BindingMode.OneWay;
            binding.Converter = new BooleanToVisibilityConverter();
            binding.Source = BanMenuVisible;

            banContextMenuItem.SetBinding(VisibilityProperty, binding);

            //FriendListContextMenu
            FriendContextMenu = new ContextMenu();
            friendWhisperContextMenuItem = new MenuItem();
            friendWhisperContextMenuItem.Header = "Whisper";
            friendWhisperContextMenuItem.Click += WhisperOnClick;
            FriendContextMenu.Items.Add(friendWhisperContextMenuItem);

            removeFriendContextMenuItem = new MenuItem();
            removeFriendContextMenuItem.Header = "Remove Friend";
            removeFriendContextMenuItem.Click += RemoveFriendOnClick;
            FriendContextMenu.Items.Add(removeFriendContextMenuItem);

            friendProfileContextMenuItem = new MenuItem();
            friendProfileContextMenuItem.Header = "Profile";
            friendProfileContextMenuItem.Click += ProfileOnClick;
            FriendContextMenu.Items.Add(friendProfileContextMenuItem);

        }

        private void ProfileOnClick(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            if (mi == null) return;
            var cm = mi.Parent as ContextMenu;
            if (cm == null) return;
            var ui = cm.PlacementTarget as UserListItem;
            if (ui == null) return;
            UserProfileWindow.Show(ui.User);
        }

        private void BanOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var mi = sender as MenuItem;
            if (mi == null) return;
            var cm = mi.Parent as ContextMenu;
            if (cm == null) return;
            var ui = cm.PlacementTarget as UserListItem;
            if (ui == null) return;
            
        }

        private void AddFriendOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var mi = sender as MenuItem;
            if (mi == null) return;
            var cm = mi.Parent as ContextMenu;
            if (cm == null) return;
            var ui = cm.PlacementTarget as UserListItem;
            if (ui == null) return;
            Program.LobbyClient.SendFriendRequest(ui.User.UserName);
        }

        private void RemoveFriendOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var mi = sender as MenuItem;
            if (mi == null) return;
            var cm = mi.Parent as ContextMenu;
            if (cm == null) return;
            var ui = cm.PlacementTarget as UserListItem;
            if (ui == null) return;
            Program.LobbyClient.RemoveFriend(ui.User);
        }

        private void WhisperOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var mi = sender as MenuItem;
            if (mi == null) return;
            var cm = mi.Parent as ContextMenu;
            if (cm == null) return;
            var ui = cm.PlacementTarget as UserListItem;
            if (ui == null) return;
            Room.Whisper(ui.User);
        }

        private void OnLoaded(object sender, EventArgs eventArgs)
        {
            ProgramOnOnOptionsChanged();
        }

        private void ProgramOnOnOptionsChanged()
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    this.IsLightTheme = Prefs.UseLightChat;
                    this.InvalidateVisual();
                }));
        }

        /// <summary>
        /// Gets the room.
        /// </summary>
        internal ChatRoom Room
        {
            get
            {
                return this.room;
            }
        }

        /// <summary>
        /// Set the room for this chat control.
        /// </summary>
        /// <param name="theRoom">
        /// The room.
        /// </param>
        public void SetRoom(ChatRoom theRoom)
        {
            if (this.room != null)
            {
                throw new InvalidOperationException("Cannot set the room more than once.");
            }
            this.room = theRoom;
            this.room.OnMessageReceived += this.RoomOnMessageReceived;
            this.room.OnUserListChange += RoomOnOnUserListChange;
            this.room.OnStateChanged += RoomOnOnStateChanged;
        }

        private void RoomOnOnStateChanged(object sender, ChatRoomState oldState, ChatRoomState newState)
        {
            RoomState = newState;
        }

        private void RoomOnOnUserListChange(object sender, List<User> users)
        {
            this.InvokeRoomUserList();
        }

        /// <summary>
        /// When a message is received in the room.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="id">The message Id</param>
        /// <param name="from">
        /// The from.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="receiveTime">
        /// The receive Time.
        /// </param>
        /// <param name="messageType">
        /// The message type.
        /// </param>
        private void RoomOnMessageReceived(object sender, string id,User @from, string message, DateTime receiveTime, LobbyMessageType messageType)
        {
            var theFrom = from;
            var theMessage = message;
            var therTime = receiveTime;
            var themType = messageType;
            var theId = id;
            if (string.IsNullOrWhiteSpace(theFrom.UserName))
            {
                theFrom.UserName = "SYSTEM";
            }
            if (theFrom.UserName.Equals("octgn-gap", StringComparison.InvariantCultureIgnoreCase))
            {
                var cl = theMessage.IndexOf(':');
                theFrom.UserName = "GAP[" + theMessage.Substring(0, cl) + "]";
                theMessage = theMessage.Substring(cl + 1, theMessage.Length - cl - 1).Trim();
            }

            if (theMessage.ToLowerInvariant().Contains("@" + Program.LobbyClient.Me.UserName.ToLowerInvariant()))
            {
                Sounds.PlayMessageSound();
            }

            Dispatcher.Invoke(
                new Action(
                    () =>
                    {
                        // Got mostly from http://stackoverflow.com/questions/13456441/wpf-richtextboxs-conditionally-scroll
                        var rtbatbottom = false;

                        var offset = Chat.VerticalOffset + Chat.ViewportHeight;
                        if (Math.Abs(offset - Chat.ExtentHeight) <= double.Epsilon)
                        {
                            rtbatbottom = true;
                            justScrolledToBottom = false;
                            //Chat.ScrollToEnd();
                        }
                        else
                        {
                            var contentIsLargerThatViewport = Chat.ExtentHeight > Chat.ViewportHeight;
                            if (Math.Abs(Chat.VerticalOffset - 0) < double.Epsilon && contentIsLargerThatViewport)
                            {
                                rtbatbottom = true;
                                justScrolledToBottom = false;
                                //Chat.ScrollToEnd();
                            }
                            else
                            {
                                if (!justScrolledToBottom)
                                {
                                    var missed = new MissedMessagesBreak();
                                    ChatRowGroup.Rows.Add(missed);
                                    justScrolledToBottom = true;
                                }
                            }
                        }

                        //if (Math.Abs(dVer - 0) > double.Epsilon)
                        //{
                        //    if (Math.Abs(dVer + dViewport - dExtent) < double.Epsilon)
                        //    {
                        //        rtbatbottom = true;
                        //        justScrolledToBottom = false;
                        //    }
                        //    else
                        //    {
                        //        if (!justScrolledToBottom)
                        //        {
                        //            var missed = new MissedMessagesBreak();
                        //            ChatRowGroup.Rows.Add(missed);
                        //            justScrolledToBottom = true;
                        //        }
                        //    }
                        //}

                        if (
                            ChatRowGroup.Rows.OfType<ChatTableRow>()
                                        .Any(x => x.Id != null && x.Id.Equals(theId,StringComparison.InvariantCultureIgnoreCase))) return;
                        var ctr = new ChatTableRow(theFrom,theId, theMessage, therTime, themType);
                        //var ctr = new ChatTableRow { User = theFrom, Message = theMessage, MessageDate = therTime, MessageType = themType };

                        ctr.OnMouseUsernameEnter += ChatTableRow_MouseEnter;
                        ctr.OnMouseUsernameLeave += ChatTableRow_MouseLeave;
                        ChatRowGroup.Rows.Add(ctr);
                        if (rtbatbottom)
                        {
                            Chat.ScrollToEnd();
                        }
                        if (ChatRowGroup.Rows.Count > Prefs.MaxChatHistory)
                        {
                            var remlist =
                                ChatRowGroup.Rows.Take(ChatRowGroup.Rows.Count - Prefs.MaxChatHistory).ToArray();
                            foreach (var r in remlist)
                            {
                                ChatRowGroup.Rows.Remove(r);
                            }
                        }
                    }));
        }

        private void ChatTableRow_MouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            Log.Info("MouseLeave");
            foreach (var r in ChatRowGroup.Rows.OfType<ChatTableRow>())
            {
                r.IsHighlighted = false;
                //r.Background = null;
            }
        }

        private void ChatTableRow_MouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            Log.Info("MouseEnter");
            var theFrom = sender as ChatTableRow;
            if (theFrom == null) return;
            foreach (var r in ChatRowGroup.Rows.OfType<ChatTableRow>())
            {
                if (r.User.UserName == theFrom.User.UserName)
                {
                    r.IsHighlighted = true;
                    //r.Background = this.HoverBackBrush;
                }
            }
        }

        #region Users

        /// <summary>
        /// Ticks and refreshes the user list if needed
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        private void OnRefreshTimerTick(object state)
        {
            this.InvokeRoomUserList();
            this.InvokeFriendList();
        }

        private void InvokeRoomUserList()
        {
            if (this.room == null) return;

            var filterText = "";
            Dispatcher.Invoke(new Func<string>(() => filterText = this.UserFilter.Text.ToLower()));

            var rar = this.room.Users.ToArray();

            var roomUserList = rar
                .Where(x => x.UserName.ToLower().Contains(filterText))
                .ToArray();
            Dispatcher.BeginInvoke(new Action(() => 
                this.ResetUserList(rar, roomUserList, UserListItems, x => new ChatUserListItem(Room, x), x => x.User.IsSubbed,UserContextMenu)));
        }

        private void InvokeFriendList()
        {
            var filterText = "";
            Dispatcher.Invoke(new Func<string>(() => filterText = this.UserFilter.Text.ToLower()));

            var fla = Program.LobbyClient.Friends.ToArray();
            var friendList = fla.Where(x => x.UserName.ToLower().Contains(filterText)).ToArray();

            Dispatcher.BeginInvoke(new Action(() => 
                this.ResetUserList(fla, friendList, FriendListItems, x => new FriendListItem(x),x=>true,FriendContextMenu)));
        }


        private readonly object resestLocker = new object();

        private bool isOffline = true;

        /// <summary>
        /// Resets the user list visually and internally. Must be called on UI thread.
        /// </summary>
        private void ResetUserList<T>(User[] fullList, User[] filteredList
            , OrderedObservableCollection<T> userItems ,Func<User,T> create 
            , Func<T,bool> resetCondition,
            ContextMenu conMenu) where T : UserListItem
        {
            lock (resestLocker)
            {
                //Add all users that should exist
                foreach (var u in fullList)
                {
                    if (userItems.All(x => x.User != u))
                        userItems.Add(create.Invoke(u));
                }

                // remove any users that aren't on the fullList
                foreach (var u in userItems.ToArray())
                {
                    if (!fullList.Contains(u.User))
                    {
                        userItems.Remove(u);
                        u.ContextMenu = null;
                        u.Dispose();
                    }
                }

                // Remove any null users
                foreach (var u in userItems.Where(x => x == null || x.User == null).ToArray())
                {
                    userItems.Remove(u);
                    if (u != null)
                    {
                        u.ContextMenu = null;
                        u.Dispose();
                    }
                }

                // Remove and re add subbed users
                foreach (var u in userItems.Where(resetCondition).ToArray())
                {
                    var u2 = create.Invoke(u.User);
                    //var u2 = new ChatUserListItem(Room, u.User);

                    userItems.Remove(u);
                    u.ContextMenu = null;
                    u.Dispose();
                    userItems.Add(u2);
                }

                // Show all users that should be shown
                for (var i = 0; i < userItems.Count; i++)
                {
                    if (!filteredList.Contains(userItems[i].User))
                    {
                        //UserListItems[i].Visibility = Visibility.Collapsed;
                        userItems[i].Hide();
                    }
                    else
                    {
                        //UserListItems[i].Visibility = Visibility.Visible;
                        userItems[i].Show();
                    }
                }

                foreach (var u in userItems)
                {
                    if (u.ContextMenu == null)
                        u.ContextMenu = conMenu;
                }

                OnPropertyChanged("IsAdmin");
                OnPropertyChanged("IsModerator");
                OnPropertyChanged("BanMenuVisible");
            }
        }

        /// <summary>
        /// Happens when you change the text of the User Filter text box.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// Event Arguments
        /// </param>
        private void UserFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            this.InvokeRoomUserList();
            this.InvokeFriendList();
        }

        #endregion Users

        #region ChatInput

        /// <summary>
        /// Happens when a key goes up in the chat text box.
        /// </summary>
        /// <param name="sender">The Sender</param>
        /// <param name="e">Event Arguments</param>
        private void ChatInputPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (this.room == null)
            {
                return;
            }

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                this.shiftDown = false;
            }
        }

        /// <summary>
        /// Happens when a key goes down in the chat text box.
        /// </summary>
        /// <param name="sender">The Sender</param>
        /// <param name="e">Event Arguments</param>
        private void ChatInputPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.room == null)
            {
                return;
            }

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                this.shiftDown = true;
            }

            if (!this.shiftDown && (e.Key == Key.Return || e.Key == Key.Enter))
            {
                this.messageCache.Add(ChatInput.Text);
                if (this.messageCache.Count >= 51)
                {
                    this.messageCache.Remove(this.messageCache.Last());
                }

                this.room.SendMessage(ChatInput.Text);
                ChatInput.Clear();
                this.curMessageCacheItem = -1;
                e.Handled = true;
            }
            else if(String.IsNullOrWhiteSpace(ChatInput.Text))
            {
                switch (e.Key)
                {
                    case Key.Up:
                        if (this.messageCache.Count == 0)
                        {
                            return;
                        }

                        if (this.curMessageCacheItem - 1 >= 0)
                        {
                            this.curMessageCacheItem--;
                        }
                        else
                        {
                            this.curMessageCacheItem = this.messageCache.Count - 1;
                        }

                        this.ChatInput.Text = this.messageCache[this.curMessageCacheItem];
                        break;
                    case Key.Down:
                        if (this.messageCache.Count == 0)
                        {
                            return;
                        }

                        if (this.curMessageCacheItem + 1 <= this.messageCache.Count - 1)
                        {
                            this.curMessageCacheItem++;
                        }
                        else
                        {
                            this.curMessageCacheItem = 0;
                        }

                        this.ChatInput.Text = this.messageCache[this.curMessageCacheItem];
                        break;
                }
            }
        }

        private void ChatInput_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(ChatInput.Text)) ShowChatInputHint = true;
            else ShowChatInputHint = false;
        }

        #endregion ChatInput

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged

        public void Dispose()
        {
            Program.OnOptionsChanged -= this.ProgramOnOnOptionsChanged;
            Program.LobbyClient.OnDataReceived -= LobbyClientOnDataReceived;
            Program.LobbyClient.OnLoginComplete -= LobbyClientOnOnLoginComplete;
            Program.LobbyClient.OnDisconnect -= LobbyClientOnOnDisconnect;
            this.Loaded -= OnLoaded;
            this.userRefreshTimer.Dispose();
            whisperContextMenuItem.Click -= this.WhisperOnClick;
            addFriendContextMenuItem.Click -= this.AddFriendOnClick;
            banContextMenuItem.Click -= this.BanOnClick;
            profileContextMenuItem.Click -= this.ProfileOnClick;
            removeFriendContextMenuItem.Click -= this.RemoveFriendOnClick;
            friendProfileContextMenuItem.Click -= this.ProfileOnClick;
            friendWhisperContextMenuItem.Click -= this.WhisperOnClick;
            if (this.room != null)
            {
                this.room.OnMessageReceived -= this.RoomOnMessageReceived;
                this.room.OnUserListChange -= RoomOnOnUserListChange;
                this.room.OnStateChanged -= this.RoomOnOnStateChanged;
            }
        }
    }
}
