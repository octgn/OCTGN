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
using System.Windows.Data;

namespace Octide.ViewModel
{
    public class DocumentTabViewModel : ViewModelBase
    {

        private Visibility _panelVisibility;
        private DocumentItemViewModel _selectedItem;
        public ObservableCollection<IdeListBoxItemBase> Items { get; private set; }

        public RelayCommand AddCommand { get; private set; }


        public DocumentTabViewModel()
        {
            AddCommand = new RelayCommand(AddItem);
            PanelVisibility = Visibility.Collapsed;

            Items = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var document in ViewModelLocator.GameLoader.Game.Documents)
            {
                Items.Add(new DocumentItemViewModel(document) { ItemSource = Items });
            }
            Items.CollectionChanged += (a, b) => {
                ViewModelLocator.GameLoader.Game.Documents = Items.Select(x => (x as DocumentItemViewModel)._document).ToList();
            };
        }
        
        public DocumentItemViewModel SelectedItem
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
            var ret = new DocumentItemViewModel();
            Items.Add(ret);
            SelectedItem = ret;
        }
    }

}