// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */


using System.Collections.ObjectModel;

namespace Octide.ProxyTab.TemplateItemModel
{
    public class OverlayLinkContainer : IBaseBlock
    {
        public TemplateLinkContainerDropHandler DropHandler { get; set; }
        public ObservableCollection<IBaseLink> Items { get; set; }

        public OverlayLinkContainer()
        {
            CanDragDrop = false;
            DropHandler = new TemplateLinkContainerDropHandler();
            Items = new ObservableCollection<IBaseLink>();
            Items.CollectionChanged += (a, b) =>
            {
                if (Items.Count == 0)
                    Remove();
            };
        }

        public OverlayLinkContainer(OverlayLinkContainer lc) //copy
        {
            CanDragDrop = false;
            DropHandler = new TemplateLinkContainerDropHandler();

            Items = new ObservableCollection<IBaseLink>();
            foreach (IBaseLink link in lc.Items)
            {
                if (link is OverlayLinkModel)
                {
                    Items.Add(new OverlayLinkModel(link as OverlayLinkModel));
                }
            //    else if (link is TextLinkModel)
            //    {
            //        Items.Add(new TextLinkModel(link as TextLinkModel));
            //    }
            }
            Items.CollectionChanged += (a, b) =>
            {
                if (Items.Count == 0)
                    Remove();
            };
            ItemSource = lc.ItemSource;
            Parent = lc;
        }
        public void AddLink(IBaseLink link)
        {
            if (link is OverlayLinkModel)
            {
                Items.Add(new OverlayLinkModel(link as OverlayLinkModel) { Parent = this, ItemSource = Items });
            }
           // if (link is TextLinkModel)
           // {
           //     Items.Add(new TextLinkModel(link as TextLinkModel) { Parent = this, ItemSource = Items });
           // }
        }
    }
}
