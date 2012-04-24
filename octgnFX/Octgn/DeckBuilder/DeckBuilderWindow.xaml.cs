using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Octgn.Data;

namespace Octgn.DeckBuilder
{
    public partial class DeckBuilderWindow : INotifyPropertyChanged
    {
        private Deck _deck;
        private string _deckFilename;
        private Data.Game _game;
        private Deck.Section _section;
        private bool _unsaved;
        private string selection = null;
        private Guid set_id;

        public DeckBuilderWindow()
        {
            Searches = new ObservableCollection<SearchControl>();
            InitializeComponent();
            // If there's only one game in the repository, create a deck of the correct kind
            if (Program.GamesRepository.Games.Count == 1)
            {
                Game = Program.GamesRepository.Games[0];
                Deck = new Deck(Game);
                _deckFilename = null;
            }
            Version oversion = Assembly.GetExecutingAssembly().GetName().Version;
            newSubMenu.ItemsSource = Program.GamesRepository.Games;
            loadSubMenu.ItemsSource = Program.GamesRepository.Games;
            Title = "Octgn Deck Editor  version " + oversion;
        }

        #region Search tabs

        private void OpenTabCommand(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            AddSearchTab();
            CommandManager.InvalidateRequerySuggested();
        }

        private void CloseTabCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var search = (SearchControl) ((FrameworkElement) e.OriginalSource).DataContext;
            Searches.Remove(search);
            CommandManager.InvalidateRequerySuggested();
        }

        private void CanCloseTab(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = Searches.Count > 1;
        }

        private void AddSearchTab()
        {
            if (Game == null) //ralig - issue 46
                return;
            var ctrl = new SearchControl(Game)
                           {SearchIndex = Searches.Count == 0 ? 1 : Searches.Max(x => x.SearchIndex) + 1};
            ctrl.CardAdded += AddResultCard;
            ctrl.CardRemoved += RemoveResultCard;
            ctrl.CardSelected += CardSelected;
            Searches.Add(ctrl);
            searchTabs.SelectedIndex = Searches.Count - 1;
        }

        #endregion

        public Deck Deck
        {
            get { return _deck; }
            set
            {
                if (_deck == value) return;
                _deck = value;
                _unsaved = false;
                ActiveSection = value.Sections.FirstOrDefault();
                OnPropertyChanged("Deck");
            }
        }

        private Data.Game Game
        {
            get { return _game; }
            set
            {
                if (_game == value || value == null ) return;

                _game = value;

                ActiveSection = null;
                var bim = new BitmapImage();
                bim.BeginInit();
                bim.CacheOption = BitmapCacheOption.OnLoad;
                bim.UriSource = value.GetCardBackUri();
                bim.EndInit();
                cardImage.Source = bim;
                Searches.Clear();
                AddSearchTab();
            }
        }

        public Deck.Section ActiveSection
        {
            get { return _section; }
            set
            {
                if (_section == value) return;
                _section = value;
                OnPropertyChanged("ActiveSection");
            }
        }

