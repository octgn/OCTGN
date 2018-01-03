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
    public class GroupViewModel : ViewModelBase
    {
        private GroupItemModel _selectedItem;
        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();
        public IList<GroupVisibility> VisibilityOptions => Enum.GetValues(typeof(GroupVisibility)).Cast<GroupVisibility>().ToList();
        
        public RelayCommand DeleteCommand { get; private set; }

        public GroupViewModel()
        {
            DeleteCommand = new RelayCommand(DeleteItem);
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

}