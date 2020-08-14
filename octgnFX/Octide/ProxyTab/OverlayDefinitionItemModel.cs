// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using Octgn.DataNew.Entities;
using Octgn.ProxyGenerator.Definitions;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Font = Octgn.DataNew.Entities.Font;
using FontFamily = System.Windows.Media.FontFamily;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.ComponentModel;

namespace Octide.ItemModel
{

    public class OverlayBlockDefinitionItemModel : IdeBaseItem
    {
        public BlockDefinition _def;
        public AssetController Asset { get; set; }

        public OverlayBlockDefinitionItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new overlay
        {
            _def = new BlockDefinition
            {
                type = "overlay"
            };
            Asset = new AssetController(AssetType.Image);
            _def.src = Asset.SelectedAsset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
            Name = "overlay";
        }

        public OverlayBlockDefinitionItemModel(BlockDefinition b, IdeCollection<IdeBaseItem> source) : base(source) // load overlay
        {
            _def = b;
            Asset = new AssetController(AssetType.Image);
            Asset.Register(b.src);
            Asset.PropertyChanged += AssetUpdated;
        }

        public OverlayBlockDefinitionItemModel(OverlayBlockDefinitionItemModel o, IdeCollection<IdeBaseItem> source) : base(source) //copy 
        {
            _def = new BlockDefinition
            {
                type = "overlay"
            };
            Asset = new AssetController(AssetType.Image);
            Asset.Register(o._def.src);
            Asset.PropertyChanged += AssetUpdated;
            _def.src = Asset.SelectedAsset.FullPath;
            Name = o.Name;
        }
        private void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _def.src = Asset.SelectedAsset?.FullPath;
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("Width");
                RaisePropertyChanged("Height");
            }
        }
        public override void Cleanup()
        {
            Asset.SelectedAsset = null;
            base.Cleanup();
        }


        public override object Clone()
        {
            return new OverlayBlockDefinitionItemModel(this, Source);
        }
        public override object Create()
        {
            return new OverlayBlockDefinitionItemModel(Source);
        }
        public IEnumerable<string> UniqueNames => Source.Select(x => ((OverlayBlockDefinitionItemModel)x).Name);

        public string Name
        {
            get { return _def.id; }
            set
            {
                if (value == _def.id) return;
                _def.id = Utils.GetUniqueName(value, UniqueNames);
                RaisePropertyChanged("Name");
            }
        }

        public int Left
        {
            get
            {
                return _def.location.x;
            }
            set
            {
                if (_def.location.x == value) return;
                _def.location.x = value;
                RaisePropertyChanged("Left");
            }
        }

        public int Top
        {
            get
            {
                return _def.location.y;
            }
            set
            {
                if (_def.location.y == value) return;
                _def.location.y = value;
                RaisePropertyChanged("Top");
            }
        }

        public int Width
        {
            get
            {
                using (var img = System.Drawing.Image.FromFile(Asset.SafePath))
                {
                    return img.Width;
                }
            }
        }
        public int Height
        {
            get
            {
                using (var img = System.Drawing.Image.FromFile(Asset.SafePath))
                {
                    return img.Height;
                }
            }
        }
    }
}
