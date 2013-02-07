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
    /// <summary>
    /// Interaction logic for ChatUserListItem.xaml
    /// </summary>
    public partial class ChatUserListItem : UserControl,IComparable<ChatUserListItem>,IEquatable<ChatUserListItem>,IEqualityComparer<ChatUserListItem>
    {
        public User User
        {
            get { return _user; }
            set { 
                _user = value;
                Dispatcher.BeginInvoke(new Action(() =>
                                                      {
                                                          UserNameTextBox.Text = _user.UserName;
                                                      }));
            }
        }

        public bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                _isAdmin = value;
                Dispatcher.BeginInvoke(new Action(() =>
                                                      {
                                                          ImageAdmin.Visibility = _isAdmin
                                                                                      ? Visibility.Visible
                                                                                      : Visibility.Collapsed;
                                                      }));
            }
        }

        public bool IsMod
        {
            get { return _isMod; }
            set
            {
                _isMod = value;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ImageMod.Visibility = _isMod
                                                                                      ? Visibility.Visible
                                                                                      : Visibility.Collapsed;

                }));
            }
        }

        public bool IsOwner
        {
            get { return _isOwner; }
            set
            {
                _isOwner = value;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ImageOwner.Visibility = _isOwner
                                                                                      ? Visibility.Visible
                                                                                      : Visibility.Collapsed;

                }));
            }
        }

        private User _user;
        private bool _isAdmin;
        private bool _isMod;
        private bool _isOwner;
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
            InitializeComponent();
            IsAdmin = room.AdminList.Any(x => x == user);
            IsMod = room.ModeratorList.Any(x => x == user);
            IsOwner = room.OwnerList.Any(x => x == user);
            User = user;
        }

        public int CompareTo(ChatUserListItem other)
        {
            if (this.IsOwner)
            {
                if (other.IsOwner) return this.User.UserName.CompareTo(other.User.UserName);
                return -1;
            }
            if (this.IsAdmin)
            {
                if (other.IsOwner) return 1;
                if (other.IsAdmin) return this.User.UserName.CompareTo(other.User.UserName);
                return -1;
            }
            if (this.IsMod)
            {
                if (this.IsOwner) return 1;
                if (this.IsAdmin) return 1;
                if (this.IsMod) return this.User.UserName.CompareTo(other.User.UserName);
                return -1;
            }
            if (other.IsOwner)
            {
                if (this.IsOwner) return other.User.UserName.CompareTo(this.User.UserName);
                return 1;
            }
            if (other.IsAdmin)
            {
                if (this.IsOwner) return -1;
                if (this.IsAdmin) return other.User.UserName.CompareTo(this.User.UserName);
                return 1;
            }
            if (other.IsMod)
            {
                if (this.IsOwner) return -1;
                if (this.IsAdmin) return -1;
                if (this.IsMod) return other.User.UserName.CompareTo(this.User.UserName);
                return 1;
            }
            return this.User.UserName.CompareTo(other.User.UserName);
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
    }
}
