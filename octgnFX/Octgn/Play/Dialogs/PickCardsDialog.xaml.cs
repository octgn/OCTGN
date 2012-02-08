﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Octgn.Controls;
using Octgn.Data;
using Octgn.Definitions;
using Octgn.Play.Gui;

namespace Octgn.Play.Dialogs
{
    public partial class PickCardsDialog
    {
        public PickCardsDialog()
        {
            CardPool = new ObservableCollection<CardModel>();
            CardPoolView = new ListCollectionView(CardPool) {Filter = FilterCard};
            UnlimitedPool = new ObservableCollection<CardModel>();
            UnlimitedPoolView = new ListCollectionView(UnlimitedPool);
            UnlimitedPoolView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            LimitedDeck = new Deck(Database.OpenedGame);
            CreateFilters();
            InitializeComponent();
        }

        public ListCollectionView CardPoolView { get; private set; }
        public ObservableCollection<CardModel> CardPool { get; private set; }
        public ListCollectionView UnlimitedPoolView { get; private set; }
        public ObservableCollection<CardModel> UnlimitedPool { get; private set; }
        public Deck LimitedDeck { get; private set; }
        public ObservableCollection<Filter> Filters { get; private set; }

        private void SortChanged(object sender, SelectionChangedEventArgs e)
        {
            CardPoolView.CustomSort = new CardPropertyComparer(((PropertyDef) sortBox.SelectedItem).Name);
        }

        public void OpenPacks(IEnumerable<Guid> packs)
        {
            StopListenningForFilterValueChanges();

            foreach (Guid packId in packs)
            {
                Pack pack = Database.GetPackById(packId);
                if (pack == null)
                {
                    Program.TraceWarning("Received pack is missing from the database. Pack is ignored.");
                    continue;
                }
                Pack.PackContent content = pack.CrackOpen();
                foreach (CardModel c in content.LimitedCards)
                    CardPool.Add(c);
                foreach (CardModel c in content.UnlimitedCards)
                    UnlimitedPool.Add(c);
            }

            UpdateFilters();

            ListenForFilterValueChanges();
        }

        private void ComputeChildWidth(object sender, RoutedEventArgs e)
        {
            var panel = sender as VirtualizingWrapPanel;
            CardDef cardDef = Program.Game.Definition.CardDefinition;
            if (panel != null) panel.ChildWidth = panel.ChildHeight*cardDef.Width/cardDef.Height;
        }

        private void SetPicture(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            if (img != null)
            {
                var element = img.DataContext as Deck.Element;
                if (element != null)
                {
                    CardModel model = element.Card;
                    ImageUtils.GetCardImage(new Uri(model.Picture), x => img.Source = x);
                }
            }
        }

        private void PickPoolCard(object sender, RoutedEventArgs e)
        {
            var src = e.OriginalSource as FrameworkElement;
            if (src == null) return;
            var card = src.DataContext as CardModel;
            if (card == null) return;
            var section = deckTabs.SelectedItem as Deck.Section;
            if (section == null) return;

            CardPool.Remove(card);
            Deck.Element element = section.Cards.FirstOrDefault(x => x.Card.Id == card.Id);
            if (element != null)
                element.Quantity++;
            else
                section.Cards.Add(new Deck.Element {Card = card, Quantity = 1});

            // TODO: Hide the big card preview, this is ineffective as it immediately comes into view again as the 
            // mouse enters another card.
            // RaiseEvent(new CardEventArgs(CardControl.CardHoveredEvent, sender));
        }

        private void PickUnlimitedCard(object sender, RoutedEventArgs e)
        {
            var src = e.OriginalSource as FrameworkElement;
            if (src == null) return;
            var card = src.DataContext as CardModel;
            if (card == null) return;
            var section = deckTabs.SelectedItem as Deck.Section;
            if (section == null) return;

            // Hide the big card preview
            RaiseEvent(new CardEventArgs(CardControl.CardHoveredEvent, sender));

            OpenQuantityPopup(qty =>
                                  {
                                      Deck.Element element = section.Cards.FirstOrDefault(x => x.Card.Id == card.Id);
                                      if (element != null)
                                          element.Quantity += (byte) qty;
                                      else
                                          section.Cards.Add(new Deck.Element {Card = card, Quantity = (byte) qty});
                                  });
        }

