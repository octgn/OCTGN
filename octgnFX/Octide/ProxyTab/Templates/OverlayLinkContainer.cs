// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Octide.ProxyTab.TemplateItemModel
{
    public class OverlayLinkContainer : IBaseBlock
    {
        public TemplateLinkContainerDropHandler DropHandler { get; set; }
        public ObservableCollection<OverlayLinkModel> Items { get; set; }

        public OverlayLinkContainer()
        {
            CanDragDrop = false;
            DropHandler = new TemplateLinkContainerDropHandler()
            {
                Container = this
            };
            Items = new ObservableCollection<OverlayLinkModel>();
            Items.CollectionChanged += LinkContainerUpdated;
        }

        public OverlayLinkContainer(OverlayLinkContainer lc) //copy
        {
            CanDragDrop = false;
            DropHandler = new TemplateLinkContainerDropHandler()
            {
                Container = this
            };

            Items = new ObservableCollection<OverlayLinkModel>();
            foreach (OverlayLinkModel link in lc.Items)
            {
                Items.Add(new OverlayLinkModel(link as OverlayLinkModel));
            }
            Items.CollectionChanged += LinkContainerUpdated;
        }

        public void LinkContainerUpdated(object sender, NotifyCollectionChangedEventArgs args)
        {
            // updates parent container data for newly-added items (usually from drag-drops)
            if (args.Action == NotifyCollectionChangedAction.Add) 
            {
                foreach (OverlayLinkModel item in args.NewItems)
                {
                    item.Parent = this;
                    item.ItemSource = Items;
                }
            }
            if (Items.Count == 0)
                Remove();
        }

        public void AddLink(OverlayLinkModel link)
        {
            if (link is OverlayLinkModel)
            {
                Items.Add(new OverlayLinkModel(link as OverlayLinkModel));
            }
        }
    }
}
