using Octgn.DataNew.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

        public void Clear() {
            Sections.Clear();
        }

        public void ResetCounters() {
            foreach(var section in Sections) {
                foreach(var card in section.Cards) {
                    card.ResetCounters();
                }
            }
        }

        public ObservableCollection<DeckStatsSectionViewModel> Sections { get; }

        public DeckStatsViewModel() {
            Sections = new ObservableCollection<DeckStatsSectionViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DeckStatsSectionViewModel : INotifyPropertyChanged
    {
        public string Name {
            get => _name;
            set {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        private string _name;

        public ObservableCollection<DeckStatsCardViewModel> Cards { get; }

        public DeckStatsSectionViewModel(string name) {
            Name = name;
            Cards = new ObservableCollection<DeckStatsCardViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DeckStatsCardViewModel : ObservableMultiCard 
    {
        public int ViewedCards {
            get => _viewedIds.Count;
        }
        private readonly HashSet<int> _viewedIds = new HashSet<int>();

        public BitmapImage Image {
            get => _image;
            set {
                if (value == _image) return;
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }
        private BitmapImage _image;

        public void ResetCounters() {
            _viewedIds.Clear();

            OnPropertyChanged(nameof(ViewedCards));
        }

        public DeckStatsCardViewModel(Guid id, Guid setId, string name, string imageuri, string alternate, CardSize size, System.Collections.Generic.IDictionary<string, CardPropertySet> properties, int quantity) : base(id, setId, name, imageuri, alternate, size, properties, quantity) {
        }

        public DeckStatsCardViewModel(ICard card, int quantity) : base(card, quantity) {
        }

        public DeckStatsCardViewModel(IMultiCard card) : base(card) {
        }

        public void AttachCard(Card card) {
            card.PropertyChanged += Card_PropertyChanged;

            if (card.FaceUp) {
                if (!_viewedIds.Contains(card.Id)) {
                    _viewedIds.Add(card.Id);
                    OnPropertyChanged(nameof(ViewedCards));
                }
            }
        }

        private void Card_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == nameof(Card.FaceUp)) {
                var card = (Card)sender;
                if (card.FaceUp) {
                    if (!_viewedIds.Contains(card.Id)) {
                        _viewedIds.Add(card.Id);
                        OnPropertyChanged(nameof(ViewedCards));
                    }
                }
            }
        }
    }
}
