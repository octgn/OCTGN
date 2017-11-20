/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using log4net;
using Octgn.Online;
using Octgn.Communication;
using Octgn.Site.Api.Models;

namespace Octgn.Controls.ControlTemplates
{
    [ContentProperty("PreSubIcon")]
    public partial class UserListItem : ContentControl, IComparable<UserListItem>, IEquatable<UserListItem>, IEqualityComparer<UserListItem>, INotifyPropertyChanged, IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static UserListItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UserListItem), new FrameworkPropertyMetadata(typeof(UserListItem)));
        }

        // Dependency Property
        public static readonly DependencyProperty PreSubIconProperty =
             DependencyProperty.Register("PreSubIcon", typeof(FrameworkElement),
             typeof(UserListItem), new FrameworkPropertyMetadata(null));

        // .NET Property wrapper
        public FrameworkElement PreSubIcon {
            get { return (FrameworkElement)GetValue(PreSubIconProperty); }
            set { SetValue(PreSubIconProperty, value); }
        }

        public User User {
            get { return this.user; }
            set {
                this.user = value;
                OnPropertyChanged("User");
                OnPropertyChanged("IsSub");
            }
        }

        public ApiUser ApiUser {
            get => _apiUser;
            set {
                if (_apiUser == value) return;
                _apiUser = value;
                OnPropertyChanged(nameof(ApiUser));
                OnPropertyChanged(nameof(IsSub));
                OnPropertyChanged(nameof(ImageSubSource));
            }
        }

        private ApiUser _apiUser;

        public bool IsSub => ApiUser?.IsSubscribed == true;

        public string ImageSubSource {
            get {
                if (IsSub && !string.IsNullOrWhiteSpace(ApiUser.IconUrl))
                    return ApiUser.IconUrl;

                return "/Resources/sub.png";
            }
        }

        protected User user;
        public UserListItem() {
            this.DataContext = this;
        }

        public UserListItem(User user) {
            this.DataContext = this;
            User = user;
            ApiUserCache.Instance.OnUpdate += UserManagerOnUpdate;
        }

        private void UserManagerOnUpdate() {
            ApiUser = ApiUserCache.Instance.ApiUser(user);
        }

        private Visibility pretendVisible = Visibility.Visible;

        public void Hide() {
            if (pretendVisible == Visibility.Hidden) return;
            pretendVisible = Visibility.Hidden;
            this.Visibility = Visibility.Collapsed;
        }

        public void Show() {
            if (pretendVisible == Visibility.Visible) return;
            pretendVisible = Visibility.Visible;
            this.Visibility = Visibility.Visible;
        }

        public int CompareTo(UserListItem other) {
            if (this.User == null) return 1;
            if (other == null) return -1;
            if (other.User == null) return -1;
            if (this.IsSub) {
                if (other.IsSub) return String.Compare(this.User.DisplayName, other.User.DisplayName, StringComparison.InvariantCultureIgnoreCase);
                return -1;
            }
            if (other.IsSub) {
                if (this.IsSub) return String.Compare(other.User.DisplayName, this.User.DisplayName, StringComparison.InvariantCultureIgnoreCase);
                return 1;
            }
            return String.Compare(this.User.DisplayName, other.User.DisplayName, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool Equals(UserListItem other) {
            return other.User == User;
        }

        public bool Equals(UserListItem x, UserListItem y) {
            return x.User.Equals(y.User);
        }

        public int GetHashCode(UserListItem obj) {
            return obj.User.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose() {
            ApiUserCache.Instance.OnUpdate -= UserManagerOnUpdate;
            if (PropertyChanged != null) {
                foreach (var d in PropertyChanged.GetInvocationList()) {
                    PropertyChanged -= (PropertyChangedEventHandler)d;
                }
            }
        }

        #endregion
    }
}