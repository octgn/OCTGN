using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Octgn.Annotations;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew.Entities;

namespace Octgn.Windows
{
    /// <summary>
    /// Interaction logic for PickCardFromList.xaml
    /// </summary>
    public partial class PickCardFromList : INotifyPropertyChanged
    {
        public ObservableCollection<Card> CardList
        {
            get { return cardList; }
            set
            {
                if (value == cardList) return;
                cardList = value;
                OnPropertyChanged("CardList");
            }
        }

        private ObservableCollection<Card> cardList;

        public Card SelectedCard
        {
            get { return selectedCard; }
            set
            {
                if (value == selectedCard) return;
                selectedCard = value;
                OnPropertyChanged("SelectedCard");
            }
        }

        private Card selectedCard;

        public PickCardFromList()
        {
            CardList = new ObservableCollection<Card>();
            InitializeComponent();
        }

        public bool? PickCard(string cardName, IEnumerable<Card> cards)
        {
            Title = "Multiple Matches Found For " + cardName;
            CardList = new ObservableCollection<Card>(cards);
            return this.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnPickCard(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCard != null)
                DialogResult = true;
            this.Close();
        }
    }

    [ValueConversion(typeof(ICard), typeof(ImageSource))]
    public class ICardToImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var card = value as ICard;
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
