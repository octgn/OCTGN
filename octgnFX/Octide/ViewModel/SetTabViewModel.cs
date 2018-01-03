using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octgn.DataNew.Entities;
using Octgn.Core.DataExtensionMethods;
using Octide.Messages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using Octgn.Library;
using GongSolutions.Wpf.DragDrop;
using Octide.ItemModel;

namespace Octide.ViewModel
{
    public class SetTabViewModel : ViewModelBase
    {

        public SetTabViewModel()
        {
            AddSetCommand = new RelayCommand(AddSet);
            SetPanelVisibility = Visibility.Hidden;

            Items = new ObservableCollection<SetItemModel>(ViewModelLocator.GameLoader.Sets.Select(x => new SetItemModel(x)));

            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Sets = Items.Select(x => x._set).ToList();
            };
            SelectedItem = Items.FirstOrDefault();
        }
        
        private Visibility _setPanelVisibility;
        private SetItemModel _selectedItem;
        public ObservableCollection<SetItemModel> Items { get; private set; }

        public RelayCommand AddSetCommand { get; private set; }


        public SetItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                SetPanelVisibility = value == null ? Visibility.Hidden : Visibility.Visible;
                RaisePropertyChanged("SelectedItem");
                ViewModelLocator.SetPackagesViewModel.ParentSet = value;
                ViewModelLocator.SetCardsViewModel.ParentSet = value;
                ViewModelLocator.SetSummaryViewModel.Set = value;
                //    SelectedCard = SelectedSet.CardItems.FirstOrDefault() ?? null;
            }
        }

        public Visibility SetPanelVisibility
        {
            get { return _setPanelVisibility; }
            set
            {
                if (value == _setPanelVisibility) return;
                _setPanelVisibility = value;
                RaisePropertyChanged("SetPanelVisibility");
            }
        }

        public bool EnableSetButton()
        {
            return SelectedItem != null;
        }

        public void AddSet()
        {
            var ret = new SetItemModel() { Name = "Set" };
            Items.Add(ret);
            SelectedItem = ret;
            RaisePropertyChanged("SetItems");
        }
        
    }
}