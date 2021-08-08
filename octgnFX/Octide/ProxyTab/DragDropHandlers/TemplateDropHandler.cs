// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GongSolutions.Wpf.DragDrop;
using Octide.ItemModel;
using Octide.ProxyTab.ItemModel;

namespace Octide.ProxyTab.Handlers
{

    public class TemplateMainDropHandler : IDropTarget
    {
        public bool IsOverlayHandler { get; set; }
        public void DragEnter(IDropInfo dropInfo) { }
        public void DragLeave(IDropInfo dropInfo) { }

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.Effects = System.Windows.DragDropEffects.None;
            if (!(dropInfo.Data is IdeBaseItem sourceItem))  // prevents foreign objects from dropping in
            {
                return;
            }

            //deal specifically with the definition blocks
            if (sourceItem is TextBlockDefinitionItemModel)
            {
                if (!IsOverlayHandler)
                {
                    if (dropInfo.TargetItem is TextLinkModel)
                    {
                        dropInfo.DropTargetAdorner = dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter)
                            ? DropTargetAdorners.Highlight
                            : DropTargetAdorners.Insert;
                        dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                    }
                    else if (dropInfo.TargetItem is IBaseConditionalCase)
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                        dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                    }
                    else if (dropInfo.TargetCollection is IdeCollection<IdeBaseItem> items && items.Parent is BlockContainer)
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                        dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                    }
                }
                return;
            }
            if (sourceItem is OverlayBlockDefinitionItemModel)
            {
                if (IsOverlayHandler)
                {
                    if (dropInfo.TargetItem is IBaseConditionalCase)
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                        dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                    }
                    else if (dropInfo.TargetCollection is IdeCollection<IdeBaseItem> items && items.Parent is BlockContainer)
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                        dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                    }
                }
                return;
            }

            //checks the eligibility of the parent to accept dropped item
            if (dropInfo.TargetCollection is IdeCollection<IdeBaseItem> collection && collection.Parent is IDroppable parent)
            {
                if (parent.CanAccept(sourceItem))
                {
                    DragDrop.DefaultDropHandler.DragOver(dropInfo);
                }
            }
            else if (dropInfo.TargetItem is IDroppable item)
            {
                if (item.CanAccept(sourceItem))
                {
                    DragDrop.DefaultDropHandler.DragOver(dropInfo);
                }
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is TextBlockDefinitionItemModel textDef)
            {
                // Dropping the definition into a blockcontainer
                if (dropInfo.TargetCollection is IdeCollection<IdeBaseItem> collection && collection.Parent is BlockContainer container)
                {
                    var link = new TextLinkModel(container.Items)
                    {
                        LinkedBlock = textDef
                    };
                    container.Items.Insert(dropInfo.UnfilteredInsertIndex, link);

                }
                // change the link definition of an existing text link item
                else if (dropInfo.TargetItem is TextLinkModel textLink)
                {
                    textLink.LinkedBlock = textDef;
                }
                // Dropping the definition into a conditional case
                else if (dropInfo.TargetItem is IBaseConditionalCase conditionalCase)
                {
                    var link = new TextLinkModel(conditionalCase.BlockContainer.Items)
                    {
                        LinkedBlock = textDef
                    };
                    conditionalCase.BlockContainer.Items.Add(link);
                }
                return;
            }
            if (dropInfo.Data is OverlayBlockDefinitionItemModel overlayDef)
            {
                // Dropping the definition into a blockcontainer
                if (dropInfo.TargetCollection is IdeCollection<IdeBaseItem> collection && collection.Parent is BlockContainer container)
                {
                    var link = new OverlayLinkModel(container.Items)
                    {
                        LinkedBlock = overlayDef
                    };
                    container.AddLink(link, dropInfo.UnfilteredInsertIndex);
                }

                // Dropping the definition into a conditional case
                else if (dropInfo.TargetItem is IBaseConditionalCase conditionalCase)
                {
                    var link = new OverlayLinkModel(conditionalCase.BlockContainer.Items)
                    {
                        LinkedBlock = overlayDef,
                    };
                    conditionalCase.BlockContainer.AddLink(link, conditionalCase.BlockContainer.Items.Count);
                }
                return;
            }
            if (dropInfo.Data is OverlayLinkModel overlayLink)
            {
                if (IsOverlayHandler == false) return;
                if (dropInfo.TargetItem is IBaseConditionalCase conditionalCase)
                {
                    overlayLink.RemoveItem();
                    conditionalCase.BlockContainer.AddLink(overlayLink, conditionalCase.BlockContainer.Items.Count);
                }
                else if (dropInfo.TargetItem is IBaseBlock block)
                {
                    overlayLink.RemoveItem();
                    int insertIndex = dropInfo.UnfilteredInsertIndex;
                    (block.Source.Parent as BlockContainer).AddLink(overlayLink, insertIndex);
                }
            }
            else
            {
                DragDrop.DefaultDropHandler.Drop(dropInfo);
            }
        }
    }
}