        private void RemoveDeckCard(object sender, RoutedEventArgs e)
        {
            var src = e.OriginalSource as FrameworkElement;
            if (src == null) return;
            var element = (src.DataContext as Deck.Element);
            if (element == null) return;
            var section = deckTabs.SelectedItem as Deck.Section;

            if (element.Quantity > 1)
            {
                OpenQuantityPopup(qty =>
                                      {
                                          int actuallyRemoved = Math.Min(qty, element.Quantity);
                                          if (element.Quantity > qty)
                                              element.Quantity -= (byte) qty;
                                          else if (section != null) section.Cards.Remove(element);
                                          if (!UnlimitedPool.Contains(element.Card))
                                              for (int i = 0; i < actuallyRemoved; ++i)
                                                  // When there are multiple copies of the same card, we insert clones of the CardModel.
                                                  // Otherwise, the ListBox gets confused with selection.
                                                  CardPool.Add(element.Quantity > 1
                                                                   ? element.Card.Clone()
                                                                   : element.Card);
                                      });
            }
            else
            {
                if (element.Quantity > 1)
                    element.Quantity--;
                else if (section != null) section.Cards.Remove(element);
                if (!UnlimitedPool.Contains(element.Card))
                    CardPool.Add(element.Card);
            }
        }

        private void MouseEnterCard(object sender, MouseEventArgs e)
        {
            var src = e.Source as FrameworkElement;
            if (src != null)
            {
                var element = src.DataContext as Deck.Element;
                if (element != null)
                {
                    CardModel model = element.Card;
                    RaiseEvent(new CardEventArgs(model, CardControl.CardHoveredEvent, sender));
                }
            }
        }

        private void MouseLeaveCard(object sender, MouseEventArgs e)
        {
            RaiseEvent(new CardEventArgs(CardControl.CardHoveredEvent, sender));
        }

        #region Filters

        private readonly List<FilterValue> activeRestrictions = new List<FilterValue>();

        private bool FilterCard(object c)
        {
            if (activeRestrictions == null || activeRestrictions.Count == 0)
                return true;
            var card = (CardModel) c;

            foreach (var restriction in activeRestrictions.GroupBy(fv => fv.Property))
            {
                PropertyDef prop = restriction.Key;
                object value = card.Properties[prop.Name];
                bool isOk = false;
                foreach (FilterValue filterValue in restriction)
                {
                    if (filterValue.IsValueMatch(value))
                    {
                        isOk = true;
                        break;
                    }
                }
                if (!isOk) return false;
            }
            return true;
        }

        private void CreateFilters()
        {
            Filters = new ObservableCollection<Filter>();
            foreach (PropertyDef prop in Program.Game.Definition.CardDefinition.Properties.Values
                .Where(p => !p.Hidden && (p.Type == PropertyType.Integer || p.TextKind != PropertyTextKind.FreeText)))
            {
                var filter = new Filter
                                 {
                                     Name = prop.Name,
                                     Property = prop,
                                     Values = new ObservableCollection<FilterValue>()
                                 };
                Filters.Add(filter);
            }
        }

        private void StopListenningForFilterValueChanges()
        {
            CardPool.CollectionChanged -= CardPoolChanged;
        }

        private void ListenForFilterValueChanges()
        {
            CardPool.CollectionChanged += CardPoolChanged;
        }

        private void UpdateFilters()
        {
            activeRestrictions.Clear();
            foreach (Filter filter in Filters)
            {
                PropertyDef prop = filter.Property;
                filter.Values.Clear();
                if (prop.Type == PropertyType.String)
                    switch (prop.TextKind)
                    {
                        case PropertyTextKind.Enumeration:
                            Filter filter2 = filter;
                            IEnumerable<EnumFilterValue> q = from CardModel c in CardPoolView
                                                             group c by (string) c.Properties[prop.Name]
                                                             into g
                                                             orderby g.Key
                                                             select
                                                                 new EnumFilterValue
                                                                     {
                                                                         Property = filter2.Property,
                                                                         Value = g.Key,
                                                                         Count = g.Count()
                                                                     };
                            foreach (EnumFilterValue filterValue in q)
                                filter.Values.Add(filterValue);
                            break;
                        case PropertyTextKind.Tokens:
                            Filter filter1 = filter;
                            IEnumerable<TokenFilterValue> q2 = from CardModel c in CardPoolView
                                                               let all = (string) c.Properties[prop.Name]
                                                               where all != null
                                                               from token in
                                                                   all.Split(new[] {' '},
                                                                             StringSplitOptions.RemoveEmptyEntries)
                                                               group c by token
                                                               into g
                                                               orderby g.Key
                                                               select
                                                                   new TokenFilterValue
                                                                       {
                                                                           Property = filter1.Property,
                                                                           Value = g.Key,
                                                                           Count = g.Count()
                                                                       };
                            foreach (TokenFilterValue filterValue in q2)
                                filter.Values.Add(filterValue);
                            break;
                    }
            }
        }

