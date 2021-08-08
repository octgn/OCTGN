// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Octide.ItemModel
{
    public class SymbolItemModel : IdeBaseItem
    {
        public Symbol _symbol;
        public AssetController Asset { get; set; }

        public SymbolItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            _symbol = new Symbol();
            Name = "New Symbol";
            Id = "symbol";
            Asset = new AssetController(AssetType.Image);
            _symbol.Source = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
        }

        public SymbolItemModel(Symbol s, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            _symbol = s;
            Asset = new AssetController(AssetType.Image);
            Asset.Register(s.Source);
            Asset.PropertyChanged += AssetUpdated;
        }

        public SymbolItemModel(SymbolItemModel s, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            _symbol = new Symbol();
            Asset = new AssetController(AssetType.Image);
            Asset.Register(s._symbol.Source);
            _symbol.Source = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
            Name = s.Name;
            Id = s.Id;
        }

        private void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _symbol.Source = Asset.FullPath;
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
            return new SymbolItemModel(this, Source);
        }
        public override object Create()
        {
            return new SymbolItemModel(Source);
        }

        public string Name
        {
            get
            {
                return _symbol.Name;
            }
            set
            {
                if (_symbol.Name == value) return;
                _symbol.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public IEnumerable<string> UniqueNames => Source.Select(x => ((SymbolItemModel)x).Id);

        public string Id
        {
            get
            {
                return _symbol.Id;
            }
            set
            {
                if (_symbol.Id == value) return;
                _symbol.Id = Utils.GetUniqueName(value, UniqueNames);
                RaisePropertyChanged("Id");
            }
        }

        public new string Icon => Asset.SafePath;


    }
}
