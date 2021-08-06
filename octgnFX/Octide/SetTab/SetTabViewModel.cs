// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octgn.DataNew;
using Octgn.DataNew.Entities;
using Octgn.Library;
using Octide.ItemModel;
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

            Items = new IdeCollection<IdeBaseItem>(this, typeof(SetModel));

            var setSerializer = new SetSerializer() { Game = ViewModelLocator.GameLoader.Game };

            var setAssets = ViewModelLocator.AssetsTabViewModel.Assets.Where(x => x.Type == AssetType.Xml && x.Name == "set");
            foreach (var asset in setAssets)
            {
                asset.IsReserved = true;
                asset.LockName = true;
                var set = (Set)setSerializer.Deserialize(asset.TargetFilePath);
                var setModel = new SetModel(set, Items);
                Items.Add(setModel);
            }

            Items.CollectionChanged += (a, b) =>
            {
                //            ViewModelLocator.GameLoader.Sets = Items.Select(x => ((SetModel)x)._set);
            };

            // import set markers from deprecated code
            foreach (var marker in setSerializer.Game.Markers)
            {
                if (!ViewModelLocator.MarkerTabViewModel.Items.Any(x => ((MarkerItemModel)x).Id == marker.Key))
                {
                    ViewModelLocator.MarkerTabViewModel.Items.Add(new MarkerItemModel(marker.Value, ViewModelLocator.MarkerTabViewModel.Items));
                }
            }
        }


        public void AddSet()
        {
            var ret = new SetModel(Items);
            Items.Add(ret);
            Items.SelectedItem = ret;
        }

    }
}