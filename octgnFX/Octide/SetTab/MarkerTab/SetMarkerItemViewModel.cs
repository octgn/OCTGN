// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using System;
using System.Linq;

namespace Octide.ItemModel
{
    public class SetMarkerItemViewModel : IdeListBoxItemBase
    {
        public Marker _marker;

        public object _parent;

        public SetMarkerItemViewModel()
        {
            _marker = new Marker
            {
                Id = Guid.NewGuid(),
                Name = "Marker",
                IconUri = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath,
            };
            RaisePropertyChanged("Asset");
        }

        public SetMarkerItemViewModel(Marker m)
        {
            _marker = m;
        }

        public SetMarkerItemViewModel(SetMarkerItemViewModel m)
        {
            _marker = new Marker
            {
                IconUri = m.Asset.FullPath,
                Name = m.Name,
                Id = Guid.NewGuid()
            };
            ItemSource = m.ItemSource;
            Parent = m.Parent;
        }

        public override object Clone()
        {
            return new SetMarkerItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as SetMarkerItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new SetMarkerItemViewModel() { Parent = Parent, ItemSource = ItemSource });
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
                return Asset.Load(_marker.IconUri);
            }
            set
            {
                _marker.IconUri = value?.FullPath;
                RaisePropertyChanged("Asset");
            }
        }
    }
}
