// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Octide.ItemModel
{
    public interface IBaseAction
    {
        IGroupAction Action { get; set; }
        bool IsExpanded { get; set; }
    }


    public class ActionItemModel : IdeListBoxItemBase, IBaseAction
    {

        public IGroupAction _action;
        public bool _batch;

        public ActionItemModel()
        {
            Action = new GroupAction() { Name = "Action", IsGroup = true, };
        }

        public ActionItemModel(GroupAction a)
        {
            Action = a;
            if (a.DefaultAction) IsDefault = true;
            if (a.BatchExecute != null) _batch = true;
        }

        public ActionItemModel(ActionItemModel a)
        {
            Action = new GroupAction
            {
                BatchExecute = ((GroupAction)a.Action).BatchExecute,
                DefaultAction = ((GroupAction)a.Action).DefaultAction,
                Execute = ((GroupAction)a.Action).Execute,
                HeaderExecute = ((GroupAction)a.Action).HeaderExecute,
                IsGroup = ((GroupAction)a.Action).IsGroup,
                Name = ((GroupAction)a.Action).Name,
                Shortcut = ((GroupAction)a.Action).Shortcut,
                ShowExecute = ((GroupAction)a.Action).ShowExecute
            };
            Parent = a.Parent;
            ItemSource = a.ItemSource;
            if (a.Batch) _batch = true;
        }

        public void DeleteItem(ContentPresenter content)
        {
            ObservableCollection<IBaseAction> ParentList = ItemsControl.ItemsControlFromItemContainer(content.TemplatedParent as TreeViewItem).ItemsSource as ObservableCollection<IBaseAction>;
            ParentList.Remove(this);
        }

        public override object Clone()
        {
            return new ActionItemModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = (Parent as ObservableCollection<IBaseAction>).IndexOf(this);
            (Parent as ObservableCollection<IBaseAction>).Insert(index, Clone() as ActionItemModel);
        }

        public override void Remove()
        {
            if (CanRemove == false) return;
            (Parent as ObservableCollection<IBaseAction>).Remove(this);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = (Parent as ObservableCollection<IBaseAction>).IndexOf(this);
            (Parent as ObservableCollection<IBaseAction>).Insert(index, new ActionItemModel() { Parent = Parent, ItemSource = ItemSource });
        }

        public bool IsExpanded
        {
            get { return false; }
            set { }
        }

        public new bool IsDefault
        {
            get
            {
                return ViewModelLocator.ActionViewModel.Default == this;
            }
            set
            {
                ViewModelLocator.ActionViewModel.Default = value == true ? this : null;
                ((GroupAction)Action).DefaultAction = value;
                RaisePropertyChanged("IsDefault");
            }
        }

        public IGroupAction Action
        {
            get
            {
                return _action;
            }
            set
            {
                if (_action == value) return;
                _action = value;
                RaisePropertyChanged("Action");
            }
        }

        public string Name
        {
            get
            {
                return ((GroupAction)Action).Name;
            }
            set
            {
                if (value == ((GroupAction)Action).Name) return;
                ((GroupAction)Action).Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public string Shortcut
        {
            get
            {
                return ((GroupAction)Action).Shortcut;
            }
            set
            {
                if (value == ((GroupAction)Action).Shortcut) return;
                ((GroupAction)Action).Shortcut = value;
                RaisePropertyChanged("Shortcut");
            }
        }


        public string Execute
        {
            get
            {
                return (Batch == true) ? ((GroupAction)Action).BatchExecute : ((GroupAction)Action).Execute;
            }
            set
            {
                if (Batch == true)
                {
                    if (value == ((GroupAction)Action).BatchExecute) return;
                    ((GroupAction)Action).BatchExecute = value;
                }
                else
                {
                    if (value == ((GroupAction)Action).Execute) return;
                    ((GroupAction)Action).Execute = value;
                }
                RaisePropertyChanged("Execute");
            }
        }

        public bool Batch
        {
            get
            {
                return _batch;
            }
            set
            {
                if (value == _batch) return;
                _batch = value;
                if (value == true)
                {
                    Execute = ((GroupAction)Action).Execute;
                    ((GroupAction)Action).Execute = null;
                }
                else
                {
                    Execute = ((GroupAction)Action).BatchExecute;
                    ((GroupAction)Action).BatchExecute = null;
                }
                RaisePropertyChanged("Batch");
            }
        }

        public string HeaderExecute
        {
            get
            {
                return ((GroupAction)Action).HeaderExecute;
            }
            set
            {
                if (value == ((GroupAction)Action).HeaderExecute) return;
                ((GroupAction)Action).HeaderExecute = value;
                RaisePropertyChanged("HeaderExecute");
            }
        }

        public string ShowExecute
        {
            get
            {
                return ((GroupAction)Action).ShowExecute;
            }
            set
            {
                if (value == ((GroupAction)Action).ShowExecute) return;
                ((GroupAction)Action).ShowExecute = value;
                RaisePropertyChanged("ShowExecute");
            }
        }
    }

    public class ActionSubmenuItemModel : IdeListBoxItemBase, IBaseAction
    {
        public IGroupAction _action;

        public ObservableCollection<IBaseAction> Items { get; set; }

        public RelayCommand<ContentPresenter> DeleteCommand { get; set; }

        private bool isExpanded;

        public bool IsExpanded
        {
            get
            { return isExpanded; }
            set
            {
                if (isExpanded == value) return;
                isExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }

        public ActionSubmenuItemModel()
        {
            Action = new GroupActionGroup() { Name = "Action", IsGroup = true };
            Items = new ObservableCollection<IBaseAction>();
            Items.CollectionChanged += (a, b) =>
            {
                ((GroupActionGroup)Action).Children = Items.Select(x => x.Action).ToList();
            };
            Items.Add(new ActionItemModel() { Parent = Items });
        }

        public ActionSubmenuItemModel(GroupActionGroup a)
        {
            Action = a;
            Items = new ObservableCollection<IBaseAction>();
            foreach (var action in a.Children)
            {
                Items.Add(ViewModelLocator.ActionViewModel.CreateActionItem(action, Items));
            }
            Items.CollectionChanged += (b, c) =>
            {
                ((GroupActionGroup)Action).Children = Items.Select(x => x.Action).ToList();
            };
        }

        public ActionSubmenuItemModel(ActionSubmenuItemModel a)
        {
            Action = new GroupActionGroup
            {

                HeaderExecute = ((GroupActionGroup)a.Action).HeaderExecute,
                IsGroup = ((GroupActionGroup)a.Action).IsGroup,
                Name = ((GroupActionGroup)a.Action).Name,
                ShowExecute = ((GroupActionGroup)a.Action).ShowExecute
            };
            Items = new ObservableCollection<IBaseAction>(a.Items.Select(x => ViewModelLocator.ActionViewModel.CopyActionItems(x)));
            ((GroupActionGroup)Action).Children = Items.Select(x => x.Action).ToList();
            Items.CollectionChanged += (b, c) =>
            {
                ((GroupActionGroup)Action).Children = Items.Select(x => x.Action).ToList();
            };
            Parent = a.Parent;
            ItemSource = a.ItemSource;
        }

        public void DeleteItem(ContentPresenter content)
        {
            ObservableCollection<IBaseAction> ParentList = ItemsControl.ItemsControlFromItemContainer(content.TemplatedParent as TreeViewItem).ItemsSource as ObservableCollection<IBaseAction>;
            ParentList.Remove(this);
        }

        public override object Clone()
        {
            return new ActionSubmenuItemModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = (Parent as ObservableCollection<IBaseAction>).IndexOf(this);
            (Parent as ObservableCollection<IBaseAction>).Insert(index, Clone() as ActionItemModel);
        }

        public override void Remove()
        {
            if (CanRemove == false) return;
            (Parent as ObservableCollection<IBaseAction>).Remove(this);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = (Parent as ObservableCollection<IBaseAction>).IndexOf(this);
            (Parent as ObservableCollection<IBaseAction>).Insert(index, new ActionItemModel() { Parent = Parent, ItemSource = ItemSource });
        }

        public IGroupAction Action
        {
            get
            {
                return _action;
            }
            set
            {
                if (_action == value) return;
                _action = value;
                RaisePropertyChanged("Action");
            }
        }

        public string Name
        {
            get
            {
                return _action.Name;
            }
            set
            {
                if (value == _action.Name) return;
                _action.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public string HeaderExecute
        {
            get
            {
                return ((GroupActionGroup)Action).HeaderExecute;
            }
            set
            {
                if (value == ((GroupAction)Action).HeaderExecute) return;
                ((GroupActionGroup)Action).HeaderExecute = value;
                RaisePropertyChanged("HeaderExecute");
            }
        }

        public string ShowExecute
        {
            get
            {
                return ((GroupActionGroup)Action).ShowExecute;
            }
            set
            {
                if (value == ((GroupActionGroup)Action).ShowExecute) return;
                ((GroupActionGroup)Action).ShowExecute = value;
                RaisePropertyChanged("ShowExecute");
            }
        }
    }

    public class ActionSeparatorItemModel : IdeListBoxItemBase, IBaseAction
    {
        public IGroupAction _action;

        public RelayCommand<ContentPresenter> DeleteCommand { get; set; }


        public ActionSeparatorItemModel()
        {
            Action = new GroupActionSeparator() { Name = "Action", IsGroup = true };
        }

        public ActionSeparatorItemModel(GroupActionSeparator a)
        {
            Action = a;
        }

        public ActionSeparatorItemModel(ActionSeparatorItemModel a)
        {
            Action = new GroupActionSeparator()
            {
                HeaderExecute = ((GroupActionSeparator)a.Action).HeaderExecute,
                IsGroup = ((GroupActionSeparator)a.Action).IsGroup,
                Name = ((GroupActionSeparator)a.Action).Name,
                ShowExecute = ((GroupActionSeparator)a.Action).ShowExecute
            };
            Parent = a.Parent;
            ItemSource = a.ItemSource;
        }

        public void DeleteItem(ContentPresenter content)
        {
            ObservableCollection<IBaseAction> ParentList = ItemsControl.ItemsControlFromItemContainer(content.TemplatedParent as TreeViewItem).ItemsSource as ObservableCollection<IBaseAction>;
            ParentList.Remove(this);
        }

        public override object Clone()
        {
            return new ActionSeparatorItemModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = (Parent as ObservableCollection<IBaseAction>).IndexOf(this);
            (Parent as ObservableCollection<IBaseAction>).Insert(index, Clone() as ActionItemModel);
        }

        public override void Remove()
        {
            if (CanRemove == false) return;
            (Parent as ObservableCollection<IBaseAction>).Remove(this);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = (Parent as ObservableCollection<IBaseAction>).IndexOf(this);
            (Parent as ObservableCollection<IBaseAction>).Insert(index, new ActionItemModel() { Parent = Parent, ItemSource = ItemSource });
        }

        public bool IsExpanded
        {
            get { return false; }
            set { }
        }

        public IGroupAction Action
        {
            get
            {
                return _action;
            }
            set
            {
                if (_action == value) return;
                _action = value;
                RaisePropertyChanged("Action");
            }
        }
        public string HeaderExecute
        {
            get
            {
                return ((GroupActionSeparator)Action).HeaderExecute;
            }
            set
            {
                if (value == ((GroupActionSeparator)Action).HeaderExecute) return;
                ((GroupActionSeparator)Action).HeaderExecute = value;
                RaisePropertyChanged("HeaderExecute");
            }
        }

        public string ShowExecute
        {
            get
            {
                return ((GroupActionSeparator)Action).ShowExecute;
            }
            set
            {
                if (value == ((GroupActionSeparator)Action).ShowExecute) return;
                ((GroupActionSeparator)Action).ShowExecute = value;
                RaisePropertyChanged("ShowExecute");
            }
        }

    }
}
