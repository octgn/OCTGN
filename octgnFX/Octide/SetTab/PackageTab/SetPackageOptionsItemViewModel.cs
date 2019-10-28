// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.ItemModel
{
    public partial class PackOptionsItemModel : ViewModelBase, IBasePack, ICloneable
    {
        public ObservableCollection<IBasePack> ParentCollection { get; set; }
        public IPackItem PackItem { get; set; }
        public ObservableCollection<PackOptionItemModel> _items;
        public RelayCommand AddOptionCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }

        public PackOptionsItemModel() // new item
        {
            PackItem = new OptionsList();

            Items = new ObservableCollection<PackOptionItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                (PackItem as OptionsList).Options = Items.Select(x => x.Option).ToList();
            };
            AddOptionCommand = new RelayCommand(AddOption);
            RemoveCommand = new RelayCommand(Remove);
        }

        public PackOptionsItemModel(IPackItem p) // load item
        {
            PackItem = p;
            Items = new ObservableCollection<PackOptionItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                (PackItem as OptionsList).Options = Items.Select(x => x.Option).ToList();
            };
            foreach (var item in (PackItem as OptionsList).Options)
            {
                Items.Add(new PackOptionItemModel(item) { ParentCollection = Items });
            }
            AddOptionCommand = new RelayCommand(AddOption);
            RemoveCommand = new RelayCommand(Remove);

        }

        public PackOptionsItemModel(PackOptionsItemModel p) // copy item
        {
            PackItem = new OptionsList();
            Items = new ObservableCollection<PackOptionItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                (PackItem as OptionsList).Options = Items.Select(x => x.Option).ToList();
            };
            foreach (var item in p.Items)
            {
                Items.Add(new PackOptionItemModel(item) { ParentCollection = Items });
            }
            AddOptionCommand = new RelayCommand(AddOption);
            RemoveCommand = new RelayCommand(Remove);
        }

        public void AddOption()
        {
            Items.Add(new PackOptionItemModel() { ParentCollection = Items });
        }

        public void Remove()
        {
            ParentCollection.Remove(this);
        }

        public object Clone()
        {
            return new PackOptionsItemModel(this) { ParentCollection = ParentCollection };
        }

        public ObservableCollection<PackOptionItemModel> Items
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
