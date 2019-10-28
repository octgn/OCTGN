// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using System.Linq;

namespace Octide.ItemModel
{
    public class PhaseItemViewModel : IdeListBoxItemBase
    {
        public GamePhase _phase;

        public PhaseItemViewModel()
        {
            _phase = new GamePhase
            {
                Icon = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath,
                Name = "Phase"
            };
            RaisePropertyChanged("Asset");
        }

        public PhaseItemViewModel(GamePhase p)
        {
            _phase = p;
        }

        public PhaseItemViewModel(PhaseItemViewModel p)
        {
            _phase = new GamePhase
            {
                Icon = p.Asset.FullPath,
                Name = p.Name
            };
            ItemSource = p.ItemSource;
            Parent = p.Parent;
        }

        public override object Clone()
        {
            return new PhaseItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as PhaseItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new PhaseItemViewModel() { Parent = Parent, ItemSource = ItemSource });
        }


        public string Name
        {
            get
            {
                return _phase.Name;
            }
            set
            {
                if (_phase.Name == value) return;
                _phase.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public Asset Asset
        {
            get
            {
                return Asset.Load(_phase.Icon);
            }
            set
            {
                _phase.Icon = value?.FullPath;
                RaisePropertyChanged("Asset");
            }
        }
    }
}
