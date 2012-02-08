﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Skylabs.Lobby;

namespace Octgn.Controls
{
    /// <summary>
    ///   Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class GroupChatListItem
    {
        public static DependencyProperty UsernameProperty = DependencyProperty.Register(
            "UserName", typeof (string), typeof (GroupChatListItem));

        public static DependencyProperty CustomStatusProperty = DependencyProperty.Register(
            "CustomStatus", typeof (string), typeof (GroupChatListItem));

        public static DependencyProperty PictureProperty = DependencyProperty.Register(
            "Picture", typeof (ImageSource), typeof (GroupChatListItem));

        public static DependencyProperty StatusPictureProperty = DependencyProperty.Register(
            "StatusPicture", typeof (ImageSource), typeof (GroupChatListItem));


        private long ChatRoomID;

        public GroupChatListItem()
        {
            InitializeComponent();
            ThisRoom = new ChatRoom(0);
        }

        public bool IsDragging { get; set; }

        public FrameworkElement DragScope { get; set; }

        public ChatRoom ThisRoom
        {
            get { return Program.lobbyClient.Chatting.GetChatRoomFromRID(ChatRoomID); }
            set
            {
                ChatRoomID = value.ID;
                ChatRoom cr = Program.lobbyClient.Chatting.GetChatRoomFromRID(ChatRoomID);
                if (cr != null)
                {
                    if (cr.ID == 0)
                    {
                        SetValue(UsernameProperty, "Lobby Chat");
                        image1.Opacity = 0;
                    }
                    else
                    {
                        image1.Opacity = 1;
                        String users = String.Join<User>(",", cr.GetUserList());
                        if (users.Length > 100)
                            users = users.Substring(0, 97);
                        users += "...";
                        SetValue(UsernameProperty, users);
                    }
                }
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Focus();
        }

        private void flistitem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Focus();
        }

        private void image1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ChatRoomID != 0)
            {
                var firstOrDefault = Program.ChatWindows.FirstOrDefault(cw => cw.ID == ThisRoom.ID);
                if (firstOrDefault != null)
                    firstOrDefault.CloseChatWindow();
                var sp = Parent as StackPanel;
                if (sp != null)
                    sp.Children.Remove(this);
            }
        }
    }
}