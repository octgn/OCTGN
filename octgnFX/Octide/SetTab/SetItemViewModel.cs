// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ItemModel;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Octide.ViewModel
{
    public class SetItemViewModel : IdeListBoxItemBase
    {
        public Set _set;

        public RelayCommand AddCardCommand { get; private set; }
        public RelayCommand AddPackageCommand { get; private set; }

        public ObservableCollection<IdeListBoxItemBase> CardItems { get; private set; }
        public ObservableCollection<IdeListBoxItemBase> PackageItems { get; private set; }

        public SetItemViewModel() //for creating a new set
        {
            _set = new Set
            {
                Name = "Set",
                Id = Guid.NewGuid(),
                GameId = ViewModelLocator.GameLoader.Game.Id,
                Hidden = false,
                Cards = new List<Card>(),
                Packs = new List<Pack>()
            };
            string installPath = Path.Combine(ViewModelLocator.GameLoader.GamePath, "Sets", _set.Id.ToString());
            _set.InstallPath = installPath;
            _set.Filename = Path.Combine(installPath, "set.xml");
            _set.PackUri = Path.Combine(installPath, "Cards");

            CardItems = new ObservableCollection<IdeListBoxItemBase>();
            CardItems.CollectionChanged += (a, b) =>
            {
                _set.Cards = CardItems.Select(x => (x as SetCardItemViewModel)._card).ToList();
            };
            RaisePropertyChanged("CardItems");

            PackageItems = new ObservableCollection<IdeListBoxItemBase>();
            PackageItems.CollectionChanged += (a, b) =>
            {
                _set.Packs = PackageItems.Select(x => (x as SetPackageItemViewModel)._pack).ToList();
            };


            AddCardCommand = new RelayCommand(AddCard);
            AddPackageCommand = new RelayCommand(AddPackage);

            RaisePropertyChanged("PackItems");
            CanDragDrop = false;
        }

        public SetItemViewModel(Set s) // For loading existing set data
        {
            _set = s;
            CardItems = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var card in _set.Cards)
            {
                CardItems.Add(new SetCardItemViewModel(card) {ItemSource = CardItems, Parent = this });
            }
            CardItems.CollectionChanged += (a, b) =>
            {
                _set.Cards = CardItems.Select(x => (x as SetCardItemViewModel)._card).ToList();
            };
            RaisePropertyChanged("CardItems");

            PackageItems = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var package in _set.Packs)
            {
                PackageItems.Add(new SetPackageItemViewModel(package) {ItemSource = PackageItems, Parent = this });
            }
            PackageItems.CollectionChanged += (a, b) =>
            {
                _set.Packs = PackageItems.Select(x => (x as SetPackageItemViewModel)._pack).ToList();
            };

            AddCardCommand = new RelayCommand(AddCard);
            AddPackageCommand = new RelayCommand(AddPackage);

            RaisePropertyChanged("PackItems");
            CanDragDrop = false;
        }

        public SetItemViewModel(SetItemViewModel s) //for copying the item
        {
            var id = Guid.NewGuid();
            string installPath = Path.Combine(ViewModelLocator.GameLoader.GamePath, "Sets", id.ToString());
            _set = new Set()
            {
                Name = s._set.Name,
                Id = id,
                GameId = s._set.GameId,
                Hidden = s._set.Hidden,
                ShortName = s._set.ShortName,
                Description = s._set.Description,
                ReleaseDate = s._set.ReleaseDate,
                InstallPath = installPath,
                Filename = Path.Combine(installPath, "set.xml"),
                PackUri = Path.Combine(installPath, "Cards")
            };
            CardItems = new ObservableCollection<IdeListBoxItemBase>();
            CardItems.CollectionChanged += (a, b) =>
            {
                _set.Cards = CardItems.Select(x => (x as SetCardItemViewModel)._card).ToList();
            };
            foreach (SetCardItemViewModel card in s.CardItems)
            {
                CardItems.Add(new SetCardItemViewModel(card) {ItemSource = CardItems, Parent = this });
            }
       //     _set.Cards = CardItems.Select(x => (x as SetCardItemViewModel)._card).ToList();
            RaisePropertyChanged("CardItems");

            PackageItems = new ObservableCollection<IdeListBoxItemBase>();
            PackageItems.CollectionChanged += (a, b) =>
            {
                _set.Packs = PackageItems.Select(x => (x as SetPackageItemViewModel)._pack).ToList();
            };
            foreach (SetPackageItemViewModel package in s.PackageItems)
            {
                PackageItems.Add(new SetPackageItemViewModel(package) {ItemSource = PackageItems, Parent = this });
            }
            //  _set.Packs = PackageItems.Select(x => (x as SetPackageItemViewModel)._pack).ToList();
            RaisePropertyChanged("PackItems");

            AddCardCommand = new RelayCommand(AddCard);
            AddPackageCommand = new RelayCommand(AddPackage);

            ItemSource = s.ItemSource;
            Parent = s.Parent;

            CanDragDrop = false;
        }

        private SetPackageItemViewModel _selectedPackage;

        public SetPackageItemViewModel SelectedPackage
        {
            get
            {
                return _selectedPackage;
            }
            set
            {
                if (_selectedPackage == value) return;
                _selectedPackage = value;
                RaisePropertyChanged("SelectedPackage");
            }
        }

        private SetCardItemViewModel _selectedCard;

        public SetCardItemViewModel SelectedCard
        {
            get
            {
                return _selectedCard;
            }
            set
            {
                if (_selectedCard == value) return;
                _selectedCard = value;
                if (value != null)
                    _selectedCard.SelectedItem = _selectedCard.BaseCardAlt;
                RaisePropertyChanged("SelectedCard");
            }
        }

        public void AddPackage()
        {
            var ret = new SetPackageItemViewModel() {ItemSource = PackageItems, Parent = this };
            PackageItems.Add(ret);
            SelectedPackage = ret;
            RaisePropertyChanged("SelectedPackage");
        }

        public void AddCard()
        {
            var ret = new SetCardItemViewModel() {ItemSource = CardItems, Parent = this };
            CardItems.Add(ret);
            SelectedCard = ret;
            RaisePropertyChanged("SelectedCard");
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

        public string Code
        {
            get
            {
                return _set.ShortName;
            }
            set
            {
                if (value == _set.ShortName) return;
                _set.ShortName = value;
                RaisePropertyChanged("Code");
            }
        }
        public string Description
        {
            get
            {
                return _set.Description;
            }
            set
            {
                if (value == _set.Description) return;
                _set.Description = value;
                RaisePropertyChanged("Description");
            }
        }
        public DateTime ReleaseDate
        {
            get
            {
                return _set.ReleaseDate;
            }
            set
            {
                if (value == _set.ReleaseDate) return;
                _set.ReleaseDate = value;
                RaisePropertyChanged("ReleaseDate");
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

        public override object Clone()
        {
            return new SetItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as SetItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new SetItemViewModel() { Parent = Parent, ItemSource = ItemSource });
        }
    }
}
