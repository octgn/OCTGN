using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Skylabs.Lobby;
using agsXMPP;

namespace Octgn.Controls
{
    using System.ComponentModel;
    using System.Linq;

    using Octgn.Controls.ControlTemplates;

    /// <summary>
    ///   Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class FriendListItem : IComparable<FriendListItem>
    {
        private string statusImageSource;

        public FriendListItem()
            : base()
        {
            DataContext = this;
            this.StatusImageSource = @"pack://application:,,,/Octgn;component/Resources/statusOffline.png";
        }

        public FriendListItem(User user)
            : base(user)
        {
            DataContext = this;
            InitializeComponent();
            this.SetStatusImage();
            this.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName.Equals("user", StringComparison.InvariantCultureIgnoreCase))
            {
                this.SetStatusImage();
            }
        }

        public string StatusImageSource
        {
            get
            {
                return this.statusImageSource;
            }
            set
            {
                if (this.statusImageSource == value) return;
                this.statusImageSource = value;
                OnPropertyChanged("StatusImageSource");
            }
        }

        private void SetStatusImage()
        {
            switch (User.Status)
            {
                case UserStatus.Online:
                    StatusImageSource = @"pack://application:,,,/Octgn;component/Resources/statusOnline.png";
                    break;
                case UserStatus.Away:
                    StatusImageSource = @"pack://application:,,,/Octgn;component/Resources/statusAway.png";
                    break;
                case UserStatus.DoNotDisturb:
                    StatusImageSource = @"pack://application:,,,/Octgn;component/Resources/statusDND.png";
                    break;
                default:
                    StatusImageSource = @"pack://application:,,,/Octgn;component/Resources/statusOffline.png";
                    break;
            }
        }

        public int CompareTo(FriendListItem other)
        {
            if (this.User == null) return 1;
            if (other == null || other.User == null) return -1;
            if (this.User.Status == UserStatus.Online)
            {
                if (other.User.Status == UserStatus.Online) return base.CompareTo(other);
                return -1;
            }
            if (this.User.Status == UserStatus.Away)
            {
                if (other.User.Status == UserStatus.Online) return 1;
                if (other.User.Status == UserStatus.Away) return base.CompareTo(other);
                return -1;
            }
            if (this.User.Status == UserStatus.DoNotDisturb)
            {
                if (other.User.Status == UserStatus.Online) return 1;
                if (other.User.Status == UserStatus.Away) return 1;
                if (other.User.Status == UserStatus.DoNotDisturb) return base.CompareTo(other);
                return -1;
            }
            if (other.User.Status == UserStatus.Online)
            {
                if (this.User.Status == UserStatus.Online) return base.CompareTo(this);
                return 1;
            }
            if (other.User.Status == UserStatus.Away)
            {
                if (this.User.Status == UserStatus.Online) return -1;
                if (this.User.Status == UserStatus.Away) return base.CompareTo(this);
                return 1;
            }
            if (other.User.Status == UserStatus.DoNotDisturb)
            {
                if (this.User.Status == UserStatus.Online) return -1;
                if (this.User.Status == UserStatus.Away) return -1;
                if (this.User.Status == UserStatus.DoNotDisturb) return base.CompareTo(this);
                return 1;
            }
            return base.CompareTo(other);
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public new void Dispose()
        {
            this.PropertyChanged -= OnPropertyChanged;
            base.Dispose();
        }

        #endregion
    }
}