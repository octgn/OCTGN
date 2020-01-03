// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Octgn.ProxyGenerator.Definitions;
using Octide.ItemModel;
using Octide.Messages;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octide.ProxyTab.TemplateItemModel
{
    public abstract class IBaseBlock : IdeListBoxItemBase
    {
        public LinkDefinition.LinkWrapper _wrapper;
        public new ObservableCollection<IBaseBlock> ItemSource { get; set; }

        public override void Remove()
        {
            if (!CanRemove) return;
            ItemSource.Remove(this);
        }
    }
    public class BlockContainer : ViewModelBase
    {
        public event EventHandler OnContainerChanged;
        public ObservableCollection<IBaseBlock> Items { get; set; }

        public BlockContainer() //new
        {
            Items = new ObservableCollection<IBaseBlock>();
            Items.CollectionChanged += (a, b) =>
            {
                OnContainerChanged?.Invoke(this, EventArgs.Empty);
            };
        }

        public BlockContainer(List<LinkDefinition.LinkWrapper> items) //load
        {
            Items = new ObservableCollection<IBaseBlock>();
            foreach (LinkDefinition.LinkWrapper item in items)
            {
                if (item.Conditional != null)
                {
                    if (item.Conditional.switchProperty != null)
                        Items.Add(new SwitchBlockModel(item) { ItemSource = Items, Parent = this });
                    else if (item.Conditional.ifNode != null)
                        Items.Add(new ConditionalBlockModel(item) { ItemSource = Items, Parent = this });
                }
                else if (item.Link != null)
                {
                    if (item.Link.IsTextLink)
                    {
                        Items.Add(new TextLinkModel(item) { Parent = this, ItemSource = Items });
                    }
                    else
                    {
                        OverlayLinkContainer container;
                        if (!(Items.LastOrDefault() is OverlayLinkContainer))
                        {
                            container = new OverlayLinkContainer() { ItemSource = Items, Parent = this };
                            Items.Add(container); //adds a new link container at the end
                        }
                        else
                        {
                            container = Items.Last() as OverlayLinkContainer;
                            container.Items.CollectionChanged -= (a, b) =>
                            {
                                OnContainerChanged?.Invoke(this, EventArgs.Empty);
                            };
                        }
                        container.Items.Add(new OverlayLinkModel(item) { Parent = this, ItemSource = container.Items });
                        container.Items.CollectionChanged += (a, b) =>
                        {
                            OnContainerChanged?.Invoke(this, EventArgs.Empty);
                        };
                    }
                }
            }
            Items.CollectionChanged += (a, b) =>
            {
                OnContainerChanged?.Invoke(this, EventArgs.Empty);
            };
        }

        public BlockContainer(BlockContainer bc) //copy
        {
            Items = new ObservableCollection<IBaseBlock>();
            foreach (var block in bc.Items)
            {
                if (block is SwitchBlockModel)
                {
                    Items.Add(new SwitchBlockModel(block as SwitchBlockModel));
                }
                else if (block is ConditionalBlockModel)
                {
                    Items.Add(new ConditionalBlockModel(block as ConditionalBlockModel));
                }
                else if (block is TextLinkModel)
                {
                    Items.Add(new TextLinkModel(block as TextLinkModel));
                }
                else if (block is OverlayLinkContainer)
                {
                    Items.Add(new OverlayLinkContainer(block as OverlayLinkContainer));
                    Items.CollectionChanged += (a, b) =>
                    {
                        OnContainerChanged?.Invoke(this, EventArgs.Empty);
                    };
                }
            }
            Items.CollectionChanged += (a, b) =>
            {
                OnContainerChanged?.Invoke(this, EventArgs.Empty);
            };
        }

        public List<LinkDefinition.LinkWrapper> BuildTemplateBlockDef()
        {
            var ret = new List<LinkDefinition.LinkWrapper>();
            foreach (IBaseBlock block in Items)
            {
                if (block is OverlayLinkContainer)
                {
                    var container = block as OverlayLinkContainer;
                    foreach (OverlayLinkModel item in container.Items)
                    {
                        ret.Add(item._wrapper);
                    }
                }
                else
                {
                    ret.Add(block._wrapper);
                }
            }
            return ret;
        }

        public void AddLink(OverlayLinkModel link, int index)
        {
            OverlayLinkContainer container = FindAdjacentLinkContainer(index);
            if (container == null)
            {
                container = new OverlayLinkContainer { ItemSource = Items, Parent = this };
                container.Items.CollectionChanged += (a, b) =>
                {
                    OnContainerChanged?.Invoke(this, EventArgs.Empty);
                };
                Items.Insert(index, container);
            }

            container.AddLink(link);
            link.ItemSource = container.Items;
            link.Parent = container;

        }

        public OverlayLinkContainer FindAdjacentLinkContainer(int index)
        {
            if (Items.Count == 0) return null;
            if (index < Items.Count && Items[index] is OverlayLinkContainer)
                return Items[index] as OverlayLinkContainer;
            if (index > 0 && Items[index - 1] is OverlayLinkContainer)
                return Items[index - 1] as OverlayLinkContainer;
            return null;
            
        }
    }
    public abstract class IBaseConditionalCase : IdeListBoxItemBase
    {
        public CaseDefinition _case;
        public PropertyItemViewModel _property;
        public ObservableCollection<PropertyItemViewModel> CustomProperties => ViewModelLocator.PropertyTabViewModel.Items;
        public BlockContainer BlockContainer { get; set; }
        public new ObservableCollection<IBaseConditionalCase> ItemSource { get; set; }

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
                _case.property = value.Name;
                RaisePropertyChanged("Property");
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
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
                return (_case.value != null);
            }
            set
            {
                if ((_case.value != null) == value) return;
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
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }
        public string Value
        {
            get
            {
                return (_case.value != null) ? _case.value : _case.contains;
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
                Messenger.Default.Send(new ProxyTemplateChangedMessage());
            }
        }
    }

}
