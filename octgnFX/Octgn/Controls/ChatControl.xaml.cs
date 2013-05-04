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
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;

    using Octgn.Extentions;
    using Octgn.Library.Utils;

    using Skylabs.Lobby;

    /// <summary>
    /// Interaction logic for ChatControl
    /// </summary>
    public partial class ChatControl : UserControl
    {
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

        public SortableObservableCollection<ChatUserListItem> UserListItems { get; set; }


        protected Brush HoverBackBrush { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatControl"/> class.
        /// </summary>
        public ChatControl()
        {
            this.UserListItems = new SortableObservableCollection<ChatUserListItem>();
            this.InitializeComponent();
            this.messageCache = new List<string>();
            this.DataContext = UserListItems;

            if (this.IsInDesignMode())
            {
                return;
            }
            this.Loaded += (sender, args) =>
                { 
                    Program.OnOptionsChanged += ProgramOnOnOptionsChanged;
                    ProgramOnOnOptionsChanged();
                };
            this.Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Program.OnOptionsChanged -= this.ProgramOnOnOptionsChanged;
            Unloaded -= this.OnUnloaded;
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

                        var ctr = new ChatTableRow { User = theFrom, Message = theMessage, MessageDate = therTime, MessageType = themType };

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
                        if (ChatRowGroup.Rows.Count > 100)
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

            var roomUserList = this.room.Users.ToArray()
                .Where(x => x.UserName.ToLower().Contains(filterText)).ToArray();
            //foreach (var missingUser in roomUserList.Except(this.UserListItems.Select(x => x.User)).ToList())
            //{
            //    this.UserListItems.Add(missingUser);
            //}

            //foreach (var offlineUser in this.UserListItems.Select(x => x.User).Except(roomUserList).ToList())
            //{
            //    this.UserListItems.Remove(offlineUser);
            //}

            Dispatcher.BeginInvoke(
                new Action(
                    () => this.ResetUserList(
                        roomUserList.Except(this.UserListItems.Select(x => x.User)).ToArray(),
                        this.UserListItems.Select(x => x.User).Except(roomUserList).ToArray()
                         ))
                );
        }

        /// <summary>
        /// Resets the user list visually and internally. Must be called on UI thread.
        /// </summary>
        private void ResetUserList(IEnumerable<User> usersToAdd, IEnumerable<User> usersToRemove)
        {
            foreach (var u in usersToAdd) UserListItems.Add(new ChatUserListItem(this.room, u));
            foreach (var u in usersToRemove) UserListItems.Remove(new ChatUserListItem(this.room,u));
            UserListItems.Sort();
            this.needsRefresh = false;
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

            if (!this.shiftDown && e.Key == Key.Enter)
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
    }
}
