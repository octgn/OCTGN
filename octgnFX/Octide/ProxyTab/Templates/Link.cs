// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */


using Octgn.ProxyGenerator.Definitions;
using Octide.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GongSolutions.Wpf.DragDrop;
using Octide.ItemModel;
using System.Collections.Specialized;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octide.Messages;

namespace Octide.ProxyTab.TemplateItemModel
{

    public class OverlayLinkModel : IBaseBlock
    {
        public new ObservableCollection<OverlayLinkModel> ItemSource { get; set; }
        public OverlayLinkModel() //new
        {
            var _linkDefinition = new LinkDefinition()
            {
                NestedProperties = new List<Property>()
            };
            _wrapper = new LinkDefinition.LinkWrapper() { Link = _linkDefinition };

        }

        public OverlayLinkModel(LinkDefinition.LinkWrapper lw) //load
        {
            _wrapper = lw;
        }
        
        public OverlayLinkModel(OverlayLinkModel link)  //copy
        {
            var _linkDefinition = new LinkDefinition()
            {
                Block = link._wrapper.Link.Block,
                NestedProperties = new List<Property>()
            };
            _wrapper = new LinkDefinition.LinkWrapper() { Link = _linkDefinition };

        }
        public ProxyOverlayDefinitionItemModel LinkedBlock
        {
            get
            {
                return ViewModelLocator.ProxyTabViewModel.OverlayBlocks.FirstOrDefault(x => x._def.id == _wrapper.Link.Block);
            }
            set
            {
                _wrapper.Link.Block = value._def.id;
                RaisePropertyChanged("LinkedBlock");
            }
        }

        public override void Remove()
        {
            if (!CanRemove) return;
            ItemSource.Remove(this);
        }

        public override object Clone()
        {
            return new OverlayLinkModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as OverlayLinkModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new OverlayLinkModel());
        }
    }

    public class TextLinkModel : IBaseBlock
    {
        public ObservableCollection<TextLinkPropertyModel> Items { get; set; }
        public RelayCommand AddPropertyCommand { get; set; }
        public TextLinkModel() //new
        {
            var _linkDefinition = new LinkDefinition()
            {
                IsTextLink = true,
                NestedProperties = new List<Property>()
            };
            _wrapper = new LinkDefinition.LinkWrapper() { Link = _linkDefinition };
            Items = new ObservableCollection<TextLinkPropertyModel>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildPropertyDefinition(b);
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
        }

        public TextLinkModel(LinkDefinition.LinkWrapper lw) //load
        {
            _wrapper = lw;
            Items = new ObservableCollection<TextLinkPropertyModel>();
            foreach (var property in lw.Link.NestedProperties)
            {
                Items.Add(new TextLinkPropertyModel(property) { ItemSource = Items, Parent = this });
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildPropertyDefinition(b);
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
        }

        public TextLinkModel(TextLinkModel link)  //copy
        {
            var _linkDefinition = new LinkDefinition()
            {
                Block = link._wrapper.Link.Block,
                IsTextLink = true,
                Separator = link._wrapper.Link.Separator,
                NestedProperties = new List<Property>()
            };
            _wrapper = new LinkDefinition.LinkWrapper() { Link = _linkDefinition };

            Items = new ObservableCollection<TextLinkPropertyModel>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildPropertyDefinition(b);
            };
            foreach (var property in link.Items)
            {
                Items.Add(new TextLinkPropertyModel(property));
            }
            AddPropertyCommand = new RelayCommand(AddProperty);
        }
        public void BuildPropertyDefinition(NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TextLinkPropertyModel x in args.NewItems)
                {
                    x.ItemSource = Items;
                    x.Parent = this;
                }
            }
            _wrapper.Link.NestedProperties = Items.Select(x => x._property).ToList();
        }

        public override object Clone()
        {
            return new TextLinkModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as TextLinkModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new TextLinkModel());
        }

        public void AddProperty()
        {
            Items.Add(new TextLinkPropertyModel());
        }
        public ProxyTextDefinitionItemModel LinkedBlock
        {
            get
            {
                return ViewModelLocator.ProxyTabViewModel.TextBlocks.FirstOrDefault(x => x._def.id == _wrapper.Link.Block);
            }
            set
            {
                _wrapper.Link.Block = value._def.id;
                RaisePropertyChanged("LinkedBlock");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }

        public string Separator
        {
            get
            {
                return _wrapper.Link.Separator;
            }
            set
            {
                if (_wrapper.Link.Separator == value) return;
                _wrapper.Link.Separator = value;
                RaisePropertyChanged("Separator");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }
    }

    public class TextLinkPropertyModel : IdeListBoxItemBase
    {
        public Property _property;
        public PropertyItemViewModel _activeProperty;
        public ObservableCollection<PropertyItemViewModel> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;
        public new ObservableCollection<TextLinkPropertyModel> ItemSource { get; set; }

        public TextLinkPropertyModel() //new
        {
            _property = new Property()
            {
                Value = ""
            };
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            ActiveProperty = CustomProperties.First();
        }
        public TextLinkPropertyModel(Property prop) //load
        {
            _property = prop;
            _activeProperty = CustomProperties.FirstOrDefault(x => x._property.Name == _property.Name);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));

        }
        public TextLinkPropertyModel(TextLinkPropertyModel lp)  //copy
        {
            _property = new Property()
            {
                Name = lp._property.Name,
                Format = lp._property.Format
            };
            _activeProperty = lp._activeProperty;
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));

        }
        public void CustomPropertyChanged(CustomPropertyChangedMessage args)
        {
            if (args.Prop == _activeProperty)
            {
                _property.Name = args.Prop.Name;
                RaisePropertyChanged("ActiveProperty");
            }
        }
        public PropertyItemViewModel ActiveProperty
        {
            get
            {
                return _activeProperty;
            }
            set
            {
                if (_activeProperty == value) return;
                if (value == null)
                {
                    value = CustomProperties.First();
                }
                _activeProperty = value;
                _property.Name = value.Name;
                RaisePropertyChanged("ActiveProperty");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }

        public override object Clone()
        {
            return new TextLinkPropertyModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as TextLinkPropertyModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new TextLinkPropertyModel());
        }
        public override void Remove()
        {
            if (!CanRemove) return;
            ItemSource.Remove(this);
        }
    }
}
