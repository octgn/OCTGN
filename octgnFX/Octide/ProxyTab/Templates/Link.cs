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

        }

        public OverlayLinkModel(LinkDefinition.LinkWrapper lw, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _wrapper = lw;
        }
        
        public OverlayLinkModel(OverlayLinkModel link, IdeCollection<IdeBaseItem> source) : base(source)  //copy
        {
            var _linkDefinition = new LinkDefinition()
            {
                Block = link._wrapper.Link.Block,
                NestedProperties = new List<Property>()
            };
            _wrapper = new LinkDefinition.LinkWrapper() { Link = _linkDefinition };

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
            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                BuildPropertyDefinition(b);
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
        }

        public TextLinkModel(LinkDefinition.LinkWrapper lw, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _wrapper = lw;
            Items = new IdeCollection<IdeBaseItem>(this);
            foreach (var property in lw.Link.NestedProperties)
            {
                Items.Add(new TextLinkPropertyModel(property, Items));
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildPropertyDefinition(b);
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
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

            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                BuildPropertyDefinition(b);
            };
            foreach (TextLinkPropertyModel property in link.Items)
            {
                Items.Add(new TextLinkPropertyModel(property, Items));
            }
            AddPropertyCommand = new RelayCommand(AddProperty);
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

    public class TextLinkPropertyModel : IdeBaseItem
    {
        public Property _property;
        public PropertyItemModel _activeProperty;
        public IdeCollection<IdeBaseItem> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;

        public TextLinkPropertyModel(IdeCollection<IdeBaseItem> source) : base(source) //new
        {
            _property = new Property()
            {
                Value = ""
            };
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            ActiveProperty = (PropertyItemModel)CustomProperties.First();
        }
        public TextLinkPropertyModel(Property prop, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _property = prop;
            _activeProperty = (PropertyItemModel)CustomProperties.FirstOrDefault(x => ((PropertyItemModel)x)._property.Name == _property.Name);
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));

        }
        public TextLinkPropertyModel(TextLinkPropertyModel lp, IdeCollection<IdeBaseItem> source) : base(source)  //copy
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
                    value = (PropertyItemModel)CustomProperties.First();
                }
                _activeProperty = value;
                _property.Name = value.Name;
                RaisePropertyChanged("ActiveProperty");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
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
