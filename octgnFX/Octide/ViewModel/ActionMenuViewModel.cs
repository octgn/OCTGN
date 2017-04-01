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

namespace Octide.ViewModel
{
    public class ActionMenuViewModel : ViewModelBase, IDropTarget
    {
        public RelayCommand NewActionCommand { get; private set; }
        public RelayCommand NewSubmenuCommand { get; private set; }
        public RelayCommand NewSeparatorCommand { get; private set; }

        public GroupItemModel group { get; set; }
        public CardViewModel card { get; set; }

        public string GroupHeader
        {
            get
            {
                if (group == null) return null;
                return group.Name;
            }
        }
        
        public Visibility GroupVisibility
        {
            get
            {
                if (card == null || group.Name != "Table") return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public string CardHeader
        {
            get
            {
                if (card == null) return "Card";
                return card.Size.Name;
            }
        }

        public Visibility CardVisibility
        {
            get
            {
                if (card == null && group.Name == "Table") return Visibility.Collapsed;
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
            group.GroupActions.Add(new ActionItemModel());
        }

        private void NewSubmenu()
        {
            group.GroupActions.Add(new ActionSubmenuItemModel());
        }

        private void NewSeparator()
        {
            group.GroupActions.Add(new ActionSeparatorItemModel());
        }
    }
}