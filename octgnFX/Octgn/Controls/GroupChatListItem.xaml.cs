using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Skylabs;
using Skylabs.Lobby;
using SimplestDragDrop;
using System.Windows.Documents;
using System;
using System.Linq;
using System.IO;
using System.Windows.Shapes;

namespace Octgn.Controls
{
    /// <summary>
    /// Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class GroupChatListItem : UserControl
    {
        private bool _isDragging;
        public bool IsDragging
        {
            get { return _isDragging; }
            set { _isDragging = value; }
        } 
        FrameworkElement _dragScope;
        public FrameworkElement DragScope
        {
            get { return _dragScope; }
            set { _dragScope = value; }
        }
        public static DependencyProperty UsernameProperty = DependencyProperty.Register(
    "UserName", typeof(string), typeof(GroupChatListItem));
        public static DependencyProperty CustomStatusProperty = DependencyProperty.Register(
    "CustomStatus", typeof(string), typeof(GroupChatListItem));
        public static DependencyProperty PictureProperty = DependencyProperty.Register(
    "Picture", typeof(ImageSource), typeof(GroupChatListItem));
        public static DependencyProperty StatusPictureProperty = DependencyProperty.Register(
    "StatusPicture", typeof(ImageSource), typeof(GroupChatListItem));


        private long ChatRoomID = 0;

        public ChatRoom ThisRoom
        {
            get
            {
                return Program.lobbyClient.Chatting.GetChatRoomFromRID(ChatRoomID);
            }
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
                            users.Substring(0, 97);
                        users += "...";
                        SetValue(UsernameProperty, users);
                    }
                }
            }
        }

        public GroupChatListItem()
        {
            InitializeComponent();
            ThisRoom = new ChatRoom(0);
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
            if(ChatRoomID != 0)
            {
                Program.ChatWindows.FirstOrDefault(cw => cw.ID == ThisRoom.ID).CloseChatWindow();
                StackPanel sp = Parent as StackPanel;
                if(sp != null)
                    sp.Children.Remove(this);
            }
        }
    }
}