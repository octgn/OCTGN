using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Skylabs.Lobby;

namespace Octgn.Controls
{
    using System;
    using System.ComponentModel;
    using System.Linq;

    using Octgn.Annotations;

    /// <summary>
    ///   Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class GroupChatListItem : INotifyPropertyChanged,IDisposable
    {
        private bool _isRemoving = false;

        public string UserName
        {
            get
            {
                return (string)this.GetValue(UsernameProperty);
            }
            set
            {
                this.SetValue(UsernameProperty, value);
            }
        }

        public static DependencyProperty UsernameProperty = DependencyProperty.Register(
            "UserName", typeof(string), typeof(GroupChatListItem));

        public static DependencyProperty SelectedProperty = DependencyProperty.Register(
            "Selected", typeof(bool), typeof(GroupChatListItem));

        private long _chatRoomId;

        private ChatRoom _chatRoom;

        public GroupChatListItem():this(null)
        {
        }

        public GroupChatListItem(ChatRoom room)
        {
            ThisRoom = room;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            InitializeComponent();
            this.GotFocus += OnGotFocus;
            this.LostFocus += OnLostFocus;
        }

        public ChatRoom ThisRoom
        {
            get { return _chatRoom; }
            set
            {
                if (value != null)
                {
                    _chatRoomId = value.Rid;
                    //image1.Opacity = 1;
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

        public bool Selected
        {
            get
            {
                return (bool)this.GetValue(SelectedProperty);
            }
            set
            {
                this.SetValue(SelectedProperty, value);
                OnPropertyChanged("Selected");
            }
        }

        private void UserControlMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Focus();
        }

        private void FlistitemMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isRemoving)
                Focus();
        }

        private void Image1MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_chatRoom.GroupUser.UserName == "lobby") return;
            Program.LobbyClient.Chatting.LeaveRoom(_chatRoom);
            _isRemoving = true;
        }


        private void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Selected = false;
        }

        private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Selected = true;
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

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.GotFocus -= this.OnGotFocus;
            this.LostFocus -= this.OnLostFocus;
            if (PropertyChanged != null)
            {
                foreach (var d in PropertyChanged.GetInvocationList().ToArray())
                {
                    PropertyChanged -= (PropertyChangedEventHandler)d;
                }
            }
        }

        #endregion
    }
}