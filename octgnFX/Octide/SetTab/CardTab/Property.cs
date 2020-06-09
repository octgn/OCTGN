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

namespace Octide.SetTab.ItemModel
{
    public class CardPropertyModel : ViewModelBase
    {
        public AlternateModel Parent { get; set; }
        public PropertyItemModel Property { get; set; }
        public object _cachedValue;
        public bool _isDefined;

        public bool IsRich => Property.Type == Octgn.DataNew.Entities.PropertyType.RichText;

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
                if (value == true)
                {
                    Parent._altDef.Properties[Property._property] = _cachedValue;
                }
                else
                {
                    Parent._altDef.Properties.Remove(Property._property);
                }
                RaisePropertyChanged("IsDefined");
            }
        }

        public string Name => Property.Name;

        public string Value
        {
            get
            {
                if (_cachedValue is RichTextPropertyValue richValue)
                    return richValue.ToLiteralString();
                return _cachedValue?.ToString();
            }
            set
            {
                if (IsRich)
                {
                    var richText = ConvertStringToRichText(value);
                    if (_cachedValue == richText) return;
                    _cachedValue = richText;
                }
                else
                {
                    if ((string)_cachedValue == value) return;
                    _cachedValue = value;

                }

                Parent._altDef.Properties[Property._property] = _cachedValue;

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
