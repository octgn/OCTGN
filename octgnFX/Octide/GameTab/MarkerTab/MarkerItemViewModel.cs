// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using System;
using System.ComponentModel;
using System.Linq;

namespace Octide.ItemModel
{
    public class MarkerItemModel : IdeBaseItem
    {
        public GameMarker _marker;
        public AssetController Asset { get; set; }
        public MarkerItemModel(IdeCollection<IdeBaseItem> source) : base(source)
        {
            _marker = new GameMarker
            {
                Id = Guid.NewGuid().ToString(), //TODO: Proper ID generation, not as a GUID
            };
            Asset = new AssetController(AssetType.Image);
            _marker.Source = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
            Name = "New Marker";
            RaisePropertyChanged("Asset");
        }

        public MarkerItemModel(GameMarker m, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _marker = m;
            Asset = new AssetController(AssetType.Image, m.Source);
            Asset.PropertyChanged += AssetUpdated;
        }

        public MarkerItemModel(MarkerItemModel m, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _marker = new GameMarker
            {
                Id = Guid.NewGuid().ToString() //TODO: Proper ID generation, not as a GUID
            };
            Asset = new AssetController(AssetType.Image, m._marker.Source);
            _marker.Source = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
            Name = m.Name;
        }

        private void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _marker.Source = Asset.FullPath;
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("Icon");
            }
        }

        public override void Cleanup()
        {
            Asset.SelectedAsset = null;
            base.Cleanup();
        }

        public override object Clone()
        {
            return new MarkerItemModel(this, Source);
        }

        public override object Create()
        {
            return new MarkerItemModel(Source);
        }

        public string Name
        {
            get
            {
                return _marker.Name;
            }
            set
            {
                if (value == _marker.Name) return;
                _marker.Name = value;
                RaisePropertyChanged("Name");
            }
        }
        public new string Icon => Asset.SafePath;
    }
}
