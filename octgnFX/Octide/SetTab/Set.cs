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
using Octide.SetTab.CardItemModel;
using Octide.SetTab.PackageItemModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Octide.Messages;
using Octide.Views;

namespace Octide.SetTab
{
    public class SetModel : IdeListBoxItemBase
    {
        public Set _set;

        public new ObservableCollection<SetModel> ItemSource { get; set; }

        public RelayCommand AddCardCommand { get; private set; }
        public RelayCommand AddPackageCommand { get; private set; }
        public RelayCommand ImportCSVCommand { get; private set; }

        public ObservableCollection<CardModel> CardItems { get; private set; }
        public ObservableCollection<PackageModel> PackageItems { get; private set; }

        public SetModel() //for creating a new set
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

            CardItems = new ObservableCollection<CardModel>();
            CardItems.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };

            PackageItems = new ObservableCollection<PackageModel>();
            PackageItems.CollectionChanged += (a, b) =>
            {
                BuildPackageDef(b);
            };


            AddCardCommand = new RelayCommand(AddCard);
            AddPackageCommand = new RelayCommand(AddPackage);
            ImportCSVCommand = new RelayCommand(ImportCSV);

            CanDragDrop = false;
        }

        public SetModel(Set s) // For loading existing set data
        {
            _set = s;
            CardItems = new ObservableCollection<CardModel>();
            foreach (var card in _set.Cards)
            {
                CardItems.Add(new CardModel(card) {ItemSource = CardItems, Parent = this });
            }
            CardItems.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };

            PackageItems = new ObservableCollection<PackageModel>();
            foreach (var package in _set.Packs)
            {
                PackageItems.Add(new PackageModel(package) {ItemSource = PackageItems, Parent = this });
            }
            PackageItems.CollectionChanged += (a, b) =>
            {
                BuildPackageDef(b);
            };

            AddCardCommand = new RelayCommand(AddCard);
            AddPackageCommand = new RelayCommand(AddPackage);
            ImportCSVCommand = new RelayCommand(ImportCSV);

            CanDragDrop = false;
        }

        public SetModel(SetModel s) //for copying the item
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
            CardItems = new ObservableCollection<CardModel>();
            CardItems.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };
            foreach (CardModel card in s.CardItems)
            {
                CardItems.Add(new CardModel(card) );
            }

            PackageItems = new ObservableCollection<PackageModel>();
            PackageItems.CollectionChanged += (a, b) =>
            {
                BuildPackageDef(b);
            };
            foreach (PackageModel package in s.PackageItems)
            {
                PackageItems.Add(new PackageModel(package));
            }

            AddCardCommand = new RelayCommand(AddCard);
            AddPackageCommand = new RelayCommand(AddPackage);
            ImportCSVCommand = new RelayCommand(ImportCSV);

            CanDragDrop = false;
        }

        public void AddPackage()
        {
            var ret = new PackageModel();
            PackageItems.Add(ret);
            SelectedPackage = ret;
        }

        public void AddCard()
        {
            var ret = new CardModel();
            CardItems.Add(ret);
            SelectedCard = ret;
        }

        public void ImportCSV()
        {
            var dlg = new ImportCardsWindow();
            dlg.DataContext = new ImportCardsViewModel() { Parent = this };
            dlg.ShowDialog();

        }

        public void BuildCardDef(NotifyCollectionChangedEventArgs args)
        {
            if (args?.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CardModel x in args.NewItems)
                {
                    x.ItemSource = CardItems;
                    x.Parent = this;
                }
            }
            _set.Cards = CardItems.Select(x => x._card).ToList();
        }

        public void BuildPackageDef(NotifyCollectionChangedEventArgs args)
        {
            if (args?.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PackageModel x in args.NewItems)
                {
                    x.ItemSource = PackageItems;
                    x.Parent = this;
                    x.Set = _set;
                }
            }
            _set.Packs = PackageItems.Select(x => x._pack).ToList();
        }

        public override object Clone()
        {
            return new SetModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as SetModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new SetModel());
        }

        public override void Remove()
        {
            if (CanRemove == false) return;
            ItemSource.Remove(this);
        }

        private PackageModel _selectedPackage;

        public PackageModel SelectedPackage
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

        private CardModel _selectedCard;

        public CardModel SelectedCard
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

    }
}
