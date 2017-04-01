using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octgn.DataNew.Entities;
using Octgn.Core.DataExtensionMethods;
using Octide.Messages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using Octgn.Library;
using GongSolutions.Wpf.DragDrop;

namespace Octide.ViewModel
{
    public class SetTabViewModel : ViewModelBase
    {

        public SetTabViewModel()
        {
            AddSetCommand = new RelayCommand(AddSet);
            PackageDropHandler = new PackageDropHandler();
            RemoveSetCommand = new RelayCommand(RemoveSet, EnableSetButton);
            AddCardCommand = new RelayCommand(AddCard);
            RemoveCardCommand = new RelayCommand(RemoveCard, EnableCardButton);
            AddPackCommand = new RelayCommand(AddPack);
            RemovePackCommand = new RelayCommand(RemovePack, EnablePackButton);
            AddAltCommand = new RelayCommand(AddAlt);
            SetPanelVisibility = Visibility.Hidden;
            CardPanelVisibility = Visibility.Hidden;
            AltPanelVisibility = Visibility.Hidden;

            SetItems = new ObservableCollection<SetItemModel>(ViewModelLocator.GameLoader.Sets.Select(x => new SetItemModel(x)));

            SetItems.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Sets = SetItems.Select(x => x._set).ToList();
            };
            SelectedSet = SetItems.FirstOrDefault();
            SelectedPack = SelectedSet.PackItems.FirstOrDefault();
        }

        #region set tab

        private Visibility _setPanelVisibility;
        private SetItemModel _selectedSet;
        public ObservableCollection<SetItemModel> SetItems { get; private set; }

        public RelayCommand AddSetCommand { get; private set; }
        public RelayCommand RemoveSetCommand { get; private set; }


        public SetItemModel SelectedSet
        {
            get { return _selectedSet; }
            set
            {
                if (value == _selectedSet) return;
                _selectedSet = value;
                SetPanelVisibility = value == null ? Visibility.Hidden : Visibility.Visible;
                RaisePropertyChanged("SelectedSet");
                RemoveSetCommand.RaiseCanExecuteChanged();
                //    SelectedCard = SelectedSet.CardItems.FirstOrDefault() ?? null;
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

        public bool EnableSetButton()
        {
            return SelectedSet != null;
        }

        public void AddSet()
        {
            var ret = new SetItemModel() { Name = "Set" };
            SetItems.Add(ret);
            SelectedSet = ret;
            RaisePropertyChanged("SetItems");
        }

        public void RemoveSet()
        {
            SetItems.Remove(SelectedSet);
        }
        #endregion

        #region package tab

        public PackageDropHandler PackageDropHandler { get; set; }
        private Visibility _packPanelVisibility;
        private PackageItemModel _selectedPack;


        public RelayCommand AddPackCommand { get; private set; }
        public RelayCommand RemovePackCommand { get; private set; }

        public PackageItemModel SelectedPack
        {
            get { return _selectedPack; }
            set
            {
                if (_selectedPack == value) return;
                _selectedPack = value;

                PackPanelVisibility = value == null ? Visibility.Hidden : Visibility.Visible;

                RaisePropertyChanged("SelectedPack");
                RemovePackCommand.RaiseCanExecuteChanged();
            }
        }

        public Visibility PackPanelVisibility
        {
            get { return _packPanelVisibility; }
            set { Set(ref _packPanelVisibility, value); }
        }

        public bool EnablePackButton() => SelectedPack != null;

        public void AddPack()
        {
            var ret = new PackageItemModel();
            SelectedSet.PackItems.Add(ret);
            SelectedPack = ret;
            RaisePropertyChanged("SelectedPack");
        }

        public void RemovePack()
        {
            SelectedSet.PackItems.Remove(SelectedPack);
            RaisePropertyChanged("SelectedPack");
        }

        public static IBasePack LoadPackItem(IPackItem p)
        {
            IBasePack pack = null;
            if (p is OptionsList)
                pack = new PackOptionsItemModel(p);
            else if (p is Pick)
                pack = new PackPickItemModel(p);
            return pack;
        }

        #endregion

        #region card tab

        private Visibility _cardPanelVisibility;
        private CardItemModel _selectedCard;


        public RelayCommand AddCardCommand { get; private set; }
        public RelayCommand RemoveCardCommand { get; private set; }

        public CardItemModel SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                if (_selectedCard == value) return;
                _selectedCard = value;

                CardPanelVisibility = value == null ? Visibility.Hidden : Visibility.Visible;

                RaisePropertyChanged("SelectedCard");
                RemoveCardCommand.RaiseCanExecuteChanged();
                if (value != null)
                {
                    SelectedAlt = SelectedCard.Default;
                    RaisePropertyChanged("SelectedAlt");
                }
            }
        }

        public Visibility CardPanelVisibility
        {
            get { return _cardPanelVisibility; }
            set { Set(ref _cardPanelVisibility, value); }
        }

        public bool EnableCardButton() => SelectedCard != null;

        public void AddCard()
        {
            var ret = new CardItemModel() { ParentSet = SelectedSet };
            SelectedSet.CardItems.Add(ret);
            SelectedCard = ret;
            RaisePropertyChanged("SelectedCard");
        }

