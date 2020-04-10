// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using Octgn.ProxyGenerator.Definitions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Octide.ProxyTab.TemplateItemModel
{
    public class BlockContainer : ViewModelBase
    {
        public delegate void ContainerChangeEventHandler(object sender, NotifyCollectionChangedEventArgs e);

        public event ContainerChangeEventHandler OnContainerChanged;

        public IdeCollection<IdeBaseItem> Items { get; private set; }

        public BlockContainer() //new
        {
            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                OnContainerChanged?.Invoke(this, b);
            };
        }

        public BlockContainer(List<LinkDefinition.LinkWrapper> items) //load
        {
            Items = new IdeCollection<IdeBaseItem>(this);
            foreach (LinkDefinition.LinkWrapper item in items)
            {
                if (item.Conditional != null)
                {
                    if (item.Conditional.switchProperty != null)
                        Items.Add(new SwitchBlockModel(item, Items));
                    else if (item.Conditional.ifNode != null)
                        Items.Add(new ConditionalBlockModel(item, Items));
                }
                else if (item.Link != null)
                {
                    if (item.Link.IsTextLink)
                    {
                        Items.Add(new TextLinkModel(item, Items));
                    }
                    else
                    {
                        OverlayLinkContainer container;
                        if (!(Items.LastOrDefault() is OverlayLinkContainer))
                        {
                            container = new OverlayLinkContainer(Items) ;
                            Items.Add(container); //adds a new link container at the end
                        }
                        else
                        {
                            container = Items.Last() as OverlayLinkContainer;
                            container.Items.CollectionChanged -= (a, b) =>
                            {
                                OnContainerChanged?.Invoke(this, b);
                            };
                        }
                        container.Items.Add(new OverlayLinkModel(item, container.Items));
                        container.Items.CollectionChanged += (a, b) =>
                        {
                            OnContainerChanged?.Invoke(this, b);
                        };
                    }
                }
                else if (item.CardArtCrop != null)
                {
                    Items.Add(new ArtOverlayBlockModel(item, Items));
                }
            }
            Items.CollectionChanged += (a, b) =>
            {
                OnContainerChanged?.Invoke(this, b);
            };
        }

        public BlockContainer(BlockContainer bc) //copy
        {
            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                OnContainerChanged?.Invoke(this, b);
            };
            foreach (var block in bc.Items)
            {
                if (block is SwitchBlockModel switchBlock)
                {
                    Items.Add(new SwitchBlockModel(switchBlock, Items));
                }
                else if (block is ConditionalBlockModel conditionalBlock)
                {
                    Items.Add(new ConditionalBlockModel(conditionalBlock, Items));
                }
                else if (block is TextLinkModel textLink)
                {
                    Items.Add(new TextLinkModel(textLink, Items));
                }
                else if (block is OverlayLinkContainer overlayContainer)
                {
                    Items.Add(new OverlayLinkContainer(overlayContainer, Items));
                }
                else if (block is ArtOverlayBlockModel artOverlay)
                {
                    Items.Add(new ArtOverlayBlockModel(artOverlay, Items));
                }
            }
        }

        public List<LinkDefinition.LinkWrapper> BuildTemplateBlockDef(NotifyCollectionChangedEventArgs args)
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
                container = new OverlayLinkContainer(Items);
                container.Items.CollectionChanged += (a, b) =>
                {
                    OnContainerChanged?.Invoke(this, b);
                };
                Items.Insert(index, container);
            }

            container.AddLink(link);
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

}
