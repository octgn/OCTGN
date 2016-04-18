using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Octide.ViewModel
{
    public class SetTabViewModel : ViewModelBase
    {

        private Visibility _setPanelVisibility;
        private Visibility _cardPanelVisibility;
        private SetListItemModel _selectedSet;
        private CardListItemModel _selectedCard;
        public ObservableCollection<SetListItemModel> SetItems { get; private set; }

        public RelayCommand AddSetCommand { get; private set; }
        public RelayCommand RemoveSetCommand { get; private set; }
        public RelayCommand AddCardCommand { get; private set; }
        public RelayCommand RemoveCardCommand { get; private set; }
        public RelayCommand CopyCardCommand { get; private set; }
        public RelayCommand UpCardCommand { get; private set; }
        public RelayCommand DownCardCommand { get; private set; }


        public SetTabViewModel()
        {
            AddSetCommand = new RelayCommand(AddSet);
            AddCardCommand = new RelayCommand(AddCard);
            RemoveSetCommand = new RelayCommand(RemoveSet, EnableSetButton);
            RemoveCardCommand = new RelayCommand(RemoveCard, EnableCardButton);
            CopyCardCommand = new RelayCommand(CopyItem, EnableCardButton);
            UpCardCommand = new RelayCommand(MoveItemUp, EnableCardButton);
            DownCardCommand= new RelayCommand(MoveItemDown, EnableCardButton);
            SetPanelVisibility = Visibility.Hidden;
            CardPanelVisibility = Visibility.Hidden;
            SetItems = new ObservableCollection<SetListItemModel>(ViewModelLocator.GameLoader.Sets.Select(x => new SetListItemModel(x)));
            SetItems.CollectionChanged += (a, b) => 
            {
                ViewModelLocator.GameLoader.Sets = SetItems.Select(x => x._set).ToList();
            };
        }
        
        public SetListItemModel SelectedSet
        {
            get { return _selectedSet; }
            set
            {
                if (value == _selectedSet) return;
                _selectedSet = value;
                if (value == null) SetPanelVisibility = Visibility.Hidden;
                else SetPanelVisibility = Visibility.Visible;
                RaisePropertyChanged("SelectedSet");
            }
        }

        public Visibility SetPanelVisibility
        {
            get { return _setPanelVisibility; }
            set
            {
                if (value == _setPanelVisibility) return;
                _setPanelVisibility = value;
                RaisePropertyChanged("SetPanelVisibility");
            }
        }

        public CardListItemModel SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                if (value == _selectedCard) return;
                _selectedCard = value;
                if (value == null) CardPanelVisibility = Visibility.Hidden;
                else CardPanelVisibility = Visibility.Visible;
                RaisePropertyChanged("SelectedCard");
            }
        }

        public Visibility CardPanelVisibility
        {
            get { return _cardPanelVisibility; }
            set
            {
                if (value == _cardPanelVisibility) return;
                _cardPanelVisibility = value;
                RaisePropertyChanged("CardPanelVisibility");
            }
        }

        public bool EnableSetButton()
        {
            return SelectedSet != null;
        }
        
        public void AddSet()
        {
            var ret = new SetListItemModel() { Name = "Set" };
            SetItems.Add(ret);
            SelectedSet = ret;
        }

        public void RemoveSet()
        {
            SetItems.Remove(SelectedSet);
        }

        public bool EnableCardButton()
        {
            return SelectedCard != null;
        }

        public void AddCard()
        {
            var ret = new CardListItemModel() { SetId = SelectedSet._set.Id };
            SelectedSet.Cards.Add(ret);
            SelectedCard = ret;
        }
        
        public void RemoveCard()
        {
            SelectedSet.Cards.Remove(SelectedCard);
        }

        public void CopyItem()
        {
            var ret = new CardListItemModel(SelectedCard);
            ret._card.Id = Guid.NewGuid();
            SelectedSet.Cards.Add(ret);
            SelectedCard = ret;
        }

        public void MoveItemUp()
        {
            MoveItem(-1);
        }

        public void MoveItemDown()
        {
            MoveItem(1);
        }

        public void MoveItem(int move)
        {
            var index = SelectedSet.Cards.IndexOf(SelectedCard);
            int newIndex = index + move;
            if (newIndex < 0 || newIndex >= SelectedSet.Cards.Count) return;
            SelectedSet.Cards.Move(index, index + move);
        }
    }

    public class SetListItemModel : ViewModelBase
    {
        public Set _set;
        public ObservableCollection<CardListItemModel> Cards { get; private set; }

        public SetListItemModel()
        {
            _set = new Set();
            _set.Id = Guid.NewGuid();
            _set.GameId = ViewModelLocator.GameLoader.Game.Id;
            _set.Version = Version.Parse("1.0");
            _set.GameVersion = ViewModelLocator.GameLoader.Game.Version;
            string installPath = Path.Combine(ViewModelLocator.GameLoader.GamePath, "Sets", _set.Id.ToString());
            _set.InstallPath = installPath;
            _set.Filename = Path.Combine(installPath, "set.xml");
            _set.PackUri = Path.Combine(installPath, "Cards");
            _set.Hidden = false;
            _set.Cards = new List<Card>();
            Cards = new ObservableCollection<CardListItemModel>(_set.Cards.Select(x => new CardListItemModel(x) { SetId = _set.Id }));
            Cards.CollectionChanged += (a, b) =>
            {
                _set.Cards = Cards.Select(x => x._card).ToList();
            };
        }

        public SetListItemModel(Set s)
        {
            _set = s;
            Cards = new ObservableCollection<CardListItemModel>(_set.Cards.Select(x => new CardListItemModel(x) { SetId = _set.Id }));
            Cards.CollectionChanged += (a, b) =>
            {
                _set.Cards = Cards.Select(x => x._card).ToList();
            };
        }

        public string Name
        {
            get
            {
                return _set.Name;
            }
            set
            {
                if (value == _set.Name) return;
                _set.Name = value;
                RaisePropertyChanged("Name");
            }
        }
    }

    public class CardListItemModel : ViewModelBase
    {
        public Card _card;

        public CardListItemModel() //for adding new items
        {
            _card = new Card(
                Guid.NewGuid(), 
                Guid.NewGuid(),
                "Card", 
                "",
                "",
                ViewModelLocator.GameLoader.Game.CardSize,
                new Dictionary<string, CardPropertySet>());
        }

        public CardListItemModel(Card c) //for regenerating the collection
        {
            _card = c;
        }

        public CardListItemModel(CardListItemModel c) //for copying items
        {
            _card = new Card(c._card);
            _card.Id = Guid.NewGuid(); //gotta give the copy a new guid
        }

        public string Name
        {
            get
            {
                return _card.Name;
            }
            set
            {
                if (value == _card.Name) return;
                _card.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public Guid Id
        {
            get
            {
                return _card.Id;
            }
            set
            {
                if (value == _card.Id) return;
                _card.Id = value;
                RaisePropertyChanged("Id");
            }
        }

        public Guid SetId
        {
            get
            {
                return _card.SetId;
            }
            set
            {
                if (value == _card.SetId) return;
                _card.SetId = value;
                RaisePropertyChanged("SetId");
            }
        }
    }
}