// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using System;
using System.Linq;

namespace Octide.ItemModel
{
    public class MarkerItemModel : IdeBaseItem
    {
        public GameMarker _marker;

        public object _parent;

        public MarkerItemModel(IdeCollection<IdeBaseItem> source) : base(source)
        {
            _marker = new GameMarker
            {
                Id = Guid.NewGuid().ToString(), //TODO: Proper ID generation, not as a GUID
                Source = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath,
            };
            Name = "New Marker";
            RaisePropertyChanged("Asset");
        }

        public MarkerItemModel(GameMarker m, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _marker = m;
        }

        public MarkerItemModel(MarkerItemModel m, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _marker = new GameMarker
            {
                Source = m.Asset.FullPath,
                Id = Guid.NewGuid().ToString() //TODO: Proper ID generation, not as a GUID
            };
            Name = m.Name;
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
        public new string Icon => Asset?.FullPath;
        public Asset Asset
        {
            get
            {
                return Asset.Load(_marker.Source);
            }
            set
            {
                _marker.Source = value?.FullPath;
                RaisePropertyChanged("Asset");
            }
        }
    }
}
