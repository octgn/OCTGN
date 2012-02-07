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
using Skylabs;

namespace Octgn.Controls
{
    /// <summary>
    /// Interaction logic for GameListItem.xaml
    /// </summary>
    public partial class FriendRequestNotification : UserControl
    {
        public EventHandler OnDismiss;
        public static DependencyProperty FriendNameProperty = DependencyProperty.Register(
    "FriendName", typeof(string), typeof(FriendRequestNotification));
        public static DependencyProperty PictureProperty = DependencyProperty.Register(
    "Picture", typeof(ImageSource), typeof(FriendRequestNotification));

        private Skylabs.Lobby.FriendRequestNotification _notify;

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

        public FriendRequestNotification()
        {
            InitializeComponent();
            _notify = null;
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Focus();
        }

        private void flistitem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Focus();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            _notify.Accept();
            this.Visibility = Visibility.Hidden;
            if (OnDismiss != null)
                OnDismiss(this, null);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            _notify.Decline();
            this.Visibility = Visibility.Hidden;
            if (OnDismiss != null)
                OnDismiss(this, null);
        }
    }
}
