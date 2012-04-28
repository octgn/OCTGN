using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Skylabs.Lobby;

namespace Octgn.Controls
{
    /// <summary>
    ///   Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class HostedGameListItem
    {
        public static DependencyProperty GameNameProperty = DependencyProperty.Register(
            "GameName", typeof (string), typeof (HostedGameListItem));

        public static DependencyProperty UserNameProperty = DependencyProperty.Register(
            "UserName", typeof (string), typeof (HostedGameListItem));

        public static DependencyProperty GameLengthProperty = DependencyProperty.Register(
            "GameLength", typeof (string), typeof (HostedGameListItem));

        public static DependencyProperty GameVersionProperty = DependencyProperty.Register(
            "GameVersion", typeof (string), typeof (HostedGameListItem)
            );

        public static DependencyProperty GamePictureProperty = DependencyProperty.Register(
            "GamePicture", typeof (ImageSource), typeof (HostedGameListItem));

        public static DependencyProperty PictureProperty = DependencyProperty.Register(
            "Picture", typeof (ImageSource), typeof (HostedGameListItem));

        private HostedGameData _hostedGame;

        public HostedGameListItem(HostedGameData g)
        {
            InitializeComponent();
            Game = g;
        }

        public HostedGameData Game
        {
            get { return _hostedGame; }
            set
            {
                _hostedGame = value;
                SetValue(UserNameProperty, _hostedGame.UserHosting.User.User);
                int r = (int)((DateTime.Now.ToUniversalTime() - _hostedGame.TimeStarted).TotalMinutes);
                r = r < 0 ? 0 : r;
                SetValue(GameLengthProperty,
                         string.Format("runtime: about {0,0} minutes",r));
                SetValue(GameVersionProperty,_hostedGame.GameVersion.ToString());
                //SetValue(GameLengthProperty, "runtime: "+(System.DateTime.Now.ToUniversalTime() - _hostedGame.TimeStarted).TotalMinutes.ToString("N")+" minutes");
                foreach(var g in Program.GamesRepository.Games)
                {
                    if(_hostedGame.GameGuid == g.Id)
                    {
                        var bim = new BitmapImage();
                        bim.BeginInit();
                        bim.CacheOption = BitmapCacheOption.OnLoad;
                        bim.UriSource = g.GetCardBackUri();
                        bim.EndInit();
                        SetValue(GamePictureProperty, bim);
                    }
                }

                string guri;
                SetValue(GameNameProperty, _hostedGame.Name);
                switch (_hostedGame.GameStatus)
                {
                    case EHostedGame.GameInProgress:
                        guri = @"pack://application:,,,/Octgn;component/Resources/statusAway.png";
                        break;
                    case EHostedGame.StartedHosting:
                        guri = @"pack://application:,,,/Octgn;component/Resources/statusOnline.png";
                        break;
                    default: //Offline or anything else
                        guri = @"pack://application:,,,/Octgn;component/Resources/statusOffline.png";
                        break;
                }
                SetValue(PictureProperty, new ImageSourceConverter().ConvertFromString(guri) as ImageSource);
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
    }
}