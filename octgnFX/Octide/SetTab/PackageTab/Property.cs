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

namespace Octide.SetTab.ItemModel
{
    public class PackagePropertyModel : IdeBaseItem
    {
        public bool isIncludeProperty { get; set; }
        public PickProperty PropertyDef { get; set; }
        public PropertyItemModel _activeProperty;

        public IdeCollection<IdeBaseItem> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;

        public PackagePropertyModel(IdeCollection<IdeBaseItem> src) : base(src) // new item
        {
            PropertyDef = new PickProperty();
            ActiveProperty = (PropertyItemModel)CustomProperties.First();
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public PackagePropertyModel(PickProperty p, IdeCollection<IdeBaseItem> src) : base(src) // loading item
        {
            PropertyDef = p;
            ActiveProperty = (PropertyItemModel)CustomProperties.FirstOrDefault(x => ((PropertyItemModel)x)._property == PropertyDef.Property);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
        }

        public PackagePropertyModel(PackagePropertyModel p, IdeCollection<IdeBaseItem> src) : base(src) // copy item
        {
            PropertyDef = new PickProperty();
            ActiveProperty = p.ActiveProperty;
            PropertyDef.Value = p.PropertyDef.Value;
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
            return new PackagePropertyModel(this, Source);
        }
        public override object Create()
        {
            return new PackagePropertyModel(Source);
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
