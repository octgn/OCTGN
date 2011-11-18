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
using Octgn.Controls;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for GameList.xaml
    /// </summary>
    public partial class GameList : Page
    {
        public event EventHandler OnGameDoubleClick;
        public GameList()
        {
            InitializeComponent();
            Program.GamesRepository.GameInstalled += new EventHandler(GamesRepository_GameInstalled);
        }
        private void Reload_Game_List()
        {
            stackPanel1.Children.Clear();
            foreach (Octgn.Data.Game g in Program.GamesRepository.AllGames)
            {
                GameListItem gs = new GameListItem();
                gs.Game = g;
                gs.MouseDoubleClick += new MouseButtonEventHandler(gs_MouseDoubleClick);
                stackPanel1.Children.Add(gs);
            }            
        }
        void GamesRepository_GameInstalled(object sender, EventArgs e)
        {
            Reload_Game_List();
        }

        void gs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GameListItem gs = (GameListItem) sender;
            if (OnGameDoubleClick != null)
            {
                OnGameDoubleClick.Invoke(gs.Game,new EventArgs());
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            Reload_Game_List();
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Program.GamesRepository.GameInstalled -= GamesRepository_GameInstalled;
        }
    }
}
