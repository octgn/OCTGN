// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ItemModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Octide.SetTab.PackageItemModel
{
    public partial class PickModel : IBasePack
    {
        public new Pick _packItem;
        public ObservableCollection<PropertyModel> _items;
        public bool _isUnlimited;
        public RelayCommand AddPropertyCommand { get; private set; }


        public PickModel() // new item
        {
            _packItem = new Pick()
            {
                Properties = new List<PickProperty>()
            };
            Items = new ObservableCollection<PropertyModel>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildPickDef(b);
            };
            Items.Add(new PropertyModel());
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);
        }

        public PickModel(Pick p) // load item
        {
            _packItem = p;
            IsUnlimited = (p as Pick).Quantity == -1 ? true : false;
            Items = new ObservableCollection<PropertyModel>();
            foreach (var item in _packItem.Properties)
            {
                Items.Add(new PropertyModel(item) { ItemSource = Items });
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildPickDef(b);
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);

        }

        public PickModel(PickModel p) // copy item
        {
            _packItem = new Pick();
            Quantity = p.Quantity;
            Items = new ObservableCollection<PropertyModel>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildPickDef(b);
            };
            foreach (var item in p.Items)
            {
                Items.Add(new PropertyModel(item) );
            }
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);
        }

        public void BuildPickDef(NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PropertyModel x in args.NewItems)
                {
                    x.ItemSource = Items;
                    x.Parent = this;
                }
            }
            _packItem.Properties = Items.Select(x => x.PropertyDef).ToList();
        }

        public void AddProperty()
        {
            Items.Add(new PropertyModel() );
        }

        public override object Clone()
        {
            return new PickModel(this);
        }
        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as PickModel);
        }
        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new PickModel());
        }

        public override void Remove()
        {
            if (!CanRemove) return;
            ItemSource.Remove(this);
        }

        public bool IsUnlimited
        {
            get
            {
                return _isUnlimited;
            }
            set
            {
                if (_isUnlimited == value) return;
                _isUnlimited = value;
                _packItem.Quantity = (value == true) ? -1 : 0;
                RaisePropertyChanged("IsUnlimited");
                RaisePropertyChanged("Quantity");
            }

        }

        public int Quantity
        {
            get
            {
                return _packItem.Quantity;
            }
            set
            {
                if (_packItem.Quantity == value) return;
                if (value < 0)
                {
                    IsUnlimited = true;
                }
                else
                {
                    IsUnlimited = false;
                    _packItem.Quantity = value;
                }
                RaisePropertyChanged("Quantity");
            }
        }

        public ObservableCollection<PropertyModel> Items
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
