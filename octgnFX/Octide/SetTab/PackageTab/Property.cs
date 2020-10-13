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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;

namespace Octide.SetTab.ItemModel
{
    public class PackagePropertyModel : IdeBaseItem, IDroppable
    {
        public bool isIncludeProperty { get; set; }
        public PickProperty _def { get; set; }
        public PropertyItemModel _activeProperty;

        public ObservableCollection<IdeBaseItem> CustomProperties
        {
            get
            {
                var ret = new ObservableCollection<IdeBaseItem>(ViewModelLocator.PropertyTabViewModel.Items);
                ret.Insert(0, PropertyTabViewModel.NameProperty);
                return ret;
            }
        }

        public PackagePropertyModel(IdeCollection<IdeBaseItem> src) : base(src) // new item
        {
            _def = new PickProperty();
            ActiveProperty = (PropertyItemModel)CustomProperties.First();
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public PackagePropertyModel(PickProperty p, IdeCollection<IdeBaseItem> src) : base(src) // loading item
        {
            if (p is NamePickProperty)
            {
                _def = new PickProperty
                {
                    Property = PropertyTabViewModel.NameProperty._property,
                    Value = p.Value
                };
                ActiveProperty = PropertyTabViewModel.NameProperty;
            }
            else
            {
                _def = (PickProperty)p;
                ActiveProperty = (PropertyItemModel)CustomProperties.FirstOrDefault(x => ((PropertyItemModel)x)._property == _def.Property);
            }
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public PackagePropertyModel(PackagePropertyModel p, IdeCollection<IdeBaseItem> src) : base(src) // copy item
        {
            _def = new PickProperty();
            ActiveProperty = p.ActiveProperty;
            _def.Value = p._def.Value;
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public void CustomPropertyChanged(CustomPropertyChangedMessage args)
        {
            if (ActiveProperty == args.Prop && _def is PickProperty pickprop)
            {
                pickprop.Property = args.Prop._property;
                RaisePropertyChanged("ActiveProperty");
            }
        }

        public override object Clone()
        {
            return new PackagePropertyModel(this, Source);
        }
        public override object Create()
        {
            return new PackagePropertyModel(Source);
        }

        public bool CanAccept(object item)
        {
            return false;
        }

        public PropertyItemModel ActiveProperty
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
                    value = (PropertyItemModel)CustomProperties.First();
                }
                _activeProperty = value;
                _def.Property = value._property;
                RaisePropertyChanged("ActiveProperty");
            }
        }

        public object Value
        {
            get
            {
                return _def.Value;
            }
            set
            {
                if (_def.Value == value) return;
                _def.Value = value;
                RaisePropertyChanged("Value");
            }
        }
    }
}
