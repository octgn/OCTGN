using System;
using System.Windows;
using System.Linq;
using System.ComponentModel;
using System.Windows.Media;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Octide.Messages;
using Octide.ItemModel;

namespace Octide.ViewModel
{
    public class PhaseViewModel : ViewModelBase
    {
        private PhaseItemModel _selectedItem;
        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();
                
        public RelayCommand DeleteCommand { get; private set; }

        public PhaseViewModel()
        {
            DeleteCommand = new RelayCommand(DeleteItem);
        }
        
        public void DeleteItem()
        {
            if (SelectedItem == null) return;
            ViewModelLocator.PreviewTabViewModel.Phases.Remove(SelectedItem);
        }
        
        public PhaseItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
                RaisePropertyChanged("DefaultVisibility");
            }
        }
    }

}