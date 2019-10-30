// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using Octide.ItemModel;
using Octide.Messages;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;

namespace Octide.ViewModel
{
    public class SetTabViewModel : ViewModelBase
    {
        private SetItemViewModel _selectedItem;

        public ObservableCollection<IdeListBoxItemBase> Items { get; private set; }

        public RelayCommand AddSetCommand { get; private set; }

        public SetTabViewModel()
        {
            AddSetCommand = new RelayCommand(AddSet);

            Items = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var set in ViewModelLocator.GameLoader.Sets)
            {
                Items.Add(new SetItemViewModel(set) { ItemSource = Items });
            }

            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Sets = Items.Select(x => (x as SetItemViewModel)._set).ToList();
            };
        }

        public SetItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }

        public void AddSet()
        {
            var ret = new SetItemViewModel() {ItemSource = Items };
            Items.Add(ret);
            SelectedItem = ret;
        }
        
    }
}