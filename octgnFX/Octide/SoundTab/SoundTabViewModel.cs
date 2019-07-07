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
    public class SoundTabViewModel : ViewModelBase
    {
        private SoundItemViewModel _selectedItem;
        public ObservableCollection<IdeListBoxItemBase> Items { get; private set; }

        public RelayCommand AddCommand { get; private set; }


        public SoundTabViewModel()
        {
            AddCommand = new RelayCommand(AddItem);

            Items = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var sound in ViewModelLocator.GameLoader.Game.Sounds)
            {
                Items.Add(new SoundItemViewModel(sound.Value) { ItemSource = Items });
            }
            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Game.Sounds = Items.ToDictionary(x => (x as SoundItemViewModel).Name, y => (y as SoundItemViewModel)._sound);
            };
        }
        
        public SoundItemViewModel SelectedItem
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
            var ret = new SoundItemViewModel();
            Items.Add(ret);
            SelectedItem = ret;
        }
        
    }

}