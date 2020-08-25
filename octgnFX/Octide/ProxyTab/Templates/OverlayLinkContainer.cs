// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octide.ProxyTab.Handlers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Octide.ProxyTab.ItemModel
{
    public class OverlayLinkContainer : IBaseBlock, IDroppable
    {
        public TemplateLinkContainerDropHandler DropHandler { get; set; }
        public IdeCollection<IdeBaseItem> Items { get; set; }

        public OverlayLinkContainer(IdeCollection<IdeBaseItem> source) : base(source)
        {
            CanDragDrop = false;
            DropHandler = new TemplateLinkContainerDropHandler()
            {
                Container = this
            };
            Items = new IdeCollection<IdeBaseItem>(this, typeof(OverlayLinkModel));
            Items.CollectionChanged += LinkContainerUpdated;
        }

        public OverlayLinkContainer(OverlayLinkContainer lc, IdeCollection<IdeBaseItem> source) : base(source) //copy
        {
            CanDragDrop = false;
            DropHandler = new TemplateLinkContainerDropHandler()
            {
                Container = this
            };

            Items = new IdeCollection<IdeBaseItem>(this, typeof(OverlayLinkModel));
            foreach (OverlayLinkModel link in lc.Items)
            {
                Items.Add(new OverlayLinkModel(link, Items));
            }
            Items.CollectionChanged += LinkContainerUpdated;
        }

        public void LinkContainerUpdated(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (Items.Count == 0)
                RemoveItem();
        }

        public void AddLink(int index, OverlayLinkModel link)
        {
            Items.Insert(index, new OverlayLinkModel(link, Items));
        }
        public bool CanAccept(object item)
        {
            if (item is OverlayLinkModel)
            {
                return true;
            }
            return false;
        }
    }
}
