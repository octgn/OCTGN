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
    public class GameModeTabViewModel : ViewModelBase
    {
        public IdeCollection<IdeBaseItem> Items { get; private set; }
        public RelayCommand AddCommand { get; private set; }

        public GameModeTabViewModel()
        {
            AddCommand = new RelayCommand(AddItem);

            Items = new IdeCollection<IdeBaseItem>(this);
            foreach (var mode in ViewModelLocator.GameLoader.Game.Modes)
            {
                Items.Add(new GameModeItemModel(mode, Items));
            }
            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Game.Modes = Items.Select(x => (x as GameModeItemModel)._gameMode).ToList();
            };
        }


        public void AddItem()
        {
            var ret = new GameModeItemModel(Items);
            Items.Add(ret);
            Items.SelectedItem = ret;
        }
    }
}