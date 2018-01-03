using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octide.ItemModel
{
    public class SetItemModel : IdeListBoxItemBase, ICloneable
    {
        public Set _set;
        private ObservableCollection<CardItemModel> _cardItems;
        private ObservableCollection<PackageItemModel> _packItems;

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
                _set.Cards = CardItems.Select(x => x._cardDef).ToList();
            };
            RaisePropertyChanged("CardItems");

            PackItems = new ObservableCollection<PackageItemModel>();
            PackItems.CollectionChanged += (a, b) =>
            {
                _set.Packs = PackItems.Select(x => x._pack).ToList();
            };
            RaisePropertyChanged("PackItems");
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
            CanDragDrop = false;
        }

        public SetItemModel(Set s) // For loading existing set data
        {
            _set = s;
            CardItems = new ObservableCollection<CardItemModel>(_set.Cards.Select(x => new CardItemModel(x) { Parent = this }));
            CardItems.CollectionChanged += (a, b) =>
            {
                _set.Cards = CardItems.Select(x => x._cardDef).ToList();
            };
            RaisePropertyChanged("CardItems");
            
            PackItems = new ObservableCollection<PackageItemModel>(_set.Packs.Select(x => new PackageItemModel(x) { Parent = this }));
            PackItems.CollectionChanged += (a, b) =>
            {
                _set.Packs = PackItems.Select(x => x._pack).ToList();
            };
            RaisePropertyChanged("PackItems");
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
            CanDragDrop = false;
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
            CardItems = new ObservableCollection<CardItemModel>(s.CardItems.Select(x => new CardItemModel(x) { Parent = this }));
            CardItems.CollectionChanged += (a, b) =>
            {
                _set.Cards = CardItems.Select(x => x._cardDef).ToList();
            };
            _set.Cards = CardItems.Select(x => x._cardDef).ToList();
            RaisePropertyChanged("CardItems");

            PackItems = new ObservableCollection<PackageItemModel>(s.PackItems.Select(x => new PackageItemModel(x) { Parent = this } ));
            PackItems.CollectionChanged += (a, b) =>
            {
                _set.Packs = PackItems.Select(x => x._pack).ToList();
            };
            _set.Packs = PackItems.Select(x => x._pack).ToList();
            RaisePropertyChanged("PackItems");
            RemoveCommand = new RelayCommand(Remove);
            CopyCommand = new RelayCommand(Copy);
            InsertCommand = new RelayCommand(Insert);
            CanDragDrop = false;
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

        
        public void Copy()
        {
            if (CanCopy == false) return;

            ViewModelLocator.SetTabViewModel.Items.IndexOf(this);
            var index = ViewModelLocator.SetTabViewModel.Items.IndexOf(this);
            ViewModelLocator.SetTabViewModel.Items.Insert(index, Clone() as SetItemModel);
        }

        public void Remove()
        {
            if (CanRemove == false) return;
            ViewModelLocator.SetTabViewModel.Items.Remove(this);
        }

        public void Insert()
        {
            if (CanInsert == false) return;
            var index = ViewModelLocator.SetTabViewModel.Items.IndexOf(this);
            ViewModelLocator.SetTabViewModel.Items.Insert(index, new SetItemModel());
        }

    }
}
