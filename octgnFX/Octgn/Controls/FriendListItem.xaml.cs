using System;
using System.ComponentModel;
using Octgn.Controls.ControlTemplates;
using Octgn.Online;

namespace Octgn.Controls
{
    public partial class FriendListItem : IComparable<FriendListItem>
    {
        private string statusImageSource;

        public FriendListItem()
            : base()
        {
            DataContext = this;
            this.SetStatusImage();
            this.InitializeComponent();
        }

        public FriendListItem(User user)
            : base(user)
        {
            DataContext = this;
            this.SetStatusImage();
            InitializeComponent();
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
            if (User == null)
            {
                StatusImageSource = "/Resources/statusOffline.png";
                return;
            }

            switch (User.Status)
            {
                case UserStatus.Online:
                    StatusImageSource = "/Resources/statusOnline.png";
                    break;
                default:
                    StatusImageSource = @"/Resources/statusOffline.png";
                    break;
            }
        }

        public new int CompareTo(UserListItem other)
        {
            return CompareTo(new FriendListItem(other.User));
        }

        public int CompareTo(FriendListItem other)
        {
            if (this.User == null) return 1;
            if (other == null || other.User == null) return -1;
            if (this.User.Status == UserStatus.Online)
            {
                if (other.User.Status == UserStatus.Online) return String.Compare(this.User.UserName, other.User.UserName, StringComparison.InvariantCultureIgnoreCase);
                return -1;
            }
            if (other.User.Status == UserStatus.Online)
            {
                if (this.User.Status == UserStatus.Online) return String.Compare(other.User.UserName, this.User.UserName, StringComparison.InvariantCultureIgnoreCase);
                return 1;
            }
            return String.Compare(this.User.UserName, other.User.UserName, StringComparison.InvariantCultureIgnoreCase);
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            this.PropertyChanged -= OnPropertyChanged;
            base.Dispose();
        }

        #endregion
    }
}