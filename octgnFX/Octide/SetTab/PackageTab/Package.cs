// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Octide.SetTab.ItemModel
{
    public class PackageModel : IdeBaseItem
    {
        public Pack _pack;
        public IdeCollection<IdeBaseItem> Items { get; private set; }
        public IdeCollection<IdeBaseItem> Includes { get; private set; }

        public RelayCommand AddOptionsCommand { get; private set; }
        public RelayCommand AddPickCommand { get; private set; }
        public RelayCommand GeneratePackCommand { get; private set; }
        public RelayCommand AddIncludeCommand { get; private set; }
        public PackageDropHandler PackageDropHandler { get; set; }
        public IncludeDropHandler IncludeDropHandler { get; set; }

        public ObservableCollection<string> _boosterCards;

        public PackageModel(IdeCollection<IdeBaseItem> src) : base(src) //new item
        {
            _pack = new Pack
            {
                Name = "Package",
                Id = Guid.NewGuid(),
                Items = new List<object>(),
                Set = ((SetModel)Source.Parent)._set
            };
            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                BuildPackDef(b);
            };
            Includes = new IdeCollection<IdeBaseItem>(this);
            Includes.CollectionChanged += (a, b) =>
            {
                BuildIncludesDef(b);
            };
            PackageDropHandler = new PackageDropHandler();
            IncludeDropHandler = new IncludeDropHandler();
            GeneratePackCommand = new RelayCommand(GeneratePack);
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
            AddIncludeCommand = new RelayCommand(AddInclude);
        }

        public PackageModel(Pack p, IdeCollection<IdeBaseItem> src) : base(src) //loading item
        {
            _pack = p;
            Items = new IdeCollection<IdeBaseItem>(this);
            foreach (var item in p.Items)
            {
                if (item is OptionsList options)
                    Items.Add(new OptionsModel(options, Items));
                else if (item is Pick pick)
                    Items.Add(new PickModel(pick, Items) );
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildPackDef(b);
            };

            Includes = new IdeCollection<IdeBaseItem>(this);
            foreach (var include in p.Includes)
            {
                Includes.Add(new IncludeModel(include, Includes));
            }       
            Includes.CollectionChanged += (a, b) =>
            {
                BuildIncludesDef(b);
            };

            PackageDropHandler = new PackageDropHandler();
            IncludeDropHandler = new IncludeDropHandler();
            GeneratePackCommand = new RelayCommand(GeneratePack);
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
            AddIncludeCommand = new RelayCommand(AddInclude);
        }

        public PackageModel(PackageModel p, IdeCollection<IdeBaseItem> src) : base(src) //copying item
        {
            _pack = new Pack
            {
                Name = p.Name,
                Id = Guid.NewGuid(),
                Set = p._pack.Set,
                Items = new List<object>(),
                Includes = new List<Include>()
            };
            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                BuildPackDef(b);
            };
            foreach (var packItem in p.Items)
            {
                if (packItem is PickModel pick)
                {
                    Items.Add(new PickModel(pick, Items) );
                }
                else if (packItem is OptionsModel options)
                {
                    Items.Add(new OptionsModel(options, Items) );
                }
            }
            Includes = new IdeCollection<IdeBaseItem>(this);
            Includes.CollectionChanged += (a, b) =>
            {
                BuildIncludesDef(b);
            };
            foreach (IncludeModel include in p.Includes)
            {
                Includes.Add(new IncludeModel(include, Includes));
            }
            PackageDropHandler = new PackageDropHandler();
            IncludeDropHandler = new IncludeDropHandler();
            GeneratePackCommand = new RelayCommand(GeneratePack);
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
            AddIncludeCommand = new RelayCommand(AddInclude);
        }

        public void BuildPackDef(NotifyCollectionChangedEventArgs args)
        {
            List<object> items = new List<object>();
            foreach (var packItem in Items)
            {
                if (packItem is PickModel pick)
                {
                    items.Add(pick._pick);
                }
                else if (packItem is OptionsModel options)
                {
                    items.Add(options._options);
                }
            }
            _pack.Items = items;
        }
        
        public void BuildIncludesDef(NotifyCollectionChangedEventArgs args)
        {
            _pack.Includes = Includes.Select(x => ((IncludeModel)x)._include).ToList();
        }

        public override object Clone()
        {
            return new PackageModel(this, Source);
        }
        public override object Create()
        {
            return new PackageModel(Source);
        }

        public void AddInclude()
        {
            Includes.Add(new IncludeModel(Includes));
        }
        public void AddPick()
        {
            Items.Add(new PickModel(Items));
        }

        public void AddOptions()
        {
            Items.Add(new OptionsModel(Items));
        }

        public void GeneratePack()
        {
            BoosterCards = new ObservableCollection<string>(_pack.GenerateContent().LimitedCards.Select(x => x.GetPicture()));
        }

        public ObservableCollection<string> BoosterCards
        {
            get
            {
                return _boosterCards;
            }
            set
            {
                if (_boosterCards == value) return;
                _boosterCards = value;
                RaisePropertyChanged("BoosterCards");
            }
        }

        public string Name
        {
            get
            {
                return _pack.Name;
            }
            set
            {
                if (_pack.Name == value) return;
                _pack.Name = value;
                RaisePropertyChanged("Name");
            }
        }
    }
}
