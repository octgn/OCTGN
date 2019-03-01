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

        public void Clear() {
            Groups.Clear();
        }

        public void ResetCounters() {
            foreach(var group in Groups) {
                foreach(var card in group.Cards) {
                    card.ResetCounters();
                }
            }
        }

        public ObservableCollection<DeckStatsGroupViewModel> Groups { get; }

        public DeckStatsViewModel() {
            Groups = new ObservableCollection<DeckStatsGroupViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DeckStatsGroupViewModel : INotifyPropertyChanged
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

        public DeckStatsGroupViewModel(Group group) {
            Name = group.Name;
            Cards = new ObservableCollection<DeckStatsCardViewModel>();

            group.Cards.CollectionChanged += Cards_CollectionChanged;
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

        private void Cards_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var addedCard in e.NewItems.Cast<Card>()) {
                        var cardVm = Cards.FirstOrDefault(x => x.ModelId == addedCard.Type.Model.Id);
                        if(cardVm == null) {
                            cardVm = new DeckStatsCardViewModel(addedCard);
                            AddCardVm(cardVm);
                        }

                        cardVm.AddCard(addedCard);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach(var removedCard in e.OldItems.Cast<Card>()) {
                        var cardVm = Cards.FirstOrDefault(x => x.ModelId == removedCard.Type.Model.Id);
                        cardVm.RemoveCard(removedCard);

                        if(cardVm.CardCount == 0) {
                            Cards.Remove(cardVm);
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    for(var i = 0; i < e.OldItems.Count; i++) {
                        var oldCard = (Card)e.OldItems[i];
                        var newCard = (Card)e.NewItems[i];

                        var cardVm = Cards.FirstOrDefault(x => x.ModelId == oldCard.Type.Model.Id);
                        cardVm.RemoveCard(oldCard);
                        if (cardVm.CardCount == 0)
                            Cards.Remove(cardVm);

                        var newVm = Cards.FirstOrDefault(x => x.ModelId == newCard.Type.Model.Id);
                        if (newVm == null) {
                            newVm = new DeckStatsCardViewModel(newCard);
                            AddCardVm(cardVm);
                        }
                        newVm.AddCard(newCard);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    // Don't care, the order is unimportant
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Cards.Clear();
                    break;
                default: throw new InvalidOperationException($"Can't handle {e.Action}");
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
            get => _ids.Count;
        }

        public IEnumerable<int> Ids => _ids.AsEnumerable();

        private readonly HashSet<int> _ids = new HashSet<int>();

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

        public void ResetCounters() {
            _ids.Clear();

            OnPropertyChanged(nameof(CardCount));
        }

        public void AddCard(Card card) {
            _ids.Add(card.Id);
            OnPropertyChanged(nameof(CardCount));
            Flash();
        }

        public void RemoveCard(Card card) {
            _ids.Remove(card.Id);
            OnPropertyChanged(nameof(CardCount));
            Flash();
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
