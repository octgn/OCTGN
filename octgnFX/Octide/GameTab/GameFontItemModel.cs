// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Octgn.DataNew.Entities;

namespace Octide.ViewModel
{
    public class GameFontItemModel : ViewModelBase
    {
        public Font _font;
        public RelayCommand RemoveFontCommand { get; set; }

        public GameFontItemModel(Font f)
        {
            _font = f;
            RemoveFontCommand = new RelayCommand(RemoveFont);
            RaisePropertyChanged("FontControlVisibility");
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
                ViewModelLocator.GameTabViewModel.UpdateFonts();
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("Path");
                RaisePropertyChanged("Size");
                RaisePropertyChanged("FontControlVisibility");
            }
        }

        public void RemoveFont()
        {
            _font = null;
            ViewModelLocator.GameTabViewModel.UpdateFonts();
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
    }
}