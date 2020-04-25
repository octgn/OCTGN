// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace Octide.ItemModel
{
    public class ActionSubmenuItemModel : IBaseAction
    {


        public IdeCollection<IdeBaseItem> Items { get; private set; }

        public ActionSubmenuItemModel(IdeCollection<IdeBaseItem> source) : base(source) //new item
        {
            _action = new GroupActionSubmenu();
            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                ((GroupActionSubmenu)_action).Children = Items.Select(x => ((ActionSubmenuItemModel)x)._action);
            };
            Name = "New Submenu";
            Items.Add(new ActionItemModel(Items));
        }

        public ActionSubmenuItemModel(GroupActionSubmenu a, IdeCollection<IdeBaseItem> source) : base(source)  //load item
        {
            _action = a;
            Items = new IdeCollection<IdeBaseItem>(this);
            foreach (var action in a.Children)
            {
                Items.Add(CreateActionItem(action, Items));
            }
            Items.CollectionChanged += (b, c) =>
            {
                ((GroupActionSubmenu)_action).Children = Items.Select(x => ((ActionSubmenuItemModel)x)._action);
            };
        }

        public ActionSubmenuItemModel(ActionSubmenuItemModel a, IdeCollection<IdeBaseItem> source) : base(source)  //copy item
        {
            _action = new GroupActionSubmenu
            {

                HeaderExecute = a._action.HeaderExecute,
                IsGroup = a.IsGroup,
                Name = a.Name,
                ShowExecute = a._action.ShowExecute
            };
            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (b, c) =>
            {
                ((GroupActionSubmenu)_action).Children = Items.Select(x => ((ActionSubmenuItemModel)x)._action);
            };
            foreach (ActionSubmenuItemModel action in a.Items)
            {
                Items.Add(CopyActionItems(action, Items));
            };
        }

        public override object Clone()
        {
            return new ActionSubmenuItemModel(this, Source);
        }
        
        public override object Create()
        {
            return new ActionSubmenuItemModel(Source);
        }

        private bool _isExpanded;

        public bool IsExpanded
        {
            get
            { return _isExpanded; }
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }
    }
}
