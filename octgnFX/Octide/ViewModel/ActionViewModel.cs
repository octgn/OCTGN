using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Command;

using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Octide.ItemModel;

namespace Octide.ViewModel
{
    public class ActionViewModel : ViewModelBase
    {
        private IBaseAction _selectedItem;
        private ActionItemModel _default;

        public GroupItemModel group { get; set; }

        public ActionViewModel()
        {
        }


        public Visibility ActionTypeVisibility
        {
            get
            {
                return (SelectedItem is ActionItemModel ? Visibility.Visible : Visibility.Collapsed);
            }
        }

        public Visibility SeparatorNameVisibility
        {
            get
            {
                return (SelectedItem is ActionSeparatorItemModel ? Visibility.Collapsed : Visibility.Visible);
            }
        }

        public IBaseAction SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
                RaisePropertyChanged("ActionTypeVisibility");
                RaisePropertyChanged("SeparatorNameVisibility");
            }
        }

        public IBaseAction CreateActionItem(IGroupAction a, ObservableCollection<IBaseAction> p)
        {
            IBaseAction action = null;
            if (a is GroupAction)
                action = new ActionItemModel((GroupAction)a) { Parent = p };
            else if (a is GroupActionGroup)
                action = new ActionSubmenuItemModel((GroupActionGroup)a) { Parent = p };
            else if (a is GroupActionSeparator)
                action = new ActionSeparatorItemModel((GroupActionSeparator)a) { Parent = p };
            return action;
        }

        public IBaseAction CopyActionItems(IBaseAction a)
        {
            IBaseAction action = null;
            if (a is ActionItemModel)
                action = new ActionItemModel((ActionItemModel)a);
            else if (a is ActionSubmenuItemModel)
                action = new ActionSubmenuItemModel((ActionSubmenuItemModel)a);
            else if (a is ActionSeparatorItemModel)
                action = new ActionSeparatorItemModel((ActionSeparatorItemModel)a);
            return action;
        }

        public ActionItemModel Default
        {
            get
            {
                return _default;
            }
            set
            {
                if (_default == value) return;
                if (_default != null) ((GroupAction)_default.Action).DefaultAction = false;
                _default = value;
                RaisePropertyChanged("Default");
            }
        }
    }
}
