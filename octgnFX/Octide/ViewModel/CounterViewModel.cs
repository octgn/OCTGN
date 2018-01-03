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

namespace Octide.ViewModel
{
    public class CounterViewModel : ViewModelBase
    {
        private CounterItemModel _selectedItem;
        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();

        public RelayCommand DeleteCommand { get; private set; }

        public CounterViewModel()
        {
            DeleteCommand = new RelayCommand(DeleteItem);
        }

        public void DeleteItem()
        {
            if (SelectedItem == null) return;
            ViewModelLocator.PreviewTabViewModel.Counters.Remove(SelectedItem);
        }

        public CounterItemModel SelectedItem
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