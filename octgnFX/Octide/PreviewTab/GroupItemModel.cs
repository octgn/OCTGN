// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GongSolutions.Wpf.DragDrop;
using Octgn.DataNew.Entities;
using Octide.Messages;
using Octide.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Octide.ItemModel
{
    public class GroupItemModel : IdeBaseItem
    {
        public Group _group;
        public bool _isHand;

        public IdeCollection<IdeBaseItem> GroupActions { get; set; }
        public IdeCollection<IdeBaseItem> CardActions { get; set; }
        public AssetController Asset { get; set; }

        public GroupItemModel(IdeCollection<IdeBaseItem> source) : base(source) //new item
        {
            NewActionCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewAction);
            NewSubmenuCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewSubmenu);
            NewSeparatorCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewSeparator);
            ActionsDropHandler = new ActionsDropHandler();

            _group = new Group();
            GroupActions = new IdeCollection<IdeBaseItem>(this);
            CardActions = new IdeCollection<IdeBaseItem>(this);
            GroupActions.CollectionChanged += (a, b) =>
            {
                _group.GroupActions = GroupActions.Select(x => ((IBaseAction)x)._action);
            };
            CardActions.CollectionChanged += (a, b) =>
            {
                _group.CardActions = CardActions.Select(x => ((IBaseAction)x)._action);
            };
            Asset = new AssetController(AssetType.Image);
            _group.Icon = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
            Name = "New Group";
            RaisePropertyChanged("Asset");
        }

        public GroupItemModel(Group g, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            NewActionCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewAction);
            NewSubmenuCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewSubmenu);
            NewSeparatorCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewSeparator);
            ActionsDropHandler = new ActionsDropHandler();

            _group = g;
            GroupActions = new IdeCollection<IdeBaseItem>(this);
            foreach (var action in g.GroupActions)
            {
                GroupActions.Add(IBaseAction.CreateActionItem(action, GroupActions));
            }
            GroupActions.CollectionChanged += (a, b) =>
            {
                _group.GroupActions = GroupActions.Select(x => ((IBaseAction)x)._action);
            };
            CardActions = new IdeCollection<IdeBaseItem>(this);
            foreach (var action in g.CardActions)
            {
                CardActions.Add(IBaseAction.CreateActionItem(action, CardActions));
            }
            CardActions.CollectionChanged += (a, b) =>
            {
                _group.CardActions = CardActions.Select(x => ((IBaseAction)x)._action);
            };
            Asset = new AssetController(AssetType.Image, g.Icon);
            Asset.PropertyChanged += AssetUpdated;
        }

        public GroupItemModel(GroupItemModel g, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            NewActionCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewAction);
            NewSubmenuCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewSubmenu);
            NewSeparatorCommand = new RelayCommand<IdeCollection<IdeBaseItem>>(NewSeparator);
            ActionsDropHandler = new ActionsDropHandler();

            _group = new Group
            {
                Visibility = g.GroupVisibility,
                Shortcut = g.Shortcut,
                Ordered = g.Ordered,
                MoveTo = g.MoveTo,
                ViewState = g.ViewState
            };

            Name = g.Name;

            GroupActions = new IdeCollection<IdeBaseItem>(this);
            GroupActions.CollectionChanged += (a, b) =>
            {
                _group.GroupActions = GroupActions.Select(x => ((IBaseAction)x)._action);
            };
            foreach (var item in g.GroupActions)
                GroupActions.Add(IBaseAction.CopyActionItems(item, GroupActions));

            CardActions = new IdeCollection<IdeBaseItem>(this);
            CardActions.CollectionChanged += (a, b) =>
            {
                _group.CardActions = CardActions.Select(x => ((IBaseAction)x)._action);
            };
            foreach (var item in g.CardActions)
                CardActions.Add(IBaseAction.CopyActionItems(item, CardActions));
            
            Asset = new AssetController(AssetType.Image, g._group.Icon);
            _group.Icon = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
        }

        private void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedAsset")
            {
                _group.Icon = Asset.FullPath;
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("Icon");
            }
        }
        public override void Cleanup()
        {
            Asset.SelectedAsset = null;
            base.Cleanup();
        }

        public override object Clone()
        {
            return new GroupItemModel(this, Source);
        }
        public override object Create()
        {
            return new GroupItemModel(Source);
        }

        public IEnumerable<string> UniqueNames => Source.Select(x => ((GroupItemModel)x).Name);
        public new string Icon => Asset.FullPath;

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

        public GroupViewState ViewState
        {
            get
            {
                return _group.ViewState;
            }
            set
            {
                if (_group.ViewState == value) return;
                _group.ViewState = value;
                RaisePropertyChanged("ViewState");
            }
        }

        public bool MoveTo
        {

            get
            {
                return _group.MoveTo;
            }
            set
            {
                if (value == _group.MoveTo) return;
                _group.MoveTo = value;
                RaisePropertyChanged("MoveTo");
            }
        }

        public bool Ordered
        {

            get
            {
                return _group.Ordered;
            }
            set
            {
                if (value == _group.Ordered) return;
                _group.Ordered = value;
                RaisePropertyChanged("Ordered");
            }
        }


        public string Shortcut
        {
            get
            {
                return _group.Shortcut;
            }
            set
            {
                if (value == _group.Shortcut) return;
                _group.Shortcut = value;
                RaisePropertyChanged("Shortcut");
            }
        }

        #region ActionMenu

        public RelayCommand<IdeCollection<IdeBaseItem>> NewActionCommand { get; private set; }
        public RelayCommand<IdeCollection<IdeBaseItem>> NewSubmenuCommand { get; private set; }
        public RelayCommand<IdeCollection<IdeBaseItem>> NewSeparatorCommand { get; private set; }

        public ActionsDropHandler ActionsDropHandler { get; set; }

        private void NewAction(IdeCollection<IdeBaseItem> actions)
        {
            actions.Add(new ActionItemModel(actions) );
        }

        private void NewSubmenu(IdeCollection<IdeBaseItem> actions)
        {
            actions.Add(new ActionSubmenuItemModel(actions) );
        }

        private void NewSeparator(IdeCollection<IdeBaseItem> actions)
        {
            actions.Add(new ActionSeparatorItemModel(actions) );
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
    }
}

