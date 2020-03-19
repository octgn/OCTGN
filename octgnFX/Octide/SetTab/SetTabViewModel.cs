// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using Octide.ItemModel;
using Octide.SetTab;
using Octide.Messages;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;

namespace Octide.ViewModel
{
    public class SetTabViewModel : ViewModelBase
    {
        private SetModel _selectedItem;

        public ObservableCollection<SetModel> Items { get; private set; }

        public RelayCommand AddSetCommand { get; private set; }

        public SetTabViewModel()
        {
            AddSetCommand = new RelayCommand(AddSet);

            Items = new ObservableCollection<SetModel>();
            foreach (var set in ViewModelLocator.GameLoader.Sets)
            {
                Items.Add(new SetModel(set) { ItemSource = Items, Parent = this });
            }

            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Sets = Items.Select(x => x._set).ToList();
            };
        }

        public SetModel SelectedItem
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
            var ret = new SetModel() {ItemSource = Items };
            Items.Add(ret);
            SelectedItem = ret;
        }
        
    }
}