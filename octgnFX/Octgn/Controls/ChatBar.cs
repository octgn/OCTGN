// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatBar.cs" company="OCTGN">
//   GNU Whatever
// </copyright>
// <summary>
//   Defines the ChatBar type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Octgn.Controls
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Octgn.Extentions;

    using Skylabs.Lobby;

    /// <summary>
    /// The chat bar. 
    /// </summary>
    public class ChatBar : TabControl
    {
        /// <summary>
        /// The bar height.
        /// </summary>
        private GridLength barHeight = new GridLength(1, GridUnitType.Auto);

        /// <summary>
        /// The current tab selection.
        /// </summary>
        private object currentTabSelection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatBar"/> class.
        /// </summary>
        public ChatBar()
        {
            this.TabStripPlacement = Dock.Bottom;
            this.Items.Add(new TabItem { Visibility = Visibility.Collapsed });
            this.currentTabSelection = this.Items[0];
            if (!this.IsInDesignMode())
            {
                this.Loaded +=
                    (sender, args) => Program.LobbyClient.Chatting.OnCreateRoom += this.LobbyCreateRoom;
            }
        }

        /// <summary>
        /// Gets or sets the height of the bar.
        /// </summary>
        public GridLength BarHeight
        {
            get
            {
                return this.barHeight;
            }

            set
            {
                this.barHeight = value;
                this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (var cb in this.Items.OfType<ChatBarItem>())
                        {
                            cb.Height = this.barHeight.Value;
                        }
                    }));
            }
        }

        /// <summary>
        /// If any private chat windows are open, minimize them.
        /// </summary>
        public void HideChat()
        {
            this.SelectedIndex = 0;
            this.InvalidateVisual();
            this.currentTabSelection = this.Items[0];
        }

        /// <summary>
        /// This happens when a new room is created.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="room">
        /// The room.
        /// </param>
        private void LobbyCreateRoom(object sender, NewChatRoom room)
        {
            var r = room;
            this.Dispatcher.Invoke(new Action(() =>
                {
                    var chatBarItem = new ChatBarItem(r) { Height = this.barHeight.Value };
                    chatBarItem.HeaderMouseUp += ChatBarItemOnPreviewMouseUp;
                    this.Items.Add(chatBarItem);
                    if (room.GroupUser != null && room.GroupUser.UserName.ToLowerInvariant() == "lobby")
                    {
                        return;
                    }

                    this.SelectedItem = chatBarItem;
                }));
        }

        /// <summary>
        /// The chat bar item on preview mouse up.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="mouseButtonEventArgs">
        /// The mouse button event arguments.
        /// </param>
        private void ChatBarItemOnPreviewMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (this.currentTabSelection is ChatBarItem && sender == this.currentTabSelection)
            {
                this.SelectedIndex = 0;
                this.InvalidateVisual();
                this.currentTabSelection = this.Items[0];
            }
            else
            {
                this.currentTabSelection = sender;
            }
        }
    }
}
