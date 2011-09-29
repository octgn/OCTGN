using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Octgn.Controls
{
    /// <summary>
    /// Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class FriendListItem : UserControl
    {
        public static DependencyProperty MessageProperty = DependencyProperty.Register(
    "UserName", typeof(string), typeof(FriendListItem));
        public static DependencyProperty PictureProperty = DependencyProperty.Register(
    "Picture", typeof(ImageSource), typeof(FriendListItem));

        public string UserName
        {
            get
            {
                return (string)GetValue(MessageProperty);
            }
            set
            {
                SetValue(MessageProperty, value);
            }
        }

        public ImageSource Picture
        {
            get
            {
                return (ImageSource)GetValue(PictureProperty);
            }
            set
            {
                SetValue(PictureProperty, value);
            }
        }

        public FriendListItem()
        {
            InitializeComponent();
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }
    }
}