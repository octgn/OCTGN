// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using Octide.ViewModel;
using Octide.ItemModel;
using Octgn.DataNew.Entities;
using System.Xml;
using System.Xml.Linq;
using Octgn.DataNew;
using System.Linq;

namespace Octide.SetTab.ItemModel
{
    public class CachedPropertyValue : ViewModelBase
    {
        public object Property { get; set; }

        public override string ToString()
        {
            return Property?.ToString();
        }
    }
    public class CardPropertyModel : ViewModelBase, ICleanup
    {
        public AlternateModel Parent { get; set; }
        public PropertyItemModel LinkedProperty { get; set; }

        public bool IsRich => LinkedProperty.Type == Octgn.DataNew.Entities.PropertyType.RichText;

        public bool IsValid => ViewModelLocator.PropertyTabViewModel.Items.Contains(LinkedProperty);

        public CardPropertyModel(AlternateModel parent, PropertyItemModel property)
        {
            Parent = parent;
            LinkedProperty = property;
        }

        public bool IsDefined
        {
            get
            {
                if (Parent._altDef.Properties.TryGetValue(LinkedProperty._property, out object ret))
                    return !(ret is CachedPropertyValue);
                return false;
            }
            set
            {
                if (value == true) // define this property value and add its last known value to the card data
                {
                    if (PropertyValue is CachedPropertyValue cachedValue)
                    {
                        Parent._altDef.Properties[LinkedProperty._property] = cachedValue.Property;
                    }
                    else
                    {
                        Parent._altDef.Properties[LinkedProperty._property] = PropertyValue;
                    }
                }
                else // remove this 
                {
                    var cachedValue = new CachedPropertyValue() { Property = PropertyValue };
                    Parent._altDef.Properties[LinkedProperty._property] = cachedValue;
                }
                RaisePropertyChanged("IsDefined");
                RaisePropertyChanged("Value");
            }
        }

        public string Name => LinkedProperty.Name;

        public object PropertyValue
        {
            get
            {
                Parent._altDef.Properties.TryGetValue(LinkedProperty._property, out object ret);
                return ret;
            }
        }

        public string Value
        {
            get
            {
                if (PropertyValue is RichTextPropertyValue richValue)
                    return richValue.ToLiteralString();
                return PropertyValue?.ToString();
            }
            set
            {
                if (IsRich)
                {
                    var richText = ConvertStringToRichText(value);
                    if (PropertyValue == richText) return;
                    Parent._altDef.Properties[LinkedProperty._property] = richText;
                }
                else
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        value = null;
                    }
                    if (PropertyValue?.ToString() == value) return;
                    Parent._altDef.Properties[LinkedProperty._property] = value;
                }


               // Parent.UpdateAltPropertySet();
                Parent.UpdateProxyTemplate();
                RaisePropertyChanged("Value");
            }
        }

        public RichTextPropertyValue ConvertStringToRichText(string value)
        {
            var xmlElement = XElement.Parse("<rich>" + value + "</rich>");

            var richSpan = new RichSpan();

            SetSerializer.DeserializeRichCardProperty(richSpan, xmlElement, ViewModelLocator.GameLoader.Game);
            

            return new RichTextPropertyValue() { Value = richSpan };
        }
    }
}
