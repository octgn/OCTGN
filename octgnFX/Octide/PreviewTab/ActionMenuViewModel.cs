// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Media;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Command;

using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;
using System.Windows.Controls;
using Octide.ItemModel;

namespace Octide.ViewModel
{
    public class ActionMenuViewModel : ViewModelBase, IDropTarget
    {
        public RelayCommand NewActionCommand { get; private set; }
        public RelayCommand NewSubmenuCommand { get; private set; }
        public RelayCommand NewSeparatorCommand { get; private set; }

        public GroupItemViewModel Group { get; set; }
        public CardViewModel Card { get; set; }

        public string GroupHeader
        {
            get
            {
                if (Group == null) return null;
                return Group.Name;
            }
        }
        
        public Visibility GroupVisibility
        {
            get
            {
                if (Card == null || Group.Name != "Table") return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public string CardHeader
        {
            get
            {
                if (Card == null) return "Card";
                return Card.Size.Name;
            }
        }

        public Visibility CardVisibility
        {
            get
            {
                if (Card == null && Group.Name == "Table") return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public ActionMenuViewModel()
        {
            NewActionCommand = new RelayCommand(NewAction);
            NewSubmenuCommand = new RelayCommand(NewSubmenu);
            NewSeparatorCommand = new RelayCommand(NewSeparator);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter) && (dropInfo.TargetItem is ActionItemModel || dropInfo.TargetItem is ActionSeparatorItemModel))
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
            else
                DragDrop.DefaultDropHandler.DragOver(dropInfo);
        }

        public void Drop(IDropInfo dropInfo)
        {
            DragDrop.DefaultDropHandler.Drop(dropInfo);
            (dropInfo.Data as IBaseAction).Action.IsGroup = ((dropInfo.VisualTarget as TreeView).Name == "GroupActions") ? true : false;
        }


        private void NewAction()
        {
            Group.GroupActions.Add(new ActionItemModel() { Parent = Group.GroupActions });
        }

        private void NewSubmenu()
        {
            Group.GroupActions.Add(new ActionSubmenuItemModel() { Parent = Group.GroupActions });
        }

        private void NewSeparator()
        {
            Group.GroupActions.Add(new ActionSeparatorItemModel() { Parent = Group.GroupActions });
        }
    }
}