// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.Specialized;
using Octide.Views;

namespace Octide.SetTab.ItemModel
{
    public class SetModel : IdeBaseItem
    {
        public Set _set;

        public RelayCommand AddCardCommand { get; private set; }
        public RelayCommand AddPackageCommand { get; private set; }
        public RelayCommand ImportCSVCommand { get; private set; }

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
                Packs = new List<Pack>()
            };
            Name = "New Set";
            string installPath = Path.Combine(ViewModelLocator.GameLoader.GamePath, "Sets", _set.Id.ToString());
            _set.InstallPath = installPath;
            _set.Filename = Path.Combine(installPath, "set.xml");
            _set.PackUri = Path.Combine(installPath, "Cards");

            CardItems = new IdeCollection<IdeBaseItem>(this);
            CardItems.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };
            CardItems.SelectedItemChanged += (a, b) =>
            {
                SelectDefaultAlt(b);
            };

            PackageItems = new IdeCollection<IdeBaseItem>(this);
            PackageItems.CollectionChanged += (a, b) =>
            {
                BuildPackageDef(b);
            };


            AddCardCommand = new RelayCommand(AddCard);
            AddPackageCommand = new RelayCommand(AddPackage);
            ImportCSVCommand = new RelayCommand(ImportCSV);

            CanDragDrop = false;
        }

        public SetModel(Set s, IdeCollection<IdeBaseItem> src) : base(src) // For loading existing set data
        {
            _set = s;
            CardItems = new IdeCollection<IdeBaseItem>(this);
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

            PackageItems = new IdeCollection<IdeBaseItem>(this);
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
            ImportCSVCommand = new RelayCommand(ImportCSV);

            CanDragDrop = false;
        }

        public SetModel(SetModel s, IdeCollection<IdeBaseItem> src) : base(src) //for copying the item
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
            CardItems = new IdeCollection<IdeBaseItem>(this);
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
                CardItems.Add(new CardModel(card, CardItems) );
            }

            PackageItems = new IdeCollection<IdeBaseItem>(this);
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
            ImportCSVCommand = new RelayCommand(ImportCSV);

            CanDragDrop = false;
        }

        public void AddPackage()
        {
            var ret = new PackageModel(PackageItems);
            PackageItems.Add(ret);
            PackageItems.SelectedItem = ret;
        }

        public void AddCard()
        {
            var ret = new CardModel(CardItems);
            CardItems.Add(ret);
            CardItems.SelectedItem = ret;
        }

        public void ImportCSV()
        {
            var dlg = new ImportCardsWindow();
            dlg.DataContext = new ImportCardsViewModel() { Parent = this };
            dlg.ShowDialog();

        }

        public void SelectDefaultAlt(NotifySelectedItemChangedEventArgs args)
        {
            if (args.NewItem is CardModel selectedCard)
                selectedCard.Items.SelectedItem = selectedCard.Items.DefaultItem;
        }

        public void BuildCardDef(NotifyCollectionChangedEventArgs args)
        {
            _set.Cards = CardItems.Select(x => ((CardModel)x)._card);
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
