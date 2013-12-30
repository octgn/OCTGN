using System;
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
using Octgn.Play.Gui;
using Octgn.Utils;

namespace Octgn.Play.Dialogs
{
    using System.Threading.Tasks;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew;
    using Octgn.DataNew.Entities;

    public partial class PickCardsDialog
    {
        public Game CurrentGame { get; set; }
        public PickCardsDialog()
        {
            CurrentGame = Program.GameEngine.Definition;
            CardPool = new ObservableCollection<ObservableMultiCard>();
            CardPoolView = new ListCollectionView(CardPool) { Filter = FilterCard };
            UnlimitedPool = new ObservableCollection<ObservableMultiCard>();
            UnlimitedPoolView = new ListCollectionView(UnlimitedPool);
            UnlimitedPoolView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            LimitedDeck = Program.GameEngine.Definition.CreateDeck().AsObservable();
            CreateFilters();
            InitializeComponent();
        }

        public ListCollectionView CardPoolView { get; private set; }
        public ObservableCollection<ObservableMultiCard> CardPool { get; private set; }
        public ListCollectionView UnlimitedPoolView { get; private set; }
        public ObservableCollection<ObservableMultiCard> UnlimitedPool { get; private set; }
        public ObservableDeck LimitedDeck { get; private set; }
        public ObservableCollection<Filter> Filters { get; private set; }
        public bool FiltersVisible { get; set; }

