// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Linq;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using System.Collections.ObjectModel;
using Octide.ItemModel;

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
            var ret = new SymbolItemViewModel() { ItemSource = Items, Name = "Symbol", Id = "Symbol" };
            Items.Add(ret);
            SelectedItem = ret;
        }
    }
}