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
    public partial class OptionsModel : IBasePack
    {
        public new OptionsList _packItem;
        public ObservableCollection<OptionModel> _items;
        public RelayCommand AddOptionCommand { get; private set; }

        public OptionsModel() // new item
        {
            _packItem = new OptionsList();

            Items = new ObservableCollection<OptionModel>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildOptionsDef(b);
            };
            Items.Add(new OptionModel() );
            AddOptionCommand = new RelayCommand(AddOption);
            RemoveCommand = new RelayCommand(Remove);
        }

        public OptionsModel(OptionsList p) // load item
        {
            _packItem = p;
            Items = new ObservableCollection<OptionModel>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildOptionsDef(b);
            };
            foreach (var item in _packItem.Options)
            {
                Items.Add(new OptionModel(item) { ItemSource = Items });
            }
            AddOptionCommand = new RelayCommand(AddOption);
            RemoveCommand = new RelayCommand(Remove);

        }

        public OptionsModel(OptionsModel p) // copy item
        {
            _packItem = new OptionsList();
            Items = new ObservableCollection<OptionModel>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildOptionsDef(b);
            };
            foreach (var item in p.Items)
            {
                Items.Add(new OptionModel(item) );
            }
            AddOptionCommand = new RelayCommand(AddOption);
            RemoveCommand = new RelayCommand(Remove);
        }

        public void BuildOptionsDef(NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (OptionModel x in args.NewItems)
                {
                    x.ItemSource = Items;
                    x.Parent = this;
                }
            }
            _packItem.Options = Items.Select(x => x.Option).ToList();
        }

        public void AddOption()
        {
            Items.Add(new OptionModel() );
        }

        public override object Clone()
        {
            return new OptionsModel(this);
        }
        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as OptionsModel);
        }
        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new OptionsModel());
        }

        public override void Remove()
        {
            if (!CanRemove) return;
            ItemSource.Remove(this);
        }

        public ObservableCollection<OptionModel> Items
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
