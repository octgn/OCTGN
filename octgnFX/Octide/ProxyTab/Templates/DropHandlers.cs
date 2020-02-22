// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Octide.ItemModel;
using System;
using System.Collections.ObjectModel;

namespace Octide.ProxyTab.TemplateItemModel
{
    public class TemplateMainDragHandler : IDragSource
    {
        public bool CanStartDrag(IDragInfo dragInfo)
        {
            if (!(dragInfo.SourceItem is IdeListBoxItemBase item))
            {
                return false;
            }
            else if (!item.CanDragDrop)
            {
                return false;
            }
            return DragDrop.DefaultDragHandler.CanStartDrag(dragInfo);
        }

        public void DragCancelled()
        {
            DragDrop.DefaultDragHandler.DragCancelled();
        }

        public void DragDropOperationFinished(System.Windows.DragDropEffects operationResult, IDragInfo dragInfo)
        {
            DragDrop.DefaultDragHandler.DragDropOperationFinished(operationResult, dragInfo);
        }

        public void Dropped(IDropInfo dropInfo)
        {
            DragDrop.DefaultDragHandler.Dropped(dropInfo);
        }

        public void StartDrag(IDragInfo dragInfo)
        {
            DragDrop.DefaultDragHandler.StartDrag(dragInfo);
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            return DragDrop.DefaultDragHandler.TryCatchOccurredException(exception);
        }
    }

    public class TemplateMainDropHandler : IDropTarget
    {
        public bool IsOverlayHandler { get; set; }
        public BlockContainer Container { get; set; }

