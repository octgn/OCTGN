// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octide.ProxyTab.Handlers;
using System.Collections.Specialized;

namespace Octide.ProxyTab.ItemModel
{
    public class OverlayLinkContainer : IBaseBlock, IDroppable
    {
        public TemplateLinkContainerDropHandler DropHandler { get; set; }
        public IdeCollection<IdeBaseItem> ContainerItems { get; set; }

        public OverlayLinkContainer(IdeCollection<IdeBaseItem> source) : base(source)
        {
            CanDragDrop = false;
            DropHandler = new TemplateLinkContainerDropHandler()
            {
                Container = this
            };
            ContainerItems = new IdeCollection<IdeBaseItem>(this, typeof(OverlayLinkModel));
            ContainerItems.CollectionChanged += LinkContainerUpdated;
        }

        public OverlayLinkContainer(OverlayLinkContainer lc, IdeCollection<IdeBaseItem> source) : base(source) //copy
        {
            CanDragDrop = false;
            DropHandler = new TemplateLinkContainerDropHandler()
            {
                Container = this
            };

            ContainerItems = new IdeCollection<IdeBaseItem>(this, typeof(OverlayLinkModel));
            foreach (OverlayLinkModel link in lc.ContainerItems)
            {
                ContainerItems.Add(new OverlayLinkModel(link, ContainerItems));
            }
            ContainerItems.CollectionChanged += LinkContainerUpdated;
        }

        public void LinkContainerUpdated(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (ContainerItems.Count == 0)
                RemoveItem();
        }

        public void AddLink(int index, OverlayLinkModel link)
        {
            ContainerItems.Insert(index, new OverlayLinkModel(link, ContainerItems));
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
