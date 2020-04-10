// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.ItemModel
{
    public class PhaseItemModel : IdeBaseItem
    {
        public GamePhase _phase;

        public PhaseItemModel(IdeCollection<IdeBaseItem> source) : base(source)
        {
            _phase = new GamePhase
            {
                Icon = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath,
            };
            Name = "New Phase";
            RaisePropertyChanged("Asset");
        }

        public PhaseItemModel(GamePhase p, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _phase = p;
        }

        public PhaseItemModel(PhaseItemModel p, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _phase = new GamePhase
            {
                Icon = p.Asset.FullPath,
            };
            Name = p.Name;
        }
        public override object Clone()
        {
            return new PhaseItemModel(this, Source);
        }

        public override object Create()
        {
            return new PhaseItemModel(Source);
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
