using System;
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
    public partial class SearchControl
    {
        private DataView _CurrentView = null;
        public SearchControl(DataNew.Entities.Game game)
        {
            Game = game;
            InitializeComponent();
            filtersList.ItemsSource =
                Enumerable.Repeat<object>("First", 1).Union(
                    Enumerable.Repeat<object>(new SetPropertyDef(Game.Sets), 1).Union(
                        game.AllProperties().Where(p => !p.Hidden)));
            GenerateColumns(game);
            //resultsGrid.ItemsSource = game.SelectCards(null).DefaultView;
            UpdateDataGrid(game.AllCards().ToDataTable().DefaultView);
        }//Why are we populating the list on load? I'd rather wait until the search is run with no parameters (V)_V

        public int SearchIndex { get; set; }

        public string SearchName
        {
            get { return "Search #" + SearchIndex; }
        }

        public DataNew.Entities.Game Game { get; private set; }
        public event EventHandler<SearchCardIdEventArgs> CardRemoved , CardAdded;
        public event EventHandler<SearchCardImageEventArgs> CardSelected;

        private void ResultKeyDownHandler(object sender, KeyEventArgs e)
        {
            var row = (DataRowView) resultsGrid.SelectedItem;
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
                            CardAdded(this, new SearchCardIdEventArgs {CardId = Guid.Parse(rowid)});
                    }
                    e.Handled = true;
                    break;

                case Key.Delete:
                case Key.Subtract:
                    if (CardRemoved != null)
                    {
                        var rowid = row["id"] as string;
                        if (rowid != null)
                            CardRemoved(this, new SearchCardIdEventArgs {CardId = Guid.Parse(rowid)});
                    }
                    e.Handled = true;
                    break;
            }
        }

        private void ResultDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var row = (DataRowView) resultsGrid.SelectedItem;
            if (row == null) return;
            if (CardAdded == null) return;
            var rowid = row["id"] as string;
            if (rowid != null)
                CardAdded(this, new SearchCardIdEventArgs {CardId = Guid.Parse(rowid)});
        }

        private void ResultCardSelected(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            var row = (DataRowView) resultsGrid.SelectedItem;
            if (CardSelected != null)
                if (row != null)
                {
                    var setid = row["set_id"] as string;
                    if (setid != null)
                        CardSelected(this,
                                     new SearchCardImageEventArgs
                                         {SetId = Guid.Parse(setid), Image = (string) row["image"]});
                }
                else
                {
                    CardSelected(this, new SearchCardImageEventArgs());
                }
        }

        private void GenerateColumns(DataNew.Entities.Game game)
        {
            foreach (DataNew.Entities.PropertyDef prop in game.CustomProperties)
            {
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
            filterList.Items.Add(((FrameworkElement) sender).DataContext);
        }

        private void RemoveFilter(object sender, EventArgs e)
        {
            int idx = filterList.ItemContainerGenerator.IndexFromContainer((DependencyObject) sender);
            filterList.Items.RemoveAt(idx);
        }

        private void RefreshSearch(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            var conditions = new string[filterList.Items.Count];
            ItemContainerGenerator generator = filterList.ItemContainerGenerator;
            for (int i = 0; i < filterList.Items.Count; i++)
            {
                DependencyObject container = generator.ContainerFromIndex(i);
                var filterCtrl = (FilterControl) VisualTreeHelper.GetChild(container, 0);
                conditions[i] = filterCtrl.GetSqlCondition();
                //HACK: strip out the Card. that the sql query inserts
                conditions[i] = conditions[i].Replace("Card.", "");
            }
            //resultsGrid.ItemsSource = Game.SelectCards(conditions).DefaultView;
            // TODO [DB MIGRATION]  figure how to make this shit work.(line below)
            //UpdateDataGrid(Game.SelectCards(conditions).DefaultView);
            e.Handled = true;
            ((Button)sender).IsEnabled = true;
        }
        private string ConvertToSQLString(string[] conditions)
        {
            var sb = new StringBuilder();
            sb.Append("SELECT * FROM Card");
            if (conditions != null)
            {
                string connector = " WHERE ";
                foreach (string condition in conditions)
                {
                    sb.Append(connector);
                    sb.Append("(");
                    sb.Append(condition);
                    sb.Append(")");
                    connector = " AND ";
                }
            }
            return sb.ToString();
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
        private DataGridRow SearchCard = new DataGridRow();
        private void PickUpCard(object sender, MouseEventArgs e)
        {
            if (MouseButtonState.Pressed.Equals(e.LeftButton))
            {
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
        }
        private void SearchDragEnter(object sender, DragEventArgs e)
        {
                e.Effects = DragDropEffects.None;
        }
        private static T FindRow<T>(DependencyObject Current)
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

    }

    public class SearchCardIdEventArgs : EventArgs
    {
        public Guid CardId { get; set; }
    }

    public class SearchCardImageEventArgs : EventArgs
    {
        public Guid SetId { get; set; }
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