// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System.Linq;
using Octide.Messages;
using System.Collections.Generic;

namespace Octide.ItemModel
{
    public class CardPropertyItemModel : ViewModelBase
    {
        public SetCardAltItemViewModel Parent { get; set; }
        public PropertyItemViewModel Property { get; set; }
        public object _value;
        public bool _isUndefined;

        public bool IsValid => ViewModelLocator.PropertyTabViewModel.Items.Contains(Property);

        public CardPropertyItemModel()
        { 
        }

        public bool IsUndefined
        {
            get
            {
                return _isUndefined;
            }
            set
            {
                if (_isUndefined == value) return;
                _isUndefined = value;
                if (value == true)
                {
                    Parent._altDef.Properties.Remove(Property._property);
                }
                else
                    Parent._altDef.Properties[Property._property] = _value;
                RaisePropertyChanged("IsUndefined");
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

    public class CardNamePropertyItemModel : CardPropertyItemModel
    {
        public CardNamePropertyItemModel()
        {
            Property = ViewModelLocator.PropertyTabViewModel.NameProperty;
        }
    
        new public object Value  // this is the alt's actual card name
        {
            get
            {
                return Parent._altDef.Name;
            }
            set
            {
                if (Parent._altDef.Name == value.ToString()) return;
                Parent._altDef.Name = value.ToString();
                Parent.Parent.UpdateCardName();
                RaisePropertyChanged("Value");
            }
        }
    }

    public class CardSizePropertyItemModel : CardPropertyItemModel
    {
        public CardSizePropertyItemModel()
        {
            Property = ViewModelLocator.PropertyTabViewModel.SizeProperty;
        }

        new public SizeItemViewModel _value;
        new public SizeItemViewModel Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value == value) return;
                _value = value ?? ViewModelLocator.PreviewTabViewModel.DefaultSize;
                Parent._altDef.Size = _value?._size;
                RaisePropertyChanged("Value");
            }
        }
    }
}
