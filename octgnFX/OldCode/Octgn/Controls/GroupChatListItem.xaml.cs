using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Octgn.Launcher;
using Skylabs.Lobby;
using ChatWindow = Octgn.Windows.ChatWindow;

namespace Octgn.Controls
{
    /// <summary>
    ///   Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class GroupChatListItem
    {
        private bool _isRemoving = false;

        public static DependencyProperty UsernameProperty = DependencyProperty.Register(
            "UserName", typeof (string), typeof (GroupChatListItem));

        public static DependencyProperty CustomStatusProperty = DependencyProperty.Register(
            "CustomStatus", typeof (string), typeof (GroupChatListItem));

        public static DependencyProperty PictureProperty = DependencyProperty.Register(
            "Picture", typeof (ImageSource), typeof (GroupChatListItem));

        public static DependencyProperty StatusPictureProperty = DependencyProperty.Register(
            "StatusPicture", typeof (ImageSource), typeof (GroupChatListItem));


        private long _chatRoomId;

        private NewChatRoom _chatRoom;

        public GroupChatListItem()
        {
            InitializeComponent();
            ThisRoom = null;
        }

        public bool IsDragging { get; set; }

        public FrameworkElement DragScope { get; set; }

        public NewChatRoom ThisRoom
        {
            get { return _chatRoom; }
            set
            {
                if (value != null)
                {
                    _chatRoomId = value.RID;
                    image1.Opacity = 1;
                    SetValue(UsernameProperty , value.GroupUser.User.User);
                }
                else
                {
                    _chatRoomId = 0;
                    SetValue(UsernameProperty,"null");
                }
                _chatRoom = value;
            }
        }

        private void UserControlMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Focus();
        }

        private void FlistitemMouseUp(object sender, MouseButtonEventArgs e)
        {
            if(!_isRemoving)
                Focus();
        }

        private void Image1MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_chatRoom.GroupUser.User.User == "lobby") return;
            ChatWindow firstOrDefault = Program.ChatWindows.FirstOrDefault(cw => cw.Id == ThisRoom.RID);
            if (firstOrDefault != null)
                firstOrDefault.CloseChatWindow();
            _isRemoving = true;
        }
    }
}