using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octide.Messages;
using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GongSolutions.Wpf.DragDrop;
using System.Collections.Specialized;

namespace Octide.ViewModel
{
    public class PropertyTabViewModel : ViewModelBase, IDropTarget
    {

        private Visibility _panelVisibility;
        private Game _game;
        private CustomPropertyItemModel _selectedItem;
        public ObservableCollection<CustomPropertyItemModel> Items { get; private set; }
        public IList<PropertyType> TypeOptions => Enum.GetValues(typeof(PropertyType)).Cast<PropertyType>().ToList();
        public IList<PropertyTextKind> TextKindOptions => Enum.GetValues(typeof(PropertyTextKind)).Cast<PropertyTextKind>().ToList();

        public RelayCommand AddCommand { get; private set; }


        public PropertyTabViewModel()
        {
            _game = ViewModelLocator.GameLoader.Game;
            AddCommand = new RelayCommand(AddItem);
            PanelVisibility = Visibility.Collapsed;
            Items = new ObservableCollection<CustomPropertyItemModel>(_game.CustomProperties.Select(x => new CustomPropertyItemModel(x)));
            Items.CollectionChanged += (a, b) => {
                _game.CustomProperties = Items.Select(x => x.PropertyDef).ToList();
                Messenger.Default.Send(new CustomPropertyChangedMessage());
            };
        }
        
        public CustomPropertyItemModel SelectedItem
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
            var ret = new CustomPropertyItemModel() { Name = "Property" };
            Items.Add(ret);
            SelectedItem = ret;
        }

        public void RemoveItem()
        {
            Items.Remove(SelectedItem);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.SourceCollection == dropInfo.TargetCollection)
            {
                var coll = dropInfo.TargetCollection as ObservableCollection<CustomPropertyItemModel>;
                coll.Move(dropInfo.DragInfo.SourceIndex, dropInfo.InsertIndex);
            }
            else
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
        }
    }

    public class CustomPropertyItemModel : ViewModelBase, ICloneable
    {
        public PropertyDef PropertyDef { get; set; }
        private Guid _id;
        public RelayCommand RemoveCommand { get; private set; }

        public CustomPropertyItemModel()
        {
            PropertyDef = new PropertyDef();
            _id = Guid.NewGuid();
            RemoveCommand = new RelayCommand(Remove);
        }
        
        public CustomPropertyItemModel(PropertyDef p)
        {
            PropertyDef = p;
            _id = Guid.NewGuid();
            RemoveCommand = new RelayCommand(Remove);
        }

        public CustomPropertyItemModel(CustomPropertyItemModel p)
        {
            PropertyDef = p.PropertyDef.Clone() as PropertyDef;
            _id = Guid.NewGuid();
            RemoveCommand = new RelayCommand(Remove);
        }

        public object Clone()
        {
            return new CustomPropertyItemModel(this);
        }

        public void Remove()
        {
            ViewModelLocator.PropertyTabViewModel.Items.Remove(this);
        }

        public Guid Id
        {
            get
            {
                return _id;
            }
        }

        public string Name
        {
            get
            {
                return PropertyDef.Name;
            }
            set
            {
                if (value == PropertyDef.Name) return;
                PropertyDef.Name = value;
                RaisePropertyChanged("Name");
                Messenger.Default.Send(new CustomPropertyChangedMessage() { Prop = this });
            }
        }

        public PropertyType Type
        {
            get
            {
                return PropertyDef.Type;
            }
            set
            {
                if (value == PropertyDef.Type) return;
                PropertyDef.Type = value;
                RaisePropertyChanged("Type");
            }
        }

        public PropertyTextKind TextKind
        {
            get
            {
                return PropertyDef.TextKind;
            }
            set
            {
                if (value == PropertyDef.TextKind) return;
                PropertyDef.TextKind = value;
                RaisePropertyChanged("TextKind");
            }
        }
        
        public bool Hidden
        {

            get
            {
                return PropertyDef.Hidden;
            }
            set
            {
                if (value == PropertyDef.Hidden) return;
                PropertyDef.Hidden = value;
                RaisePropertyChanged("Hidden");
            }
        }
        public bool IgnoreText
        {

            get
            {
                return PropertyDef.IgnoreText;
            }
            set
            {
                if (value == PropertyDef.IgnoreText) return;
                PropertyDef.IgnoreText = value;
                RaisePropertyChanged("IgnoreText");
            }
        }

        public Visibility IsName
        {
            get
            {
                return Name == "Name" ? Visibility.Collapsed : Visibility.Visible;
            }
        }

    }
}