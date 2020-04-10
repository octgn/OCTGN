// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using Octide.ViewModel;
using Octide.ItemModel;

namespace Octide.SetTab.ItemModel
{
    public class CardPropertyModel : ViewModelBase
    {
        public AlternateModel Parent { get; set; }
        public PropertyItemModel Property { get; set; }
        public object _value;
        public bool _isDefined;

        public bool IsValid => ViewModelLocator.PropertyTabViewModel.Items.Contains(Property);

        public CardPropertyModel()
        { 
        }

        public bool IsDefined
        {
            get
            {
                return _isDefined;
            }
            set
            {
                if (_isDefined == value) return;
                _isDefined = value;
                if (value)
                {
                    Parent._altDef.Properties[Property._property] = _value;
                }
                else
                {
                    Parent._altDef.Properties.Remove(Property._property);
                }
                RaisePropertyChanged("IsDefined");
            }
        }

        public string Name => Property.Name;

        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value == value) return;
                _value = value;

                Parent._altDef.Properties[Property._property] = _value;

               // Parent.UpdateAltPropertySet();
                Parent.UpdateProxyTemplate();
                RaisePropertyChanged("Value");
            }
        }
    }
}
