// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using System;
using System.Linq;

namespace Octide.ItemModel
{
    public class MarkerItemViewModel : IdeListBoxItemBase
    {
        public GameMarker _marker;

        public object _parent;

        public MarkerItemViewModel()
        {
            _marker = new GameMarker
            {
                Id = Guid.NewGuid().ToString(), //TODO: Proper ID generation, not as a GUID
                Name = "Marker",
                Source = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath,
            };
            RaisePropertyChanged("Asset");
        }

        public MarkerItemViewModel(GameMarker m)
        {
            _marker = m;
        }

        public MarkerItemViewModel(MarkerItemViewModel m)
        {
            _marker = new GameMarker
            {
                Source = m.Asset.FullPath,
                Name = m.Name,
                Id = Guid.NewGuid().ToString() //TODO: Proper ID generation, not as a GUID
            };
            ItemSource = m.ItemSource;
            Parent = m.Parent;
        }

        public override object Clone()
        {
            return new MarkerItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as MarkerItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new MarkerItemViewModel() { Parent = Parent, ItemSource = ItemSource });
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
                if (string.IsNullOrEmpty(value)) return;
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
