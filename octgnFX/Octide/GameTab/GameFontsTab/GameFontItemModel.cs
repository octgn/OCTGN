// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows;
using System.Windows.Media;
using System.Drawing.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Octgn.DataNew.Entities;
using System.IO;
using System.ComponentModel;

namespace Octide.ViewModel
{
    //TODO: Fix asset always setting a default value for missing font def
    public class GameFontItemModel : ViewModelBase
    {
        public Font _font;
        public AssetController Asset { get; set; }

        public GameFontItemModel(Font f)
        {
            _font = f;
            if (f == null)
            {
                _font = new Font();
            }
            Asset = new AssetController(AssetType.Font, f?.Src) { CanRemove = true };
            Asset.PropertyChanged += AssetUpdated;
            RaisePropertyChanged("Asset");
        }

        private void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _font.Src = Asset.FullPath;
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("FontAsset");
                RaisePropertyChanged("Size");
            }
        }

        public int Size
        {
            get
            {
                if (_font == null) return 0;
                return _font.Size;
            }
            set
            {
                if (_font.Size == value) return;
                if (value < 0) return;
                _font.Size = value;
                RaisePropertyChanged("Size");
            }
        }

        public FontFamily FontAsset
        {
            get
            {
                if (Asset.SelectedAsset == null) return null;
                using (var pf = new PrivateFontCollection())
                {
                    pf.AddFontFile(Asset.SafePath);
                    if (pf.Families.Length == 0)
                    {
                        return null;
                    }

                    return new FontFamily("file:///" + Path.GetDirectoryName(Asset.SafePath) + "/#" + pf.Families[0].Name);
                }
            }
        }

        public static void SetFont(Asset font)
        {
        }
    }
}