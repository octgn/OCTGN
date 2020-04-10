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
        public IdeCollection<IdeBaseItem> Items { get; private set; }
        public RelayCommand AddCommand { get; private set; }

        public SymbolTabViewModel()
        {
            AddCommand = new RelayCommand(AddItem);

            Items = new IdeCollection<IdeBaseItem>(this);
            foreach (var symbol in ViewModelLocator.GameLoader.Game.Symbols)
            {
                Items.Add(new SymbolItemModel(symbol, Items));
            }
            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Game.Symbols = Items.Select(x => (x as SymbolItemModel)._symbol).ToList();
            };
        }


        public void AddItem()
        {
            var ret = new SymbolItemModel(Items);
            Items.Add(ret);
            Items.SelectedItem = ret;
        }
    }
}