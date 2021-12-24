// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.ItemModel;
using Octide.Messages;
using Octide.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.SetTab.ItemModel
{
    public class PackagePropertyModel : IdeBaseItem, IDroppable
    {
        public bool IsIncludeProperty { get; set; }
        public PickProperty Def { get; set; }
        public PropertyItemModel _activeProperty;

        public PackagePropertyModel(IdeCollection<IdeBaseItem> src) : base(src) // new item
        {
            Def = new PickProperty();
            ActiveProperty = PropertyTabViewModel.NameProperty;
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, CustomPropertyChanged);
        }

        public PackagePropertyModel(PickProperty p, IdeCollection<IdeBaseItem> src) : base(src) // loading item
        {
            if (p is NamePickProperty)
            {
                Def = new PickProperty
                {
                    Property = PropertyTabViewModel.NameProperty.Property,
                    Value = p.Value
                };
                ActiveProperty = PropertyTabViewModel.NameProperty;
            }
            else
            {
                Def = (PickProperty)p;
                ActiveProperty = (PropertyItemModel)ViewModelLocator.PropertyTabViewModel.SetPackageItems.FirstOrDefault(x => ((PropertyItemModel)x).Property == Def.Property);
            }
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, CustomPropertyChanged);
        }

        public PackagePropertyModel(PackagePropertyModel p, IdeCollection<IdeBaseItem> src) : base(src) // copy item
        {
            Def = new PickProperty();
            ActiveProperty = p.ActiveProperty;
            Def.Value = p.Def.Value;
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, CustomPropertyChanged);
        }

        public void CustomPropertyChanged(CustomPropertyChangedMessage args)
        {
            if (ActiveProperty == args.Prop && Def is PickProperty pickprop)
            {
                pickprop.Property = args.Prop.Property;
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
                    value = PropertyTabViewModel.NameProperty;
                }
                _activeProperty = value;
                Def.Property = value.Property;
                RaisePropertyChanged("ActiveProperty");
            }
        }

        public object Value
        {
            get
            {
                return Def.Value;
            }
            set
            {
                if (Def.Value == value) return;
                Def.Value = value;
                RaisePropertyChanged("Value");
            }
        }
    }
}
