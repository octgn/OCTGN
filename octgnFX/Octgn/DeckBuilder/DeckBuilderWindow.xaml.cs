﻿using System;
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
using System.Windows.Navigation;
using Microsoft.Win32;
using Octgn.Core;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using Octgn.Core.DataExtensionMethods;
using Octgn.Core.DataManagers;
using Octgn.Core.Plugin;
using Octgn.DataNew.Entities;
using Octgn.Library.Exceptions;
using Octgn.Library.Plugin;
using Octgn.Site.Api.Models;
using Octgn.Windows;

using log4net;
using Octgn.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using Octgn.Extentions;

namespace Octgn.DeckBuilder
{

    public partial class DeckBuilderWindow : INotifyPropertyChanged, IDeckBuilderPluginController
    {
        internal new static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
                if (g == null) this.Close();
                Game = g;
                LoadDeck(deck);
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
            var ctrl = new SearchControl(Game, this) { SearchIndex = Searches.Count == 0 ? 1 : Searches.Max(x => x.SearchIndex) + 1 };
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

        public DataNew.Entities.Game Game
        {
            get { return _game; }
            private set
            {
                if (_game == value || value == null)
                {
                    cardImageControl.SetGame(new Game() { Name = "No Game Selected", CardSize= new CardSize(){Back ="pack://application:,,,/Resources/Back.jpg" }});
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

        public new event PropertyChangedEventHandler PropertyChanged;

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
            control.SetFont(Game.DeckEditorFont);
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

        private void ExportDeckAsHandler(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            ExportAs();
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

        private void ExportAs()
        {
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                Filter = "Text File|*.txt",
                InitialDirectory = Game.GetDefaultDeckPath()
            };
            if (!sfd.ShowDialog().GetValueOrDefault()) return;
            try
            {
                Deck.ExportAsText(_game, sfd.FileName);
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

        private void showShortcutsClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/octgn/OCTGN/wiki/Octgn-Keyboard-Shortcuts#deck-editor");
            // this doesn't seem to support pointing to the specific section via #deck-editor
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            // Expected: Managed Debugging Assistant NotMarshalable
            // See Also: http://stackoverflow.com/questions/31362077/loadfromcontext-occured
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
            card.Alternate = e.Alternate;
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
            var sc = new Card(nc);
                         //{
                         //    ImageUri = nc.ImageUri,
                         //    Name = nc.Name,
                         //    Alternate = nc.Alternate,
                         //    Id = nc.Id,
                         //    Properties =
                         //        nc.Properties.ToDictionary(
                         //            x => x.Key.Clone() as string, x => x.Value.Clone() as CardPropertySet),
                         //    SetId = nc.SetId
                         //};
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
                if (grid.Items.SortDescriptions.Count != 0) // only allow re-ordering when not sorted
                {
                    _unsaved = true;
                    if (moveDown <= items)
                        ActiveSection.Cards.Move(element, moveDown);
                    grid.Focus();
                    grid.ScrollIntoView(element);
                    e.Handled = true;
                }
            }
            else if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.KeyboardDevice.IsKeyDown(Key.Subtract))
            {
                if (grid.Items.SortDescriptions.Count != 0) // only allow re-ordering when not sorted
                {
                    _unsaved = true;
                    if (moveUp >= 0)
                        ActiveSection.Cards.Move(element, moveUp);
                    grid.Focus();
                    grid.ScrollIntoView(element);
                    e.Handled = true;
                }
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

        private void CardImage_MouseDown(object sender, MouseButtonEventArgs e)
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
            if (activeRow == null) return;
            var getCard = (ICard) activeRow.Item;
            CardSelected(sender, new SearchCardImageEventArgs { SetId = getCard.SetId, Image = getCard.ImageUri, CardId = getCard.Id, Alternate = ""});
        }
        private DropAdorner adorner;
        private void ShowAdorner(UIElement element, bool fullBorder = false)
        {
            AdornerLayer aLayer = AdornerLayer.GetAdornerLayer(element);
            RemoveAdorner();
            adorner = new DropAdorner(element, fullBorder);
            adorner.IsHitTestVisible = false;
            aLayer.Add(adorner);
        }
        private void RemoveAdorner()
        {
            if (adorner != null)
            {
                AdornerLayer.GetAdornerLayer(adorner).Remove(adorner);
                adorner = null;
            }
        }
        private void PickUpDeckCard(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            if (MouseButtonState.Pressed.Equals(e.LeftButton) && activeRow != null && !dragging)
            {
                try
                {
                    ObservableMultiCard getCard = (ObservableMultiCard)activeRow.Item;
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
            else
            {
                DataGridRow adornedRow = FindAncestor<DataGridRow>(e.OriginalSource as FrameworkElement); // used to place drop adorner
                DataGrid grid = (DataGrid)FindAncestor<Expander>(e.OriginalSource as FrameworkElement).Content;

                if (e.Effects == DragDropEffects.Copy)
                {
                    Expander exp = FindAncestor<Expander>(sender as FrameworkElement);
                    ObservableSection dropSection = (ObservableSection)((FrameworkElement)exp).DataContext;
                    var dragCard = e.Data.GetData("Card") as IMultiCard;
                    var element = dropSection.Cards.FirstOrDefault(c => c.Id == dragCard.Id);
                    if (element != null) //i.e. card already in section
                    {
                        // Highlight the existing card
                        adornedRow = grid.ItemContainerGenerator.ContainerFromIndex(grid.Items.IndexOf(element)) as DataGridRow; // Move adorner to existing card
                        adornedRow.BringIntoView();
                        ShowAdorner(adornedRow, true);
                    }
                    else
                    {
                        e.Effects = DragDropEffects.All;
                    }
                }
                if (e.Effects == DragDropEffects.All)
                {
                    if (adornedRow != null && grid.Items.SortDescriptions.Count == 0)
                    {
                        ShowAdorner(adornedRow, false);
                    }
                    else if (grid != null)
                    {
                        ShowAdorner(grid, true);
                    }
                }
            }
        }

        private void TabControl_DragLeave(object sender, DragEventArgs e)
        {
            RemoveAdorner();
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
                    DataGridRow row = FindAncestor<DataGridRow>(e.OriginalSource as FrameworkElement);
                    if(row != null && FindAncestor<DataGrid>(row).Items.SortDescriptions.Count == 0) // do not move if no valid target or deck is sorted when dropped
                        dropSection.Cards.Move(dragCard, row.GetIndex());
                }
                RemoveAdorner();
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
                        "This feature is only for subscribers. Please visit http://www.octgn.net for subscription information."
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
                        "This feature is only for subscribers. Please visit http://www.octgn.net for subscription information."
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
                        "This feature is only for subscribers. Please visit http://www.octgn.net for subscription information."
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

        private void ImportImagesClicked(object sender, RoutedEventArgs e)
        {
            if (Game == null) return;
            if (SubscriptionModule.Get().IsSubscribed == false)
            {
                TopMostMessageBox.Show("You must be a subscriber to use this functionality", "Subscriber Warning",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var dlg = new ImportImages(Game);
            dlg.ShowDialog();
        }

        private void ChangeSleeve(object sender, RequestNavigateEventArgs e)
        {
            if (SubscriptionModule.Get().IsSubscribed == false)
            {
                TopMostMessageBox.Show("You must be a subscriber to use this functionality", "Subscriber Warning",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            SleeveManager.Show();
        }

        private void RemoveSleeve(object sender, RequestNavigateEventArgs e)
        {
			if(Deck != null)
				Deck.SleeveId = 0;
        }

        private void OnSleeveManagerClose(ApiSleeve obj)
        {
            if (obj == null)
                return;
            if (Deck == null)
                return;
            Deck.SleeveId = obj.Id;
        }

        private void ChangeSortButton_Click(object sender, RoutedEventArgs e)
        {
            Button buttonSender = (Button)sender;
            var sortGrid = (DataGrid) FindAncestor<Expander>((Button)sender).Content;

            if (sortGrid.Items.SortDescriptions.Count == 0) // Not sorted
            {
                // sort and show headers
                buttonSender.Content = "Sorted";
                SortDataGrid(sortGrid);
                sortGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
                sortGrid.CanUserSortColumns = true;
            }
            else // Is sorted
            {
                // Hide Headers and clear sorts to allow manual sorting
                buttonSender.Content = "Manual";
                sortGrid.CanUserSortColumns = false;
                sortGrid.HeadersVisibility = DataGridHeadersVisibility.None;
                SortDataGrid(sortGrid, -1);
            } 
        }
        private static void SortDataGrid(DataGrid dataGrid, int columnIndex = 0, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            dataGrid.Items.SortDescriptions.Clear();

            foreach (var col in dataGrid.Columns)
            {
                col.SortDirection = null;
            }

            if (columnIndex != -1) // -1 clears any existing sort
            {
                var column = dataGrid.Columns[columnIndex];

                dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, sortDirection));
                column.SortDirection = sortDirection;
            }

            dataGrid.Items.Refresh();
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid ParentGrid = (Grid)sender;
            RowDefinitionCollection parentRows = ParentGrid.RowDefinitions;
            ColumnDefinitionCollection parentCols = ParentGrid.ColumnDefinitions;

            if (parentRows.Count == 4)
            {
                Double maxH = e.NewSize.Height - parentRows[3].MinHeight -
                              parentRows[2].ActualHeight - parentRows[0].ActualHeight;
                parentRows[1].MaxHeight = maxH - 1; // work around window resizing issue

                if (e.NewSize.Height < e.PreviousSize.Height && parentRows[3].ActualHeight <= parentRows[3].MinHeight)
                {
                    parentRows[1].Height = new GridLength(parentRows[1].ActualHeight - (e.PreviousSize.Height - e.NewSize.Height));
                }
            }
            if (parentCols.Count == 3)
            {
                double maxW = e.NewSize.Width - parentCols[2].MinWidth - parentCols[1].ActualWidth;
                parentCols[0].MaxWidth = maxW - 1; // work around window resizing issue

                if (e.NewSize.Width < e.PreviousSize.Width && parentCols[2].ActualWidth <= parentCols[2].MinWidth)
                {
                    parentCols[0].Width = new GridLength(parentCols[0].ActualWidth - (e.PreviousSize.Width - e.NewSize.Width));
                }
            }
        }
    }

    internal class DropAdorner : System.Windows.Documents.Adorner
    {
        bool fullBorder;
        public DropAdorner(UIElement adornedElement, bool fullBorder = false)
            : base(adornedElement)
        {
            this.fullBorder = fullBorder;
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Red);
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Red), 2);

            drawingContext.DrawLine(renderPen, adornedElementRect.TopLeft, adornedElementRect.TopRight); // Top
            if (fullBorder)
            {
                drawingContext.DrawLine(renderPen, adornedElementRect.BottomLeft, adornedElementRect.BottomRight); // Bottom
                drawingContext.DrawLine(renderPen, adornedElementRect.TopLeft, adornedElementRect.BottomLeft); // Left
                drawingContext.DrawLine(renderPen, adornedElementRect.TopRight, adornedElementRect.BottomRight); // Right
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