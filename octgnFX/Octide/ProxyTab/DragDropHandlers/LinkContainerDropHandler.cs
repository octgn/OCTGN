// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Octide.ItemModel;
using Octide.ProxyTab.ItemModel;
using System;

namespace Octide.ProxyTab.Handlers
{
    public class TemplateLinkContainerDropHandler : IDropTarget
    {
        public OverlayLinkContainer Container { get; set; }

        public void DragEnter(IDropInfo dropInfo) { }
        public void DragLeave(IDropInfo dropInfo) { }

        public void DragOver(IDropInfo dropInfo)
        {
            if (!(dropInfo.Data is IdeBaseItem sourceItem))  // prevents foreign objects from dropping in
            {
                return;
            }

            //dropping a definition into the overlaycontainer
            if (sourceItem is OverlayBlockDefinitionItemModel overlayDef)
            {
                //targeting an existing overlaylink will update it to the definition
                if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = System.Windows.DragDropEffects.Copy;

                }
                //Inserting into an overlaycontainer
                else
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                    dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                }
            }

            else if (dropInfo.TargetCollection == dropInfo.DragInfo.SourceCollection && dropInfo.TargetCollection.TryGetList().Count == 1)
            {
                //stops the linkcontainer from collapsing when the only item gets removed by the dragdrop
                return;
            }
            else if (sourceItem is OverlayLinkModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = System.Windows.DragDropEffects.Move;

            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is OverlayBlockDefinitionItemModel dropItem)
            {
                if (dropInfo.TargetItem is OverlayLinkModel overlayLink && dropInfo.DropTargetAdorner == DropTargetAdorners.Highlight)
                {
                    // change the link definition of an existing text link item
                    overlayLink.LinkedBlock = dropItem;
                }
                else
                {
                    var item = new OverlayLinkModel(Container.Items)
                    {
                        LinkedBlock = dropItem
                    };
                    Container.Items.Insert(dropInfo.UnfilteredInsertIndex, item);
                }
            }
            else if (dropInfo.Data is OverlayLinkModel)
            {
                DragDrop.DefaultDropHandler.Drop(dropInfo);
            }
        }
    }
}