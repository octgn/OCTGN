// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GongSolutions.Wpf.DragDrop;
using Octgn.DataNew.Entities;
using Octide.Messages;
using System.Collections.Generic;
using System.Linq;

namespace Octide.ItemModel
{
    public class BaseGroupItemModel : IdeBaseItem
    {
        public Group _group;

        public IdeCollection<IdeBaseItem> GroupActions { get; set; }
        public IdeCollection<IdeBaseItem> CardActions { get; set; }
        public BaseGroupItemModel(IdeCollection<IdeBaseItem> source) : base(source) //new item
        {

            NewActionCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewAction);
            NewSubmenuCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewSubmenu);
            NewSeparatorCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewSeparator);
            ActionsDropHandler = new ActionsDropHandler();

            _group = new Group();
            GroupActions = new IdeCollection<IdeBaseItem>(this, typeof(IBaseAction));
            CardActions = new IdeCollection<IdeBaseItem>(this, typeof(IBaseAction));
            GroupActions.CollectionChanged += (a, b) =>
            {
                _group.GroupActions = GroupActions.Select(x => ((IBaseAction)x)._action);
            };
            CardActions.CollectionChanged += (a, b) =>
            {
                _group.CardActions = CardActions.Select(x => ((IBaseAction)x)._action);
            };
        }

        public BaseGroupItemModel(Group g, IdeCollection<IdeBaseItem> source) : base(source) //load item
        {

            NewActionCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewAction);
            NewSubmenuCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewSubmenu);
            NewSeparatorCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewSeparator);
            ActionsDropHandler = new ActionsDropHandler();

            _group = g;
            GroupActions = new IdeCollection<IdeBaseItem>(this, typeof(IBaseAction));
            foreach (var action in g.GroupActions)
            {
                GroupActions.Add(IBaseAction.CreateActionItem(action, GroupActions));
            }
            GroupActions.CollectionChanged += (a, b) =>
            {
                _group.GroupActions = GroupActions.Select(x => ((IBaseAction)x)._action);
            };
            CardActions = new IdeCollection<IdeBaseItem>(this, typeof(IBaseAction));
            foreach (var action in g.CardActions)
            {
                CardActions.Add(IBaseAction.CreateActionItem(action, CardActions));
            }
            CardActions.CollectionChanged += (a, b) =>
            {
                _group.CardActions = CardActions.Select(x => ((IBaseAction)x)._action);
            };
        }

        public BaseGroupItemModel(BaseGroupItemModel g, IdeCollection<IdeBaseItem> source) : base(source) //copy item
        {
            _group = new Group
            {
                Visibility = g.GroupVisibility,
            };
            Name = g.Name;

            NewActionCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewAction);
            NewSubmenuCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewSubmenu);
            NewSeparatorCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewSeparator);
            ActionsDropHandler = new ActionsDropHandler();

            GroupActions = new IdeCollection<IdeBaseItem>(this, typeof(IBaseAction));
            GroupActions.CollectionChanged += (a, b) =>
            {
                _group.GroupActions = GroupActions.Select(x => ((IBaseAction)x)._action);
            };
            foreach (var item in g.GroupActions)
                GroupActions.Add(IBaseAction.CopyActionItems(item, GroupActions));

            CardActions = new IdeCollection<IdeBaseItem>(this, typeof(IBaseAction));
            CardActions.CollectionChanged += (a, b) =>
            {
                _group.CardActions = CardActions.Select(x => ((IBaseAction)x)._action);
            };
            foreach (var item in g.CardActions)
                CardActions.Add(IBaseAction.CopyActionItems(item, CardActions));
        }

        public IEnumerable<string> UniqueNames => Source.Select(x => ((PileItemModel)x).Name);
        public string Name
        {
            get
            {
                return _group.Name;
            }
            set
            {
                if (_group.Name == value) return;
                _group.Name = Utils.GetUniqueName(value, UniqueNames);
                RaisePropertyChanged("Name");
                Messenger.Default.Send(new GroupChangedMessage() { Group = this, Action = PropertyChangedMessageAction.Modify });
            }
        }

        public GroupVisibility GroupVisibility
        {
            get
            {
                return _group.Visibility;
            }
            set
            {
                if (value == _group.Visibility) return;
                _group.Visibility = value;
                RaisePropertyChanged("GroupVisibility");
            }
        }


        #region ActionMenu

        public RelayCommand<IdeCollection<IdeBaseItem>> NewActionCommand { get; private set; }
        public RelayCommand<IdeCollection<IdeBaseItem>> NewSubmenuCommand { get; private set; }
        public RelayCommand<IdeCollection<IdeBaseItem>> NewSeparatorCommand { get; private set; }

        public ActionsDropHandler ActionsDropHandler { get; set; }

        private void NewAction(IdeCollection<IdeBaseItem> actions)
        {
            actions.Add(new ActionItemModel(actions));
        }

        private void NewSubmenu(IdeCollection<IdeBaseItem> actions)
        {
            actions.Add(new ActionSubmenuItemModel(actions));
        }

        private void NewSeparator(IdeCollection<IdeBaseItem> actions)
        {
            actions.Add(new ActionSeparatorItemModel(actions));
        }
        #endregion
    }
    public class ActionsDropHandler : IDropTarget
    {
        public void Drop(IDropInfo dropInfo)
        {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
            //   (dropInfo.Data as IBaseAction).ItemSource = ((IBaseAction)dropInfo.TargetItem).ItemSource;
            (dropInfo.Data as IBaseAction).Source = dropInfo.TargetCollection as IdeCollection<IdeBaseItem>;
            //   (dropInfo.Data as IBaseAction).IsGroup = ((dropInfo.VisualTarget as TreeView).Name == "GroupActions") ? true : false;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter) && (dropInfo.TargetItem is ActionItemModel || dropInfo.TargetItem is ActionSeparatorItemModel))
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
            else
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
        }

        public void DragEnter(IDropInfo dropInfo) {
            
        }

        public void DragLeave(IDropInfo dropInfo) {
            
        }
    }
}
