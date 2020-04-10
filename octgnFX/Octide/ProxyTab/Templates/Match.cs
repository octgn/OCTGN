// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.ProxyGenerator;
using Octgn.ProxyGenerator.Definitions;
using Octide.ItemModel;
using Octide.Messages;
using Octide.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.ProxyTab.TemplateItemModel
{
    public class MatchModel : IdeBaseItem
    {
        public IdeCollection<IdeBaseItem> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;


        public PropertyItemModel _property;

        public Property _match;

        public MatchModel(IdeCollection<IdeBaseItem> source) : base(source)  //new match
        {
            _match = new Property()
            {

            };
            Property = (PropertyItemModel)CustomProperties.First();
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));

        }

        public MatchModel(Property property, IdeCollection<IdeBaseItem> source) : base(source)  //load match
        {
            _match = property;
            _property = (PropertyItemModel)CustomProperties.FirstOrDefault(x => ((PropertyItemModel)x)._property.Name == _match.Name);

            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));

        }

        public MatchModel(MatchModel property, IdeCollection<IdeBaseItem> source) : base(source)  // copy match
        {
            _match = new Property()
            {
                Value = property.Value,
                Format = property.Format
            };
            Property = property.Property;
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));

        }

        public override object Clone()
        {
            return new MatchModel(this, Source);
        }
        public override object Create()
        {
            return new MatchModel(Source);
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
                _match.Name = value.Name;
                RaisePropertyChanged("Property");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }

        public string Format
        {
            get
            {
                return _match.Format;
            }
            set
            {
                if (_match.Format == value) return;
                _match.Format = value;
                RaisePropertyChanged("Format");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }
        public string Value
        {
            get
            {
                return _match.Value;
            }
            set
            {
                if (_match.Value == value) return;
                _match.Value = value;
                RaisePropertyChanged("Value");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }
        public void CustomPropertyChanged(CustomPropertyChangedMessage args)
        {
            if (args.Prop == _property)
            {
                _match.Name = args.Prop.Name;
                RaisePropertyChanged("Property");
            }
        }



    }



}
