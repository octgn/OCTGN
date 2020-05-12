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

            var setSerializer = new SetSerializer() { Game = ViewModelLocator.GameLoader.Game };

            var setAssets = ViewModelLocator.AssetsTabViewModel.Assets.Where(x => x.Type == AssetType.Xml && x.FullFileName == "set.xml");
            foreach (var asset in setAssets)
            {
                var set = (Set)setSerializer.Deserialize(asset.FullPath);
                var setModel = new SetModel(set, Items);
                setModel.Asset = new AssetController(asset);
                setModel.Asset.PropertyChanged += AssetChanged;
                Items.Add(setModel);
            }

            Items.CollectionChanged += (a, b) =>
            {
    //            ViewModelLocator.GameLoader.Sets = Items.Select(x => ((SetModel)x)._set);
            };
        }

        public void AssetChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        public void AddSet()
        {
            var ret = new SetModel(Items);
            Items.Add(ret);
            Items.SelectedItem = ret;
        }
        
    }
}