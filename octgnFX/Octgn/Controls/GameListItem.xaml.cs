using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Octgn.Controls
{
    /// <summary>
    ///   Interaction logic for GameListItem.xaml
    /// </summary>
    public partial class GameListItem
    {
        public static DependencyProperty GameNameProperty = DependencyProperty.Register(
            "GameName", typeof (string), typeof (GameListItem));

        public static DependencyProperty VersionProperty = DependencyProperty.Register(
            "Version", typeof (string), typeof (GameListItem));

        public static DependencyProperty PictureProperty = DependencyProperty.Register(
            "Picture", typeof (ImageSource), typeof (GameListItem));

        private Data.Game _game;

        public GameListItem()
        {
            InitializeComponent();
            _game = new Data.Game();
        }

        public Data.Game Game
        {
            get { return _game; }
            set
            {
                _game = value;

                var bim = new BitmapImage();
                bim.BeginInit();
                bim.CacheOption = BitmapCacheOption.OnLoad;
                bim.UriSource = _game.GetCardBackUri();
                bim.EndInit();

                SetValue(PictureProperty, bim);
                SetValue(GameNameProperty, _game.Name);
                SetValue(VersionProperty, _game.Version.ToString());
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