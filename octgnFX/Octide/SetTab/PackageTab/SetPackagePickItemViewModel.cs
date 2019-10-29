// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.ItemModel
{
    public partial class PackPickItemModel : ViewModelBase, IBasePack, ICloneable
    {
        public ObservableCollection<IBasePack> ParentCollection { get; set; }
        public IPackItem PackItem { get; set; }
        public ObservableCollection<PackPropertyItemModel> _items;
        public bool _isUnlimited;
        public RelayCommand AddPropertyCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }


        public PackPickItemModel() // new item
        {
            PackItem = new Pick();
            (PackItem as Pick).Properties = new List<PickProperty>();
            Items = new ObservableCollection<PackPropertyItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                (PackItem as Pick).Properties = Items.Select(x => x.PropertyDef).ToList();
            };
            Items.Add(new PackPropertyItemModel() { ParentCollection = Items });
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);
        }

        public PackPickItemModel(IPackItem p) // load item
        {
            PackItem = p;
            IsUnlimited = (p as Pick).Quantity == -1 ? true : false;
            Items = new ObservableCollection<PackPropertyItemModel>();
            foreach (var item in (PackItem as Pick).Properties)
            {
                Items.Add(new PackPropertyItemModel(item) { ParentCollection = Items });
            }
            Items.CollectionChanged += (a, b) =>
            {
                (PackItem as Pick).Properties = Items.Select(x => x.PropertyDef).ToList();
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);

        }

        public PackPickItemModel(PackPickItemModel p) // copy item
        {
            PackItem = new Pick();
            Quantity = p.Quantity;
            Items = new ObservableCollection<PackPropertyItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                (PackItem as Pick).Properties = Items.Select(x => x.PropertyDef).ToList();
            };
            foreach (var item in p.Items)
            {
                Items.Add(new PackPropertyItemModel(item) { ParentCollection = Items });
            }
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);
        }

        public void AddProperty()
        {
            Items.Add(new PackPropertyItemModel() { ParentCollection = Items });
        }

        public object Clone()
        {
            return new PackPickItemModel(this) { ParentCollection = ParentCollection };
        }

        public void Remove()
        {
            ParentCollection.Remove(this);
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
                (PackItem as Pick).Quantity = (value == true) ? -1 : 0;
                RaisePropertyChanged("IsUnlimited");
                RaisePropertyChanged("Quantity");
            }

        }

        public int Quantity
        {
            get
            {
                return (PackItem as Pick).Quantity;
            }
            set
            {
                if ((PackItem as Pick).Quantity == value) return;
                if (value < 0)
                {
                    IsUnlimited = true;
                }
                else
                {
                    IsUnlimited = false;
                    (PackItem as Pick).Quantity = value;
                }
                RaisePropertyChanged("Quantity");
            }
        }

        public ObservableCollection<PackPropertyItemModel> Items
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
