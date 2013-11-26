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
using Microsoft.Win32;
using Octgn.Core;

namespace Octgn.DeckBuilder
{
    using System.Collections.Generic;
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

        private bool exitOnClose;

        public bool AlwaysShowProxy
        {
            get { return (bool)GetValue(AlwaysShowProxyProperty); }
            set { SetValue(AlwaysShowProxyProperty, value); }
        }

        public bool hideResultCount
        {
            get { return Octgn.Core.Prefs.HideResultCount; }
            set 
            { 
                Octgn.Core.Prefs.HideResultCount = value;
                foreach (SearchControl sc in searchTabs.Items)
                {
                    sc.UpdateCount();
                }
            }
        }

        public static readonly DependencyProperty AlwaysShowProxyProperty =
            DependencyProperty.Register("AlwaysShowProxy", typeof(bool), typeof(DeckBuilderWindow),
                                        new UIPropertyMetadata(false));

        public DeckBuilderWindow(IDeck deck = null, bool exitOnClose = false)
        {
            this.exitOnClose = exitOnClose;
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
            catch (Exception e)
            {
                Log.Error("", e);
                TopMostMessageBox.Show(
                    "There was an unexpected error. Please try restarting and trying again.\n If that doesn't help please let us know on our site http://www.octgn.net",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

            }
            Version oversion = Assembly.GetExecutingAssembly().GetName().Version;
            newSubMenu.ItemsSource = GameManager.Get().Games;
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
                    Log.Error("Unable to load plugin " + p.Name, e);
                }

            }
            if (deck != null)
            {
                if (deck is MetaDeck)
                {
                    this._deckFilename = (deck as MetaDeck).Path;
                }
                var g = GameManager.Get().Games.FirstOrDefault(x => x.Id == deck.GameId);
                if(g == null)this.Close();
                LoadDeck(deck);
                Game = g;
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
            var search = (SearchControl)((FrameworkElement)e.OriginalSource).DataContext;
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
            var ctrl = new SearchControl(Game,this) { SearchIndex = Searches.Count == 0 ? 1 : Searches.Max(x => x.SearchIndex) + 1 };
            ctrl.CardAdded += AddResultCard;
            ctrl.CardRemoved += RemoveResultCard;
            ctrl.CardSelected += CardSelected;
            LoadFonts(ctrl.resultsGrid);
            Searches.Add(ctrl);
            searchTabs.SelectedIndex = Searches.Count - 1;
        }

        #endregion