        public void DragOver(IDropInfo dropInfo)
        {
            if (!(dropInfo.Data is IdeListBoxItemBase item))
            {
                return;
            }
            else if (!item.CanDragDrop)
            {
                return;
            }
            else if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter) && (dropInfo.TargetItem is OverlayLinkModel || dropInfo.TargetItem is TextLinkPropertyModel || dropInfo.TargetItem is ArtOverlayBlockModel))
            {
                //can't nest within these types
                return;
            }
            else if (dropInfo.Data is ProxyTextDefinitionItemModel)
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
                    else if (dropInfo.TargetCollection is ObservableCollection<IBaseBlock>)
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                        dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                    }
                }
            }
            else if (dropInfo.Data is ProxyOverlayDefinitionItemModel)
            {
                if (IsOverlayHandler)
                {
                    if (dropInfo.TargetItem is IBaseConditionalCase)
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                        dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                    }
                    else if (dropInfo.TargetCollection is ObservableCollection<IBaseBlock>)
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                        dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                    }
                }
            }
            else if (dropInfo.Data is ElseIfCaseModel)
            {
                if (dropInfo.TargetItem is ConditionalBlockModel)
                {
                    DragDrop.DefaultDropHandler.DragOver(dropInfo);
                }
                else if (dropInfo.TargetItem is IfCaseModel && dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.AfterTargetItem))
                {
                    DragDrop.DefaultDropHandler.DragOver(dropInfo);
                }
                else if (dropInfo.TargetItem is ElseIfCaseModel)
                {
                    DragDrop.DefaultDropHandler.DragOver(dropInfo);
                }
                else if (dropInfo.TargetItem is ElseCaseModel && dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.BeforeTargetItem))
                {
                    DragDrop.DefaultDropHandler.DragOver(dropInfo);
                }
            }
            else if (dropInfo.Data is SwitchCaseModel)
            {
                if (dropInfo.TargetItem is SwitchBlockModel)
                {
                    DragDrop.DefaultDropHandler.DragOver(dropInfo);
                }
                else if (dropInfo.TargetItem is SwitchCaseModel)
                {
                    DragDrop.DefaultDropHandler.DragOver(dropInfo);
                }
                else if (dropInfo.TargetItem is DefaultCaseModel && dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.BeforeTargetItem))
                {
                    DragDrop.DefaultDropHandler.DragOver(dropInfo);
                }
            }
            else if (dropInfo.DragInfo.VisualSource != dropInfo.VisualTarget)
            {
                return;
            }
            else
            {
                DragDrop.DefaultDropHandler.DragOver(dropInfo);
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ProxyTextDefinitionItemModel textDef)
            {
                if (IsOverlayHandler) return;
                if (dropInfo.TargetItem is TextLinkModel textLink && dropInfo.DropTargetAdorner == DropTargetAdorners.Highlight)
                {
                    // change the link definition of an existing text link item
                    textLink.LinkedBlock = textDef;
                }
                else
                {
                    var link = new TextLinkModel()
                    {
                        LinkedBlock = textDef
                    };
                    if (dropInfo.TargetItem is IBaseConditionalCase conditionalCase)
                    {
                        conditionalCase.BlockContainer.Items.Add(link);
                    }
                    else if (dropInfo.TargetItem is IBaseBlock block)
                    {
                        int insertIndex = dropInfo.UnfilteredInsertIndex;
                        (block.Parent as BlockContainer).Items.Insert(insertIndex, link);
                    }
                    else if (dropInfo.TargetItem == null)
                    {
                        Container.Items.Add(link);
                    }
                }
            }
            else if (dropInfo.Data is ProxyOverlayDefinitionItemModel overlayDef)
            {
                if (IsOverlayHandler == false) return;
                var link = new OverlayLinkModel()
                {
                    LinkedBlock = overlayDef,
                };
                if (dropInfo.TargetItem is IBaseConditionalCase conditionalCase)
                {
                    conditionalCase.BlockContainer.AddLink(link, conditionalCase.BlockContainer.Items.Count);
                }
                else if (dropInfo.TargetItem is IBaseBlock block)
                {
                    int insertIndex = dropInfo.UnfilteredInsertIndex;
                    (block.Parent as BlockContainer).AddLink(link, insertIndex);
                }
            }
            else if (dropInfo.Data is OverlayLinkModel overlayLink)
            {
                if (IsOverlayHandler == false) return;
                if (dropInfo.TargetItem is IBaseConditionalCase conditionalCase)
                {
                    conditionalCase.BlockContainer.AddLink(overlayLink, conditionalCase.BlockContainer.Items.Count);
                }
                else if (dropInfo.TargetItem is IBaseBlock block)
                {
                    int insertIndex = dropInfo.UnfilteredInsertIndex;
                    (block.Parent as BlockContainer).AddLink(overlayLink, insertIndex);
                }
            }
            else
            {
                DragDrop.DefaultDropHandler.Drop(dropInfo);
            }
        }
    }

    public class TemplateLinkContainerDropHandler : IDropTarget
    {
        public OverlayLinkContainer Container { get; set; }
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ProxyOverlayDefinitionItemModel item)
            {
                if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                 //   dropInfo.EffectText = String.Format("Replace {0}", ((dropInfo.TargetItem as OverlayLinkModel).LinkedBlock.Name));
                    dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                }
                else
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                 //   dropInfo.EffectText = String.Format("Insert {0}", item.Name);
                    dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                }

            }

            else if (dropInfo.TargetCollection == dropInfo.DragInfo.SourceCollection && dropInfo.TargetCollection.TryGetList().Count == 1)
            {
                //stops the linkcontainer from collapsing
                return;
            }
            else
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
                DragDrop.DefaultDropHandler.DragOver(dropInfo);
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ProxyOverlayDefinitionItemModel dropItem)
            {
                if (dropInfo.TargetItem is OverlayLinkModel overlayLink && dropInfo.DropTargetAdorner == DropTargetAdorners.Highlight)
                {
                    // change the link definition of an existing text link item
                    overlayLink.LinkedBlock = dropItem;
                }
                else
                {
                    var item = new OverlayLinkModel()
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

    public class TemplateMatchDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.VisualSource != dropInfo.VisualTarget)
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
            else if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter) && dropInfo.TargetItem is PackPropertyItemModel)
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
            else if (dropInfo.DragInfo.SourceItem is PackPropertyItemModel && dropInfo.DragInfo.SourceCollection.TryGetList().Count <= 1 && !dropInfo.KeyStates.HasFlag(System.Windows.DragDropKeyStates.ControlKey))
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
            else
            {
                DragDrop.DefaultDropHandler.DragOver(dropInfo);
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            DragDrop.DefaultDropHandler.Drop(dropInfo);
        }
    }
}