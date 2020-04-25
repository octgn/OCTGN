// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GongSolutions.Wpf.DragDrop;

namespace Octide.SetTab.ItemModel
{

    public class PackageDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.VisualSource != dropInfo.VisualTarget) //Prevents items from outside the list to be dropped
            {
                return;
            }

            if (!(dropInfo.Data is IdeBaseItem sourceItem))
            {
                return;
            }
            else if (!sourceItem.CanDragDrop)
            {
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
            DragDrop.DefaultDropHandler.Drop(dropInfo);
        }
    }
}
