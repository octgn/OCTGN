using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Skylabs;
using Skylabs.Lobby;

namespace Octgn.Controls
{
    /// <summary>
    /// Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class FriendListItem : UserControl
    {
        public static DependencyProperty UsernameProperty = DependencyProperty.Register(
    "UserName", typeof(string), typeof(FriendListItem));
        public static DependencyProperty CustomStatusProperty = DependencyProperty.Register(
    "CustomStatus", typeof(string), typeof(FriendListItem));
        public static DependencyProperty PictureProperty = DependencyProperty.Register(
    "Picture", typeof(ImageSource), typeof(FriendListItem));
        public static DependencyProperty StatusPictureProperty = DependencyProperty.Register(
    "StatusPicture", typeof(ImageSource), typeof(FriendListItem));

        private User m_User = new User();

        public User ThisUser
        {
            get
            {
                return m_User;
            }
            set
            {
                m_User = value;
                SetValue(CustomStatusProperty, value.CustomStatus);
                string h = ValueConverters.HashEmailAddress(value.Email.ToLower().Trim());
                string guri = "http://www.gravatar.com/avatar/" + h + "?s=64&r=x";
                SetValue(PictureProperty, new ImageSourceConverter().ConvertFromString(guri) as ImageSource);
                SetValue(UsernameProperty, value.DisplayName);
                switch(value.Status)
                {
                    case UserStatus.Away:
                        guri = @"pack://application:,,,/OCTGN;component/Resources/statusAway.png";
                        break;
                    case UserStatus.DoNotDisturb:
                        guri = @"pack://application:,,,/OCTGN;component/Resources/statusDND.png";
                        break;
                    case UserStatus.Online:
                        guri = @"pack://application:,,,/OCTGN;component/Resources/statusOnline.png";
                        break;
                    default: //Offline or anything else
                        guri = @"pack://application:,,,/OCTGN;component/Resources/statusOffline.png";
                        break;
                }
                SetValue(StatusPictureProperty, new ImageSourceConverter().ConvertFromString(guri) as ImageSource);
            }
        }

        public FriendListItem()
        {
            InitializeComponent();
            ThisUser = new User();
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Focus();
        }

        private void flistitem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Focus();
        }
    }
}