        private void AlwaysShowProxyCommand(object sender, ExecutedRoutedEventArgs e)
        {
            AlwaysShowProxy = !AlwaysShowProxy;

            cardImageControl.AlwaysShowProxy = this.AlwaysShowProxy;
        }

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
                OnPropertyChanged("DeckSections");
                OnPropertyChanged("DeckSharedSections");
            }
        }

        public IEnumerable<ObservableSection> DeckSections
        {
            get
            {
                if (_deck == null) return new List<ObservableSection>();
                return _deck.Sections.OfType<ObservableSection>().Where(x => x.Shared == false);
            }
        }

        public IEnumerable<ObservableSection> DeckSharedSections
        {
            get
            {
                if (_deck == null) return new List<ObservableSection>();
                return _deck.Sections.OfType<ObservableSection>().Where(x => x.Shared == true);
            }
        }

        public bool IsGameLoaded
        {
            get
            {
                return Game != null;
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
                Searches.Clear();
                try
                {
                    AddSearchTab();

                }
                catch (Exception e)
                {
                    Log.Error("", e);
                    throw new UserMessageException("There was an error. Try restarting!", e);
                }
                OnPropertyChanged("Game");
                OnPropertyChanged("IsGameLoaded");
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
            if (!Prefs.UseGameFonts)
                return;
            if (Game.Fonts.Count > 0)
            {
                foreach (Font font in Game.Fonts)
                {
                    if (font.Target.ToLower().Equals("deckeditor"))
                    {
                        if (!File.Exists(font.Src))
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
                TopMostMessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Deck.Save(_game, sfd.FileName);
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
            LoadDeck();
        }        

        private void LoadClicked(object sender, RoutedEventArgs e)
        {
            LoadDeck();
        }

        private void LoadDeck()
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
                              InitialDirectory = new Game().GetDefaultDeckPath()
                          };
            if (ofd.ShowDialog() != true) return;

            // Try to load the file contents
            ObservableDeck newDeck;
            try
            {
                newDeck = new Deck().Load(ofd.FileName).AsObservable();
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
            Game = null; // Close DB if required
            WindowManager.DeckEditor = null;
            if (this.exitOnClose)
            {
                Program.Exit();
            }
        }

        private void CardSelected(object sender, SearchCardImageEventArgs e)
        {
            if (e.Image == null) return;
            selection = e.Image;
            set_id = e.SetId;
            var cardid = e.CardId;
            var set = SetManager.Get().GetById(e.SetId);
            var card = set.Cards.FirstOrDefault(x => x.Id == cardid);
            cardImageControl.Card.SetCard(card.Clone());
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
            var grid = (DataGrid)sender;
            var element = (IMultiCard)grid.SelectedItem;
            if (element == null) return;

            // jods used a Switch statement here. I needed to check conditions of multiple keys.
            int items = grid.Items.Count - 1;
            int moveUp = grid.SelectedIndex - 1;
            int moveDown = grid.SelectedIndex + 1;
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.KeyboardDevice.IsKeyDown(Key.Add))
            {
                _unsaved = true;
                if (moveDown <= items)
                    ActiveSection.Cards.Move(element, moveDown);
                grid.Focus();
                grid.ScrollIntoView(element);
                e.Handled = true;
            }
            else if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.KeyboardDevice.IsKeyDown(Key.Subtract))
            {
                _unsaved = true;
                if (moveUp >= 0)
                    ActiveSection.Cards.Move(element, moveUp);
                grid.Focus();
                grid.ScrollIntoView(element);
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
            ActiveSection = (ObservableSection)((FrameworkElement)sender).DataContext;
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
            //if (selection == null) return;
            //var bim = new BitmapImage();
            //bim.BeginInit();
            //bim.CacheOption = BitmapCacheOption.OnLoad;
            //var set = SetManager.Get().GetById(set_id);
            //bim.UriSource = set.GetPictureUri(selection) ?? Game.GetCardBackUri();
            ////bim.UriSource = CardModel.GetPictureUri(Game, set_id, selection);
            //bim.EndInit();
            //cardImage.Source = bim;
        }

        private DataGridRow activeRow;
        private ObservableSection dragSection;

        private bool dragging;

        private void DeckCardMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;
            var ansc = FindAncestor<Expander>((FrameworkElement)sender);
            if (ansc == null) return;

            activeRow = FindAncestor<DataGridRow>((DependencyObject)e.OriginalSource);
            dragSection = (ObservableSection)ansc.DataContext;
            if (activeRow != null)
            {
                int cardIndex = activeRow.GetIndex();
                var getCard = dragSection.Cards.ElementAt(cardIndex);
                CardSelected(sender, new SearchCardImageEventArgs { SetId = getCard.SetId, Image = getCard.ImageUri, CardId = getCard.Id });
            }
        }
        private void PickUpDeckCard(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            if (MouseButtonState.Pressed.Equals(e.LeftButton) && activeRow != null && !dragging)
            {
                try
                {
                    ObservableMultiCard getCard = (ObservableMultiCard) activeRow.Item;
                    DataObject dragCard = new DataObject("Card", getCard.ToMultiCard(getCard.Quantity));
                    dragging = true;
                    if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Shift || getCard.Quantity <= 1)
                    {
                        dragSection.Cards.RemoveCard(getCard);
                        DragDrop.DoDragDrop(activeRow, dragCard, DragDropEffects.All);
                    }
                    else
                    {
                        RemoveResultCard(null, new SearchCardIdEventArgs { CardId = getCard.Id });
                        DragDrop.DoDragDrop(activeRow, dragCard, DragDropEffects.Copy);
                    }
                    dragging = false;

                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
            activeRow = null;
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

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
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
            _unsaved = true;
        }

        public IDeck GetLoadedDeck()
        {
            return Deck;
        }

        #endregion

        private void SaveSearchClick(object sender, RoutedEventArgs e)
        {
            if ((SubscriptionModule.Get().IsSubscribed ?? false) == false)
            {
                var res =
                    TopMostMessageBox.Show(
                        "Bummer. This is a subscriber only feature...If you would like to enable this, please visit http://www.octgn.net and subscribe, I know I would!"
                        + Environment.NewLine + "Would you like to go there now?",
                        "Oh No!",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation);
                if (res == MessageBoxResult.Yes)
                {
                    var url = SubscriptionModule.Get().GetSubscribeUrl(new SubType() { Description = "", Name = "" });
                    if (url != null)
                    {
                        Program.LaunchUrl(url);
                    }
                }
                return;
            }
            // Grab the selected search tab
            if (searchTabs.SelectedItem == null) return;
            if (!IsGameLoaded) return;
            var search = searchTabs.SelectedItem as SearchControl;
            var save = SearchSave.Create(search);
            if (save.Save())
            {
                search.FileName = save.FileName;
                search.SearchName = save.Name;
            }
        }

        private void SaveSearchAsClick(object sender, RoutedEventArgs e)
        {
            if ((SubscriptionModule.Get().IsSubscribed ?? false) == false)
            {
                var res =
                    TopMostMessageBox.Show(
                        "Bummer. This is a subscriber only feature...If you would like to enable this, please visit http://www.octgn.net and subscribe, I know I would!"
                        + Environment.NewLine + "Would you like to go there now?",
                        "Oh No!",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation);
                if (res == MessageBoxResult.Yes)
                {
                    var url = SubscriptionModule.Get().GetSubscribeUrl(new SubType() { Description = "", Name = "" });
                    if (url != null)
                    {
                        Program.LaunchUrl(url);
                    }
                }
                return;
            }
            // Grab the selected search tab
            if (searchTabs.SelectedItem == null) return;
            if (!IsGameLoaded) return;
            var search = searchTabs.SelectedItem as SearchControl;
            var save = SearchSave.Create(search);
            if (save.SaveAs())
            {
                search.FileName = save.FileName;
                search.SearchName = save.Name;
            }
        }

        private void LoadSearchClick(object sender, RoutedEventArgs e)
        {
            if ((SubscriptionModule.Get().IsSubscribed ?? false) == false)
            {
                var res =
                    TopMostMessageBox.Show(
                        "Bummer. This is a subscriber only feature...If you would like to enable this, please visit http://www.octgn.net and subscribe, I know I would!"
                        + Environment.NewLine + "Would you like to go there now?",
                        "Oh No!",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation);
                if (res == MessageBoxResult.Yes)
                {
                    var url = SubscriptionModule.Get().GetSubscribeUrl(new SubType() { Description = "", Name = "" });
                    if (url != null)
                    {
                        Program.LaunchUrl(url);
                    }
                }
                return;
            }
            if (!IsGameLoaded) return;
            var save = SearchSave.Load();
            if (save == null) return;

            var game = GameManager.Get().GetById(save.GameId);
            if (game == null)
            {
                TopMostMessageBox.Show("You don't have the game for this search installed", "Oh No", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (Game.Id != save.GameId)
            {
                TopMostMessageBox.Show(
                    "This search is for the game " + game.Name + ". You currently have the game " + Game.Name
                    + " loaded so you can not load this search.", "Oh No", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var ctrl = new SearchControl(Game, save) { SearchIndex = Searches.Count == 0 ? 1 : Searches.Max(x => x.SearchIndex) + 1 };
            ctrl.CardAdded += AddResultCard;
            ctrl.CardRemoved += RemoveResultCard;
            ctrl.CardSelected += CardSelected;
            LoadFonts(ctrl.resultsGrid);
            Searches.Add(ctrl);
            searchTabs.SelectedIndex = Searches.Count - 1;
        }

        private void NotesTextChanged(object sender, TextChangedEventArgs e)
        {
            _unsaved = true;
            Deck.Notes = (sender as TextBox).Text;
        }

        private void ShareDeckClicked(object sender, RoutedEventArgs e)
        {
            if (Deck == null) return;
            if (Deck.CardCount() == 0) return;
            var dlg = new ShareDeck(Deck);
            dlg.ShowDialog();
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