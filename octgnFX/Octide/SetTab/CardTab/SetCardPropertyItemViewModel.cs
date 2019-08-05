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
        public bool _isDefined;

        public bool IsValid => ViewModelLocator.PropertyTabViewModel.Items.Contains(Property);
        public bool IsNameProperty => Property.Name == "Name";

        public CardPropertyItemModel(PropertyItemViewModel prop, KeyValuePair<PropertyDef, object> propDef, SetCardAltItemViewModel parent) // initial load of property
        {
            Parent = parent;
            Property = prop;
            _isDefined = !propDef.Key.IsUndefined;
            _value = propDef.Value;
        }

        public CardPropertyItemModel(PropertyItemViewModel prop)
        {
            Property = prop;
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
                RaisePropertyChanged("IsDefined");
                Parent.UpdateAltPropertySet();
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

                Parent.UpdateAltPropertySet();
                Parent.UpdateProxyTemplate();
                RaisePropertyChanged("Value");
            }
        }
    }
}
