using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Octide.ViewModel
{
    public class PropertyTabViewModel : ViewModelBase
    {

        private Visibility _panelVisibility;
        private Game _game;
        private PropertyListItemModel _selectedItem;
        public ObservableCollection<PropertyListItemModel> Items { get; private set; }
        public IList<PropertyType> TypeOptions => Enum.GetValues(typeof(PropertyType)).Cast<PropertyType>().ToList();
        public IList<PropertyTextKind> TextKindOptions => Enum.GetValues(typeof(PropertyTextKind)).Cast<PropertyTextKind>().ToList();

        public RelayCommand AddCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }
        public RelayCommand UpCommand { get; private set; }
        public RelayCommand DownCommand { get; private set; }


        public PropertyTabViewModel()
        {
            _game = ViewModelLocator.GameLoader.Game;
            AddCommand = new RelayCommand(AddItem);
            RemoveCommand = new RelayCommand(RemoveItem, EnableButton);
            UpCommand = new RelayCommand(MoveItemUp, EnableButton);
            DownCommand = new RelayCommand(MoveItemDown, EnableButton);
            PanelVisibility = Visibility.Collapsed;
            Items = new ObservableCollection<PropertyListItemModel>(_game.CustomProperties.Select(x => new PropertyListItemModel(x)));
            Items.CollectionChanged += (a, b) => {
                _game.CustomProperties = Items.Select(x => x._property).ToList();
            };
        }
        
        public PropertyListItemModel SelectedItem
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
            var ret = new PropertyListItemModel() { Name = "Property" };
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
            var index = Items.IndexOf(_selectedItem);
            int newIndex = index + move;
            if (newIndex < 0 || newIndex >= Items.Count) return;
            Items.Move(index, index + move);
        }
    }

    public class PropertyListItemModel : ViewModelBase
    {
        public PropertyDef _property;

        public PropertyListItemModel()
        {
            _property = new PropertyDef();
        }

        public PropertyListItemModel(PropertyDef p)
        {
            _property = p;
        }

        public string Name
        {
            get
            {
                return _property.Name;
            }
            set
            {
                if (value == _property.Name) return;
                _property.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public PropertyType Type
        {
            get
            {
                return _property.Type;
            }
            set
            {
                if (value == _property.Type) return;
                _property.Type = value;
                RaisePropertyChanged("Type");
            }
        }

        public PropertyTextKind TextKind
        {
            get
            {
                return _property.TextKind;
            }
            set
            {
                if (value == _property.TextKind) return;
                _property.TextKind = value;
                RaisePropertyChanged("TextKind");
            }
        }
        
        public bool Hidden
        {

            get
            {
                return _property.Hidden;
            }
            set
            {
                if (value == _property.Hidden) return;
                _property.Hidden = value;
                RaisePropertyChanged("Hidden");
            }
        }
        public bool IgnoreText
        {

            get
            {
                return _property.IgnoreText;
            }
            set
            {
                if (value == _property.IgnoreText) return;
                _property.IgnoreText = value;
                RaisePropertyChanged("IgnoreText");
            }
        }
    }
}