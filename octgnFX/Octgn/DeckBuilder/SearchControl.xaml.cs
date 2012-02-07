﻿using System;
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
using Octgn.Data;

namespace Octgn.DeckBuilder
{
    public partial class SearchControl : UserControl
    {
        public event EventHandler<SearchCardIdEventArgs> CardRemoved, CardAdded;
        public event EventHandler<SearchCardImageEventArgs> CardSelected;

        public SearchControl(Data.Game game)
        {
            Game = game;
            InitializeComponent();
            filtersList.ItemsSource = Enumerable.Union<object>(Enumerable.Repeat<object>("First", 1),
                                                                Enumerable.Union<object>(Enumerable.Repeat<object>(new SetPropertyDef(Game.Sets), 1),
                                                                                                                 game.AllProperties.Where(p => !p.Hidden).Cast<object>()));
            GenerateColumns(game);
            resultsGrid.ItemsSource = game.SelectCards(null).DefaultView;
        }

        public int SearchIndex { get; set; }
        public string SearchName { get { return "Search #" + SearchIndex; } }

        public Data.Game Game
        { get; private set; }

        private void ResultKeyDownHandler(object sender, KeyEventArgs e)
        {
            var row = (System.Data.DataRowView)resultsGrid.SelectedItem;
            if (row == null) return;

            switch (e.Key)
            {
                case Key.Insert:
                case Key.Add:
                case Key.Enter:
                    if (CardAdded != null)
                        CardAdded(this, new SearchCardIdEventArgs { CardId = Guid.Parse(row["id"] as string) });
                    e.Handled = true;
                    break;

                case Key.Delete:
                case Key.Subtract:
                    if (CardRemoved != null)
                        CardRemoved(this, new SearchCardIdEventArgs { CardId = Guid.Parse(row["id"] as string) });
                    e.Handled = true;
                    break;
            }
        }

        private void ResultDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var row = (System.Data.DataRowView)resultsGrid.SelectedItem;
            if (row == null) return;
            if (CardAdded != null)
                CardAdded(this, new SearchCardIdEventArgs { CardId = Guid.Parse(row["id"] as string) });
        }

        private void ResultCardSelected(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            var row = (System.Data.DataRowView)resultsGrid.SelectedItem;

            if (CardSelected != null)
                CardSelected(this,
                    row != null ?
                    new SearchCardImageEventArgs { SetId = Guid.Parse(row["set_id"] as string), Image = (string)row["image"] } :
                    new SearchCardImageEventArgs());
        }

        private void GenerateColumns(Data.Game game)
        {
            foreach (PropertyDef prop in game.CustomProperties)
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
            filterList.Items.Add(((FrameworkElement)sender).DataContext);
        }

        private void RemoveFilter(object sender, EventArgs e)
        {
            var idx = filterList.ItemContainerGenerator.IndexFromContainer((DependencyObject)sender);
            filterList.Items.RemoveAt(idx);
        }

        private void RefreshSearch(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var conditions = new string[filterList.Items.Count];
            var generator = filterList.ItemContainerGenerator;
            for (int i = 0; i < filterList.Items.Count; i++)
            {
                var container = generator.ContainerFromIndex(i);
                var filterCtrl = (FilterControl)VisualTreeHelper.GetChild(container, 0);
                conditions[i] = filterCtrl.GetSqlCondition();
            }

            resultsGrid.ItemsSource = Game.SelectCards(conditions).DefaultView;
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
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 2) return Binding.DoNothing;
            if (values[0] == DependencyProperty.UnsetValue ||
                    values[1] == DependencyProperty.UnsetValue)
                return Binding.DoNothing;

            var setId = Guid.Parse(values[0] as string);
            var game = (Data.Game)values[1];
            var set = game.GetSet(setId);
            return set != null ? set.Name : "(unknown)";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }

}