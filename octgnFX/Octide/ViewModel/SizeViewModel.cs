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
    public class SizeViewModel : ViewModelBase
    {
        private SizeItemModel _selectedItem;
        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();

        
        public RelayCommand DeleteCommand { get; private set; }

        public SizeViewModel()
        {
            DeleteCommand = new RelayCommand(DeleteItem);
        }

        public void UpdateSizes()
        {
            ViewModelLocator.GameLoader.Game.CardSizes = ViewModelLocator.PreviewTabViewModel.CardSizes.ToDictionary(x => x.Name, y => y.SizeDef);
        }

        public void DeleteItem()
        {
            if (SelectedItem == null) return;
            ViewModelLocator.PreviewTabViewModel.CardSizes.Remove(SelectedItem);
        }

        public Visibility DefaultVisibility
        {
            get
            {
                return (SelectedItem.Default ? Visibility.Collapsed : Visibility.Visible);
            }
        }

        public SizeItemModel SelectedItem
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