// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.ItemModel;
using Octide.Messages;
using Octide.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.SetTab.PackageItemModel
{
    public class PropertyModel : IdeListBoxItemBase
    {
        public bool isIncludeProperty { get; set; }
        public new ObservableCollection<PropertyModel> ItemSource { get; set; }
        public PickProperty PropertyDef { get; set; }
        public PropertyItemViewModel _activeProperty;

        public ObservableCollection<PropertyItemViewModel> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;

        public PropertyModel() // new item
        {
            PropertyDef = new PickProperty();
            ActiveProperty = CustomProperties.First();
            RemoveCommand = new RelayCommand(Remove);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public PropertyModel(PickProperty p) // loading item
        {
            PropertyDef = p;
            ActiveProperty = CustomProperties.FirstOrDefault(x => x._property == PropertyDef.Property);
            RemoveCommand = new RelayCommand(Remove);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public PropertyModel(PropertyModel p) // copy item
        {
            PropertyDef = new PickProperty();
            ActiveProperty = p.ActiveProperty;
            PropertyDef.Value = p.PropertyDef.Value;
            RemoveCommand = new RelayCommand(Remove);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public void CustomPropertyChanged(CustomPropertyChangedMessage args)
        {
            if (ActiveProperty == args.Prop)
            {
                PropertyDef.Property = args.Prop._property;
                RaisePropertyChanged("ActiveProperty");
            }
        }

        public override object Clone()
        {
            return new PropertyModel(this);
        }
        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as PropertyModel);
        }
        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new PropertyModel());
        }

        public override void Remove()
        {
            if (!CanRemove) return;
            if (isIncludeProperty || ItemSource.Count > 1)
                ItemSource.Remove(this);
        }
        public PropertyItemViewModel ActiveProperty
        {
            get
            {
                return _activeProperty;
            }
            set
            {
                if (_activeProperty == value) return;
                if (value == null)
                {
                    value = CustomProperties.First();
                }
                _activeProperty = value;
                PropertyDef.Property = value._property;
                RaisePropertyChanged("ActiveProperty");
            }
        }

        public object Value
        {
            get
            {
                return PropertyDef.Value;
            }
            set
            {
                if (PropertyDef.Value == value) return;
                PropertyDef.Value = value;
                RaisePropertyChanged("Value");
            }
        }
    }
}
