using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Windows.Controls.Ribbon;
using Octgn.DeckBuilder;
using Skylabs.Net;
using System.IO;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : RibbonWindow
    {
        private bool                        _allowDirectNavigation = false;
        private NavigatingCancelEventArgs   _navArgs = null;
        private Duration                    _duration = new Duration(TimeSpan.FromSeconds(1));
        private SetList _currentSetList;
        private void frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if(Content != null && !_allowDirectNavigation)
            {
                e.Cancel = true;

                _navArgs = e;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DoubleAnimation animation0 = new DoubleAnimation();
                    animation0.From = 1;
                    animation0.To = 0;
                    animation0.Duration = _duration;
                    animation0.Completed += SlideCompleted;
                    frame1.BeginAnimation(OpacityProperty, animation0);
                }));
            }
            _allowDirectNavigation = false;
        }

        private void SlideCompleted(object sender, EventArgs e)
        {
            _allowDirectNavigation = true;
            switch(_navArgs.NavigationMode)
            {
                case NavigationMode.New:
                    if(_navArgs.Uri == null)
                        frame1.Navigate(_navArgs.Content);
                    else
                        frame1.Navigate(_navArgs.Uri);
                    break;
                case NavigationMode.Back:
                    frame1.GoBack();
                    break;
                case NavigationMode.Forward:
                    frame1.GoForward();
                    break;
                case NavigationMode.Refresh:
                    frame1.Refresh();
                    break;
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                (ThreadStart)delegate()
                {
                    frame1.UpdateLayout();
                    UpdateLayout();
                    DoubleAnimation animation0 = new DoubleAnimation();
                    animation0.From = 0;
                    animation0.To = 1;
                    animation0.Duration = _duration;
                    frame1.BeginAnimation(OpacityProperty, animation0);
                });
        }

        public Main()
        {
            InitializeComponent();
            frame1.Navigate(new ContactList());
            // Insert code required on object creation below this point.
        }

        private void Ribbon_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RibbonTab tab = Ribbon.SelectedItem as RibbonTab;
            if(tab != null)
            {
                switch((String)tab.Header)
                {
                    case "Lobby":
                        frame1.Navigate(new ContactList());
                        break;
                    case "Games":
                        frame1.Navigate(new GameList());
                        break;
                    case "Sets":
                        GameList gl = new GameList();
                        gl.OnGameDoubleClick += new EventHandler(gl_OnGameDoubleClick);
                        frame1.Navigate(gl);
                        break;
                    case "!":

                        break;
                }
            }
        }

        void gl_OnGameDoubleClick(object sender, EventArgs e)
        {
            Data.Game g = sender as Data.Game;
            _currentSetList = new SetList(g);
            frame1.Navigate(_currentSetList);
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Program.lobbyClient.Close(DisconnectReason.CleanDisconnect);
            Program.Exit();
        }

        private void LogOff_Click(object sender, RoutedEventArgs e)
        {
            if (Program.DeckEditor != null)
                Program.DeckEditor.Close();
            Program.LauncherWindow = new LauncherWindow();
            Program.LauncherWindow.Show();
            Program.ClientWindow.Close();
            Program.lobbyClient.Close(DisconnectReason.CleanDisconnect);            
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
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
            catch (System.IO.FileFormatException ex)
            {
                //Removed ex.Message. The user doesn't need to see the exception
                MessageBox.Show("Your game definition file is corrupt. Please redownload it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RibbonButton_Click_1(object sender, RoutedEventArgs e)
        {
            _currentSetList = null;
            GameList gl = new GameList();
            gl.OnGameDoubleClick += new EventHandler(gl_OnGameDoubleClick);
            frame1.Navigate(gl);
        }

        private void RibbonButton_Click_2(object sender, RoutedEventArgs e)
        {
            if(_currentSetList != null)
            _currentSetList.Deleted_Selected();
        }

        private void bInstallSets_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSetList != null)
            _currentSetList.Install_Sets();
        }

        private void bPatchSets_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSetList != null)
            _currentSetList.Patch_Selected();
        }

        private void bDeckEditor_Click(object sender, RoutedEventArgs e)
        {
            if (Program.GamesRepository.Games.Count == 0)
            {
                MessageBox.Show("You have no game installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Program.DeckEditor == null)
            {
                Program.DeckEditor = new DeckBuilderWindow();
                Program.DeckEditor.Show();
            }
            else if (Program.DeckEditor.IsVisible == false)
            {
                Program.DeckEditor = new DeckBuilderWindow();
                Program.DeckEditor.Show();                
            }
        }
    }
}