using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Skylabs;

namespace Octgn.Controls
{
    /// <summary>
    ///   Interaction logic for GameListItem.xaml
    /// </summary>
    public partial class FriendRequestNotification
    {
        public static DependencyProperty FriendNameProperty = DependencyProperty.Register(
            "FriendName", typeof (string), typeof (FriendRequestNotification));

        public static DependencyProperty PictureProperty = DependencyProperty.Register(
            "Picture", typeof (ImageSource), typeof (FriendRequestNotification));

        public EventHandler OnDismiss;

        private Skylabs.Lobby.FriendRequestNotification _notify;

        public FriendRequestNotification()
        {
            InitializeComponent();
            _notify = null;
        }

        public Skylabs.Lobby.FriendRequestNotification Notification
        {
            get { return _notify; }
            set
            {
                _notify = value;
                string h = ValueConverters.HashEmailAddress(_notify.User.Email.ToLower().Trim());
                string guri = "http://www.gravatar.com/avatar/" + h + "?s=64&r=x";
                SetValue(PictureProperty, new ImageSourceConverter().ConvertFromString(guri) as ImageSource);
                SetValue(FriendNameProperty, _notify.User.DisplayName);
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

        private void Button1Click(object sender, RoutedEventArgs e)
        {
            _notify.Accept();
            Visibility = Visibility.Hidden;
            if (OnDismiss != null)
                OnDismiss(this, null);
        }

        private void Button2Click(object sender, RoutedEventArgs e)
        {
            _notify.Decline();
            Visibility = Visibility.Hidden;
            if (OnDismiss != null)
                OnDismiss(this, null);
        }
    }
}