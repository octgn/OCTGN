// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octgn.Library;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Octide.SetTab.ItemModel
{
    public class SetModel : IdeBaseItem
    {
        public Set _set;

        public AssetController Asset { get; set; }

        public RelayCommand AddCardCommand { get; private set; }
        public RelayCommand AddPackageCommand { get; private set; }

        public IdeCollection<IdeBaseItem> CardItems { get; private set; }
        public IdeCollection<IdeBaseItem> PackageItems { get; private set; }

        public SetModel(IdeCollection<IdeBaseItem> src) : base(src) //for creating a new set
        {
            _set = new Set
            {
                Id = Guid.NewGuid(),
                GameId = ViewModelLocator.GameLoader.Game.Id,
                Hidden = false,
                Cards = new List<Card>(),
                Packs = new List<Pack>(),
                ReleaseDate = DateTime.Today
            };
            Name = "New Set";

            var setAsset = ViewModelLocator.AssetsTabViewModel.NewAsset(new string[] { "Sets", _set.Id.ToString() }, "set", ".xml");
            setAsset.IsReserved = true;
            setAsset.LockName = true;
            Asset = new AssetController(AssetType.Xml);
            Asset.SelectedAsset = setAsset;
            Asset.PropertyChanged += AssetUpdated;

            _set.Filename = setAsset.FullPath;
            _set.ImagePackUri = Path.Combine(Config.Instance.ImageDirectoryFull, _set.GameId.ToString(), "Sets", _set.Id.ToString());

            CardItems = new IdeCollection<IdeBaseItem>(this, typeof(CardModel));
            CardItems.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };
            CardItems.SelectedItemChanged += (a, b) =>
            {
                SelectDefaultAlt(b);
            };

            PackageItems = new IdeCollection<IdeBaseItem>(this, typeof(PackageModel));
            PackageItems.CollectionChanged += (a, b) =>
            {
                BuildPackageDef(b);
            };


            AddCardCommand = new RelayCommand(AddCard);
            AddPackageCommand = new RelayCommand(AddPackage);

            CanDragDrop = false;
        }

        public SetModel(Set s, IdeCollection<IdeBaseItem> src) : base(src) // For loading existing set data
        {
            _set = s;

            Asset = new AssetController(AssetType.Xml);
            Asset.Register(s.Filename);
            Asset.PropertyChanged += AssetUpdated;
            if (Asset.SelectedAsset != null)
            {
                Asset.SelectedAsset.IsReserved = true;
            };

            CardItems = new IdeCollection<IdeBaseItem>(this, typeof(CardModel));
            foreach (var card in _set.Cards)
            {
                CardItems.Add(new CardModel(card, CardItems));
            }
            CardItems.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };
            CardItems.SelectedItemChanged += (a, b) =>
            {
                SelectDefaultAlt(b);
            };

            PackageItems = new IdeCollection<IdeBaseItem>(this, typeof(PackageModel));
            foreach (var package in _set.Packs)
            {
                PackageItems.Add(new PackageModel(package, PackageItems));
            }
            PackageItems.CollectionChanged += (a, b) =>
            {
                BuildPackageDef(b);
            };

            AddCardCommand = new RelayCommand(AddCard);
            AddPackageCommand = new RelayCommand(AddPackage);

            CanDragDrop = false;
        }

        public SetModel(SetModel s, IdeCollection<IdeBaseItem> src) : base(src) //for copying the item
        {
            _set = new Set()
            {
                Name = s._set.Name,
                Id = Guid.NewGuid(),
                GameId = s._set.GameId,
                Hidden = s._set.Hidden,
                ShortName = s._set.ShortName,
                Description = s._set.Description,
                ReleaseDate = s._set.ReleaseDate
            };

            var setAsset = ViewModelLocator.AssetsTabViewModel.NewAsset(new string[] { "Sets", _set.Id.ToString() }, "set", ".xml");

            _set.Filename = setAsset.FullPath;
            _set.ImagePackUri = Path.Combine(Config.Instance.ImageDirectoryFull, _set.GameId.ToString(), "Sets", _set.Id.ToString());

            setAsset.IsReserved = true;
            setAsset.LockName = true;
            Asset = new AssetController(AssetType.Xml);
            Asset.SelectedAsset = setAsset;
            Asset.PropertyChanged += AssetUpdated;

            CardItems = new IdeCollection<IdeBaseItem>(this, typeof(CardModel));
            CardItems.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };
            CardItems.SelectedItemChanged += (a, b) =>
            {
                SelectDefaultAlt(b);
            };
            foreach (CardModel card in s.CardItems)
            {
                CardItems.Add(new CardModel(card, CardItems));
            }

            PackageItems = new IdeCollection<IdeBaseItem>(this, typeof(PackageModel));
            PackageItems.CollectionChanged += (a, b) =>
            {
                BuildPackageDef(b);
            };
            foreach (PackageModel package in s.PackageItems)
            {
                PackageItems.Add(new PackageModel(package, PackageItems));
            };

            AddCardCommand = new RelayCommand(AddCard);
            AddPackageCommand = new RelayCommand(AddPackage);

            CanDragDrop = false;
        }

        public void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _set.Filename = Asset.FullPath;
            }
        }
        public void AddPackage()
        {
            var ret = new PackageModel(PackageItems);
            PackageItems.Add(ret);
        }

        public void AddCard()
        {
            var ret = new CardModel(CardItems);
            CardItems.Add(ret);
        }

        public void SelectDefaultAlt(NotifySelectedItemChangedEventArgs args)
        {
            if (args.NewItem is CardModel selectedCard)
                selectedCard.Items.SelectedItem = selectedCard.Items.DefaultItem;
        }

        public void BuildCardDef(NotifyCollectionChangedEventArgs args)
        {
            _set.Cards = CardItems.Select(x => ((CardModel)x).Card);
        }

        public void BuildPackageDef(NotifyCollectionChangedEventArgs args)
        {
            _set.Packs = PackageItems.Select(x => ((PackageModel)x)._pack);
        }

        public override object Clone()
        {
            return new SetModel(this, Source);
        }
        public override object Create()
        {
            return new SetModel(Source);
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
