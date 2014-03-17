/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Exceptionless.Json;
using Exceptionless.Json.Linq;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew.Entities;

namespace Octgn.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using CodeBits;

    using Octgn.Annotations;
    using Octgn.Controls.ControlTemplates;
    using Octgn.Core;
    using Octgn.Extentions;
    using Octgn.Utils;
    using Octgn.Windows;

    using Skylabs.Lobby;
    using log4net;

    using Timer = System.Threading.Timer;
    using System.Drawing;

    /// <summary>
    /// Interaction logic for ChatControl
    /// </summary>
    public partial class ChatControl : UserControl, INotifyPropertyChanged, IDisposable
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
        private bool shouldDisplayMissedMessagesBreak;

        private bool isLightTheme;

        private bool showChatInputHint = true;

        private ChatRoomState roomState;

        private System.Timers.Timer ScrollDownTimer;

        #endregion Privates

        public OrderedObservableCollection<ChatUserListItem> UserListItems { get; set; }

        public OrderedObservableCollection<FriendListItem> FriendListItems { get; set; }

        public OrderedObservableCollection<DescriptionItem<string>> AutoCompleteCollection { get; set; }

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

        public double ChatFontSize
        {
            get
            {
                var ret = Prefs.ChatFontSize;
                return ret;
            }
        }

        public ContextMenu UserContextMenu { get; set; }

        public ContextMenu FriendContextMenu { get; set; }

        public bool AutoCompleteVisible
        {
            get { return _autoCompleteVisible; }
            set
            {
                if (value == _autoCompleteVisible) return;
                _autoCompleteVisible = value;
                OnPropertyChanged("AutoCompleteVisible");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatControl"/> class.
        /// </summary>
        public ChatControl()
        {
            this.AutoCompleteCollection = new OrderedObservableCollection<DescriptionItem<string>>();
            AutoCompleteCollection.CollectionChanged += AutoCompleteCollectionOnCollectionChanged;
            this.UserListItems = new OrderedObservableCollection<ChatUserListItem>();
            this.FriendListItems = new OrderedObservableCollection<FriendListItem>();
            if (!this.IsInDesignMode())
            {
                if (Program.LobbyClient != null && Program.LobbyClient.IsConnected)
                {
                    IsOffline = false;
                }
                //ScrollDownTimer = new System.Timers.Timer(150);
                //ScrollDownTimer.Elapsed += ScrollDownTimerOnElapsed;
                //ScrollDownTimer.Start();
            }
            this.InitializeComponent();
            Chat.TextChanged += delegate
            {
                if (keepScrolledToBottom)
                {
                    //Dispatcher.BeginInvoke(new Action(() =>
                    //{
                    this.Chat.InvalidateMeasure();
                    this.Chat.InvalidateVisual();
                    this.Chat.ScrollToEnd();
                    //}));
                }
            };
            this.messageCache = new List<string>();
            this.DataContext = UserListItems;

            if (this.IsInDesignMode())
            {
                return;
            }
            this.shouldDisplayMissedMessagesBreak = true;
            this.CreateUserContextMenu();
            Program.OnOptionsChanged += ProgramOnOnOptionsChanged;
            Program.LobbyClient.OnDataReceived += LobbyClientOnDataReceived;
            Program.LobbyClient.OnLoginComplete += LobbyClientOnOnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClientOnOnDisconnect;
            this.userRefreshTimer = new Timer(this.OnRefreshTimerTick, this, 100, 10000);
            this.Loaded += OnLoaded;
        }

        private void ScrollDownTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (keepScrolledToBottom)
            {
                this.Chat.InvalidateMeasure();
                this.Chat.InvalidateVisual();
                this.Chat.ScrollToEnd();
            }
        }

        private void AutoCompleteCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            AutoCompleteVisible = AutoCompleteCollection.Count != 0;
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
                //this.InvokeFriendList();
            }
        }

        private MenuItem whisperContextMenuItem;
        private MenuItem profileContextMenuItem;
        private MenuItem addFriendContextMenuItem;
        private MenuItem friendWhisperContextMenuItem;
        private MenuItem friendProfileContextMenuItem;
        private MenuItem removeFriendContextMenuItem;
        private MenuItem inviteToGameContextMenuItem;
        private MenuItem friendInviteToGameContextMenuItem;

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

            profileContextMenuItem = new MenuItem();
            profileContextMenuItem.Header = "Profile";
            profileContextMenuItem.Click += ProfileOnClick;
            UserContextMenu.Items.Add(profileContextMenuItem);

            this.inviteToGameContextMenuItem = new MenuItem();
            this.inviteToGameContextMenuItem.Header = "Invite To Game";
            this.inviteToGameContextMenuItem.Click += this.InviteToGameContextOnClick;
            UserContextMenu.Items.Add(this.inviteToGameContextMenuItem);

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

            this.friendInviteToGameContextMenuItem = new MenuItem();
            this.friendInviteToGameContextMenuItem.Header = "Invite To Game";
            this.friendInviteToGameContextMenuItem.Click += this.InviteToGameContextOnClick;
            FriendContextMenu.Items.Add(this.friendInviteToGameContextMenuItem);

        }

        private void InviteToGameContextOnClick(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            if (mi == null) return;
            var cm = mi.Parent as ContextMenu;
            if (cm == null) return;
            var ui = cm.PlacementTarget as UserListItem;
            if (ui == null) return;
            //if (WindowManager.PreGameLobbyWindow == null) return;
            if (Program.GameEngine == null) return;
            if (Program.GameEngine.IsLocal) return;
            if (!Program.GameEngine.IsConnected) return;
            if (!Program.InPreGame) return;
            if (Program.LobbyClient == null) return;
            Program.LobbyClient.SendGameInvite(ui.User, Program.GameEngine.SessionId, Program.GameEngine.Password);
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
                    OnPropertyChanged("ChatFontSize");
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
            //this.InvokeRoomUserList();
        }

        private bool keepScrolledToBottom = true;

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
        private void RoomOnMessageReceived(object sender, string id, User @from, string message, DateTime receiveTime, LobbyMessageType messageType)
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

            if (theMessage.ToLowerInvariant().Contains(Program.LobbyClient.Me.UserName.ToLowerInvariant()))
            {
                Sounds.PlayMessageSound();
            }

            Dispatcher.Invoke(
                new Action(
                    () =>
                    {
                        // Got mostly from http://stackoverflow.com/questions/13456441/wpf-richtextboxs-conditionally-scroll
                        keepScrolledToBottom = false;

                        var offset = Chat.VerticalOffset + Chat.ViewportHeight;
                        // How far the scroll bar is up from the bottom
                        if (Math.Abs(offset - Chat.ExtentHeight) <= 15)
                        {
                            // We should auto scroll 
                            keepScrolledToBottom = true;
                            this.shouldDisplayMissedMessagesBreak = false;
                            //Chat.ScrollToEnd();
                        }
                        else
                        {
                            var contentIsLargerThatViewport = Chat.ExtentHeight > Chat.ViewportHeight;
                            // How far the scroll bar is up from the bottom
                            if (Math.Abs(Chat.VerticalOffset - 0) < 15 && contentIsLargerThatViewport)
                            {
                                // We should auto scroll
                                keepScrolledToBottom = true;
                                this.shouldDisplayMissedMessagesBreak = false;
                                //Chat.ScrollToEnd();
                            }
                            else
                            {
                                // We shouldn't auto scroll, instead show the missed message thingy
                                if (!this.shouldDisplayMissedMessagesBreak)
                                {
                                    var missed = new MissedMessagesBreak();
                                    ChatRowGroup.Rows.Add(missed);
                                    this.shouldDisplayMissedMessagesBreak = true;
                                }
                            }
                        }

                        var ctr = new ChatTableRow(theFrom, theId, theMessage, therTime, themType);
                        //var ctr = new ChatTableRow { User = theFrom, Message = theMessage, MessageDate = therTime, MessageType = themType };

                        ctr.OnMouseUsernameEnter += ChatTableRow_MouseEnter;
                        ctr.OnMouseUsernameLeave += ChatTableRow_MouseLeave;
                        ChatRowGroup.Rows.Add(ctr);
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
            Dispatcher.BeginInvoke(new Action(
                () =>
                {
                    if (keepScrolledToBottom)
                    {
                        Chat.ScrollToEnd();
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
                this.ResetUserList(rar, roomUserList, UserListItems, x => new ChatUserListItem(Room, x), x => x.User.IsSubbed, UserContextMenu)));
        }

        private void InvokeFriendList()
        {
            var filterText = "";
            Dispatcher.Invoke(new Func<string>(() => filterText = this.UserFilter.Text.ToLower()));

            var fla = Program.LobbyClient.Friends.ToArray();
            var friendList = fla.Where(x => x.UserName.ToLower().Contains(filterText)).ToArray();

            Dispatcher.BeginInvoke(new Action(() =>
                this.ResetUserList(fla, friendList, FriendListItems, x => new FriendListItem(x), x => true, FriendContextMenu)));
        }


        private readonly object resestLocker = new object();

        private bool isOffline = true;
        private bool _autoCompleteVisible;

        /// <summary>
        /// Resets the user list visually and internally. Must be called on UI thread.
        /// </summary>
        private void ResetUserList<T>(User[] fullList, User[] filteredList
            , OrderedObservableCollection<T> userItems, Func<User, T> create
            , Func<T, bool> resetCondition,
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

        private int autoCompleteStart = -1;

        private Key autoCompleteTriggerKey;


        private void AutoCompleteDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var args = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter)
                           {
                               RoutedEvent
                                   =
                                   Keyboard
                                   .KeyDownEvent
                           };
                HandleAutoComplete(args);

            }
            catch (Exception ex)
            {
                Log.Warn("AutoCompleteDoubleClick Error", ex);
            }
        }

        /// <summary>
        /// Happens when a key goes up in the chat text box.
        /// </summary>
        /// <param name="sender">The Sender</param>
        /// <param name="e">Event Arguments</param>
        private void ChatInputPreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (this.room == null)
                {
                    return;
                }

                if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                {
                    this.shiftDown = false;
                }
                if (this.AutoCompleteVisible)
                {
                    if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Escape
                        || e.Key == Key.Space) return;
                    var oldSelect = this.AutoCompleteListBox.SelectedItem;
                    var oldSpot = this.ChatInput.CaretIndex;
                    if ((this.ChatInput.Text.Length - this.autoCompleteStart) < 0)
                    {
                        this.AutoCompleteCollection.Clear();
                        return;
                    }
                    this.ChatInput.Select(this.autoCompleteStart, this.ChatInput.Text.Length - this.autoCompleteStart + 1);

                    var set = this.ChatInput.SelectedText;

                    this.ChatInput.Select(oldSpot, 0);

                    this.AutoCompleteCollection.Clear();
                    if (this.autoCompleteTriggerKey == Key.D2)
                    {
                        foreach (var i in this.UserListItems.ToArray())
                        {
                            if (i.User.UserName.StartsWith(set))
                                this.AutoCompleteCollection.Add(new DescriptionItem<string>(i.User.UserName));
                        }
                    }
                    else if (this.autoCompleteTriggerKey == Key.Oem2)
                    {
                        foreach (var i in ChatRoom.SlashCommands)
                        {
                            if (i.Key.StartsWith(set))
                                this.AutoCompleteCollection.Add(new DescriptionItem<string>(i.Key, i.Value));
                        }
                    }
                    if (this.AutoCompleteListBox.Items.Count == 0) return;
                    if (this.AutoCompleteListBox.SelectedIndex == -1)
                        this.AutoCompleteListBox.SelectedIndex = 0;
                    if (oldSelect != null) this.AutoCompleteListBox.SelectedItem = oldSelect;
                    this.AutoCompleteListBox.ScrollIntoView(this.AutoCompleteListBox.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("ChatInputPreviewKeyUp Error", ex);
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

            if (this.HandleAutoComplete(e))
                return;

            if (AutoCompleteVisible) return;

            if (!this.shiftDown && (e.Key == Key.Return || e.Key == Key.Enter))
            {
                var message = ParseCards(ChatInput.Text);
                this.messageCache.Add(message);
                if (this.messageCache.Count >= 51)
                {
                    this.messageCache.Remove(this.messageCache.Last());
                }

                this.room.SendMessage(message);
                ChatInput.Clear();
                this.curMessageCacheItem = -1;
                e.Handled = true;
            }
            else if (String.IsNullOrWhiteSpace(ChatInput.Text))
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

        private string ParseCards(string message)
        {
            var regex = new Regex(@"'([^']*)'", RegexOptions.IgnoreCase);
            var matches = regex.Matches(message);

            var strMatches = matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();

            foreach (var m in strMatches)
            {
                var tm = m.Trim('\'', ' ', '\t');
                var match = Octgn.DataNew.DbContext.Get().Cards.Where(x => x.Name.Contains(tm)).ToArray();
                if (match.Length == 0)
                    continue;

                Card card = null;

                if (match.Length == 1)
                {
                    card = match[0];
                }
                else
                {
                    var pc = new PickCardFromList();
                    var res = pc.PickCard(tm, match);
                    if (res ?? false)
                    {
                        card = pc.SelectedCard;
                    }

                }

                if (card != null)
                {
                    //message = message.Replace(m, string.Format("{{c:{0}}}",SerializeCard(card)));
                    message = message.Replace(m, string.Format("{{c:{0}:{1}:{2}:{3}}}", card.GetSet().GameId, card.SetId, card.Id, card.Name));
                }
            }

            return message;
        }

        private bool HandleAutoComplete(KeyEventArgs e)
        {
            // /
            if (e.Key == Key.Oem2 && ChatInput.CaretIndex == 0)
            {
                AutoCompleteCollection.Clear();
                autoCompleteTriggerKey = e.Key;
                foreach (var c in ChatRoom.SlashCommands)
                {
                    AutoCompleteCollection.Add(new DescriptionItem<string>(c.Key, c.Value));
                }
                autoCompleteStart = ChatInput.CaretIndex + 1;
                return true;
            }
            // @ key
            if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && e.Key == Key.D2)
            {
                AutoCompleteCollection.Clear();
                autoCompleteTriggerKey = e.Key;
                foreach (var u in this.UserListItems)
                {
                    AutoCompleteCollection.Add(new DescriptionItem<string>(u.User.UserName));
                }
                autoCompleteStart = ChatInput.CaretIndex + 1;
                return true;
            }
            if (e.Key == Key.Escape)
            {
                if (AutoCompleteVisible)
                {
                    this.AutoCompleteCollection.Clear();
                    return true;
                }
                return false;
            }
            if (e.Key == Key.Space)
            {
                this.AutoCompleteCollection.Clear();
                if (this.AutoCompleteVisible)
                {
                    if (this.autoCompleteTriggerKey == Key.D2)
                    {
                        this.ChatInput.Select(this.autoCompleteStart - 1, 1);
                        this.ChatInput.SelectedText = "";
                        this.ChatInput.Select(this.ChatInput.Text.Length, 0);
                        return true;
                    }
                }
                return false;
            }
            if (e.Key == Key.Enter)
            {
                if (this.AutoCompleteVisible)
                {
                    if (this.AutoCompleteListBox.Items.Count == 0) return true;
                    if (this.AutoCompleteListBox.SelectedIndex == -1) return true;
                    var item = (DescriptionItem<string>)this.AutoCompleteListBox.SelectedItem;
                    this.ChatInput.Select(this.autoCompleteStart, this.ChatInput.Text.Length - this.autoCompleteStart);
                    this.ChatInput.SelectedText = item.Item;
                    this.ChatInput.Select(this.ChatInput.Text.Length, 0);
                    e.Handled = true;
                    this.AutoCompleteCollection.Clear();
                    if (this.autoCompleteTriggerKey == Key.D2)
                    {
                        this.ChatInput.Select(this.autoCompleteStart - 1, 1);
                        this.ChatInput.SelectedText = "";
                        this.ChatInput.Select(this.ChatInput.Text.Length, 0);
                    }
                    return true;
                }
                return false;
            }
            if (e.Key == Key.Up)
            {
                if (this.AutoCompleteVisible)
                {
                    if (this.AutoCompleteListBox.Items.Count > 0)
                    {
                        if (this.AutoCompleteListBox.SelectedIndex - 1 < 0)
                        {
                            this.AutoCompleteListBox.SelectedIndex = this.AutoCompleteListBox.Items.Count - 1;
                        }
                        else
                        {
                            this.AutoCompleteListBox.SelectedIndex--;
                        }
                        e.Handled = true;
                        this.AutoCompleteListBox.ScrollIntoView(this.AutoCompleteListBox.SelectedItem);
                        return true;
                    }
                }
                return false;
            }
            if (e.Key == Key.Down)
            {
                if (this.AutoCompleteVisible)
                {
                    if (this.AutoCompleteListBox.Items.Count > 0)
                    {
                        if (this.AutoCompleteListBox.SelectedIndex + 1 >= this.AutoCompleteListBox.Items.Count)
                        {
                            this.AutoCompleteListBox.SelectedIndex = 0;
                        }
                        else
                        {
                            this.AutoCompleteListBox.SelectedIndex++;
                        }
                        e.Handled = true;
                        this.AutoCompleteListBox.ScrollIntoView(this.AutoCompleteListBox.SelectedItem);
                        return true;
                    }
                }
                return false;
            }
            return false;
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
            this.AutoCompleteCollection.CollectionChanged -= this.AutoCompleteCollectionOnCollectionChanged;
            whisperContextMenuItem.Click -= this.WhisperOnClick;
            addFriendContextMenuItem.Click -= this.AddFriendOnClick;
            inviteToGameContextMenuItem.Click -= this.InviteToGameContextOnClick;
            profileContextMenuItem.Click -= this.ProfileOnClick;
            removeFriendContextMenuItem.Click -= this.RemoveFriendOnClick;
            friendProfileContextMenuItem.Click -= this.ProfileOnClick;
            friendWhisperContextMenuItem.Click -= this.WhisperOnClick;
            friendInviteToGameContextMenuItem.Click -= this.InviteToGameContextOnClick;
            ScrollDownTimer.Stop();
            ScrollDownTimer.Dispose();
            if (this.room != null)
            {
                this.room.OnMessageReceived -= this.RoomOnMessageReceived;
                this.room.OnUserListChange -= RoomOnOnUserListChange;
                this.room.OnStateChanged -= this.RoomOnOnStateChanged;
            }
        }
    }

    public class DescriptionItem<T> : IComparable<DescriptionItem<T>>, INotifyPropertyChanged where T : IComparable<T>
    {
        private T item;

        private string description;

        public T Item
        {
            get
            {
                return this.item;
            }
            set
            {
                if (Equals(value, this.item))
                {
                    return;
                }
                this.item = value;
                this.OnPropertyChanged("Item");
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                if (value == this.description)
                {
                    return;
                }
                this.description = value;
                this.OnPropertyChanged("Description");
                this.OnPropertyChanged("HasDescription");
            }
        }

        public bool HasDescription
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Description);
            }
        }

        public DescriptionItem()
        {
        }

        public DescriptionItem(T item)
        {
            Item = item;
            Description = "";
        }

        public DescriptionItem(T item, string description)
        {
            Item = item;
            Description = description;
        }

        public int CompareTo(DescriptionItem<T> other)
        {
            return Item.CompareTo(other.Item);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    internal class ComparableCard : DataNew.Entities.Card, IComparable<ComparableCard>
    {
        public ComparableCard(ICard c)
        {
            this.Alternate = c.Alternate;
            this.Id = c.Id;
            this.ImageUri = c.ImageUri;
            this.Name = c.Name;
            this.Properties = c.Properties;
            this.SetId = c.SetId;
        }

        public int CompareTo(ComparableCard other)
        {
            if (other.Name == Name)
                return Id.CompareTo(other.Id);
            return System.String.Compare(Name, other.Name, System.StringComparison.Ordinal);
        }
    }
}
