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
                return Parent._altDef.Properties.ContainsKey(LinkedProperty._property);
            }
            set
            {
                if (value == true) // define this property value and add its last known value to the card data
                {
                    Parent.CachedProperties.TryGetValue(LinkedProperty, out object cachedValue);
                    Parent._altDef.Properties[LinkedProperty._property] = cachedValue;
                    Parent.CachedProperties.Remove(LinkedProperty);
                }
                else // remove this 
                {
                    if (Parent._altDef.Properties.TryGetValue(LinkedProperty._property, out object cachedValue));
                    {
                        Parent.CachedProperties.Add(LinkedProperty, cachedValue);
                        Parent._altDef.Properties.Remove(LinkedProperty._property);
                    }
                }
                RaisePropertyChanged("IsDefined");
                RaisePropertyChanged("Value");
            }
        }

        public string Name => LinkedProperty.Name;

        public object CurrentValue
        {
            get
            {
                if (IsDefined)
                {
                    Parent._altDef.Properties.TryGetValue(LinkedProperty._property, out object ret);
                    return ret;
                }
                else
                {
                    Parent.CachedProperties.TryGetValue(LinkedProperty, out object ret);
                    return ret;
                }
            }
        }

        public string Value
        {
            get
            {
                if (CurrentValue is RichTextPropertyValue richValue)
                    return richValue.ToLiteralString();
                return CurrentValue?.ToString();
            }
            set
            {
                if (IsRich)
                {
                    var richText = ConvertStringToRichText(value);
                    if (CurrentValue == richText) return;
                    Parent._altDef.Properties[LinkedProperty._property] = richText;
                }
                else
                {
                    if ((string)CurrentValue == value) return;
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
