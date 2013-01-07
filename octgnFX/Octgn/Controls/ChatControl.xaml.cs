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
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;

    using Octgn.Extentions;

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
        private NewChatRoom room;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatControl"/> class.
        /// </summary>
        public ChatControl()
        {
            this.InitializeComponent();
            this.messageCache = new List<string>();
            if (this.IsInDesignMode())
            {
                return;
            }
        }

        /// <summary>
        /// Gets the room.
        /// </summary>
        protected NewChatRoom Room
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
        public void SetRoom(NewChatRoom theRoom)
        {
            this.room = theRoom;
            this.InvokeResetUserList();
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
        private void RoomOnMessageReceived(object sender, NewUser @from, string message, DateTime receiveTime, LobbyMessageType messageType)
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
                            Chat.ScrollToVerticalOffset(Chat.VerticalOffset);

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
                                        var tc = new TableCell()
                                            { BorderThickness = new Thickness(0,1,0,1), BorderBrush = Brushes.Gainsboro, ColumnSpan = 3 };
                                        tr.Cells.Add(tc);
                                        ChatRowGroup.Rows.Add(tr);
                                        justScrolledToBottom = true;
                                    }
                                }
                            }

                            var ctr = new ChatTableRow
                                { User = theFrom, Message = theMessage, MessageDate = therTime, MessageType = themType };

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
                                        r.Background = Brushes.DimGray;
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
        private void RoomOnUserListChange(object sender, List<NewUser> newusers)
        {
            this.needsRefresh = true;
        }

        /// <summary>
        /// Invoke a reset and redraw of the user list
        /// </summary>
        private void InvokeResetUserList()
        {
            Dispatcher.BeginInvoke(new Action(this.ResetUserList));
        }

        /// <summary>
        /// Resets the user list visually and internally. Must be called on UI thread.
        /// </summary>
        private void ResetUserList()
        {
            if (this.room == null)
            {
                return;
            }

            var us =
                this.room.Users.ToArray().Where(x => x.UserName.ToLower().Contains(UserFilter.Text.ToLower())).OrderBy(
                    x => x.UserName);
            UserList.Items.Clear();
            var userListItemList = new List<ChatUserListItem>();
            foreach (var u in us)
            {
                var ci = new ChatUserListItem();
                if (this.room.AdminList.Any(x => x.UserName == u.UserName))
                {
                    ci.IsAdmin = true;
                }

                if (this.room.ModeratorList.Any(x => x.UserName == u.UserName))
                {
                    ci.IsMod = true;
                }

                if (this.room.OwnerList.Any(x => x.UserName == u.UserName))
                {
                    ci.IsOwner = true;
                }

                ci.User = u;
                userListItemList.Add(ci);
            }

            foreach (
                var u in
                    userListItemList.OrderByDescending(x => x.IsOwner).ThenByDescending(x => x.IsAdmin).ThenByDescending(x => x.IsMod))
            {
                UserList.Items.Add(u);
            }

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
            this.ResetUserList();
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
