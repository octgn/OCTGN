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

namespace Octide.ViewModel
{
    public class GroupViewModel : ViewModelBase
    {
        private GroupItemModel _selectedItem;
        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();
        public IList<GroupVisibility> VisibilityOptions => Enum.GetValues(typeof(GroupVisibility)).Cast<GroupVisibility>().ToList();

        public RelayCommand CopyCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }

        public GroupViewModel()
        {
            CopyCommand = new RelayCommand(CopyItem);
            DeleteCommand = new RelayCommand(DeleteItem);
        }
        
        public void CopyItem()
        {

            if (SelectedItem == null) return;
            var ret = new GroupItemModel(SelectedItem);
            ViewModelLocator.PreviewTabViewModel.Piles.Add(ret);
        }

        public void DeleteItem()
        {
            if (SelectedItem == null) return;
            ViewModelLocator.PreviewTabViewModel.Piles.Remove(SelectedItem);
        }

        public GroupItemModel SelectedItem
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
            }
        }

    }

    public class GroupItemModel : ViewModelBase
    {
        public Group _group;
        public bool _isHand;

        public GroupItemModel()
        {
            _group = new Group();
            _group.Icon = ViewModelLocator.GroupViewModel.Images.First().FullPath;
            RaisePropertyChanged("Icon");
            RaisePropertyChanged("IconImage");
        }

        public GroupItemModel(Group g)
        {
            _group = g;
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