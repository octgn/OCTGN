using System.Windows;
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
        private bool _isRemoving = false;

        public string UserName { get
        {
            return (string)this.GetValue(UsernameProperty);
        } set
        {
            this.SetValue(UsernameProperty,value);
        } }

        public static DependencyProperty UsernameProperty = DependencyProperty.Register(
            "UserName", typeof (string), typeof (GroupChatListItem));

        private long _chatRoomId;

        private ChatRoom _chatRoom;

        public GroupChatListItem()
        {
            InitializeComponent();
            ThisRoom = null;
        }

        public ChatRoom ThisRoom
        {
            get { return _chatRoom; }
            set
            {
                if (value != null)
                {
                    _chatRoomId = value.Rid;
                    image1.Opacity = 1;
                    UserName = value.GroupUser.UserName;
                }
                else
                {
                    _chatRoomId = 0;
                    UserName = "null";
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
            if (_chatRoom.GroupUser.UserName == "lobby") return;
			_chatRoom.LeaveRoom();
            _isRemoving = true;
        }
    }
}