// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Octide.SetTab.ItemModel
{
    public partial class PickModel : IdeBaseItem, IDroppable
    {
        public IdeCollection<IdeBaseItem> Items { get; private set; }
        public bool _isUnlimited;
        public Pick _pick;
        public RelayCommand AddPropertyCommand { get; private set; }


        public PickModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            _pick = new Pick()
            {
                Properties = new List<PickProperty>()
            };
            Items = new IdeCollection<IdeBaseItem>(this, typeof(PackagePropertyModel));
            Items.CollectionChanged += (a, b) =>
            {
                BuildPickDef(b);
            };
            Items.Add(new PackagePropertyModel(Items));
            AddPropertyCommand = new RelayCommand(AddProperty);
        }

        public PickModel(Pick p, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            _pick = p;
            IsUnlimited = p.Quantity == -1;
            Items = new IdeCollection<IdeBaseItem>(this, typeof(PackagePropertyModel));
            foreach (PickProperty item in p.Properties)
            {
                Items.Add(new PackagePropertyModel(item, Items));
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildPickDef(b);
            };
            AddPropertyCommand = new RelayCommand(AddProperty);

        }

        public PickModel(PickModel p, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            _pick = new Pick();
            Quantity = p.Quantity;
            Items = new IdeCollection<IdeBaseItem>(this, typeof(PackagePropertyModel));
            Items.CollectionChanged += (a, b) =>
            {
                BuildPickDef(b);
            };
            foreach (PackagePropertyModel item in p.Items)
            {
                Items.Add(new PackagePropertyModel(item, Items));
            }
            AddPropertyCommand = new RelayCommand(AddProperty);
        }

        public void BuildPickDef(NotifyCollectionChangedEventArgs args)
        {
            _pick.Properties = Items.Select(x => ((PackagePropertyModel)x).Def).ToList();
        }

        public void AddProperty()
        {
            Items.Add(new PackagePropertyModel(Items));
        }

        public override object Clone()
        {
            return new PickModel(this, Source);
        }
        public override object Create()
        {
            return new PickModel(Source);
        }

        public bool CanAccept(object item)
        {
            if (item is PackagePropertyModel)
            {
                return true;
            }
            return false;
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
                _pick.Quantity = (value == true) ? -1 : 1;
                RaisePropertyChanged("IsUnlimited");
                RaisePropertyChanged("Quantity");
            }
        }

        public int Quantity
        {
            get
            {
                return _pick.Quantity;
            }
            set
            {
                if (_pick.Quantity == value) return;
                if (value < 0)
                {
                    IsUnlimited = true;
                }
                else
                {
                    IsUnlimited = false;
                    _pick.Quantity = value;
                }
                RaisePropertyChanged("Quantity");
            }
        }
    }
}
