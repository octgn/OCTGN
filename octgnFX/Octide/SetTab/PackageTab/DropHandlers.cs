﻿// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace Octide.SetTab.ItemModel
{
    public class PackageDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.VisualSource != dropInfo.VisualTarget)
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
            else if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter) && dropInfo.TargetItem is PackagePropertyModel)
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
            else if (dropInfo.DragInfo.SourceItem is PackagePropertyModel && dropInfo.DragInfo.SourceCollection.TryGetList().Count <= 1 && !dropInfo.KeyStates.HasFlag(System.Windows.DragDropKeyStates.ControlKey))
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

    public class IncludeDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.VisualSource != dropInfo.VisualTarget)
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
            else if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter) && dropInfo.TargetItem is PackagePropertyModel)
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