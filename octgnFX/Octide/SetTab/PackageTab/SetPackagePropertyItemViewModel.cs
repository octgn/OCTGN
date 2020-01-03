// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.Messages;
using Octide.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.ItemModel
{

    public class PackPropertyItemModel : ViewModelBase, ICloneable
    {
        public bool isIncludeProperty { get; set; }
        public ObservableCollection<PackPropertyItemModel> ParentCollection { get; set; }
        public PickProperty PropertyDef { get; set; }
        public PropertyItemViewModel _activeProperty;
        public RelayCommand RemoveCommand { get; private set; }

        public ObservableCollection<PropertyItemViewModel> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;

        public PackPropertyItemModel() // new item
        {
            PropertyDef = new PickProperty();
            ActiveProperty = CustomProperties.First();
            RemoveCommand = new RelayCommand(Remove);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public PackPropertyItemModel(PickProperty p) // loading item
        {
            PropertyDef = p;
            ActiveProperty = CustomProperties.FirstOrDefault(x => x._property == PropertyDef.Property);
            RemoveCommand = new RelayCommand(Remove);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public PackPropertyItemModel(PackPropertyItemModel p) // copy item
        {
            PropertyDef = new PickProperty();
            ActiveProperty = p.ActiveProperty;
            PropertyDef.Value = p.PropertyDef.Value;
            RemoveCommand = new RelayCommand(Remove);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public object Clone()
        {
            return new PackPropertyItemModel(this) { ParentCollection = ParentCollection };
        }

        public void CustomPropertyChanged(CustomPropertyChangedMessage args)
        {
            if (ActiveProperty == args.Prop)
            {
                PropertyDef.Property = args.Prop._property;
                RaisePropertyChanged("ActiveProperty");
            }
        }

        public void Remove()
        {
            if (isIncludeProperty || ParentCollection.Count > 1)
                ParentCollection.Remove(this);
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
