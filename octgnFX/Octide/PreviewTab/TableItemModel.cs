// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;

using Octgn.DataNew.Entities;
using Octgn.Library;
using Octide.ViewModel;

namespace Octide.ItemModel
{
    public class TableItemModel : PileItemModel
    {
        private double _zoom;
        private Vector _offset;
        private ImageBrush _backgroundImage;

        public AssetController Background { get; set; }

        public TableItemModel(Group group, IdeCollection<IdeBaseItem> src) : base(group, src)
        {
            Background = new AssetController(AssetType.Image);
            Background.Register(group.Background);
            Background.PropertyChanged += BackgroundAssetUpdated;
            Zoom = 1;
            CenterView();
            SetBackground();
        }

        private void BackgroundAssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _group.Background = Asset.FullPath;
                SetBackground();
                RaisePropertyChanged("Background");
            }
        }

        public double Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                _zoom = value;
                RaisePropertyChanged("Zoom");
            }
        }

        public Vector Offset
        {
            get
            {
                return _offset;
            }
            set
            {
                _offset = value;
                RaisePropertyChanged("Offset");
            }
        }

        public ImageBrush BackgroundImage
        {
            get
            {
                return _backgroundImage;
            }
            set
            {
                _backgroundImage = value;
                RaisePropertyChanged("BackgroundImage");
            }
        }

        public BackgroundStyle BackgroundStyle
        {
            get
            {
                return _group.BackgroundStyle;
            }
            set
            {
                if (_group.BackgroundStyle == value) return;
                _group.BackgroundStyle = value;
                SetBackground();
                RaisePropertyChanged("BackgroundStyle");
            }
        }

        public int Width
        {
            get
            {
                return _group.Width;
            }
            set
            {
                if (_group.Width == value) return;
                _group.Width = value;

                RaisePropertyChanged("Width");
                CenterView();
            }
        }

        public int Height
        {
            get
            {
                return _group.Height;
            }
            set
            {
                if (_group.Height == value) return;
                _group.Height = value;

                RaisePropertyChanged("Height");
                CenterView();
            }
        }

        internal void SetBackground()
        {
            if (Background.SafePath == null)
            {
                BackgroundImage = null;
                return;
            }
            var bim = new BitmapImage();
            bim.BeginInit();
            bim.CacheOption = BitmapCacheOption.OnLoad;
            bim.UriSource = new Uri(Background.SafePath);
            bim.EndInit();

            var backBrush = new ImageBrush(bim);
            switch (_group.BackgroundStyle)
            {
                case BackgroundStyle.Tile:
                    backBrush.TileMode = TileMode.Tile;
                    backBrush.Viewport = new Rect(0, 0, backBrush.ImageSource.Width, backBrush.ImageSource.Height);
                    backBrush.ViewportUnits = BrushMappingMode.Absolute;
                    break;
                case BackgroundStyle.Uniform:
                    backBrush.Stretch = Stretch.Uniform;
                    break;
                case BackgroundStyle.UniformToFill:
                    backBrush.Stretch = Stretch.UniformToFill;
                    break;
                case BackgroundStyle.Stretch:
                    backBrush.Stretch = Stretch.Fill;
                    break;
            }
            BackgroundImage = backBrush;
        }

        public void CenterView()
        {
            Offset = new Vector(Width / 2, Height / 2);
        }
    }
}