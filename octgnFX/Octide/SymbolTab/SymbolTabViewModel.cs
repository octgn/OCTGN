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
    public class SymbolTabViewModel : ViewModelBase
    {
        private SymbolItemViewModel _selectedItem;

        public ObservableCollection<IdeListBoxItemBase> Items { get; private set; }

        public RelayCommand AddCommand { get; private set; }

        public SymbolTabViewModel()
        {
            AddCommand = new RelayCommand(AddItem);

            Items = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var symbol in ViewModelLocator.GameLoader.Game.Symbols)
            {
                Items.Add(new SymbolItemViewModel(symbol) { ItemSource = Items });
            }
            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Game.Symbols = Items.Select(x => (x as SymbolItemViewModel)._symbol).ToList();
            };
        }

        public SymbolItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }

        public void AddItem()
        {
            var ret = new SymbolItemViewModel();
            Items.Add(ret);
            SelectedItem = ret;
        }
    }
}