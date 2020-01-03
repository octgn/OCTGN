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

namespace Octide.ItemModel
{

    public class ProxyOverlayDefinitionItemModel : IdeListBoxItemBase
    {
        public BlockDefinition _def;
        public new ObservableCollection<ProxyOverlayDefinitionItemModel> ItemSource { get; set; }

        public ProxyOverlayDefinitionItemModel() // new overlay
        {
            _def = new BlockDefinition
            {
                src = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.RelativePath,
                type = "overlay",
                id = Utils.GetUniqueName("overlay", ViewModelLocator.ProxyTabViewModel.OverlayBlocks.Select(x => x.Name))
            };
        }

        public ProxyOverlayDefinitionItemModel(BlockDefinition b) // load overlay
        {
            _def = b;
        }

        public ProxyOverlayDefinitionItemModel(ProxyOverlayDefinitionItemModel o)
        {
            _def = new BlockDefinition
            {
                src = o.Asset.RelativePath,
                id = Utils.GetUniqueName(o.Name, ViewModelLocator.ProxyTabViewModel.OverlayBlocks.Select(x => x.Name))
            };
        }
        
        public string Name
        {
            get { return _def.id; }
            set
            {
                if (value == _def.id) return; if (string.IsNullOrEmpty(value)) return;
                _def.id = Utils.GetUniqueName(value, ViewModelLocator.ProxyTabViewModel.OverlayBlocks.Select(x => x.Name));

                RaisePropertyChanged("Name");
            }
        }

        public Asset Asset
        {
            get
            {
                if (_def.src == null)
                    return new Asset();
                return Asset.Load(Path.Combine(_def.Manager.RootPath, _def.src));
            }
            set
            {
                _def.src = value?.RelativePath;
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("Width");
                RaisePropertyChanged("Height");
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
                using (var img = System.Drawing.Image.FromFile(Asset?.FullPath))
                {
                    return img.Width;
                }
            }
        }
        public int Height
        {
            get
            {
                using (var img = System.Drawing.Image.FromFile(Asset?.FullPath))
                {
                    return img.Height;
                }
            }
        }

        public override object Clone()
        {
            return new ProxyOverlayDefinitionItemModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ViewModelLocator.ProxyTabViewModel.OverlayBlocks.IndexOf(this);
            ViewModelLocator.ProxyTabViewModel.OverlayBlocks.Insert(index, Clone() as ProxyOverlayDefinitionItemModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ViewModelLocator.ProxyTabViewModel.OverlayBlocks.IndexOf(this);
            ViewModelLocator.ProxyTabViewModel.OverlayBlocks.Insert(index, new ProxyOverlayDefinitionItemModel());
        }
    }

}
