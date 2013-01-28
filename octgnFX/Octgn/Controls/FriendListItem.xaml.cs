using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Skylabs.Lobby;
using agsXMPP;

namespace Octgn.Controls
{
    /// <summary>
    ///   Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class FriendListItem
    {
        public static DependencyProperty UsernameProperty = DependencyProperty.Register(
            "UserName", typeof (string), typeof (FriendListItem));

        public static DependencyProperty CustomStatusProperty = DependencyProperty.Register(
            "CustomStatus", typeof (string), typeof (FriendListItem));

        public static DependencyProperty PictureProperty = DependencyProperty.Register(
            "Picture", typeof (ImageSource), typeof (FriendListItem));

        public static DependencyProperty StatusPictureProperty = DependencyProperty.Register(
            "StatusPicture", typeof (ImageSource), typeof (FriendListItem));

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

        private NewUser _mUser = new NewUser(new Jid(""));

        public FriendListItem()
        {
            InitializeComponent();
        }

        public NewUser ThisUser
        {
            get { return _mUser; }
            set
            {
                _mUser = value;
                SetValue(CustomStatusProperty, value.CustomStatus);
                string guri = "";
                if(String.IsNullOrWhiteSpace(_mUser.Email))
                {
                    guri = @"pack://application:,,,/Octgn;component/Resources/usernoimage.png";
                }
                else
                {
                    string h = ValueConverters.HashEmailAddress(_mUser.Email);
                    guri = "http://www.gravatar.com/avatar/" + h + "?s=64&r=x&salt=";
                }
                SetValue(PictureProperty, new ImageSourceConverter().ConvertFromString(guri) as ImageSource);                    
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

        private void image1_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
        }

        private void image1_MouseUp(object sender, MouseButtonEventArgs e)
        {

            Program.LobbyClient.RemoveFriend(ThisUser);
        }
    }
}