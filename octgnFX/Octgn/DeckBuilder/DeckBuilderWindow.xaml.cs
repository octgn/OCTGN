using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Octgn.DeckBuilder
{
    using System.Windows.Controls.Primitives;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.Core.Plugin;
    using Octgn.DataNew.Entities;
    using Octgn.Library.Exceptions;
    using Octgn.Library.Plugin;
    using Octgn.Windows;

    using log4net;
    using Octgn.Controls;

    public partial class DeckBuilderWindow : INotifyPropertyChanged, IDeckBuilderPluginController
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ObservableDeck _deck;
        private string _deckFilename;
        private Game _game;
        private ObservableSection _section;
        private bool _unsaved;
        private string selection = null;
        private Guid set_id;

        public DeckBuilderWindow()
        {
            Searches = new ObservableCollection<SearchControl>();
            InitializeComponent();
            // If there's only one game in the repository, create a deck of the correct kind
            try
            {
                if (GameManager.Get().GameCount == 1)
                {
                    var g = GameManager.Get().Games.First();
                    if (g.SharedDeckSections.Count > 0 || g.DeckSections.Count > 0)
                    {
                        Game = g;
                        Deck = Game.CreateDeck().AsObservable();
                        _deckFilename = null;
                    }

                }

            }
            catch (UserMessageException e)
            {
                TopMostMessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch(Exception e) 
            {
                Log.Error("",e);
                TopMostMessageBox.Show(
                    "There was an unexpected error. Please try restarting and trying again.\n If that doesn't help please let us know on our site http://www.octgn.net",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

            }
            Version oversion = Assembly.GetExecutingAssembly().GetName().Version;
            newSubMenu.ItemsSource = GameManager.Get().Games;
            loadSubMenu.ItemsSource = GameManager.Get().Games;
            //Title = "Octgn Deck Editor  version " + oversion;

            var deplugins = PluginManager.GetPlugins<IDeckBuilderPlugin>();
            foreach (var p in deplugins)
            {
                try
                {
                    p.OnLoad(GameManager.Get());
                    foreach (var m in p.MenuItems)
                    {
                        var mi = new MenuItem() { Header = m.Name };
                        var m1 = m;
                        mi.Click += (sender, args) =>
                            {
                                try
                                {
                                    m1.OnClick(this);
                                }
                                catch (Exception e)
                                {
                                    new ErrorWindow(e).Show();
                                }
                            };
                        MenuPlugins.Items.Add(mi);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Unable to load plugin " + p.Name,e);
                }

            }
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
            LoadFonts(ctrl.resultsGrid);
            Searches.Add(ctrl);
            searchTabs.SelectedIndex = Searches.Count - 1;
        }

        #endregion

        public ObservableDeck Deck
        {
            get { return _deck; }
            set
            {
                if (_deck == value) return;
                _deck = value;
                _unsaved = false;
                ActiveSection = value.Sections.FirstOrDefault() as ObservableSection;
                OnPropertyChanged("Deck");
            }
        }

        private DataNew.Entities.Game Game
        {
            get { return _game; }
            set
            {
                if (_game == value || value == null)
                {
                    cardImageControl.SetGame(new Game() { Name = "No Game Selected", CardBack = "pack://application:,,,/Resources/Back.jpg" });
                    return;
                }

                _game = value;
                cardImageControl.SetGame(_game);
                ActiveSection = null;
                var bim = new BitmapImage();
                bim.BeginInit();
                bim.CacheOption = BitmapCacheOption.OnLoad;
                bim.UriSource = value.GetCardBackUri();
                bim.EndInit();
                cardImage.Source = bim;
                Searches.Clear();
                try
                {
                    AddSearchTab();

                }
                catch (Exception e)
                {
                    Log.Error("",e);
                    throw new UserMessageException("There was an error. Try restarting!", e);
                }
                OnPropertyChanged("Game");
            }
        }

        public ObservableSection ActiveSection
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
                if (GameManager.Get().GameCount == 1) Game = GameManager.Get().Games.First();
                else
                {
                    TopMostMessageBox.Show("You have to select a game before you can use this command.", "Error",
                                    MessageBoxButton.OK);
                    return;
                }
                //if (Program.GamesRepository.Games.Count == 1)
                //    Game = Program.GamesRepository.Games[0];
                //else
                //{
                //    MessageBox.Show("You have to select a game before you can use this command.", "Error",
                //                    MessageBoxButton.OK);
                //    return;
                //}
            }
            Deck = Game.CreateDeck().AsObservable();
            //Deck = new Deck(Game);
            _deckFilename = null;
        }

        private void NewClicked(object sender, RoutedEventArgs e)
        {
            var game = (DataNew.Entities.Game)((MenuItem)e.OriginalSource).DataContext;
            if (game.DeckSections.Count == 0 && game.SharedDeckSections.Count == 0)
            {
                TopMostMessageBox.Show(
                    "This game has no deck sections, so you cannot build a deck for it.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
            if (_unsaved)
            {
                MessageBoxResult result = TopMostMessageBox.Show("This deck contains unsaved modifications. Save?", "Warning",
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
            Game = game;
            CommandManager.InvalidateRequerySuggested();
            Deck = Game.CreateDeck().AsObservable();
            //Deck = new Deck(Game);
            _deckFilename = null;
        }

        private void LoadFonts(Control control)
        {
            if (Game == null)
            {
                return;
            }
            if (Game.Fonts.Count > 0)
            {
                foreach (Font font in Game.Fonts)
                {
                    if (font.Target.ToLower().Equals("deckeditor"))
                    {
                        if(!File.Exists(font.Src))
                        {
                            return;
                        }
                        System.Drawing.Text.PrivateFontCollection pfc = new System.Drawing.Text.PrivateFontCollection();
                        control.FontSize = font.Size; 
                        pfc.AddFontFile(font.Src);
                        string font1 = "file:///" + Path.GetDirectoryName(font.Src) + "/#" + pfc.Families[0].Name;
                        control.FontFamily = new System.Windows.Media.FontFamily(font1.Replace("\\", "/"));
                    }
                }
            }
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
                Deck.Save(_game, _deckFilename);
                _unsaved = false;
            }
            catch (UserMessageException ex)
            {
                TopMostMessageBox.Show(ex.Message, "Error",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAs()
        {
            var sfd = new SaveFileDialog
                          {
                              AddExtension = true,
                              Filter = "Octgn decks|*.o8d",
                              InitialDirectory = Game.GetDefaultDeckPath()
                          };
            if (!sfd.ShowDialog().GetValueOrDefault()) return;
            try
            {
                Deck.Save(_game,sfd.FileName);
                _unsaved = false;
                _deckFilename = sfd.FileName;
            }
            catch (UserMessageException ex)
            {
                TopMostMessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDeckCommand(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            LoadDeck(Game);
        }

        private void LoadClicked(object sender, RoutedEventArgs e)
        {
            var game = (DataNew.Entities.Game) ((MenuItem) e.OriginalSource).DataContext;
            LoadDeck(game);
        }

        private void LoadDeck(DataNew.Entities.Game game)
        {
            if (_unsaved)
            {
                MessageBoxResult result = TopMostMessageBox.Show("This deck contains unsaved modifications. Save?", "Warning",
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
                              InitialDirectory = game.GetDefaultDeckPath()
                          };
            if (ofd.ShowDialog() != true) return;

            // Try to load the file contents
            ObservableDeck newDeck;
            try
            {
                newDeck = new Deck().Load(game,ofd.FileName).AsObservable();
                Game = GameManager.Get().Games.First(x => x.Id == newDeck.GameId);
            }
            catch (UserMessageException ex)
            {
                TopMostMessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                TopMostMessageBox.Show("Octgn couldn't load the deck.\r\nDetails:\r\n\r\n" + ex.Message, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //Game = Program.GamesRepository.Games.First(g => g.Id == newDeck.GameId);
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
                MessageBoxResult result = TopMostMessageBox.Show("This deck contains unsaved modifications. Save?", "Warning",
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
            if(cardImage != null)
                cardImage.Source = null;
            cardImage = null;
            Game = null; // Close DB if required
            WindowManager.DeckEditor = null;
        }

        private void CardSelected(object sender, SearchCardImageEventArgs e)
        {
            selection = e.Image;
            set_id = e.SetId;
            var cardid = e.CardId;
            var bim = new BitmapImage();
            bim.BeginInit();
            bim.CacheOption = BitmapCacheOption.OnLoad;

            try
            {
                var set = SetManager.Get().GetById(e.SetId);
                var card = set.Cards.FirstOrDefault(x => x.Id == cardid);
                cardImageControl.Card.SetCard(card.Clone());
                //var card = CardManager.Get().GetCardById(cardid);
                var uri = card.GetPicture();
                if(uri != null)
                    bim.UriSource = new Uri(uri);
                else
                {
                    bim.UriSource = Game.GetCardBackUri();
                }
                //bim.UriSource = e.Image != null ? new Uri(card.GetPicture()) : Game.GetCardBackUri();
                //bim.UriSource = e.Image != null ? CardModel.GetPictureUri(Game, e.SetId, e.Image) : Game.GetCardBackUri();
                bim.EndInit();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Error loading picture uri from game pack: " + ex.ToString());
                bim = new BitmapImage();
                bim.CacheOption = BitmapCacheOption.OnLoad;
                bim.BeginInit();
                bim.UriSource = new Uri(@"pack://application:,,,/Octgn;component/Resources/Front.jpg");
                bim.EndInit();
                
            }
            cardImage.Source = bim;
        }

        private void ElementSelected(object sender, SelectionChangedEventArgs e)
        {
            var grid = (DataGrid)sender;
            var element = (ICard)grid.SelectedItem;

            // Don't hide the picture if the selected element was removed 
            // with a keyboard shortcut from the results grid
            if (element == null && !grid.IsFocused) return;
            var nc = element.ToMultiCard();
            var sc = new Card()
                         {
                             ImageUri = nc.ImageUri,
                             Name = nc.Name,
                             Alternate = nc.Alternate,
                             Id = nc.Id,
                             Properties =
                                 nc.Properties.ToDictionary(
                                     x => x.Key.Clone() as string, x => x.Value.Clone() as CardPropertySet),
                             SetId = nc.SetId
                         };
            cardImageControl.Card.SetCard(sc);
            selection = element.ImageUri;
            set_id = element.GetSet().Id;

            var bim = new BitmapImage();
            bim.BeginInit();
            bim.CacheOption = BitmapCacheOption.OnLoad;
            bim.UriSource = String.IsNullOrWhiteSpace(element.GetPicture()) ? Game.GetCardBackUri() : new Uri(element.GetPicture());
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
            var element = ActiveSection.Cards.FirstOrDefault(c => c.Id == e.CardId);
            if (element != null)
                element.Quantity += 1;
            else
            {
                var card = Game.GetCardById(e.CardId);
                ActiveSection.Cards.AddCard(card.ToMultiCard());
                this.InvalidateVisual();
            }
        }

        private void RemoveResultCard(object sender, SearchCardIdEventArgs e)
        {
            _unsaved = true;
            var element = ActiveSection.Cards.FirstOrDefault(c => c.Id == e.CardId);
            if (element == null) return;
            element.Quantity -= 1;
            if (element.Quantity <= 0)
                ActiveSection.Cards.RemoveCard(element);
            this.InvalidateVisual();
        }

        private void DeckKeyDownHandler(object sender, KeyEventArgs e)
        {
            var grid = (DataGrid) sender;
            var element = (IMultiCard)grid.SelectedItem;
            if (element == null) return;

            // jods used a Switch statement here. I needed to check conditions of multiple keys.
            int items = grid.Items.Count - 1;
            int moveUp = grid.SelectedIndex - 1;
            int moveDown = grid.SelectedIndex + 1;
            //TODO [DB MIGRATION]  Reimplement whatever this is
            //if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.KeyboardDevice.IsKeyDown(Key.Add))
            //{
            //    _unsaved = true;
            //    if (moveDown <= items)
            //        ActiveSection.Cards.Move(grid.SelectedIndex, moveDown);
            //    grid.Focus();
            //    e.Handled = true;
            //}
            //else if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.KeyboardDevice.IsKeyDown(Key.Subtract))
            //{
            //    _unsaved = true;
            //    if (moveUp >= 0)
            //        ActiveSection.Cards.Move(grid.SelectedIndex, moveUp);
            //    grid.Focus();
            //    e.Handled = true;
            //}
            //else 
                if (e.KeyboardDevice.IsKeyDown(Key.Add) || e.KeyboardDevice.IsKeyDown(Key.Insert))
            {
                _unsaved = true;
                element.Quantity += 1;
                e.Handled = true;
            }
            else if (e.KeyboardDevice.IsKeyDown(Key.Delete) || e.KeyboardDevice.IsKeyDown(Key.Subtract))
            {
                _unsaved = true;
                element.Quantity -= 1;
                if (element.Quantity <= 0) ActiveSection.Cards.RemoveCard(element);
                e.Handled = true;
            }
		}

        private void DeckSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }		

        private void ElementEditEnd(object sender, DataGridCellEditEndingEventArgs e)
        {
            _unsaved = true;
            var tb = e.EditingElement as TextBox;
            if (tb == null) return;
            int val = -1;
            if (int.TryParse(tb.Text, out val))
            {
                if (val <= 0)
                {
                    var ic = sender as Selector;
                    if (ic == null) return;
                    var item = ic.SelectedItem as IMultiCard;
                    if (item == null) return;
                    ActiveSection.Cards.RemoveCard(item);
                }
            }

        }

        private void SetActiveSection(object sender, RoutedEventArgs e)
        {
            ActiveSection = (ObservableSection) ((FrameworkElement) sender).DataContext;
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
            var bim = new BitmapImage();
            bim.BeginInit();
            bim.CacheOption = BitmapCacheOption.OnLoad;
            var set = SetManager.Get().GetById(set_id);
            bim.UriSource = set.GetPictureUri(selection) ?? Game.GetCardBackUri();
            //bim.UriSource = CardModel.GetPictureUri(Game, set_id, selection);
            bim.EndInit();
            cardImage.Source = bim;
        }

        private DataGridRow activeCard;
        private ObservableSection dragSection;
        private void DeckCardMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //Hack try catch, this stems from concurrency issues I believe.
                activeCard = FindAncestor<DataGridRow>((DependencyObject)e.OriginalSource);
                if (sender == null) return; //(from above) Somehow we get a null here
                var ansc = FindAncestor<Expander>((FrameworkElement)sender);
                if (ansc == null) return;
                dragSection = (ObservableSection)ansc.DataContext;
                if (activeCard != null)
                {
                    int cardIndex = activeCard.GetIndex();
                    var getCard = dragSection.Cards.ElementAt(cardIndex);
                    CardSelected(sender, new SearchCardImageEventArgs { SetId = getCard.SetId, Image = getCard.ImageUri, CardId = getCard.Id });
                }

            }
            catch (Exception ex)
            {
                Log.Warn("DeckCardMouseDown",ex);
            }
        }
        private void PickUpDeckCard(object sender, MouseEventArgs e)
        {
            if (MouseButtonState.Pressed.Equals(e.LeftButton) && activeCard != null)
            {
                try
                {
                    int cardIndex = activeCard.GetIndex();
                    var clist = dragSection.Cards.ToArray();
                    if (cardIndex >= clist.Length) return;
                    var getCard = clist[cardIndex];
                    DataObject dragCard = new DataObject("Card", getCard.ToMultiCard(getCard.Quantity));
                    if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Shift || getCard.Quantity <= 1)
                    {
                        dragSection.Cards.RemoveCard(getCard);
                        DragDrop.DoDragDrop(activeCard, dragCard, DragDropEffects.All);
                    }
                    else
                    {
                        RemoveResultCard(null, new SearchCardIdEventArgs { CardId = getCard.Id });
                        DragDrop.DoDragDrop(activeCard, dragCard, DragDropEffects.Copy);
                    }

                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
            activeCard = null;
        }
        private void DeckDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Card"))
            {
                e.Effects = DragDropEffects.None;
            }
        }
        private void DeckDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Card"))
            {
                _unsaved = true;
                var dragCard = e.Data.GetData("Card") as IMultiCard;
                ObservableSection dropSection = (ObservableSection)((FrameworkElement)sender).DataContext;
                var element = dropSection.Cards.FirstOrDefault(c => c.Id == dragCard.Id);
                    if (e.Effects == DragDropEffects.Copy) dragCard.Quantity = 1;
                    if (element != null)
                    {
                        element.Quantity = (byte)(element.Quantity + dragCard.Quantity);
                    }
                    else
                    {
                        dropSection.Cards.AddCard(dragCard);
                    }
            }
            e.Handled = true;
        }
        private static T FindAncestor<T>(DependencyObject Current)
            where T : DependencyObject
        {
            do
            {
                if (Current is T)
                {
                    return (T)Current;
                }
                Current = System.Windows.Media.VisualTreeHelper.GetParent(Current);
            }
            while (Current != null);
            return null;
        }
        #region IDeckBuilderPluginController
        public GameManager Games
        {
            get
            {
                return GameManager.Get();
            }
        }

        public void SetLoadedGame(Game game)
        {
            Game = game;
        }

        public Game GetLoadedGame()
        {
            return Game;
        }

        public void LoadDeck(IDeck deck)
        {
            Deck = deck.AsObservable();
        }

        public IDeck GetLoadedDeck()
        {
            return Deck;
        }

        #endregion 
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