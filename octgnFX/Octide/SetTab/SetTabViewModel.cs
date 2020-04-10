// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octide.SetTab.ItemModel;

namespace Octide.ViewModel
{
    public class SetTabViewModel : ViewModelBase
    {
        public IdeCollection<IdeBaseItem> Items { get; private set; }

        public RelayCommand AddSetCommand { get; private set; }

        public SetTabViewModel()
        {
            AddSetCommand = new RelayCommand(AddSet);

            Items = new IdeCollection<IdeBaseItem>(this);
            foreach (var set in ViewModelLocator.GameLoader.Sets)
            {
                Items.Add(new SetModel(set, Items));
            }

            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Sets = Items.Select(x => ((SetModel)x)._set);
            };
        }

        public void AddSet()
        {
            var ret = new SetModel(Items);
            Items.Add(ret);
            Items.SelectedItem = ret;
        }
        
    }
}