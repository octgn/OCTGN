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

    /// <summary>
    ///   Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class FriendListItem : INotifyPropertyChanged,IDisposable
    {
        public static DependencyProperty UsernameProperty = DependencyProperty.Register(
            "UserName", typeof(string), typeof(FriendListItem));

        public static DependencyProperty CustomStatusProperty = DependencyProperty.Register(
            "CustomStatus", typeof(string), typeof(FriendListItem));

        public static DependencyProperty PictureProperty = DependencyProperty.Register(
            "Picture", typeof(ImageSource), typeof(FriendListItem));

        public static DependencyProperty StatusPictureProperty = DependencyProperty.Register(
            "StatusPicture", typeof(ImageSource), typeof(FriendListItem));

        public static DependencyProperty SelectedProperty = DependencyProperty.Register(
            "Selected", typeof(bool), typeof(FriendListItem));

        public string UserName
        {
            get
            {
                return this.GetValue(UsernameProperty) as string;
            }
        }

        public string CustomStatus
        {
            get
            {
                return this.GetValue(CustomStatusProperty) as string;
            }
        }

        public ImageSource StatusPicture
        {
            get
            {
                return this.GetValue(StatusPictureProperty) as ImageSource;
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

        private User _mUser = null;

        public FriendListItem()
        {
            InitializeComponent();
            this.GotFocus += OnGotFocus;
            this.LostFocus += OnLostFocus;
        }

        private void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Selected = false;
        }

        private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Selected = true;
        }

        public User ThisUser
        {
            get { return _mUser; }
            set
            {
                _mUser = value;
                SetValue(CustomStatusProperty, value.CustomStatus);
                string guri = @"pack://application:,,,/Octgn;component/Resources/usernoimage.png";
                SetValue(UsernameProperty, value.UserName);
                switch (value.Status)
                {
                    case UserStatus.Away:
                        guri = @"pack://application:,,,/Octgn;component/Resources/statusAway.png";
                        break;
                    case UserStatus.DoNotDisturb:
                        guri = @"pack://application:,,,/Octgn;component/Resources/statusDND.png";
                        break;
                    case UserStatus.Online:
                        guri = @"pack://application:,,,/Octgn;component/Resources/statusOnline.png";
                        break;
                    default: //Offline or anything else
                        guri = @"pack://application:,,,/Octgn;component/Resources/statusOffline.png";
                        break;
                }
                SetValue(StatusPictureProperty, new ImageSourceConverter().ConvertFromString(guri) as ImageSource);
            }
        }

        private void UserControlMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Focus();
        }

        private void FlistitemMouseUp(object sender, MouseButtonEventArgs e)
        {
            Focus();
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