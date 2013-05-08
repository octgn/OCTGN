using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Skylabs.Lobby;
using agsXMPP;

namespace Octgn.Controls
{
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows.Media.Animation;

    using log4net;

    /// <summary>
    /// Interaction logic for ChatUserListItem.xaml
    /// </summary>
    public partial class ChatUserListItem : IComparable<ChatUserListItem>,IEquatable<ChatUserListItem>,IEqualityComparer<ChatUserListItem>,INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        
        public User User
        {
            get { return _user; }
            set { 
                _user = value;
                OnPropertyChanged("User");
                OnPropertyChanged("IsSub");
            }
        }

        public bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                _isAdmin = value;
                OnPropertyChanged("IsAdmin");
            }
        }

        public bool IsMod
        {
            get { return _isMod; }
            set
            {
                _isMod = value;
                OnPropertyChanged("IsMod");
            }
        }

        public bool IsOwner
        {
            get { return _isOwner; }
            set
            {
                _isOwner = value;
                OnPropertyChanged("IsOwner");
            }
        }

        public bool IsSub
        {
            get
            {
                if (User == null) 
                    return false;
                return User.IsSubbed;
            }
        }

        private User _user;
        private bool _isAdmin;
        private bool _isMod;
        private bool _isOwner;
        private bool _isSub;
        private double _realHeight;
        public ChatUserListItem()
        {
            InitializeComponent();
            User = new User(new Jid("noone@server.octgn.info"));
            IsAdmin = false;
            IsMod = false;
            IsOwner = false;
        }

        public ChatUserListItem(ChatRoom room, User user)
        {
            User = user;
            InitializeComponent();
            room.OnUserListChange += RoomOnOnUserListChange;
            Program.LobbyClient.OnDataReceived += LobbyClientOnOnDataReceived;
            this.Update(room);
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Loaded -= OnLoaded;
            _realHeight = 25;
            var hanim = new DoubleAnimation(0, _realHeight, new Duration(TimeSpan.FromMilliseconds(500)));
            this.BeginAnimation(HeightProperty, hanim, HandoffBehavior.Compose);
        }

        private void LobbyClientOnOnDataReceived(object sender, DataRecType type, object data)
        {
            if (type == DataRecType.UserSubChanged)
            {
                var d = data as User;
                if (d == null) return;
                if (d.Equals(_user))
                {
                    OnPropertyChanged("IsSub");
                }
            }
        }

        internal void Update(ChatRoom room)
        {
            IsAdmin = room.AdminList.Any(x => x == _user);
            IsMod = room.ModeratorList.Any(x => x == _user);
            IsOwner = room.OwnerList.Any(x => x == _user);
        }

        private void RoomOnOnUserListChange(object sender, List<User> users)
        {
            var room = sender as ChatRoom;
            if (room == null) return;
            this.Update(room);
        }

        private Visibility pretendVisible = Visibility.Visible;

        public void Hide()
        {
            if (pretendVisible == Visibility.Hidden) return;
            pretendVisible = Visibility.Hidden;
            //Log.Info("Hiding " + User.UserName);
            //var anim = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(500)));
            var hanim = new DoubleAnimation(_realHeight, 0, new Duration(TimeSpan.FromMilliseconds(100)));
            //this.BeginAnimation(OpacityProperty,anim,HandoffBehavior.Compose);
            this.BeginAnimation(HeightProperty,hanim,HandoffBehavior.Compose);
        }

        public void Show()
        {
            if (pretendVisible == Visibility.Visible) return;
            pretendVisible = Visibility.Visible;
            //Log.Info("Showing " + User.UserName);
            //var anim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));
            var hanim = new DoubleAnimation(0, _realHeight, new Duration(TimeSpan.FromMilliseconds(100)));
            //this.BeginAnimation(OpacityProperty,anim,HandoffBehavior.Compose);
            this.BeginAnimation(HeightProperty,hanim,HandoffBehavior.Compose);
        }

        public int CompareTo(ChatUserListItem other)
        {
            if (this.IsOwner)
            {
                if (other.IsOwner) return String.Compare(this.User.UserName, other.User.UserName, StringComparison.InvariantCultureIgnoreCase);
                return -1;
            }
            if (this.IsAdmin)
            {
                if (other.IsOwner) return 1;
                if (other.IsAdmin) return String.Compare(this.User.UserName, other.User.UserName, StringComparison.InvariantCultureIgnoreCase);
                return -1;
            }
            if (this.IsMod)
            {
                if (other.IsOwner) return 1;
                if (other.IsAdmin) return 1;
                if (other.IsMod) return String.Compare(this.User.UserName, other.User.UserName, StringComparison.InvariantCultureIgnoreCase);
                return -1;
            }
            if (this.IsSub)
            {
                if (other.IsOwner) return 1;
                if (other.IsAdmin) return 1;
                if (other.IsMod) return 1;
                if (other.IsSub) return String.Compare(this.User.UserName, other.User.UserName, StringComparison.InvariantCultureIgnoreCase);
                return -1;
            }
            if (other.IsOwner)
            {
                if (this.IsOwner) return String.Compare(other.User.UserName, this.User.UserName, StringComparison.InvariantCultureIgnoreCase);
                return 1;
            }
            if (other.IsAdmin)
            {
                if (this.IsOwner) return -1;
                if (this.IsAdmin) return String.Compare(other.User.UserName, this.User.UserName, StringComparison.InvariantCultureIgnoreCase);
                return 1;
            }
            if (other.IsMod)
            {
                if (this.IsOwner) return -1;
                if (this.IsAdmin) return -1;
                if (this.IsMod) return String.Compare(other.User.UserName, this.User.UserName, StringComparison.InvariantCultureIgnoreCase);
                return 1;
            }
            if (other.IsSub)
            {
                if (this.IsOwner) return -1;
                if (this.IsAdmin) return -1;
                if (this.IsMod) return -1;
                if (this.IsSub) return String.Compare(other.User.UserName, this.User.UserName, StringComparison.InvariantCultureIgnoreCase);
                return 1;
            }
            return String.Compare(this.User.UserName, other.User.UserName, StringComparison.InvariantCultureIgnoreCase); 
        }

        public bool Equals(ChatUserListItem other)
        {
            return other.User == User;
        }

        public bool Equals(ChatUserListItem x, ChatUserListItem y)
        {
            return x.User.Equals(y.User);
        }

        public int GetHashCode(ChatUserListItem obj)
        {
            return obj.User.GetHashCode();
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
    }
}
