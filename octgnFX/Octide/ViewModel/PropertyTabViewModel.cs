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
using Octide.ItemModel;

namespace Octide.ViewModel
{
    public class PropertyTabViewModel : ViewModelBase
    {

        private Visibility _panelVisibility;
        private PropertyItemModel _selectedItem;
        public ObservableCollection<PropertyItemModel> Items { get; private set; }
        public IList<PropertyType> TypeOptions => Enum.GetValues(typeof(PropertyType)).Cast<PropertyType>().ToList();
        public IList<PropertyTextKind> TextKindOptions => Enum.GetValues(typeof(PropertyTextKind)).Cast<PropertyTextKind>().ToList();

        public RelayCommand AddCommand { get; private set; }


        public PropertyTabViewModel()
        {
            AddCommand = new RelayCommand(AddItem);
            PanelVisibility = Visibility.Collapsed;
            Items = new ObservableCollection<PropertyItemModel>(ViewModelLocator.GameLoader.Game.CustomProperties.Select(x => new PropertyItemModel(x)));
            Items.CollectionChanged += (a, b) => {
                ViewModelLocator.GameLoader.Game.CustomProperties = Items.Select(x => x.PropertyDef).ToList();
                Messenger.Default.Send(new CustomPropertyChangedMessage());
            };
        }
        
        public PropertyItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                PanelVisibility = (value == null) ? Visibility.Collapsed : Visibility.Visible;
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
        
        public void AddItem()
        {
            var ret = new PropertyItemModel() { Name = "Property" };
            Items.Add(ret);
            SelectedItem = ret;
        }
    }

}