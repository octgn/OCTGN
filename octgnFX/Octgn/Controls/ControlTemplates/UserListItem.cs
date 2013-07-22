/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Octgn.Controls.ControlTemplates
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Windows.Media;

    using Skylabs.Lobby;

    using agsXMPP;

    using log4net;

    [ContentProperty("PreSubIcon")]
    public partial class UserListItem : ContentControl, IComparable<UserListItem>, IEquatable<UserListItem>, IEqualityComparer<UserListItem>, INotifyPropertyChanged, IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static UserListItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UserListItem), new FrameworkPropertyMetadata(typeof(UserListItem)));
        }

        // Dependency Property
        public static readonly DependencyProperty PreSubIconProperty =
             DependencyProperty.Register("PreSubIcon", typeof(FrameworkElement),
             typeof(UserListItem), new FrameworkPropertyMetadata(null));

        // .NET Property wrapper
        public FrameworkElement PreSubIcon
        {
            get { return (FrameworkElement)GetValue(PreSubIconProperty); }
            set { SetValue(PreSubIconProperty, value); }
        }

        public User User
        {
            get { return this.user; }
            set
            {
                this.user = value;
                OnPropertyChanged("User");
                OnPropertyChanged("IsSub");
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

        public string ImageSubSource
        {
            get
            {
                var au = User.ApiUser;
                if (au != null)
                {
                    if (au.IsSubscribed)
                    {
                        if (!string.IsNullOrWhiteSpace(au.IconUrl))
                        {
                            return au.IconUrl;
                        }
                    }
                }
                return "/Resources/sub.png";
            }
        }

        protected User user;
        public UserListItem()
        {
            this.DataContext = this;
            User = new User(new Jid("noone@server.octgn.info"));
        }

        public UserListItem(User user)
        {
            this.DataContext = this;
            User = user;
            UserManager.Get().OnUpdate += UserManagerOnUpdate;
        }

        private void UserManagerOnUpdate()
        {
            OnPropertyChanged("IsSub");
        }

        private Visibility pretendVisible = Visibility.Visible;

        public void Hide()
        {
            if (pretendVisible == Visibility.Hidden) return;
            pretendVisible = Visibility.Hidden;
            this.Visibility = Visibility.Collapsed;
        }

        public void Show()
        {
            if (pretendVisible == Visibility.Visible) return;
            pretendVisible = Visibility.Visible;
            this.Visibility = Visibility.Visible;
        }

        public int CompareTo(UserListItem other)
        {
            if (this.User == null) return 1;
            if (other == null) return -1;
            if (other.User == null) return -1;
            if (this.IsSub)
            {
                if (other.IsSub) return String.Compare(this.User.UserName, other.User.UserName, StringComparison.InvariantCultureIgnoreCase);
                return -1;
            }
            if (other.IsSub)
            {
                if (this.IsSub) return String.Compare(other.User.UserName, this.User.UserName, StringComparison.InvariantCultureIgnoreCase);
                return 1;
            }
            return String.Compare(this.User.UserName, other.User.UserName, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool Equals(UserListItem other)
        {
            return other.User == User;
        }

        public bool Equals(UserListItem x, UserListItem y)
        {
            return x.User.Equals(y.User);
        }

        public int GetHashCode(UserListItem obj)
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

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            UserManager.Get().OnUpdate -= UserManagerOnUpdate;
            if (PropertyChanged != null)
            {
                foreach (var d in PropertyChanged.GetInvocationList())
                {
                    PropertyChanged -= (PropertyChangedEventHandler)d;
                }
            }
        }

        #endregion
    }
}