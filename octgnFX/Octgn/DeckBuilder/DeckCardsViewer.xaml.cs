using System.Windows.Controls;

namespace Octgn.DeckBuilder
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Octgn.Annotations;
    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;

    /// <summary>
    /// Interaction logic for DeckCardsViewer.xaml
    /// </summary>
    public partial class DeckCardsViewer : INotifyPropertyChanged
    {
        public static readonly DependencyProperty DeckProperty =
             DependencyProperty.Register("Deck", typeof(MetaDeck),
             typeof(DeckCardsViewer), new FrameworkPropertyMetadata(OnDeckChanged));

        private static void OnDeckChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as DeckCardsViewer;
            var newdeck = e.NewValue as MetaDeck ?? new MetaDeck(){IsCorrupt = true};
            var g = GameManager.Get().GetById(newdeck.GameId);
            viewer.deck = newdeck.IsCorrupt ?
                null
                : g == null ?
                    null
                    : newdeck;
            viewer._view = viewer.deck == null ? 
                new ListCollectionView(new List<MetaMultiCard>()) 
                : new ListCollectionView(viewer.deck.Sections.SelectMany(x => x.Cards).ToList());
            viewer.OnPropertyChanged("Deck");
            viewer.OnPropertyChanged("SelectedCard");
            Task.Factory.StartNew(
                () =>
                {
                    Thread.Sleep(0);
                    viewer.Dispatcher.BeginInvoke(new Action(
                        () =>
                        {
                            viewer.FilterChanged(viewer._filter);
                        }));
                });
            
        }

        public MetaDeck Deck
        {
            get { return (MetaDeck)GetValue(DeckProperty); }
            set { SetValue(DeckProperty, value); }
        }
        private Predicate<MetaMultiCard> _filterCards;
        private ListCollectionView _view;

        private MetaMultiCard selectedCard;

        private string _filter = "";

        private MetaDeck deck;

        public MetaMultiCard SelectedCard
        {
            get
            {
                return this.selectedCard;
            }
            set
            {
                if (Equals(value, this.selectedCard))
                {
                    return;
                }
                this.selectedCard = value;
                this.OnPropertyChanged("SelectedCard");
            }
        }

        public Predicate<MetaMultiCard> FilterCards
        {
            get { return _filterCards; }
            set
            {
                _filterCards = value;
                _view.Filter = value == null ? (Predicate<object>)null : o => _filterCards((MetaMultiCard)o);
                _view.Refresh();
            }
        }

        public DeckCardsViewer()
        {
            InitializeComponent();
        }

        public void FilterChanged(string filter)
        {
            _filter = filter;
            if (deck == null) return;
            //if (deck.Sections.Any())
            //{
            //    if (this.SectionTabs.Items.Count != deck.Sections.Count())
            //    {
            //        var of = filter.Clone() as string;
            //        var a = new WaitCallback((x) =>
            //        {
            //            Thread.Sleep(1000);
            //            if(of == _filter)
            //                this.Dispatcher.Invoke(new Action(() => this.FilterChanged(this._filter)));
            //        });
            //        ThreadPool.QueueUserWorkItem(a);
            //        return;
            //    }
            //}
            if (filter == "")
            {
                FilterCards = null;
                var cl = SectionTabs.Items.OfType<ISection>().SelectMany(x => x.Cards).OfType<MetaMultiCard>().ToList();
                foreach (var c in cl)
                {
                    c.IsVisible = true;
                }
            }
            else
            {
                FilterCards = c =>
                {
                    var b1 = c.Name.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0;
                    if (b1) return true;
                    var b2 =
                        c.Properties.SelectMany(x => x.Value.Properties)
                            .Any(
                                x => x.Value.ToString().IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0);
                    return b2;
                };
            }
            var clist = SectionTabs.Items.OfType<ISection>().SelectMany(x => x.Cards).OfType<MetaMultiCard>().ToList();
            foreach (var c in clist)
            {
                c.IsVisible = IsCardVisible(c);
            }
            OnPropertyChanged("IsVisible");
        }

        private void SetPicture(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            if (img == null) return;
            var model = img.DataContext as IMultiCard;
            if (model == null) return;
            var bi = new BitmapImage(new Uri(model.GetPicture()));
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.Freeze();
            img.Source = bi;
        }

        public bool IsCardVisible(IMultiCard card)
        {
            if (FilterCards == null)
                return true;
            var ofList = this._view.OfType<IMultiCard>();
            return ofList.Any(x => x.Id == card.Id);
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

        private void DoneClicked(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }

    [ValueConversion(typeof(IMultiCard), typeof(ImageSource))]
    public class IMultiCardToImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var card = value as IMultiCard;
            if (card == null) 
                return "pack://application:,,,/Resources/Front.jpg";
            return card.GetPicture();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    [ValueConversion(typeof(Section), typeof(Visibility))]
    public class SectionToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var section = value as Section;
            if (section == null) return Visibility.Collapsed;

            return section.Quantity > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
