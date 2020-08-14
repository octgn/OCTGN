// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Octide.ItemModel
{
    public class PhaseItemModel : IdeBaseItem
    {
        public GamePhase _phase;
        public AssetController Asset { get; set; }

        public PhaseItemModel(IdeCollection<IdeBaseItem> source) : base(source)
        {
            _phase = new GamePhase
            {
            };
            Name = "New Phase";

            Asset = new AssetController(AssetType.Image);
            _phase.Icon = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
            RaisePropertyChanged("Asset");
        }

        public PhaseItemModel(GamePhase p, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _phase = p;
            Asset = new AssetController(AssetType.Image);
            Asset.Register(p.Icon);
            Asset.PropertyChanged += AssetUpdated;
        }

        public PhaseItemModel(PhaseItemModel p, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _phase = new GamePhase
            {
            };
            Asset = new AssetController(AssetType.Image);
            Asset.Register(p._phase.Icon);
            _phase.Icon = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
            Name = p.Name;
        }

        private void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _phase.Icon = Asset.FullPath;
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
            return new PhaseItemModel(this, Source);
        }

        public override object Create()
        {
            return new PhaseItemModel(Source);
        }

        public new string Icon => Asset.SafePath;

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
    }
}
