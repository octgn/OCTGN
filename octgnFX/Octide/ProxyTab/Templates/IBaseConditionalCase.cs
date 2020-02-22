// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Messaging;
using Octgn.ProxyGenerator.Definitions;
using Octide.ItemModel;
using Octide.Messages;
using Octide.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.ProxyTab.TemplateItemModel
{
    public abstract class IBaseConditionalCase : IdeListBoxItemBase
    {
        public CaseDefinition _case;
        public PropertyItemViewModel _property;
        public ObservableCollection<PropertyItemViewModel> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;
        public BlockContainer BlockContainer { get; set; }
        public new ObservableCollection<IBaseConditionalCase> ItemSource { get; set; }

        public PropertyItemViewModel Property
        {
            get
            {
                return _property;
            }
            set
            {
                if (_property == value) return;
                if (value == null)
                {
                    value = CustomProperties.First();
                }
                _property = value;
                _case.property = value.Name;
                RaisePropertyChanged("Property");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }
        public void CustomPropertyChanged(CustomPropertyChangedMessage args)
        {
            if (args.Prop == _property)
            {
                _case.property = args.Prop.Name;
                RaisePropertyChanged("Property");
            }
        }

        public bool ExactMatch
        {
            get
            {
                return (_case.value != null);
            }
            set
            {
                if ((_case.value != null) == value) return;
                if (value == true) // becoming exact math
                {
                    _case.value = _case.contains;
                    _case.contains = null;
                }
                else // becoming partial match
                {
                    _case.contains = _case.value;
                    _case.value = null;

                }
                RaisePropertyChanged("ExactMatch");
                RaisePropertyChanged("Value");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }
        public string Value
        {
            get
            {
                return _case.value ?? _case.contains;
            }
            set
            {
                if (Value == value) return;
                if (ExactMatch)
                {
                    _case.value = value;
                }
                else
                {
                    _case.contains = value;
                }
                RaisePropertyChanged("Value");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }
    }

}
