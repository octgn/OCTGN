using Octgn.DataNew.Entities;
using Octgn.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Octgn.Play.Gui
{
    public partial class DeckStats : UserControl
    {
        public DeckStats() {
            InitializeComponent();
        }
    }

    public class DeckStatsViewModel : INotifyPropertyChanged
    {
        public bool IsVisible {
            get => _isVisible;
            set {
                if (value == _isVisible) return;
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }
        private bool _isVisible;

        public ObservableCollection<DeckStatsCardViewModel> Cards { get; }

        public DeckStatsViewModel() {
            Cards = new ObservableCollection<DeckStatsCardViewModel>();
        }

        public void Reset() {
            foreach(var card in Cards.ToArray()) {
                card.Reset();
                Cards.Remove(card);
            }
        }

        public void AddCard(Card card) {
            var cardVm = Cards.FirstOrDefault(x => x.ModelId == card.Type.Model.Id);
            if (cardVm == null) {
                cardVm = new DeckStatsCardViewModel(card);
                AddCardVm(cardVm);
            }

            cardVm.AddCard(card);
        }

        private void AddCardVm(DeckStatsCardViewModel vm) {
            if (Cards.Count == 0) {
                Cards.Add(vm);
            } else {
                for(var i = 0; i < Cards.Count; i++) {
                    var cardA = Cards[i];
                    var cardB = vm;

                    var comparison = cardA.Name.CompareTo(cardB.Name);

                    if (comparison < 0) { // cardA comes before cardB
                        if(i == Cards.Count - 1) { // we're at the end of the list, add it to the end
                            Cards.Add(vm);
                            break;
                        }
                    } else if(comparison == 0) { // cardA ==  cardB
                        Cards.Insert(i, vm);
                        break;
                    } else { // cardA comes after cardB
                        Cards.Insert(i, vm);
                        break;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DeckStatsCardViewModel : INotifyPropertyChanged
    {
        public Guid ModelId { get; }

        public string Name { get; }

        public int CardCount {
            get => _cards.Count(x => !x.FaceUp);
        }

        private readonly HashSet<Card> _cards = new HashSet<Card>();

        public BitmapImage Image {
            get => _image;
            set {
                if (value == _image) return;
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }
        private BitmapImage _image;

        public bool IsMouseOver {
            get => _isMouseOver;
            set {
                if (value == _isMouseOver) return;
                _isMouseOver = value;
                OnPropertyChanged(nameof(IsMouseOver));
            }
        }
        private bool _isMouseOver;

        public bool IsFlashing {
            get => _isFlashing;
            set {
                if (value == _isFlashing) return;
                _isFlashing = value;
                OnPropertyChanged(nameof(IsFlashing));
            }
        }
        private bool _isFlashing;

        public DeckStatsCardViewModel(Card card) {
            ModelId = card.Type.Model.Id;
            Name = card.Type.Model.Name;

            ImageUtils.GetCardImage(card.Type.Model, x => Image = x, false);
        }

        public void AddCard(Card card) {
            _cards.Add(card);
            card.PropertyChanged += Card_PropertyChanged;
            OnPropertyChanged(nameof(CardCount));
        }

        public void Reset() {
            foreach (var card in _cards) {
                card.PropertyChanged -= Card_PropertyChanged;
            }
            _cards.Clear();
        }

        private void Card_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(Card.FaceUp)) {
                OnPropertyChanged(nameof(CardCount));
                Flash();
            }
        }

        private async void Flash() {
            await Dispatcher.Yield(DispatcherPriority.Loaded);
            IsFlashing = true;
            IsFlashing = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
