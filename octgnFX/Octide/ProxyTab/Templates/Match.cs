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
    public class MatchModel : IdeListBoxItemBase
    {
        public new TemplateModel Parent { get; set; }
        public new ObservableCollection<MatchModel> ItemSource { get; set; }
        public ObservableCollection<PropertyItemViewModel> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;


        public PropertyItemViewModel _property;

        public Property _match;

        public MatchModel()  //new match
        {
            _match = new Property()
            {

            };
            Property = CustomProperties.First();
            RemoveCommand = new RelayCommand(Remove);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));

        }

        public MatchModel(Property property)  //load match
        {
            _match = property;
            _property = CustomProperties.FirstOrDefault(x => x._property.Name == _match.Name);

            RemoveCommand = new RelayCommand(Remove);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));

        }

        public MatchModel(MatchModel property)  // copy match
        {
            _match = new Property()
            {
                Value = property.Value,
                Format = property.Format
            };
            Property = property.Property;
            RemoveCommand = new RelayCommand(Remove);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));

        }

        public override object Clone()
        {
            return new MatchModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as MatchModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new MatchModel());
        }
        public override void Remove()
        {
            if (!CanRemove) return;
            ItemSource.Remove(this);
        }

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
