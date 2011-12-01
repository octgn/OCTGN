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
using System.IO;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for GameList.xaml
    /// </summary>
    public partial class GameList : Page
    {
        public event EventHandler OnGameClick;
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
                //gs.MouseDoubleClick += new MouseButtonEventHandler(gs_MouseDoubleClick);
                gs.MouseUp += new MouseButtonEventHandler(gs_MouseUp);
                stackPanel1.Children.Add(gs);
            }            
        }

        void gs_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GameListItem gs = (GameListItem)sender;
            if (OnGameClick != null)
            {
                OnGameClick.Invoke(gs.Game, new EventArgs());
            }
        }
        void GamesRepository_GameInstalled(object sender, EventArgs e)
        {
            Reload_Game_List();
        }

        void gs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GameListItem gs = (GameListItem) sender;
            if (OnGameClick != null)
            {
                OnGameClick.Invoke(gs.Game,new EventArgs());
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
        public void Install_Game()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Game definition files (*.o8g)|*.o8g";
            if (ofd.ShowDialog() != true) return;

            //Fix def filename
            String newFilename = Uri.UnescapeDataString(ofd.FileName);
            if (!newFilename.ToLower().Equals(ofd.FileName.ToLower()))
            {
                File.Move(ofd.FileName, newFilename);
            }

            try
            {
                // Open the archive
                Definitions.GameDef game = Definitions.GameDef.FromO8G(newFilename);
                if (!game.CheckVersion()) return;

                // Check if the game already exists
                if (Program.GamesRepository.Games.Any(g => g.Id == game.Id))
                    if (MessageBox.Show("This game already exists.\r\nDo you want to overwrite it?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.Yes)
                        return;

                var gameData = new Data.Game()
                {
                    Id = game.Id,
                    Name = game.Name,
                    Filename = newFilename,
                    Version = game.Version,
                    CardWidth = game.CardDefinition.Width,
                    CardHeight = game.CardDefinition.Height,
                    CardBack = game.CardDefinition.back,
                    DeckSections = game.DeckDefinition.Sections.Keys,
                    SharedDeckSections = game.SharedDeckDefinition == null ? null : game.SharedDeckDefinition.Sections.Keys
                };
                Program.GamesRepository.InstallGame(gameData, game.CardDefinition.Properties.Values);
            }
            catch (System.IO.FileFormatException)
            {
                //Removed ex.Message. The user doesn't need to see the exception
                MessageBox.Show("Your game definition file is corrupt. Please redownload it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
