// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.ItemModel
{
    public class GroupItemViewModel : IdeListBoxItemBase
    {
        public Group _group;
        public bool _isHand;
        public ObservableCollection<IBaseAction> _groupActions;
        public ObservableCollection<IBaseAction> _cardActions;

        public GroupItemViewModel()
        {
            _group = new Group
            {
                Icon = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath,
                Name = "New Pile"
            };
            GroupActions = new ObservableCollection<IBaseAction>();
            CardActions = new ObservableCollection<IBaseAction>();
            GroupActions.CollectionChanged += (a, b) =>
            {
                _group.GroupActions = GroupActions.Select(x => x.Action).ToList();
            };
            CardActions.CollectionChanged += (a, b) =>
            {
                _group.CardActions = CardActions.Select(x => x.Action).ToList();
            };
            RaisePropertyChanged("Asset");
        }

        public GroupItemViewModel(Group g)
        {
            _group = g;
            GroupActions = new ObservableCollection<IBaseAction>();
            foreach (var action in g.GroupActions)
            {
                GroupActions.Add(ViewModelLocator.ActionViewModel.CreateActionItem(action, GroupActions));
            }
            GroupActions.CollectionChanged += (a, b) =>
            {
                _group.GroupActions = GroupActions.Select(x => x.Action).ToList();
            };
            CardActions = new ObservableCollection<IBaseAction>();
            foreach (var action in g.CardActions)
            {
                CardActions.Add(ViewModelLocator.ActionViewModel.CreateActionItem(action, CardActions));
            }
            CardActions.CollectionChanged += (a, b) =>
            {
                _group.CardActions = CardActions.Select(x => x.Action).ToList();
            };
        }

        public GroupItemViewModel(GroupItemViewModel g) // copy item
        {
            _group = new Group
            {
                Icon = g.Asset.FullPath,
                Collapsed = g.Collapsed,
                Visibility = g.GroupVisibility,
                Shortcut = g.Shortcut,
                Ordered = g.Ordered,
                Name = g.Name,
                MoveTo = g.MoveTo
            };
            GroupActions = new ObservableCollection<IBaseAction>(g.GroupActions.Select(x => ViewModelLocator.ActionViewModel.CopyActionItems(x)));
            CardActions = new ObservableCollection<IBaseAction>(g.CardActions.Select(x => ViewModelLocator.ActionViewModel.CopyActionItems(x)));
            GroupActions.CollectionChanged += (a, b) =>
            {
                _group.GroupActions = GroupActions.Select(x => x.Action).ToList();
            };
            _cardActions.CollectionChanged += (a, b) =>
            {
                _group.CardActions = CardActions.Select(x => x.Action).ToList();
            };
            _group.GroupActions = GroupActions.Select(x => x.Action).ToList();
            _group.CardActions = CardActions.Select(x => x.Action).ToList();
            ItemSource = g.ItemSource;
            Parent = g.Parent;
        }

        public override object Clone()
        {
            return new GroupItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as GroupItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new GroupItemViewModel() { Parent = Parent, ItemSource = ItemSource });
        }

        public ObservableCollection<IBaseAction> GroupActions
        {
            get
            {
                return _groupActions;
            }
            set
            {
                if (_groupActions == value) return;
                _groupActions = value;
                RaisePropertyChanged("GroupActions");
            }
        }

        public ObservableCollection<IBaseAction> CardActions
        {
            get
            {
                return _cardActions;
            }
            set
            {
                if (_cardActions == value) return;
                _cardActions = value;
                RaisePropertyChanged("CardActions");
            }
        }

        public bool IsHand
        {
            get
            {
                return _isHand;
            }
            set
            {
                if (value == _isHand) return;
                _isHand = value;
                RaisePropertyChanged("IsHand");
            }
        }

        public string Name
        {
            get
            {
                return _group.Name;
            }
            set
            {
                if (value == _group.Name) return;
                if (string.IsNullOrEmpty(value)) return;
                _group.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public Asset Asset
        {
            get
            {
                return Asset.Load(_group.Icon);
            }
            set
            {
                _group.Icon = value?.FullPath;
                RaisePropertyChanged("Asset");
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
                RaisePropertyChanged("Visibility");
            }
        }

        public bool Collapsed
        {

            get
            {
                return _group.Collapsed;
            }
            set
            {
                if (value == _group.Collapsed) return;
                _group.Collapsed = value;
                RaisePropertyChanged("Collapsed");
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
    }
}
