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

namespace Octide.ViewModel
{
    //TODO: Fix asset always setting a default value for missing font def
    public class GameFontItemModel : ViewModelBase
    {
        public Font _font;
        public RelayCommand RemoveFontCommand { get; set; }

        public GameFontItemModel(Font f)
        {
            _font = f;
            RemoveFontCommand = new RelayCommand(RemoveFont);
            RaisePropertyChanged("FontControlVisibility");
            RaisePropertyChanged("Asset");
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
                if (value <= 0) return;
                _font.Size = value;
                RaisePropertyChanged("Size");
            }
        }

        public Asset Asset
        {
            get
            {
                if (_font == null) return null;

                return Asset.Load(_font.Src);
            }
            set
            {
                if (_font == null)
                    _font = new Font();
                _font.Src = value?.FullPath;
                ViewModelLocator.GameFontTabViewModel.UpdateFonts();
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("FontAsset");
                RaisePropertyChanged("Size");
                RaisePropertyChanged("FontControlVisibility");
            }
        }

        public FontFamily FontAsset
        {
            get
            {
                if (_font == null) return null;
                using (var pf = new PrivateFontCollection())
                {
                    pf.AddFontFile(Asset.FullPath);
                    if (pf.Families.Length == 0)
                    {
                        return null;
                    }

                    return new FontFamily("file:///" + Path.GetDirectoryName(Asset.FullPath) + "/#" + pf.Families[0].Name);
                }
            }
        }

        public void RemoveFont()
        {
            _font = null;
            ViewModelLocator.GameFontTabViewModel.UpdateFonts();
            RaisePropertyChanged("FontControlVisibility");
            RaisePropertyChanged("Size");
            RaisePropertyChanged("Asset");
        }

        public Visibility FontControlVisibility
        {
            get
            {
                return _font == null ? Visibility.Hidden : Visibility.Visible;
            }
        }
        public static void SetFont(Asset font)
        {
        }
    }
}