        public ObservableCollection<SearchControl> Searches { get; private set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void NewDeckCommand(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (Game == null)
            {
                if (Program.GamesRepository.Games.Count == 1)
                    Game = Program.GamesRepository.Games[0];
                else
                {
                    MessageBox.Show("You have to select a game before you can use this command.", "Error",
                                    MessageBoxButton.OK);
                    return;
                }
            }
            Deck = new Deck(Game);
            _deckFilename = null;
        }

        private void NewClicked(object sender, RoutedEventArgs e)
        {
            if (_unsaved)
            {
                MessageBoxResult result = MessageBox.Show("This deck contains unsaved modifications. Save?", "Warning",
                                                          MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        return;
                }
            }
            Game = (Data.Game) ((MenuItem) e.OriginalSource).DataContext;
            CommandManager.InvalidateRequerySuggested();
            Deck = new Deck(Game);
            _deckFilename = null;
        }

        private void SaveDeck(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            Save();
        }

        private void SaveDeckAsHandler(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            SaveAs();
        }

        private void Save()
        {
            if (_deckFilename == null)
            {
                SaveAs();
                return;
            }
            try
            {
                Deck.Save(_deckFilename);
                _unsaved = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured while trying to save the deck:\n" + ex.Message, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAs()
        {
            var sfd = new SaveFileDialog
                          {
                              AddExtension = true,
                              Filter = "Octgn decks|*.o8d",
                              InitialDirectory =
                                  Prefs.LastFolder == ""
                                      ? Game.DefaultDecksPath
                                      : Prefs.LastFolder
                          };
            if (!sfd.ShowDialog().GetValueOrDefault()) return;
            try
            {
                Deck.Save(sfd.FileName);
                _unsaved = false;
                _deckFilename = sfd.FileName;
                Prefs.LastFolder = Path.GetDirectoryName(_deckFilename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured while trying to save the deck:\n" + ex.Message, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDeckCommand(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            LoadDeck(Game);
        }

        private void LoadClicked(object sender, RoutedEventArgs e)
        {
            var game = (Data.Game) ((MenuItem) e.OriginalSource).DataContext;
            LoadDeck(game);
        }

        private void LoadDeck(Data.Game game)
        {
            if (_unsaved)
            {
                MessageBoxResult result = MessageBox.Show("This deck contains unsaved modifications. Save?", "Warning",
                                                          MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        return;
                }
            }
            // Show the dialog to choose the file
            var ofd = new OpenFileDialog
                          {
                              Filter = "Octgn deck files (*.o8d) | *.o8d",
                              InitialDirectory =
                                  ((game != null) && Prefs.LastFolder == "")
                                      ? game.DefaultDecksPath
                                      : Prefs.LastFolder
                          };
            if (ofd.ShowDialog() != true) return;
            Prefs.LastFolder = Path.GetDirectoryName(ofd.FileName);

            // Try to load the file contents
            Deck newDeck;
            try
            {
                newDeck = Deck.Load(ofd.FileName, Program.GamesRepository);
            }
            catch (DeckException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Octgn couldn't load the deck.\r\nDetails:\r\n\r\n" + ex.Message, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Game = Program.GamesRepository.Games.First(g => g.Id == newDeck.GameId);
            Deck = newDeck;
            _deckFilename = ofd.FileName;
            CommandManager.InvalidateRequerySuggested();
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_unsaved)
            {
                MessageBoxResult result = MessageBox.Show("This deck contains unsaved modifications. Save?", "Warning",
                                                          MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        e.Cancel = true;
                        return;
                }
            }
            cardImage.Source = null;
            cardImage = null;
            Game = null; // Close DB if required
        }

        private void CardSelected(object sender, SearchCardImageEventArgs e)
        {
            selection = e.Image;
            set_id = e.SetId;

            var bim = new BitmapImage();
            bim.BeginInit();
            bim.CacheOption = BitmapCacheOption.OnLoad;
            bim.UriSource = e.Image != null ? CardModel.GetPictureUri(Game, e.SetId, e.Image) : Game.GetCardBackUri(); 
            bim.EndInit();
            cardImage.Source = bim;
        }

        private void ElementSelected(object sender, SelectionChangedEventArgs e)
        {
            var grid = (DataGrid) sender;
            var element = (Deck.Element) grid.SelectedItem;            

            // Don't hide the picture if the selected element was removed 
            // with a keyboard shortcut from the results grid
            if (element == null && !grid.IsFocused) return;

            selection = element.Card.ImageUri;
            set_id = element.Card.Set.Id;

            var bim = new BitmapImage();
            bim.BeginInit();
            bim.CacheOption = BitmapCacheOption.OnLoad;
            bim.UriSource = element != null ? new Uri(element.Card.Picture) : Game.GetCardBackUri();
            bim.EndInit();
            cardImage.Source = bim;

        }

        private void IsDeckOpen(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Deck != null;
            e.Handled = true;
        }

        private void AddResultCard(object sender, SearchCardIdEventArgs e)
        {
            _unsaved = true;
            Deck.Element element = ActiveSection.Cards.FirstOrDefault(c => c.Card.Id == e.CardId);
            if (element != null)
                element.Quantity += 1;
            else
            {
                CardModel Card = Game.GetCardById(e.CardId);
                if (Card.isDependent())
                {
                    MessageBox.Show("Unable to add " + Card.Name +
                       "to the deck. It is marked as dependent, which implies it is the alternate version of another card. Please try to add the original instead.",
                       "Warning: Add dependent card failed.", MessageBoxButton.OK);
                }
                ActiveSection.Cards.Add(new Deck.Element { Card = Game.GetCardById(e.CardId), Quantity = 1 });
            }
        }

        private void RemoveResultCard(object sender, SearchCardIdEventArgs e)
        {
            _unsaved = true;
            Deck.Element element = ActiveSection.Cards.FirstOrDefault(c => c.Card.Id == e.CardId);
            if (element == null) return;
            element.Quantity -= 1;
            if (element.Quantity == 0)
                ActiveSection.Cards.Remove(element);
        }

        private void DeckKeyDownHandler(object sender, KeyEventArgs e)
        {
            var grid = (DataGrid) sender;
            var element = (Deck.Element) grid.SelectedItem;
            if (element == null) return;

            // jods used a Switch statement here. I needed to check conditions of multiple keys.
            int items = grid.Items.Count - 1;
            int moveUp = grid.SelectedIndex - 1;
            int moveDown = grid.SelectedIndex + 1;
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.KeyboardDevice.IsKeyDown(Key.Add))
            {
                _unsaved = true;
                if (moveDown <= items)
                    ActiveSection.Cards.Move(grid.SelectedIndex, moveDown);
                grid.Focus();
                e.Handled = true;
            }
            else if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.KeyboardDevice.IsKeyDown(Key.Subtract))
            {
                _unsaved = true;
                if (moveUp >= 0)
                    ActiveSection.Cards.Move(grid.SelectedIndex, moveUp);
                grid.Focus();
                e.Handled = true;
            }
            else if (e.KeyboardDevice.IsKeyDown(Key.Add) || e.KeyboardDevice.IsKeyDown(Key.Insert))
            {
                _unsaved = true;
                element.Quantity += 1;
                e.Handled = true;
            }
            else if (e.KeyboardDevice.IsKeyDown(Key.Delete) || e.KeyboardDevice.IsKeyDown(Key.Subtract))
            {
                _unsaved = true;
                element.Quantity -= 1;
                e.Handled = true;
            }
		}

        private void DeckSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }		

        private void ElementEditEnd(object sender, DataGridCellEditEndingEventArgs e)
        {
            _unsaved = true;
        }

        private void SetActiveSection(object sender, RoutedEventArgs e)
        {
            ActiveSection = (Deck.Section) ((FrameworkElement) sender).DataContext;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PreventExpanderBehavior(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void cardImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selection == null) return;
            if (cardImage.Source.ToString().Contains(selection))
            {
                string alternate = cardImage.Source.ToString().Replace(".jpg", ".b.jpg");
                try
                {
                    var bim = new BitmapImage();
                    bim.BeginInit();
                    bim.CacheOption = BitmapCacheOption.OnLoad;
                    bim.UriSource = new Uri(alternate);
                    bim.EndInit();
                    cardImage.Source = bim;
                }
                catch
                {
                    var bim = new BitmapImage();
                    bim.BeginInit();
                    bim.CacheOption = BitmapCacheOption.OnLoad;
                    bim.UriSource = Game.GetCardBackUri();
                    bim.EndInit();
                    cardImage.Source = bim;
                }
            }
            else
            {
                var bim = new BitmapImage();
                bim.BeginInit();
                bim.CacheOption = BitmapCacheOption.OnLoad;
                bim.UriSource = CardModel.GetPictureUri(Game, set_id, selection);
                bim.EndInit();
                cardImage.Source = bim;
            }
        }
    }

    public class ActiveSectionConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return Binding.DoNothing;
            return values[0] == values[1] ? FontWeights.Bold : FontWeights.Normal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}