// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ItemModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Octide.SetTab.PackageItemModel
{
    public class OptionModel : IdeListBoxItemBase
    {
        public new ObservableCollection<OptionModel> ItemSource { get; set; }
        public Option Option { get; set; }
        public RelayCommand AddPickCommand { get; private set; }
        public RelayCommand AddOptionsCommand { get; private set; }

        public ObservableCollection<IBasePack> _items;

        public OptionModel() // new item
        {
            Option = new Option
            {
                Definition = new PackDefinition()
            };
            Items = new ObservableCollection<IBasePack>();

            Items.CollectionChanged += (a, b) =>
            {
                BuildOptionDef(b);
            };
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
            RemoveCommand = new RelayCommand(Remove);
        }

        public OptionModel(Option o) // load item
        {
            Option = o;
            Items = new ObservableCollection<IBasePack>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildOptionDef(b);
            };
            foreach (var item in o.Definition.Items)
            {
                if (item is OptionsList options)
                    Items.Add(new OptionsModel(options) { ItemSource = Items });
                else if (item is Pick pick)
                    Items.Add(new PickModel(pick) { ItemSource = Items });
            }
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
            RemoveCommand = new RelayCommand(Remove);
        }

        public OptionModel(OptionModel p) // copy item
        {
            Option = new Option
            {
                Definition = new PackDefinition(),
                Probability = p.Probability
            };
            Items = new ObservableCollection<IBasePack>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildOptionDef(b);
            };
            foreach (var packItem in p.Items)
            {
                if (packItem is PickModel)
                    Items.Add(new PickModel(packItem as PickModel) );
                if (packItem is OptionsModel)
                    Items.Add(new OptionsModel(packItem as OptionsModel) );
            }
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
            RemoveCommand = new RelayCommand(Remove);
        }
        public void BuildOptionDef(NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IBasePack x in args.NewItems)
                {
                    x.ItemSource = Items;
                }
            }
            Option.Definition.Items = Items.Select(x => x._packItem).ToList();
        }

        public void AddPick()
        {
            Items.Add(new PickModel() );
        }

        public void AddOptions()
        {
            Items.Add(new OptionsModel());
        }
        public override object Clone()
        {
            return new OptionModel(this);
        }
        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as OptionModel);
        }
        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new OptionModel());
        }

        public override void Remove()
        {
            if (!CanRemove) return;
            ItemSource.Remove(this);
        }

        public double Probability
        {
            get
            {
                return Option.Probability;
            }
            set
            {
                if (Option.Probability == value) return;
                Option.Probability = value;
                RaisePropertyChanged("Probability");
            }
        }

        public ObservableCollection<IBasePack> Items
        {
            get
            {
                return _items;
            }
            set
            {
                if (_items == value) return;
                _items = value;
                RaisePropertyChanged("Items");
            }
        }
    }
}
