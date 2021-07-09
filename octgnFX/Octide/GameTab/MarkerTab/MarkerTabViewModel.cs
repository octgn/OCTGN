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
    public class MarkerTabViewModel : ViewModelBase
    {
        public IdeCollection<IdeBaseItem> Items { get; private set; }

        public RelayCommand AddCommand { get; private set; }

        public MarkerTabViewModel()
        {
            AddCommand = new RelayCommand(AddItem);

            Items = new IdeCollection<IdeBaseItem>(this, typeof(MarkerItemModel));
            foreach (var marker in ViewModelLocator.GameLoader.Game.Markers)
            {
                Items.Add(new MarkerItemModel(marker.Value, Items));
            }
            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Game.Markers = Items.ToDictionary(
                    x => (x as MarkerItemModel)._marker.Id,
                    x => (x as MarkerItemModel)._marker);
            };
        }

        public int MarkerSize
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.MarkerSize;
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.MarkerSize) return;
                ViewModelLocator.GameLoader.Game.MarkerSize = value;
                RaisePropertyChanged("MarkerSize");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public void AddItem()
        {
            var ret = new MarkerItemModel(Items);
            Items.Add(ret);
            Items.SelectedItem = ret;
        }
    }
}