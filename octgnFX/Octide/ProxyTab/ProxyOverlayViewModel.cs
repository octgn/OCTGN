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
using Octide.ItemModel;
using Microsoft.Win32;
using GongSolutions.Wpf.DragDrop;
using System.Windows.Data;

namespace Octide.ViewModel
{
    public class ProxyOverlayViewModel : ViewModelBase
    {
        private ProxyOverlayItemModel _selectedItem;
       
        public RelayCommand DeleteCommand { get; private set; }
        
        public ProxyOverlayViewModel()
        {
            DeleteCommand = new RelayCommand(DeleteItem);
        }
        
        public void DeleteItem()
        {
            if (SelectedItem == null) return;
            ViewModelLocator.ProxyTabViewModel.OverlayBlocks.Remove(SelectedItem);
        }
        
        public ProxyOverlayItemModel SelectedItem
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

}