using System.Linq;
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
        public static DependencyProperty GameNameProperty = DependencyProperty.Register(
    "GameName", typeof(string), typeof(HostedGameListItem));
        public static DependencyProperty UserNameProperty = DependencyProperty.Register(
    "UserName", typeof(string), typeof(HostedGameListItem));
        public static DependencyProperty GameLengthProperty = DependencyProperty.Register(
    "GameLength", typeof(string), typeof(HostedGameListItem));
        public static DependencyProperty GamePictureProperty = DependencyProperty.Register(
    "GamePicture", typeof(ImageSource), typeof(HostedGameListItem));
        public static DependencyProperty PictureProperty = DependencyProperty.Register(
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
                SetValue(UserNameProperty, _hostedGame.UserHosting.DisplayName);
                SetValue(GameLengthProperty, string.Format("runtime: {0:0,0} minutes",(System.DateTime.Now.ToUniversalTime() - _hostedGame.TimeStarted).TotalMinutes));
                //SetValue(GameLengthProperty, "runtime: "+(System.DateTime.Now.ToUniversalTime() - _hostedGame.TimeStarted).TotalMinutes.ToString("N")+" minutes");
                foreach (BitmapImage bi in from g in Program.GamesRepository.AllGames where g.Id == _hostedGame.GameGuid select new BitmapImage(g.GetCardBackUri()))
                {
                    SetValue(GamePictureProperty, bi as ImageSource);
                    break;
                }

                string guri = "";
                SetValue(GameNameProperty, _hostedGame.Name);
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
                SetValue(PictureProperty, new ImageSourceConverter().ConvertFromString(guri) as ImageSource);
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