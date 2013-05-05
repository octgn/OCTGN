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

    /// <summary>
    /// Interaction logic for ChatUserListItem.xaml
    /// </summary>
    public partial class ChatUserListItem : UserControl,IComparable<ChatUserListItem>,IEquatable<ChatUserListItem>,IEqualityComparer<ChatUserListItem>,INotifyPropertyChanged
    {
        public User User
        {
            get { return _user; }
            set { 
                _user = value;
                OnPropertyChanged("User");
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
            get { return _isSub; }
            set
            {
                _isSub = value;
                OnPropertyChanged("IsSub");
            }
        }

        private User _user;
        private bool _isAdmin;
        private bool _isMod;
        private bool _isOwner;
        private bool _isSub;
        public ChatUserListItem()
        {
            InitializeComponent();
            User = new User(new Jid("noone@server.octgn.info"));
            IsAdmin = false;
            IsMod = false;
            IsOwner = false;
            IsSub = false;
        }

        public ChatUserListItem(ChatRoom room, User user)
        {
            InitializeComponent();
            room.OnUserListChange += RoomOnOnUserListChange;
            User = user;
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
                if (this.IsOwner) return 1;
                if (this.IsAdmin) return 1;
                if (this.IsMod) return String.Compare(this.User.UserName, other.User.UserName, StringComparison.InvariantCultureIgnoreCase);
                return -1;
            }
            if (this.IsSub)
            {
                if (this.IsOwner) return 1;
                if (this.IsAdmin) return 1;
                if (this.IsMod) return 1;
                if (this.IsSub) return String.Compare(this.User.UserName, other.User.UserName, StringComparison.InvariantCultureIgnoreCase);
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