        private void CardPoolChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (FilterValue fv in Filters.SelectMany(f => f.Values))
            {
                if (e.NewItems != null)
                    foreach (CardModel card in e.NewItems)
                        if (fv.IsMatch(card)) fv.Count++;
                if (e.OldItems != null)
                    foreach (CardModel card in e.OldItems)
                        if (fv.IsMatch(card)) fv.Count--;
            }
        }

        private void FilterChecked(object sender, RoutedEventArgs e)
        {
            var src = ((FrameworkElement) sender);
            var filterValue = (FilterValue) src.DataContext;
            activeRestrictions.Add(filterValue);
            CardPoolView.Refresh();
        }

        private void FilterUnchecked(object sender, RoutedEventArgs e)
        {
            var src = ((FrameworkElement) sender);
            var filterValue = (FilterValue) src.DataContext;
            activeRestrictions.Remove(filterValue);
            CardPoolView.Refresh();
        }

        #endregion

        #region Quantity popup

        private Action<int> quantityPopupAction;

        private void OpenQuantityPopup(Action<int> qtyAction)
        {
            quantityPopupAction = qtyAction;
            quantityPopup.IsOpen = true;
            quantityBox.Text = "1";
            quantityBox.Focus();
            quantityBox.SelectAll();
        }

        private void QuantityBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;
                    int qty;
                    if (!int.TryParse(quantityBox.Text, out qty)) return;
                    if (qty < 1) return;
                    quantityPopupAction(qty);
                    quantityPopup.IsOpen = false;
                    break;
                case Key.Escape:
                    e.Handled = true;
                    quantityPopup.IsOpen = false;
                    break;
            }
        }

        private void QuantityBox_LostFocus(object sender, RoutedEventArgs e)
        {
            quantityPopup.IsOpen = false;
        }

        #endregion

        #region Nested type: EnumFilterValue

        public class EnumFilterValue : FilterValue
        {
            public string Value { get; set; }

            public override bool IsValueMatch(object value)
            {
                return (value as string) == Value;
            }
        }

        #endregion

        #region Nested type: Filter

        public class Filter
        {
            public string Name { get; set; }
            public PropertyDef Property { get; set; }
            public ObservableCollection<FilterValue> Values { get; set; }
        }

        #endregion

        #region Nested type: FilterValue

        public abstract class FilterValue : INotifyPropertyChanged
        {
            private int _count;
            public PropertyDef Property { get; set; }
            public bool IsActive { get; set; }

            public int Count
            {
                get { return _count; }
                set
                {
                    if (value == _count) return;
                    _count = value;
                    OnPropertyChanged("Count");
                }
            }

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion

            public abstract bool IsValueMatch(object value);

            public bool IsMatch(CardModel c)
            {
                return IsValueMatch(c.Properties[Property.Name]);
            }

            protected void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Nested type: TokenFilterValue

        public class TokenFilterValue : FilterValue
        {
            private string _value;

            private Regex regex;

            public string Value
            {
                get { return _value; }
                set
                {
                    _value = value;
                    regex = new Regex(@"(^|\s)" + value + @"($|\s)", RegexOptions.Compiled);
                }
            }

            public override bool IsValueMatch(object value)
            {
                var strValue = value as string;
                if (strValue == null) return false;
                return regex.IsMatch(strValue);
            }
        }

        #endregion
    }

    public class FilterTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var ctrl = container as FrameworkElement;
            var filter = item as PickCardsDialog.Filter;
            if (filter == null) return base.SelectTemplate(item, container);

            switch (filter.Property.Type)
            {
                case PropertyType.String:
                    if (ctrl != null) return ctrl.FindResource("TextTemplate") as DataTemplate;
                    break;
                case PropertyType.Integer:
                    if (ctrl != null) return ctrl.FindResource("IntTemplate") as DataTemplate;
                    break;
            }
            throw new InvalidOperationException("Unexpected property type: " + filter.Property.Type);
        }
    }
}