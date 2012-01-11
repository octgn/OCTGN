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

namespace Octgn.Controls
{
    /// <summary>
    /// Interaction logic for GameListItem.xaml
    /// </summary>
    public partial class GameListItem : UserControl
    {
        public static DependencyProperty GameNameProperty = DependencyProperty.Register(
    "GameName", typeof(string), typeof(GameListItem));
        public static DependencyProperty VersionProperty = DependencyProperty.Register(
    "Version", typeof(string), typeof(GameListItem));
        public static DependencyProperty PictureProperty = DependencyProperty.Register(
    "Picture", typeof(ImageSource), typeof(GameListItem));

        private Octgn.Data.Game _game;

        public Octgn.Data.Game Game
        {
            get { return _game; }
            set 
            { 
                _game = value;
                BitmapImage bi = new BitmapImage(_game.GetCardBackUri());
                SetValue(PictureProperty, bi as ImageSource);
                SetValue(GameNameProperty, _game.Name);
                SetValue(VersionProperty,_game.Version.ToString());
            }
        }

        public GameListItem()
        {
            InitializeComponent();
            _game = new Data.Game();
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
