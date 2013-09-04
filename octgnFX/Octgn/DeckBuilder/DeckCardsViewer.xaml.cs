using System.Windows.Controls;

namespace Octgn.DeckBuilder
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Octgn.Annotations;
    using Octgn.Controls;
    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew.Entities;
    using Octgn.Extentions;

    /// <summary>
    /// Interaction logic for DeckCardsViewer.xaml
    /// </summary>
    public partial class DeckCardsViewer : INotifyPropertyChanged
    {
        public IDeck Deck { get; set; }

        private Predicate<MetaMultiCard> _filterCards;
        private ListCollectionView _view;

        private Game _game;

        private MetaMultiCard selectedCard;

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

        public DeckCardsViewer(IDeck deck, Game game)
        {
            _game = game;
            Deck = deck;
            _view = new ListCollectionView(deck.Sections.SelectMany(x => x.Cards).ToList());
            InitializeComponent();
        }

        private void FilterChanged(object sender, TextChangedEventArgs e)
        {
            string filter = filterBox.Text;
            if (filter == "")
            {
                watermark.Visibility = System.Windows.Visibility.Visible;
                FilterCards = null;
            }
            else
            {
                watermark.Visibility = System.Windows.Visibility.Hidden;
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

        private void ComputeChildWidth(object sender, RoutedEventArgs e)
        {
            var panel = sender as VirtualizingWrapPanel;
            if (panel != null) panel.ChildWidth = panel.ChildHeight * _game.CardWidth / _game.CardHeight;
        }

        private void FilterBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            e.Handled = true;
            filterBox.Text = "";
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
}
