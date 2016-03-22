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
    public class GroupTabViewModel : ViewModelBase
    {
        private Visibility _panelVisibility;
        private Game _game;
        private GroupListItemModel _selectedItem;
        public ObservableCollection<GroupListItemModel> Items { get; set; }
        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();
        public IList<GroupVisibility> VisibilityOptions => Enum.GetValues(typeof(GroupVisibility)).Cast<GroupVisibility>().ToList();


        public RelayCommand AddCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }
        public RelayCommand UpCommand { get; private set; }
        public RelayCommand DownCommand { get; private set; }
        public RelayCommand HandCommand { get; private set; }


        public GroupTabViewModel()
        {
            _game = ViewModelLocator.GameLoader.Game;
            AddCommand = new RelayCommand(AddItem);
            RemoveCommand = new RelayCommand(RemoveItem, EnableButton);
            UpCommand = new RelayCommand(MoveItemUp, EnableButton);
            DownCommand = new RelayCommand(MoveItemDown, EnableButton);
            HandCommand = new RelayCommand(SetHand, EnableButton);
            PanelVisibility = Visibility.Collapsed;
            Items = new ObservableCollection<GroupListItemModel>(_game.Player.Groups.Select(x => new GroupListItemModel(x)));
            Items.CollectionChanged += (a, b) => {
                _game.Player.Groups = Items.Select(x => x._group).ToList();
            };
        }
        
        public GroupListItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                if (value == null) PanelVisibility = Visibility.Collapsed;
                else PanelVisibility = Visibility.Visible;
                RaisePropertyChanged("SelectedItem");
            }
        }

        public Visibility PanelVisibility
        {
            get { return _panelVisibility; }
            set
            {
                if (value == _panelVisibility) return;
                _panelVisibility = value;
                RaisePropertyChanged("PanelVisibility");
            }
        }

        public bool EnableButton()
        {
            return _selectedItem != null;
        }

        public void AddItem()
        {
            var ret = new GroupListItemModel() { Name = "Group", Icon = null };
            Items.Add(ret);
            SelectedItem = ret;
        }

        public void RemoveItem()
        {
            Items.Remove(SelectedItem);
        }

        public void MoveItemUp()
        {
            MoveItem(-1);
        }

        public void MoveItemDown()
        {
            MoveItem(1);
        }

        public void MoveItem(int move)
        {
            var index = Items.IndexOf(SelectedItem);
            int newIndex = index + move;
            if (newIndex < 0 || newIndex >= Items.Count) return;
            Items.Move(index, index + move);
        }

        public void SetHand()
        {
            foreach (var g in Items)
            {
                if (g == SelectedItem) g.IsHand = !g.IsHand;
                else g.IsHand = false;
            }
        }
    }

    public class GroupListItemModel : ViewModelBase
    {
        public Group _group;
        private string _iconImage;
        public bool _isHand;

        public GroupListItemModel()
        {
            _group = new Group();
        }

        public GroupListItemModel(Group g)
        {
            _group = g;
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
                _iconImage = Asset.Load(_group.Icon).FullPath;
                RaisePropertyChanged("Icon");
                RaisePropertyChanged("IconImage");
            }
        }

        public string IconImage => _iconImage;

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