        public void RemoveCard()
        {
            SelectedSet.CardItems.Remove(SelectedCard);
            RaisePropertyChanged("SelectedCard");
        }

        #endregion


        #region alt tab

        private AltItemModel _selectedAlt;
        private Visibility _altPanelVisibility;


        public RelayCommand AddAltCommand { get; private set; }

        public Visibility AltPanelVisibility
        {
            get { return _altPanelVisibility; }
            set { Set(ref _altPanelVisibility, value); }
        }

        public AltItemModel SelectedAlt
        {
            get { return _selectedAlt; }
            set
            {
                if (!Set(ref _selectedAlt, value)) return;
                AltPanelVisibility = value == null ? Visibility.Hidden : Visibility.Visible;
                ViewModelLocator.ProxyDesignViewModel.Card = value;
                RaisePropertyChanged(nameof(SelectedAlt));
            }
        }

        public void AddAlt()
        {
            var ret = new AltItemModel() { ParentCard = SelectedCard };
            SelectedCard.AltItems.Add(ret);
            SelectedAlt = ret;
            RaisePropertyChanged("SelectedAlt");
        }

        #endregion


        #region properties

        public IEnumerable<SizeItemModel> CardSizes => ViewModelLocator.PreviewTabViewModel.CardSizes;

        #endregion
    }

    public class SetItemModel : ViewModelBase, ICloneable
    {
        public Set _set;
        private ObservableCollection<CardItemModel> _cardItems;
        private ObservableCollection<PackageItemModel> _packItems;
        public RelayCommand RemoveCommand { get; set; }

        public ObservableCollection<CardItemModel> CardItems
        {
            get { return _cardItems; }
            set { Set(ref _cardItems, value); }
        }

        public ObservableCollection<PackageItemModel> PackItems
        {
            get { return _packItems; }
            set { Set(ref _packItems, value); }
        }

        public SetItemModel() //for creating a new set
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
            _set.Packs = new List<Pack>();

            CardItems = new ObservableCollection<CardItemModel>();
            CardItems.CollectionChanged += (a, b) =>
            {
                _set.Cards = CardItems.Select(x => x.CardDef).ToList();
            };
            RaisePropertyChanged("CardItems");

            PackItems = new ObservableCollection<PackageItemModel>();
            PackItems.CollectionChanged += (a, b) =>
            {
                _set.Packs = PackItems.Select(x => x._pack).ToList();
            };
            RaisePropertyChanged("PackItems");
            RemoveCommand = new RelayCommand(Remove);
        }

        public SetItemModel(Set s) // For loading existing set data
        {
            _set = s;
            CardItems = new ObservableCollection<CardItemModel>(_set.Cards.Select(x => new CardItemModel(x) { ParentSet = this }));
            CardItems.CollectionChanged += (a, b) =>
            {
                _set.Cards = CardItems.Select(x => x.CardDef).ToList();
            };
            RaisePropertyChanged("CardItems");

            PackItems = new ObservableCollection<PackageItemModel>(_set.Packs.Select(x => new PackageItemModel(x)));
            PackItems.CollectionChanged += (a, b) =>
            {
                _set.Packs = PackItems.Select(x => x._pack).ToList();
            };
            RaisePropertyChanged("PackItems");
            RemoveCommand = new RelayCommand(Remove);

        }

        public SetItemModel(SetItemModel s) //for copying the item
        {
            _set = new Set();
            _set.Name = s._set.Name;
            _set.Id = Guid.NewGuid();
            _set.GameId = s._set.GameId;
            _set.Version = s._set.Version;
            _set.GameVersion = s._set.GameVersion;
            string installPath = Path.Combine(ViewModelLocator.GameLoader.GamePath, "Sets", _set.Id.ToString());
            _set.InstallPath = installPath;
            _set.Filename = Path.Combine(installPath, "set.xml");
            _set.PackUri = Path.Combine(installPath, "Cards");
            _set.Hidden = s._set.Hidden;
            CardItems = new ObservableCollection<CardItemModel>(s.CardItems.Select(x => new CardItemModel(x) { ParentSet = s }));
            CardItems.CollectionChanged += (a, b) =>
            {
                _set.Cards = CardItems.Select(x => x.CardDef).ToList();
            };
            _set.Cards = CardItems.Select(x => x.CardDef).ToList();
            RaisePropertyChanged("CardItems");

            PackItems = new ObservableCollection<PackageItemModel>(s.PackItems.Select(x => new PackageItemModel(x)));
            PackItems.CollectionChanged += (a, b) =>
            {
                _set.Packs = PackItems.Select(x => x._pack).ToList();
            };
            RaisePropertyChanged("PackItems");
            RemoveCommand = new RelayCommand(Remove);
        }

        public void Remove()
        {
            ViewModelLocator.SetTabViewModel.SetItems.Remove(this);
        }

        public Guid Id
        {
            get
            {
                return _set.Id;
            }
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

        public bool Hidden
        {
            get
            {
                return _set.Hidden;
            }
            set
            {
                if (value == _set.Hidden) return;
                _set.Hidden = value;
                RaisePropertyChanged("Hidden");
            }
        }

        public object Clone()
        {
            return new SetItemModel(this);
        }
    }
}