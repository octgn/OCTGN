// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatControl.xaml.cs" company="OCTGN">
//   GNU Stuff
// </copyright>
// <summary>
//   Interaction logic for ChatControl.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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

    using CodeBits;

    using Octgn.Extentions;

    using Skylabs.Lobby;

    using log4net;

    /// <summary>
    /// Interaction logic for ChatControl
    /// </summary>
    public partial class ChatControl : UserControl,INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
        /// Does the user list need to be refreshed?
        /// </summary>
        private bool needsRefresh = true;

        /// <summary>
        /// The user refresh timer.
        /// </summary>
        private Timer userRefreshTimer;

        /// <summary>
        /// Just scrolled to bottom.
        /// </summary>
        private bool justScrolledToBottom;

        private bool showChatInputHint = true;

        public OrderedObservableCollection<ChatUserListItem> UserListItems { get; set; }

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

        protected Brush HoverBackBrush { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatControl"/> class.
        /// </summary>
        public ChatControl()
        {
            this.UserListItems = new OrderedObservableCollection<ChatUserListItem>();
            this.InitializeComponent();
            this.messageCache = new List<string>();
            this.DataContext = UserListItems;

            if (this.IsInDesignMode())
            {
                return;
            }
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, EventArgs eventArgs)
        {
            this.Loaded -= OnLoaded;
            Program.OnOptionsChanged += ProgramOnOnOptionsChanged;
            Program.LobbyClient.OnDataReceived += LobbyClientOnOnDataReceived;
            ProgramOnOnOptionsChanged();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Program.OnOptionsChanged -= this.ProgramOnOnOptionsChanged;
            Program.LobbyClient.OnDataReceived -= this.LobbyClientOnOnDataReceived;
            Unloaded -= this.OnUnloaded;
        }

        private void LobbyClientOnOnDataReceived(object sender, DataRecType type, object data)
        {
            if (type == DataRecType.UserSubChanged)
            {
                needsRefresh = true;
                //InvokeResetUserList();
            }
        }

        private void ProgramOnOnOptionsChanged()
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    SolidColorBrush backr = null;
                    SolidColorBrush fontr = null;
                    if (Prefs.UseLightChat)
                    {
                        backr = this.FindResource("LightChatBackBrush") as SolidColorBrush;
                        fontr = this.FindResource("LightChatFontBrush") as SolidColorBrush;
                    }
                    else
                    {
                        backr = this.FindResource("DarkChatBackBrush") as SolidColorBrush;
                        fontr = this.FindResource("DarkChatFontBrush") as SolidColorBrush;
                    }
                    Chat.Background = backr;
                    Chat.Foreground = fontr;
                    this.HoverBackBrush = Prefs.UseLightChat ? Brushes.AliceBlue : Brushes.DimGray;
                    this.InvalidateVisual();
                }));
        }

        /// <summary>
        /// Gets the room.
        /// </summary>
        protected ChatRoom Room
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
            this.room = theRoom;
            this.room.OnUserListChange += this.RoomOnUserListChange;
            this.room.OnMessageReceived += this.RoomOnMessageReceived;
            this.userRefreshTimer = new Timer(this.OnRefreshTimerTick, this, 5000, 1000);
        }

        /// <summary>
        /// Ticks and refreshes the user list if needed
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        private void OnRefreshTimerTick(object state)
        {
            if (this.needsRefresh)
            {
                this.InvokeResetUserList();
            }
        }

        /// <summary>
        /// When a message is received in the room.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
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
        private void RoomOnMessageReceived(object sender, User @from, string message, DateTime receiveTime, LobbyMessageType messageType)
        {
            var theFrom = from;
            var theMessage = message;
            var therTime = receiveTime;
            var themType = messageType;
            if (string.IsNullOrWhiteSpace(theFrom.UserName))
            {
                theFrom.UserName = "SYSTEM";
            }

            Dispatcher.Invoke(
                new Action(
                    () =>
                    {
                        var rtbatbottom = false;

                        // bool firstAutoScroll = true; // never used 
                        //Chat.ScrollToVerticalOffset(Chat.VerticalOffset);

                        // check to see if the richtextbox is scrolled to the bottom.
                        var dVer = Chat.VerticalOffset;

                        // get the vertical size of the scrollable content area
                        var dViewport = Chat.ViewportHeight;

                        // get the vertical size of the visible content area
                        var dExtent = Chat.ExtentHeight;

                        if (Math.Abs(dVer - 0) < double.Epsilon && dViewport < dExtent)
                        {
                            rtbatbottom = true;
                        }

                        if (Math.Abs(dVer - 0) > double.Epsilon)
                        {
                            if (Math.Abs(dVer + dViewport - dExtent) < double.Epsilon)
                            {
                                rtbatbottom = true;
                                justScrolledToBottom = false;
                            }
                            else
                            {
                                if (!justScrolledToBottom)
                                {
                                    var tr = new TableRow();
                                    var tc = new TableCell() { BorderThickness = new Thickness(0, 1, 0, 1), BorderBrush = Brushes.Gainsboro, ColumnSpan = 3 };
                                    tr.Cells.Add(tc);
                                    ChatRowGroup.Rows.Add(tr);
                                    justScrolledToBottom = true;
                                }
                            }
                        }

                        var ctr = new ChatTableRow(theFrom, theMessage, therTime, themType);
                        //var ctr = new ChatTableRow { User = theFrom, Message = theMessage, MessageDate = therTime, MessageType = themType };

                        ctr.MouseEnter += (o, args) =>
                        {
                            foreach (var r in ChatRowGroup.Rows)
                            {
                                var rr = r as ChatTableRow;
                                if (rr == null)
                                {
                                    continue;
                                }

                                if (rr.User.UserName
                                    == theFrom.UserName)
                                {
                                    r.Background = this.HoverBackBrush;
                                }
                            }
                        };
                        ctr.MouseLeave += (o, args) =>
                        {
                            foreach (var r in ChatRowGroup.Rows)
                            {
                                r.Background = null;
                            }
                        };
                        ChatRowGroup.Rows.Add(ctr);
                        if (rtbatbottom)
                        {
                            Chat.ScrollToEnd();
                        }
                        if (ChatRowGroup.Rows.Count > Prefs.MaxChatHistory)
                        {
                            var remlist =
                                ChatRowGroup.Rows.Where(x=>x is ChatTableRow).Cast<ChatTableRow>()
                                            .OrderBy(x => x.MessageDate)
                                            .Take(ChatRowGroup.Rows.Count - 50).ToArray();
                            foreach (var r in remlist)
                            {
                                ChatRowGroup.Rows.Remove(r);
                            }
                        }
                    }));
        }

        /// <summary>
        /// When the rooms user list changes
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="newusers">
        /// The new list of users
        /// </param>
        private void RoomOnUserListChange(object sender, List<User> newusers)
        {
            this.needsRefresh = true;
            this.userRefreshTimer.Change(500, 1000);
        }

        /// <summary>
        /// Invoke a reset and redraw of the user list
        /// </summary>
        private void InvokeResetUserList()
        {
            if (this.room == null) return;

            var filterText = "";
            Dispatcher.Invoke(new Func<string>(() => filterText = this.UserFilter.Text.ToLower()));

            var roomUserList = this.room.Users
                .ToArray()
                .Where(x => x.UserName.ToLower().Contains(filterText))
                .ToArray();

            Dispatcher.BeginInvoke(new Action(() => this.ResetUserList(this.room.Users.ToArray(),roomUserList)));
        }

        private object resestLocker = new object();

        /// <summary>
        /// Resets the user list visually and internally. Must be called on UI thread.
        /// </summary>
        private void ResetUserList(User[] fullList, User[] filteredList)
        {
            lock (resestLocker)
            {
                //Add all users that should exist
                foreach (var u in fullList)
                {
                    if (this.UserListItems.All(x => x.User != u)) 
                        UserListItems.Add(new ChatUserListItem(this.Room, u));
                }

                // remove any users that aren't on the fullList
                foreach (var u in UserListItems.ToArray())
                {
                    if (!fullList.Contains(u.User)) UserListItems.Remove(u);
                }

                // Remove and re add subbed users
                var tlist = new OrderedObservableCollection<ChatUserListItem>();
                foreach(var i in UserListItems)
                    tlist.Add(i);
                foreach (var u in UserListItems.Where(x => x.User.IsSubbed).ToArray())
                {
                    var u2 = new ChatUserListItem(Room, u.User);
                    tlist.Remove(u);
                    tlist.Add(u2);

                    if (tlist.IndexOf(u2) == UserListItems.IndexOf(u)) continue;

                    UserListItems.Remove(u);
                    UserListItems.Add(u2);
                }

                // Show all users that should be shown
                for (var i = 0; i < UserListItems.Count; i++)
                {
                    if (!filteredList.Contains(UserListItems[i].User))
                    {
                        //UserListItems[i].Visibility = Visibility.Collapsed;
                        UserListItems[i].Hide();
                    }
                    else
                    {
                        //UserListItems[i].Visibility = Visibility.Visible;
                        UserListItems[i].Show();
                    }
                }

                this.needsRefresh = false;
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
            this.InvokeResetUserList();
        }

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
            else
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ChatInput_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(ChatInput.Text)) ShowChatInputHint = true;
            else ShowChatInputHint = false;
        }

    }
}
