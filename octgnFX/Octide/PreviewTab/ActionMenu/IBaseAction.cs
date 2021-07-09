// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace Octide.ItemModel
{
    public abstract class IBaseAction : IdeBaseItem
    {
        public IGroupAction _action;
        public List<PythonFunctionDefItemModel> PythonFunctions => ViewModelLocator.PythonTabViewModel.PythonFunctions;

        public IBaseAction(IdeCollection<IdeBaseItem> source) : base(source)
        {

        }

        public static IBaseAction CreateActionItem(IGroupAction iGroupAction, IdeCollection<IdeBaseItem> source)
        {
            IBaseAction ret = null;
            if (iGroupAction is GroupAction action)
                ret = new ActionItemModel(action, source);
            else if (iGroupAction is GroupActionSubmenu submenu)
                ret = new ActionSubmenuItemModel(submenu, source);
            else if (iGroupAction is GroupActionSeparator separator)
                ret = new ActionSeparatorItemModel(separator, source);
            return ret;
        }

        public static IBaseAction CopyActionItems(IdeBaseItem baseAction, IdeCollection<IdeBaseItem> source)
        {
            IBaseAction ret = null;
            if (baseAction is ActionItemModel action)
                ret = new ActionItemModel(action, source);
            else if (baseAction is ActionSubmenuItemModel submenu)
                ret = new ActionSubmenuItemModel(submenu, source);
            else if (baseAction is ActionSeparatorItemModel separator)
                ret = new ActionSeparatorItemModel(separator, source);
            return ret;
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

        public bool IsGroup
        {
            get
            {
                return _action.IsGroup;
            }
            set
            {
                if (value == _action.IsGroup) return;
                _action.IsGroup = value;
                RaisePropertyChanged("IsGroup");
            }

        }

        public PythonFunctionDefItemModel HeaderExecute
        {
            get
            {
                return PythonFunctions.FirstOrDefault(x => x.Name == _action.HeaderExecute);
            }
            set
            {
                if (_action.HeaderExecute == value.Name) return;
                _action.HeaderExecute = value.Name;
                RaisePropertyChanged("HeaderExecute");
            }
        }

        public PythonFunctionDefItemModel ShowExecute
        {
            get
            {
                return PythonFunctions.FirstOrDefault(x => x.Name == _action.ShowExecute);
            }
            set
            {
                if (_action.ShowExecute == value.Name) return;
                _action.ShowExecute = value.Name;
                RaisePropertyChanged("ShowExecute");
            }
        }
        public void DeleteItem(ContentPresenter content)
        {
            ObservableCollection<IBaseAction> ParentList = ItemsControl.ItemsControlFromItemContainer(content.TemplatedParent as TreeViewItem).ItemsSource as ObservableCollection<IBaseAction>;
            ParentList.Remove(this);
        }
    }
}
