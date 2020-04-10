// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Octide.ItemModel
{
    public abstract class IBaseAction : IdeBaseItem
    {
        public IGroupAction _action;

        public IBaseAction(IdeCollection<IdeBaseItem> source) : base(source)
        {

        }

        public static IBaseAction CreateActionItem(IGroupAction action, IdeCollection<IdeBaseItem> source)
        {
            IBaseAction ret = null;
            if (action is GroupAction)
                ret = new ActionItemModel((GroupAction)action, source);
            else if (action is GroupActionGroup)
                ret = new ActionSubmenuItemModel((GroupActionGroup)action, source);
            else if (action is GroupActionSeparator)
                ret = new ActionSeparatorItemModel((GroupActionSeparator)action, source);
            return ret;
        }

        public static IBaseAction CopyActionItems(IdeBaseItem action, IdeCollection<IdeBaseItem> source)
        {
            IBaseAction ret = null;
            if (action is ActionItemModel)
                ret = new ActionItemModel((ActionItemModel)action, source);
            else if (action is ActionSubmenuItemModel)
                ret = new ActionSubmenuItemModel((ActionSubmenuItemModel)action, source);
            else if (action is ActionSeparatorItemModel)
                ret = new ActionSeparatorItemModel((ActionSeparatorItemModel)action, source);
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

        public string HeaderExecute
        {
            get
            {
                return _action.HeaderExecute;
            }
            set
            {
                if (value == _action.HeaderExecute) return;
                _action.HeaderExecute = value;
                RaisePropertyChanged("HeaderExecute");
            }
        }

        public string ShowExecute
        {
            get
            {
                return _action.ShowExecute;
            }
            set
            {
                if (value == _action.ShowExecute) return;
                _action.ShowExecute = value;
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
