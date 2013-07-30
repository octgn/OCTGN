/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octgn.Controls
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Media.Imaging;

    using Octgn.Extentions;
    using Octgn.Utils;

    using Skylabs.Lobby;

    using Uri = System.Uri;

    /// <summary>
    /// The chat bar item.
    /// </summary>
    public class ChatBarItem : TabItem,IDisposable
    {
        /// <summary>
        /// Sets the Chat Room
        /// </summary>
        public readonly ChatRoom Room;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatBarItem"/> class.
        /// </summary>
        /// <param name="chatRoom">
        /// The chat Room.
        /// </param>
        public ChatBarItem(ChatRoom chatRoom = null)
        {
            this.Room = chatRoom;
            this.ConstructControl();
        }

        public ChatBarItem(ChatControl control)
        {
            Room = control.Room;
            chatControl = control;
            this.ConstructControl();
        }

        /// <summary>
        /// Happens when you mouse up on the tab header.
        /// </summary>
        public event MouseButtonEventHandler HeaderMouseUp;

        private Border mainBorder;

        public void SetAlert()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(SetAlert));
                return;
            }
            this.Effect = new DropShadowEffect()
                                         {
                                             BlurRadius = 5,
                                             Color = Colors.Red,
                                             Opacity = 1,
                                             RenderingBias = RenderingBias.Quality,
                                             ShadowDepth = 0
                                         };
            if (!WindowManager.Main.IsActive)
            {
                WindowManager.Main.FlashWindow();
            }
            Sounds.PlayMessageSound();
        }

        public void ClearAlert()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(ClearAlert));
                return;
            }
            this.Effect = null;
        }

        /// <summary>
        /// Constructs the control.
        /// </summary>
        private void ConstructControl()
        {
            this.Style = (Style)Application.Current.FindResource(typeof(TabItem));
            this.Padding = new Thickness(0);

            this.ConstructHeader();
            this.ConstructChat();
        }

        /// <summary>
        /// Fires the Header Mouse Up event
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        private void FireHeaderMouseUp(MouseButtonEventArgs args)
        {
            if (this.HeaderMouseUp != null)
            {
                this.HeaderMouseUp.Invoke(this, args);
            }
        }

        /// <summary>
        /// Constructs the header
        /// </summary>
        private void ConstructHeader()
        {
            // Main content object
            mainBorder = new Border { Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center ,BorderBrush = Brushes.White};
            
            // Main content grid
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(21) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(21) });

            // Create close button
            var borderClose = new Border { Width = 16, Height = 16, HorizontalAlignment = HorizontalAlignment.Right };
            var imageClose = new Image()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/closewindow.png")), 
                    Stretch = Stretch.None, 
                    VerticalAlignment = VerticalAlignment.Stretch, 
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
            borderClose.Cursor = Cursors.Hand;

            // Create undock button
            var borderUndock = new Border { Width = 16, Height = 16, HorizontalAlignment = HorizontalAlignment.Right };
            var imageUndock = new Image()
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/minmax.png")),
                Stretch = Stretch.None,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            borderUndock.Effect = new DropShadowEffect() { BlurRadius = 3, Opacity = 1, Color = Colors.Black, ShadowDepth = 0};
            borderUndock.Cursor = Cursors.Hand;


            // Create item label
            var label = new TextBlock() { VerticalAlignment = VerticalAlignment.Center };
            if (this.IsInDesignMode() || this.Room == null)
            {
                label.Inlines.Add(new Run("test"));
            }
            else
            {
                if (this.Room.GroupUser != null)
                {
                    label.Inlines.Add(new Run(this.Room.GroupUser.UserName));

                    // Lobby should never be able to be silenced.
                    if (this.Room.GroupUser.UserName.ToLowerInvariant() == "lobby")
                    {
                        borderClose.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    var otherUser = this.Room.Users.FirstOrDefault(x => x.UserName.ToLowerInvariant() != Program.LobbyClient.Me.UserName.ToLowerInvariant());
                    if (
                        otherUser != null)
                    {
                        label.Inlines.Add(new Run(otherUser.UserName));
                    }
                }

                borderClose.MouseLeftButtonUp += this.BorderCloseOnMouseLeftButtonUp;
                borderUndock.MouseLeftButtonUp += BorderUndockOnMouseLeftButtonUp;
                this.Room.OnMessageReceived += (sender, id,@from, message, time, type) => this.Dispatcher.BeginInvoke(
                    new Action(
                               () =>
                                   {
                                       if (this.Visibility == Visibility.Collapsed)
                                       {
                                           this.Visibility = Visibility.Visible;
                                       }
                                   }));
            }

            // --Add items to items

            // Add undock 'button' to grid
            borderUndock.Child = imageUndock;
            g.Children.Add(borderUndock);
            Grid.SetColumn(borderUndock,1);

            // Add Close 'button' to grid
            borderClose.Child = imageClose;
            g.Children.Add(borderClose);
            Grid.SetColumn(borderClose, 2);

            // Add label to main grid
            g.Children.Add(label);

            // Add main grid to main border
            mainBorder.Child = g;

            // Add main grid to this
            this.Header = mainBorder;
            mainBorder.PreviewMouseUp += this.MainBorderOnPreviewMouseUp;
        }

        private void BorderUndockOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var cc = chatControl;
            (cc.Parent as Border).Child = null;
            (this.Parent as ChatBar).Items.Remove(this);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => ChatManager.Get().MoveToWindow(cc)));
        }

        /// <summary>
        /// This fires when the 'x' button is clicked to close a chat.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="mouseButtonEventArgs">
        /// The mouse button event arguments.
        /// </param>
        private void BorderCloseOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Program.LobbyClient.Chatting.LeaveRoom(this.Room);
            this.Visibility = Visibility.Collapsed;
            mouseButtonEventArgs.Handled = true;

            // We do this because if we don't, it will cause the tab item to hide, but the chat piece will show up.
            var chatBar = this.Parent as ChatBar;
            if (chatBar != null)
            {
                this.HeaderMouseUp -= chatBar.ChatBarItemOnPreviewMouseUp;
                chatBar.SelectedIndex = 0;
                chatBar.Items.Remove(this);
            }
        }

        /// <summary>
        /// Happens when the mouse goes up over the header.
        /// </summary>
        /// <param name="sender">The header</param>
        /// <param name="mouseButtonEventArgs">Mouse Arguments</param>
        private void MainBorderOnPreviewMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            this.FireHeaderMouseUp(mouseButtonEventArgs);
        }

        private ChatControl chatControl;

        /// <summary>
        /// Constructs the chat portion of the control.
        /// </summary>
        private void ConstructChat()
        {
            var chatBorder = new Border()
                {
                    BorderBrush = Brushes.DarkGray,
                    Background = Brushes.DimGray,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(3),
                    Width = 603,
                    Height = 253,
                    MaxWidth = 603,
                    MaxHeight = 253,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
            if (chatControl == null)
                chatControl = new ChatControl {Width = 600, Height = 250};
            else
            {
                chatControl.Width = 600;
                chatControl.Height = 250;
            }
            
            chatBorder.Child = chatControl;

            this.Content = chatBorder;

            if (this.Room != null && chatControl.Room == null)
            {
                chatControl.SetRoom(this.Room);
            }
        }

        public void Dispose()
        {
            chatControl.Dispose();
        }
    }
}