        private void SortChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sortBox.SelectedItem == null) return;
            CardPoolView.CustomSort = new CardPropertyComparer(((PropertyDef)sortBox.SelectedItem).Name);
        }

        public void OpenPacks(IEnumerable<Guid> packs)
        {
            new Task(
                () =>
                {
                    Dispatcher.Invoke(new Action(() =>
                        {
                            TabControlMain.IsEnabled = false;
                            ProgressBarLoading.Visibility = Visibility.Visible;
                            ProgressBarLoading.IsIndeterminate = true;
                            ProgressBarLoading.Maximum = packs.Count();
                        }));
                    this.StopListenningForFilterValueChanges();

                    foreach (Pack pack in packs.Select(Program.GameEngine.Definition.GetPackById))
                    {
                        if (pack == null)
                        {
                            Program.TraceWarning("Received pack is missing from the database. Pack is ignored.");
                            continue;
                        }
                        PackContent content = pack.CrackOpen();
                        foreach (ObservableMultiCard c in content.LimitedCards.Select(x => x.ToMultiCard().AsObservable()))
                        {
                            Dispatcher.Invoke(new Action(() => { this.CardPool.Add(c); }));
                        }

                        foreach (ObservableMultiCard c in content.UnlimitedCards.Select(x => x.ToMultiCard().AsObservable()))
                        {
                            Dispatcher.Invoke(new Action(() => { this.UnlimitedPool.Add(c); }));
                        }

                        Dispatcher.Invoke(new Action(() =>
                            {
                                ProgressBarLoading.IsIndeterminate = false;
                                ProgressBarLoading.Value += 1;
                            }));
                    }

                    Dispatcher.Invoke(
                        new Action(() =>
                            {
                                this.UpdateFilters();

                                this.ListenForFilterValueChanges();
                                ProgressBarLoading.IsIndeterminate = false;
                                ProgressBarLoading.Visibility = Visibility.Collapsed;
                                TabControlMain.IsEnabled = true;
                            }));
                }).Start();
        }
        public string PackNames(IEnumerable<Guid> packs)
        {
            string returnString = "";
            foreach (Pack pack in packs.Select(Program.GameEngine.Definition.GetPackById))
            {
                if (pack == null)
                {
                    Program.TraceWarning("Received pack is missing from the database. Pack is ignored.");
                    continue;
                }
                returnString += string.Format("[{0} - {1}],", pack.Set().Name, pack.Name);
            }
            return returnString.Trim(new Char[] { ',', ' '});
        }

        private void ComputeChildWidth(object sender, RoutedEventArgs e)
        {
            var panel = sender as VirtualizingWrapPanel;
            if (panel != null) panel.ChildWidth = panel.ChildHeight * Program.GameEngine.Definition.CardWidth / Program.GameEngine.Definition.CardHeight;
        }

        private void SetPicture(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            if (img == null) return;
            var model = img.DataContext as ObservableMultiCard;
            if (model == null) return;
            ImageUtils.GetCardImage(new Uri(model.GetPicture()), x => img.Source = x);
        }

        private void PickPoolCard(object sender, RoutedEventArgs e)
        {
            var src = e.OriginalSource as FrameworkElement;
            if (src == null) return;
            var card = src.DataContext as ObservableMultiCard;
            if (card == null) return;
            var section = deckTabs.SelectedItem as ObservableSection;
            if (section == null) return;

            CardPool.Remove(card);
            var element = section.Cards.FirstOrDefault(x => x.Id == card.Id);//.AsObservable();
            if (element != null)
                element.Quantity++;
            else
                section.Cards.AddCard(card);

            // RaiseEvent(new CardEventArgs(CardControl.CardHoveredEvent, sender));
        }

        private void PickUnlimitedCard(object sender, RoutedEventArgs e)
        {
            var src = e.OriginalSource as FrameworkElement;
            if (src == null) return;
            var card = src.DataContext as ObservableMultiCard;
            if (card == null) return;
            var section = deckTabs.SelectedItem as ObservableSection;
            if (section == null) return;

            // Hide the big card preview
            RaiseEvent(new CardEventArgs(CardControl.CardHoveredEvent, sender));

            OpenQuantityPopup(qty =>
                                  {
                                      var element = section.Cards.FirstOrDefault(x => x.Id == card.Id);
                                      if (element != null) element.Quantity += qty;
                                      else
                                      {
                                          var nc = card.AsObservable();
                                          nc.Quantity = qty;
                                          section.Cards.AddCard(nc);
                                      }
                                  });
        }

        private void RemoveDeckCard(object sender, RoutedEventArgs e)
        {
            var src = e.OriginalSource as FrameworkElement;
            if (src == null) return;
            var element = (src.DataContext as ObservableMultiCard);
            if (element == null) return;
            var section = deckTabs.SelectedItem as ObservableSection;

            if (element.Quantity > 1)
            {
                OpenQuantityPopup(qty =>
                                      {
                                          int actuallyRemoved = Math.Min(qty, element.Quantity);
                                          if (!UnlimitedPool.Contains(element))
                                          {
                                              for (int i = 0; i < actuallyRemoved; ++i)
                                              {
                                                  // When there are multiple copies of the same card, we insert clones of the CardModel.
                                                  // Otherwise, the ListBox gets confused with selection.
                                                  if (element.Quantity > 1)
                                                  {
                                                      var tempElement = element.AsObservable();
                                                      tempElement.Quantity = 1;
                                                      CardPool.Add(tempElement);
                                                  }
                                                  else
                                                  {
                                                      var tempElement = element.AsObservable();
                                                      tempElement.Quantity = 1;
                                                      CardPool.Add(tempElement);
                                                  }

                                                  element.Quantity -= 1;
                                                  if (element.Quantity <= 0)
                                                  {
                                                      element.Quantity = 0;
                                                      section.Cards.RemoveCard(element);
                                                      break;
                                                  }
                                              }
                                          }
                                          else
                                          {
                                              if (qty >= element.Quantity)
                                              {
                                                  section.Cards.RemoveCard(element);
                                              }
                                              else
                                              {
                                                  element.Quantity -= qty;
                                              }
                                          }
                                      });
            }
            else
            {
                if (section != null)
                {
                    element.Quantity = 1;
                    section.Cards.RemoveCard(element);
                }
                if (!UnlimitedPool.Contains(element))
                    CardPool.Add(element);
            }
        }

        private void MouseEnterCard(object sender, MouseEventArgs e)
        {
            var src = e.Source as FrameworkElement;
            if (src == null) return;
            var model = src.DataContext as ObservableMultiCard;
            if (model == null) return;
            RaiseEvent(new CardEventArgs(model, CardControl.CardHoveredEvent, sender));
        }

        private void MouseLeaveCard(object sender, MouseEventArgs e)
        {
            RaiseEvent(new CardEventArgs(CardControl.CardHoveredEvent, sender));
        }

        #region Filters

        private readonly List<FilterValue> _activeRestrictions = new List<FilterValue>();

        private bool FilterCard(object c)
        {
            if (_activeRestrictions == null || _activeRestrictions.Count == 0)
                return true;
            var card = (ObservableMultiCard)c;

            return (from restriction in _activeRestrictions.GroupBy(fv => fv.Property)
                    let prop = restriction.Key
                    let value = card.PropertySet().ContainsKey(prop) ? card.PropertySet()[prop] : null
                    select restriction.Any(filterValue => filterValue.IsValueMatch(value))).All(isOk => isOk);
        }

        private void CreateFilters()
        {
            Filters = new ObservableCollection<Filter>();
            foreach (Filter filter in Program.GameEngine.Definition.CustomProperties
                .Where(p => !p.Hidden && (p.Type == PropertyType.Integer || p.TextKind != PropertyTextKind.FreeText)).
                Select(prop => new Filter
                                   {
                                       Name = prop.Name,
                                       Property = prop,
                                       Values = new ObservableCollection<FilterValue>()
                                   }))
            {
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
            _activeRestrictions.Clear();
            foreach (Filter filter in Filters)
            {
                PropertyDef prop = filter.Property;
                filter.Values.Clear();
                if (prop.Type == PropertyType.String)
                    switch (prop.TextKind)
                    {
                        case PropertyTextKind.Enumeration:
                            Filter filter2 = filter;
                            IEnumerable<EnumFilterValue> q = from ObservableMultiCard c in CardPoolView
                                                             group c by this.GetCardPropertyValue(c, prop)
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
                            IEnumerable<TokenFilterValue> q2 = from ObservableMultiCard c in CardPoolView
                                                               let all = this.GetCardPropertyValue(c, prop)
                                                               where all != null
                                                               from token in
                                                                   all.Split(new[] { ' ' },
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
            if (Filters.Count > 0)
                FilterListBox.Visibility = Visibility.Visible;
            else
                FilterListBox.Visibility = Visibility.Collapsed;
        }

        private string GetCardPropertyValue(ObservableMultiCard card, PropertyDef def)
        {
            if (!card.PropertySet().ContainsKey(def)) return null;
            return card.PropertySet()[def] as String;
        }

        private void CardPoolChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (FilterValue fv in Filters.SelectMany(f => f.Values))
            {
                if (e.NewItems != null)
                {
                    FilterValue fv1 = fv;
                    foreach (ObservableMultiCard card in e.NewItems.Cast<ObservableMultiCard>().Where(fv1.IsMatch))
                        fv.Count++;
                }
                if (e.OldItems != null)
                {
                    FilterValue fv1 = fv;
                    foreach (ObservableMultiCard card in e.OldItems.Cast<ObservableMultiCard>().Where(fv1.IsMatch))
                        fv.Count--;
                }
            }
        }

        private void FilterChecked(object sender, RoutedEventArgs e)
        {
            var src = ((FrameworkElement)sender);
            var filterValue = (FilterValue)src.DataContext;
            _activeRestrictions.Add(filterValue);
            CardPoolView.Refresh();
        }

        private void FilterUnchecked(object sender, RoutedEventArgs e)
        {
            var src = ((FrameworkElement)sender);
            var filterValue = (FilterValue)src.DataContext;
            _activeRestrictions.Remove(filterValue);
            CardPoolView.Refresh();
        }

        #endregion

        #region Quantity popup

        private Action<int> _quantityPopupAction;

        private void OpenQuantityPopup(Action<int> qtyAction)
        {
            _quantityPopupAction = qtyAction;
            quantityPopup.IsOpen = true;
            quantityBox.Text = "1";
            quantityBox.Focus();
            quantityBox.SelectAll();
        }

        private void QuantityBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;
                    int qty;
                    if (!int.TryParse(quantityBox.Text, out qty)) return;
                    if (qty < 1) return;
                    _quantityPopupAction(qty);
                    quantityPopup.IsOpen = false;
                    break;
                case Key.Escape:
                    e.Handled = true;
                    quantityPopup.IsOpen = false;
                    break;
            }
        }

        private void QuantityBoxLostFocus(object sender, RoutedEventArgs e)
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
                if (value == null) return false;
                return (value as string) == Value;
            }
        }

        #endregion

        #region Nested type: Filter

        public class Filter
        {
            public string Name { get; set; }
            public DataNew.Entities.PropertyDef Property { get; set; }
            public ObservableCollection<FilterValue> Values { get; set; }
        }

        #endregion

        #region Nested type: FilterValue

        public abstract class FilterValue : INotifyPropertyChanged
        {
            private int _count;
            public DataNew.Entities.PropertyDef Property { get; set; }
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

            public bool IsMatch(ObservableMultiCard c)
            {

                if (!c.PropertySet().ContainsKey(Property)) return false;
                return IsValueMatch(c.PropertySet()[Property]);
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
            private Regex _regex;
            private string _value;

            public string Value
            {
                get { return _value; }
                set
                {
                    _value = value;
                    _regex = new Regex(@"(^|\s)" + value + @"($|\s)", RegexOptions.Compiled);
                }
            }

            public override bool IsValueMatch(object value)
            {
                if (value == null) return false;
                var strValue = value as string;
                return strValue != null && _regex.IsMatch(strValue);
            }
        }

        #endregion

        private void ButtonMoveClick(object sender, RoutedEventArgs e)
        {
            if (ComboBoxDeckSection.SelectedIndex < 0) return;
            var list = CardPool.ToArray();
            var cbSection = (KeyValuePair<string, DeckSection>)ComboBoxDeckSection.SelectedItem;
            var section =
                deckTabs.Items.OfType<ObservableSection>()
                    .FirstOrDefault(
                        x => x.Name.Equals(cbSection.Value.Name, StringComparison.InvariantCultureIgnoreCase));
            if (section == null) return;
            foreach (var c in list)
            {
                CardPool.Remove(c);
                var element = section.Cards.FirstOrDefault(x => x.Id == c.Id); //.AsObservable();
                if (element != null) element.Quantity++;
                else section.Cards.AddCard(c);
            }

        }
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
