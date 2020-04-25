// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GongSolutions.Wpf.DragDrop;
using System;

namespace Octide.ProxyTab.Handlers
{
    public class TemplateMainDragHandler : IDragSource
    {
        public bool CanStartDrag(IDragInfo dragInfo)
        {
            if (dragInfo.SourceItem is IdeBaseItem item && item.CanDragDrop)
            {
                return DragDrop.DefaultDragHandler.CanStartDrag(dragInfo);
            }
            return false;
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
}