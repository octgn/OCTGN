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
    public class BoardViewModel : ViewModelBase
    {
        private BoardItemModel _selectedItem;
        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();

        
        public RelayCommand DeleteCommand { get; private set; }

        public BoardViewModel()
        {
            DeleteCommand = new RelayCommand(DeleteItem);
        }

        public void UpdateBoards()
        {
            ViewModelLocator.GameLoader.Game.GameBoards = ViewModelLocator.PreviewTabViewModel.Boards.ToDictionary(x => x.Name, y => y._board);
            ViewModelLocator.GameLoader.Game.GameBoards.Add("Default", ViewModelLocator.PreviewTabViewModel.DefaultBoard._board);
        }

        public void DeleteItem()
        {
            if (SelectedItem == null) return;
            ViewModelLocator.PreviewTabViewModel.Boards.Remove(SelectedItem);
            ViewModelLocator.PreviewTabViewModel.Selection = ViewModelLocator.PreviewTabViewModel.DefaultBoard;
        }

        public Visibility DefaultVisibility
        {
            get
            {
                return (SelectedItem.Default ? Visibility.Collapsed : Visibility.Visible);
            }
        }

        public BoardItemModel SelectedItem
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