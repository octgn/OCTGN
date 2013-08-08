﻿using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Octgn.Data;
using Octgn.Core.DataExtensionMethods;
using System.Text;

namespace Octgn.DeckBuilder
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;

    using Octgn.Annotations;
    using Octgn.Controls;

    using log4net;

    public partial class SearchControl : INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private DataView _CurrentView = null;
        private bool _InstantSearch = Prefs.InstantSearch;
        public SearchControl(DataNew.Entities.Game game)
        {
            Game = game;
            InitializeComponent();
            filtersList.ItemsSource =
                Enumerable.Repeat<object>("First", 1).Union(
                    Enumerable.Repeat<object>(new SetPropertyDef(Game.Sets()), 1).Union(
                        game.AllProperties().Where(p => !p.Hidden)));
            GenerateColumns(game);
            //resultsGrid.ItemsSource = game.SelectCards(null).DefaultView;
            UpdateDataGrid(game.AllCards().ToDataTable(Game).DefaultView);
            FileName = "";
        }//Why are we populating the list on load? I'd rather wait until the search is run with no parameters (V)_V

        public SearchControl(DataNew.Entities.Game loadedGame, SearchSave save)
        {
            var game = Octgn.DataNew.DbContext.Get().Games.FirstOrDefault(x => x.Id == save.GameId);
            if (game == null)
            {
                TopMostMessageBox.Show("You don't have the game for this search installed", "Oh No", MessageBoxButton.OK, MessageBoxImage.Error);
                save = null;
            }
            else if (loadedGame.Id != save.GameId)
            {
                TopMostMessageBox.Show(
                    "This search is for the game " + game.Name + ". You currently have the game " + loadedGame.Name
                    + " loaded so you can not load this search.", "Oh No", MessageBoxButton.OK, MessageBoxImage.Error);
                save = null;
            }
            Game = loadedGame;
            InitializeComponent();
            filtersList.ItemsSource =
                Enumerable.Repeat<object>("First", 1).Union(
                    Enumerable.Repeat<object>(new SetPropertyDef(Game.Sets()), 1).Union(
                        game.AllProperties().Where(p => !p.Hidden)));
            this.GenerateColumns(game);
            FileName = "";
            if (save != null)
            {

                this.SearchName = save.Name;
                this.FileName = save.FileName;
                // Load filters
                foreach (var filter in save.Filters)
                {
                    DataNew.Entities.PropertyDef prop;
                    if (filter.IsSetProperty)
                    {
                        prop = filtersList.Items.OfType<SetPropertyDef>().First();
                    }
                    else
                    {
                        prop =
                            loadedGame.CustomProperties.FirstOrDefault(
                                x => x.Name.Equals(filter.PropertyName, StringComparison.InvariantCultureIgnoreCase));
                    }
                    if (prop == null) continue;

                    filterList.Items.Add(prop);
                }
                this.Loaded += (sender, args) =>
                    {
                        var generator = filterList.ItemContainerGenerator;
                        for (int i = 0; i < filterList.Items.Count; i++)
                        {
                            DependencyObject container = generator.ContainerFromIndex(i);
                            var filterCtrl = (FilterControl)VisualTreeHelper.GetChild(container, 0);
                            var filter =
                                save.Filters.FirstOrDefault(
                                    x =>
                                    x.PropertyName.Equals(
                                        filterCtrl.Property.Name, StringComparison.InvariantCultureIgnoreCase));
                            if (filter == null) continue;
                            filterCtrl.SetFromSave(Game,filter);
                        }
                        this.RefreshSearch(SearchButton, null);
                    };
            }
            this.UpdateDataGrid(game.AllCards().ToDataTable(Game).DefaultView);
        }

        public int SearchIndex { get; set; }

        public string SearchName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.searchName))
                {
                    this.searchName = "Search #" + SearchIndex;
                }
                return this.searchName;
            }
            set
            {
                if (value == this.searchName) return;
                this.searchName = value;
                this.OnPropertyChanged("SearchName");
            }
        }

        private string searchName;

        public DataNew.Entities.Game Game { get; private set; }

        public string FileName
        {
            get
            {
                return this.fileName;
            }
            set
            {
                if (value == this.fileName)
                {
                    return;
                }
                this.fileName = value;
                this.OnPropertyChanged("FileName");
            }
        }

        public event EventHandler<SearchCardIdEventArgs> CardRemoved, CardAdded;
        public event EventHandler<SearchCardImageEventArgs> CardSelected;

        private void ResultKeyDownHandler(object sender, KeyEventArgs e)
        {
            var row = (DataRowView)resultsGrid.SelectedItem;
            if (row == null) return;

            switch (e.Key)
            {
                case Key.Insert:
                case Key.Add:
                case Key.Enter:
                    if (CardAdded != null)
                    {
                        var rowid = row["id"] as string;
                        if (rowid != null)
                            CardAdded(this, new SearchCardIdEventArgs { CardId = Guid.Parse(rowid) });
                    }
                    e.Handled = true;
                    break;

                case Key.Delete:
                case Key.Subtract:
                    if (CardRemoved != null)
                    {
                        var rowid = row["id"] as string;
                        if (rowid != null)
                            CardRemoved(this, new SearchCardIdEventArgs { CardId = Guid.Parse(rowid) });
                    }
                    e.Handled = true;
                    break;
            }
        }

        private void ResultDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var row = (DataRowView)resultsGrid.SelectedItem;
            if (row == null) return;
            if (CardAdded == null) return;
            var rowid = row["id"] as string;
            if (rowid != null)
                CardAdded(this, new SearchCardIdEventArgs { CardId = Guid.Parse(rowid) });
        }

        private void ResultCardSelected(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            Log.Debug("Item Selected");
            var row = (DataRowView)resultsGrid.SelectedItem;
            Log.Debug("Grabbed selected item");
            if (CardSelected != null)
            {
                Log.Debug("Card selected != null");
                if (row != null)
                {
                    Log.Debug("Row not null");
                    var setid = row["set_id"] as String;
                    var cardid = row["id"] as string;
                    Log.DebugFormat("Set ID: {0} \nImg Url: {1} \nCard ID: {2}", setid, row["img_uri"], cardid);
                    CardSelected(
                        this, new SearchCardImageEventArgs { SetId = new Guid(setid), Image = (string)row["img_uri"], CardId = new Guid(cardid) });
                    Log.Debug("Card selected complete");
                }
                else
                {
                    Log.Debug("Row Null. Sending empy SearchCardImageEventArgs");
                    CardSelected(this, new SearchCardImageEventArgs());
                    Log.Debug("Card selected complete");
                }
            }
            else
            {
                Log.Debug("CardSelected == null");
            }
        }

        private void GenerateColumns(DataNew.Entities.Game game)
        {
            foreach (DataNew.Entities.PropertyDef prop in game.CustomProperties)
            {
                if (prop.Name == "Name") continue;
                resultsGrid.Columns.Add(new DataGridTextColumn
                                            {
                                                Binding = new Binding
                                                              {
                                                                  Path = new PropertyPath(prop.Name),
                                                                  Mode = BindingMode.OneTime
                                                              },
                                                Header = prop.Name
                                            });
            }
        }

        private void AddFilter(object sender, RoutedEventArgs e)
        {
            filterList.Items.Add(((FrameworkElement)sender).DataContext);
        }

        private void RemoveFilter(object sender, EventArgs e)
        {
            int idx = filterList.ItemContainerGenerator.IndexFromContainer((DependencyObject)sender);
            filterList.Items.RemoveAt(idx);
        }

        private void RefreshSearch(object sender, RoutedEventArgs e)
        {
            //I'm not sure why the button was being dissabled, or if it was actually acomplishing anything, 
            //but it doesn't seem to matter
            //((Button)sender).IsEnabled = false;
            var conditions = new List<String>();
            var orconditions = new List<String>();
            ItemContainerGenerator generator = filterList.ItemContainerGenerator;
            for (int i = 0; i < filterList.Items.Count; i++)
            {
                DependencyObject container = generator.ContainerFromIndex(i);
                var filterCtrl = (FilterControl)VisualTreeHelper.GetChild(container, 0);
                if (filterCtrl.IsOr)
                {
                    var c = filterCtrl.GetSqlCondition();
                    c = c.Replace("Card.", "");
                    orconditions.Add(c);
                }
                else
                {
                    var c = filterCtrl.GetSqlCondition();
                    c = c.Replace("Card.", "");
                    conditions.Add(c);
                }
            }

            var filterString = "";
            if (orconditions.Count > 0)
            {
                filterString = String.Format("({0})", String.Join(" or ", orconditions));
            }
            if (conditions.Count > 0)
            {
                if (orconditions.Count > 0) filterString += " and ";
                filterString += String.Format("({0})", String.Join(" and ", conditions));
            }

            _CurrentView.RowFilter = filterString;
            if (e != null)
                e.Handled = true;
            //((Button)sender).IsEnabled = true;
        }

        public void UpdateDataGrid(DataView view)
        {
            if (_CurrentView == null)
            {
                _CurrentView = view;
                resultsGrid.ItemsSource = _CurrentView;
                return;
            }
            _CurrentView.Table.Clear();

            foreach (DataRow row in view.Table.Rows)
            {
                _CurrentView.Table.ImportRow(row);
            }
        }
        private bool dragActive = false;

        private string fileName;

        private void SelectPickupCard(object sender, MouseButtonEventArgs e)
        {
            dragActive = true;
            ResultCardSelected(sender, new SelectionChangedEventArgs(e.RoutedEvent, new List<Object>(), new List<Object>()));
        }
        private void PickUpCard(object sender, MouseEventArgs e)
        {
            if (MouseButtonState.Pressed.Equals(e.LeftButton) && dragActive)
            {
                DataGridRow SearchCard = new DataGridRow();
                var row = (DataRowView)resultsGrid.SelectedItem;
                if (row == null) return;
                if (CardAdded == null) return;
                var rowid = row["id"] as string;
                if (rowid != null)
                {
                    DataNew.Entities.MultiCard getCard = Game.GetCardById(Guid.Parse(rowid)).ToMultiCard();
                    DataObject dragCard = new DataObject("Card", getCard);
                    DragDrop.DoDragDrop(SearchCard, dragCard, DragDropEffects.Copy);
                }
            }
            dragActive = false;
        }
        private void SearchDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void FilterControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (_InstantSearch)
                RefreshSearch(sender, e);
        }
    }

    public class SearchCardIdEventArgs : EventArgs
    {
        public Guid CardId { get; set; }
    }

    public class SearchCardImageEventArgs : EventArgs
    {
        public Guid SetId { get; set; }
        public Guid CardId { get; set; }
        public string Image { get; set; }
    }

    public class SetConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return Binding.DoNothing;
            if (values[0] == DependencyProperty.UnsetValue ||
                values[1] == DependencyProperty.UnsetValue)
                return Binding.DoNothing;

            var guid = values[0] as string;
            if (guid != null)
            {
                Guid setId = Guid.Parse(guid);
                var game = (DataNew.Entities.Game)values[1];
                DataNew.Entities.Set set = game.GetSetById(setId);
                return set != null ? set.Name : "(unknown)";
            }
            return "(unknown)";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}