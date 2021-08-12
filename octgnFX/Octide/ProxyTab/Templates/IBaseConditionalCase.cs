// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.ProxyGenerator.Definitions;
using Octide.ItemModel;
using Octide.Messages;
using Octide.ViewModel;
using System.Linq;

namespace Octide.ProxyTab.ItemModel
{
    public abstract class IBaseConditionalCase : IdeBaseItem, IDroppable
    {
        public CaseDefinition _case;
        public PropertyItemModel _property;
        public IdeCollection<IdeBaseItem> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;
        public BlockContainer BlockContainer { get; set; }

        public IBaseConditionalCase(IdeCollection<IdeBaseItem> source) : base(source)
        {

        }
        public bool CanAccept(object item)
        {
            if (item is IBaseBlock)
                return true;
            return false;
        }

        public PropertyItemModel Property
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
                    value = (PropertyItemModel)CustomProperties.First();
                }
                _property = value;
                _case.property = value.Name;
                RaisePropertyChanged("Property");
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
                return _case.value != null;
            }
            set
            {
                if (_case.value != null == value) return;
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
            }
        }
    }

}
