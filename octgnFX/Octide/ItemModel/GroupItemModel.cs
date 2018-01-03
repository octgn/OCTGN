using GalaSoft.MvvmLight;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octide.ItemModel
{
    public class GroupItemModel : ViewModelBase, ICloneable
    {
        public Group _group;
        public bool _isHand;
        public ObservableCollection<IBaseAction> _groupActions;
        public ObservableCollection<IBaseAction> _cardActions;

        public GroupItemModel()
        {
            _group = new Group();
            _group.Icon = ViewModelLocator.GroupViewModel.Images.First().FullPath;
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
            RaisePropertyChanged("Icon");
            RaisePropertyChanged("IconImage");
        }

        public GroupItemModel(Group g)
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

        public object Clone()
        {
            return new GroupItemModel(this);
        }

        public GroupItemModel(GroupItemModel g)
        {
            _group = new Group();
            _group.Icon = g.Icon.FullPath;
            _group.Collapsed = g.Collapsed;
            _group.Visibility = g.Visibility;
            _group.Shortcut = g.Shortcut;
            _group.Ordered = g.Ordered;
            _group.Name = g.Name;
            _group.MoveTo = g.MoveTo;
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
                _group.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public Asset Icon
        {
            get
            {
                if (_group.Icon == null)
                    return new Asset();
                return Asset.Load(_group.Icon);
            }
            set
            {
                if (value == null) return;
                if (!File.Exists(value.FullPath)) return;
                _group.Icon = value.FullPath;
                RaisePropertyChanged("Icon");
                RaisePropertyChanged("IconImage");
            }
        }

        public string IconImage => _group.Icon;

        public GroupVisibility Visibility
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
