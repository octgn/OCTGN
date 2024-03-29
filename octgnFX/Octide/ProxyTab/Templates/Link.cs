﻿// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */


using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.ProxyGenerator.Definitions;
using Octide.ItemModel;
using Octide.Messages;
using Octide.ViewModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Octide.ProxyTab.ItemModel
{

    public class OverlayLinkModel : IBaseBlock, IDroppable
    {
        public OverlayLinkModel(IdeCollection<IdeBaseItem> source) : base(source) //new
        {
            var _linkDefinition = new LinkDefinition()
            {
                NestedProperties = new List<Property>()
            };
            _wrapper = new LinkDefinition.LinkWrapper() { Link = _linkDefinition };
            PropertyChanged += (a, b) => Messenger.Default.Send(new ProxyTemplateChangedMessage());

        }

        public OverlayLinkModel(LinkDefinition.LinkWrapper lw, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _wrapper = lw;
            PropertyChanged += (a, b) => Messenger.Default.Send(new ProxyTemplateChangedMessage());
        }

        public OverlayLinkModel(OverlayLinkModel link, IdeCollection<IdeBaseItem> source) : base(source)  //copy
        {
            var _linkDefinition = new LinkDefinition()
            {
                Block = link._wrapper.Link.Block,
                NestedProperties = new List<Property>()
            };
            _wrapper = new LinkDefinition.LinkWrapper() { Link = _linkDefinition };
            PropertyChanged += (a, b) => Messenger.Default.Send(new ProxyTemplateChangedMessage());

        }
        public OverlayBlockDefinitionItemModel LinkedBlock
        {
            get
            {
                return (OverlayBlockDefinitionItemModel)ViewModelLocator.ProxyTabViewModel.OverlayBlocks.FirstOrDefault(x => ((OverlayBlockDefinitionItemModel)x)._def.id == _wrapper.Link.Block);
            }
            set
            {
                _wrapper.Link.Block = value._def.id;
                RaisePropertyChanged("LinkedBlock");
            }
        }

        public override object Clone()
        {
            return new OverlayLinkModel(this, Source);
        }
        public override object Create()
        {
            return new OverlayLinkModel(Source);
        }

        public bool CanAccept(object item)
        {
            if (item is OverlayBlockDefinitionItemModel)
            {
                return true;
            }
            return false;
        }
    }

    public class TextLinkModel : IBaseBlock, IDroppable
    {
        public IdeCollection<IdeBaseItem> Items { get; private set; }
        public RelayCommand AddPropertyCommand { get; set; }


        public TextLinkModel(IdeCollection<IdeBaseItem> source) : base(source) //new
        {
            var _linkDefinition = new LinkDefinition()
            {
                IsTextLink = true,
                NestedProperties = new List<Property>()
            };
            _wrapper = new LinkDefinition.LinkWrapper() { Link = _linkDefinition };
            Items = new IdeCollection<IdeBaseItem>(this, typeof(TextLinkPropertyModel));
            Items.CollectionChanged += (a, b) =>
            {
                BuildPropertyDefinition(b);
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
            PropertyChanged += (a, b) => Messenger.Default.Send(new ProxyTemplateChangedMessage());
        }

        public TextLinkModel(LinkDefinition.LinkWrapper lw, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _wrapper = lw;
            Items = new IdeCollection<IdeBaseItem>(this, typeof(TextLinkPropertyModel));
            foreach (var property in lw.Link.NestedProperties)
            {
                Items.Add(new TextLinkPropertyModel(property, Items));
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildPropertyDefinition(b);
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
            PropertyChanged += (a, b) => Messenger.Default.Send(new ProxyTemplateChangedMessage());
        }

        public TextLinkModel(TextLinkModel link, IdeCollection<IdeBaseItem> source) : base(source)  //copy
        {
            var _linkDefinition = new LinkDefinition()
            {
                Block = link._wrapper.Link.Block,
                IsTextLink = true,
                Separator = link._wrapper.Link.Separator,
                NestedProperties = new List<Property>()
            };
            _wrapper = new LinkDefinition.LinkWrapper() { Link = _linkDefinition };

            Items = new IdeCollection<IdeBaseItem>(this, typeof(TextLinkPropertyModel));
            Items.CollectionChanged += (a, b) =>
            {
                BuildPropertyDefinition(b);
            };
            foreach (TextLinkPropertyModel property in link.Items)
            {
                Items.Add(new TextLinkPropertyModel(property, Items));
            }
            AddPropertyCommand = new RelayCommand(AddProperty);
            PropertyChanged += (a, b) => Messenger.Default.Send(new ProxyTemplateChangedMessage());
        }
        public void BuildPropertyDefinition(NotifyCollectionChangedEventArgs args)
        {
            _wrapper.Link.NestedProperties = Items.Select(x => ((TextLinkPropertyModel)x)._property).ToList();
        }

        public override object Clone()
        {
            return new TextLinkModel(this, Source);
        }

        public override object Create()
        {
            return new TextLinkModel(Source);
        }
        public bool CanAccept(object item)
        {
            if (item is TextLinkPropertyModel || item is TextBlockDefinitionItemModel)
            {
                return true;
            }
            return false;
        }

        public void AddProperty()
        {
            Items.Add(new TextLinkPropertyModel(Items));
        }
        public TextBlockDefinitionItemModel LinkedBlock
        {
            get
            {
                return (TextBlockDefinitionItemModel)ViewModelLocator.ProxyTabViewModel.TextBlocks.FirstOrDefault(x => ((TextBlockDefinitionItemModel)x)._def.id == _wrapper.Link.Block);
            }
            set
            {
                _wrapper.Link.Block = value._def.id;
                RaisePropertyChanged("LinkedBlock");
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
            }
        }
    }

    public class TextLinkPropertyModel : IdeBaseItem
    {
        public Property _property;
        public PropertyItemModel _activeProperty;

        public TextLinkPropertyModel(IdeCollection<IdeBaseItem> source) : base(source) //new
        {
            _property = new Property()
            {
                Value = ""
            };
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, CustomPropertyChanged);
            ActiveProperty = PropertyTabViewModel.NameProperty;
            PropertyChanged += (a, b) => Messenger.Default.Send(new ProxyTemplateChangedMessage());
        }
        public TextLinkPropertyModel(Property prop, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _property = prop;
            _activeProperty = (PropertyItemModel)ViewModelLocator.PropertyTabViewModel.ProxyItems.FirstOrDefault(x => ((PropertyItemModel)x).Property.Name == _property.Name);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, CustomPropertyChanged);
            PropertyChanged += (a, b) => Messenger.Default.Send(new ProxyTemplateChangedMessage());

        }
        public TextLinkPropertyModel(TextLinkPropertyModel lp, IdeCollection<IdeBaseItem> source) : base(source)  //copy
        {
            _property = new Property()
            {
                Name = lp._property.Name,
                Format = lp._property.Format
            };
            _activeProperty = lp._activeProperty;
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, CustomPropertyChanged);
            PropertyChanged += (a, b) => Messenger.Default.Send(new ProxyTemplateChangedMessage());

        }
        public void CustomPropertyChanged(CustomPropertyChangedMessage args)
        {
            if (args.Prop == _activeProperty)
            {
                _property.Name = args.Prop.Name;
                RaisePropertyChanged("ActiveProperty");
            }
        }
        public PropertyItemModel ActiveProperty
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
                    value = PropertyTabViewModel.NameProperty;
                }
                _activeProperty = value;
                _property.Name = value.Name;
                RaisePropertyChanged("ActiveProperty");
            }
        }

        public override object Clone()
        {
            return new TextLinkPropertyModel(this, Source);
        }

        public override object Create()
        {
            return new TextLinkPropertyModel(Source);
        }

    }
}
