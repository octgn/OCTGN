using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Skylabs;
using Skylabs.Lobby;

namespace Octgn.Controls
{
    /// <summary>
    /// Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class HostedGameListItem : UserControl
    {
        public static DependencyProperty UsernameProperty = DependencyProperty.Register(
    "GameName", typeof(string), typeof(HostedGameListItem));
        public static DependencyProperty CustomStatusProperty = DependencyProperty.Register(
    "UserName", typeof(string), typeof(HostedGameListItem));
        public static DependencyProperty GamePictureProperty = DependencyProperty.Register(
    "GamePicture", typeof(ImageSource), typeof(HostedGameListItem));
        public static DependencyProperty StatusPictureProperty = DependencyProperty.Register(
    "Picture", typeof(ImageSource), typeof(HostedGameListItem));

        private HostedGame _hostedGame;

        public HostedGame Game
        {
            get
            {
                return _hostedGame;
            }
            set
            {
                _hostedGame = value;
                SetValue(CustomStatusProperty, _hostedGame.UserHosting.DisplayName);
                bool gotone = false;
                foreach (Data.Game g in Program.GamesRepository.AllGames)
                {
                    if (g.Id == _hostedGame.GameGuid)
                    {
                        BitmapImage bi = new BitmapImage(g.GetCardBackUri());
                        SetValue(GamePictureProperty, bi as ImageSource);
                        gotone = true;
                        break;
                    }
                }

                string guri = "";
                SetValue(UsernameProperty, _hostedGame.UserHosting.DisplayName);
                switch(_hostedGame.GameStatus)
                {
                    case HostedGame.eHostedGame.GameInProgress:
                        guri = @"pack://application:,,,/OCTGN;component/Resources/statusAway.png";
                        break;
                    case HostedGame.eHostedGame.StartedHosting:
                        guri = @"pack://application:,,,/OCTGN;component/Resources/statusOnline.png";
                        break;
                    default: //Offline or anything else
                        guri = @"pack://application:,,,/OCTGN;component/Resources/statusOffline.png";
                        break;
                }
                SetValue(StatusPictureProperty, new ImageSourceConverter().ConvertFromString(guri) as ImageSource);
            }
        }

        public HostedGameListItem(HostedGame g)
        {
            InitializeComponent();
            Game = g;
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