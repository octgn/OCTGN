using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Skylabs.Lobby;
using agsXMPP;

namespace Octgn.Controls
{
    using System.ComponentModel;
    using System.Reflection;

    using Octgn.Controls.ControlTemplates;

    using log4net;

    /// <summary>
    /// Interaction logic for ChatUserListItem.xaml
    /// </summary>
    public partial class ChatUserListItem : UserListItem,IComparable<ChatUserListItem>
    {
        public bool IsAdmin
        {
            get { return this.isAdmin; }
            set
            {
                this.isAdmin = value;
                OnPropertyChanged("IsAdmin");
            }
        }

        public bool IsMod
        {
            get { return this.isMod; }
            set
            {
                this.isMod = value;
                OnPropertyChanged("IsMod");
            }
        }

        public bool IsOwner
        {
            get { return this.isOwner; }
            set
            {
                this.isOwner = value;
                OnPropertyChanged("IsOwner");
            }
        }

        private bool isAdmin;
        private bool isMod;
        private bool isOwner;
        private readonly ChatRoom room;

        public ChatUserListItem()
            : base()
        {
            DataContext = this;
        }

        public ChatUserListItem(ChatRoom chatroom, User user):base(user)
        {
            DataContext = this;
            InitializeComponent();
            this.room = chatroom;
            this.room.OnUserListChange += RoomOnOnUserListChange;
            this.Update(chatroom);
        }

        internal void Update(ChatRoom chatroom)
        {
            IsAdmin = chatroom.AdminList.Any(x => x == this.user);
            IsMod = chatroom.ModeratorList.Any(x => x == this.user);
            IsOwner = chatroom.OwnerList.Any(x => x == this.user);
        }

        private void RoomOnOnUserListChange(object sender, List<User> users)
        {
            var chatroom = sender as ChatRoom;
            if (chatroom == null) return;
            this.Update(chatroom);
        }

        public int CompareTo(ChatUserListItem other)
        {
            if (this.User == null) return 1;
            if (other == null) return -1;
            if (other.User == null) return -1;
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
        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public new void Dispose()
        {
            if (this.room != null)
            {
                this.room.OnUserListChange -= RoomOnOnUserListChange;
            }
            base.Dispose();
        }

        #endregion
    }
}
