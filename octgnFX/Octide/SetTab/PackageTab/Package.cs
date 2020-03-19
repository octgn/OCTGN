// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew.Entities;
using Octide.Messages;
using Octide.SetTab;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octide.SetTab.PackageItemModel
{
    public class PackageModel : IdeListBoxItemBase
    {
        public new SetModel Parent;

        public Pack _pack;
        public new ObservableCollection<PackageModel> ItemSource { get; set; }
        public ObservableCollection<IBasePack> Items { get; private set; }
        public ObservableCollection<IncludeModel> Includes { get; private set; }

        public RelayCommand AddOptionsCommand { get; private set; }
        public RelayCommand AddPickCommand { get; private set; }
        public RelayCommand GeneratePackCommand { get; private set; }
        public RelayCommand AddIncludeCommand { get; private set; }
        public PackageDropHandler PackageDropHandler { get; set; }
        public IncludeDropHandler IncludeDropHandler { get; set; }

        public ObservableCollection<string> _boosterCards;

        public PackageModel() //new item
        {
            _pack = new Pack
            {
                Name = "Package",
                Id = Guid.NewGuid(),
                Definition = new PackDefinition()
            };
            Items = new ObservableCollection<IBasePack>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildPackDef(b);
            };
            Includes = new ObservableCollection<IncludeModel>();
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

        public PackageModel(Pack p) //loading item
        {
            _pack = p;
            Items = new ObservableCollection<IBasePack>();
            foreach (var item in p.Definition.Items)
            {
                if (item is OptionsList options)
                    Items.Add(new OptionsModel(options) { ItemSource = Items });
                else if (item is Pick pick)
                    Items.Add(new PickModel(pick) { ItemSource = Items });
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildPackDef(b);
            };

            Includes = new ObservableCollection<IncludeModel>();
            foreach (var include in p.Includes)
            {
                Includes.Add(new IncludeModel(include) { Parent = this, ItemSource = Includes });
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

        public PackageModel(PackageModel p) //copying item
        {
            _pack = new Pack
            {
                Name = p.Name,
                Id = Guid.NewGuid(),
                Set = p._pack.Set,
                Definition = new PackDefinition(),
                Includes = new List<Include>()
            };
            Items = new ObservableCollection<IBasePack>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildPackDef(b);
            };
            foreach (var packItem in p.Items)
            {
                if (packItem is PickModel pick)
                {
                    Items.Add(new PickModel(pick) );
                }
                else if (packItem is OptionsModel options)
                {
                    Items.Add(new OptionsModel(options) );
                }
            }
            Includes = new ObservableCollection<IncludeModel>();
            Includes.CollectionChanged += (a, b) =>
            {
                BuildIncludesDef(b);
            };
            foreach (IncludeModel include in p.Includes)
            {
                Includes.Add(new IncludeModel(include));
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
            if (args?.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IBasePack x in args.NewItems)
                {
                    x.ItemSource = Items;
                }
            }
            _pack.Definition.Items = Items.Select(x => x._packItem).ToList();
        }
        
        public void BuildIncludesDef(NotifyCollectionChangedEventArgs args)
        {
            if (args?.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IncludeModel x in args.NewItems)
                {
                    x.ItemSource = Includes;
                }
            }
            _pack.Includes = Includes.Select(x => x._include).ToList();
        }

        public void AddInclude()
        {
            Includes.Add(new IncludeModel(this));
        }
        public void AddPick()
        {
            Items.Add(new PickModel());
        }

        public void AddOptions()
        {
            Items.Add(new OptionsModel());
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

        public Set Set
        {
            get
            {
                return _pack.Set;
            }
            set
            {
                if (_pack.Set == value) return;
                _pack.Set = value;
                RaisePropertyChanged("Set");
            }
        }

        public override object Clone()
        {
            return new PackageModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as PackageModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new PackageModel());
        }
    }
}
