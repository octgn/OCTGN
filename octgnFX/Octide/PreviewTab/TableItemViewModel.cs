// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;

using Octgn.DataNew.Entities;

namespace Octide.ViewModel
{
    public class TableItemViewModel : ViewModelBase
    {
        private Game _game;
        private double zoom;
        private Vector offset;
        private ImageBrush backgroundImage;

        public string Name => "Table";
        
        public TableItemViewModel()
        {
            _game = ViewModelLocator.GameLoader.Game;
            Zoom = 1;
            CenterView();
            SetBackground();
        }
        
        public double Zoom
        {
            get
            {
                return this.zoom;
            }
            set
            {
                this.zoom = value;
                this.RaisePropertyChanged("Zoom");
            }
        }

        public Vector Offset
        {
            get
            {
                return this.offset;
            }
            set
            {
                this.offset = value;
                this.RaisePropertyChanged("Offset");
            }
        }

        public ImageBrush BackgroundImage
        {
            get
            {
                return this.backgroundImage;
            }
            set
            {
                this.backgroundImage = value;
                this.RaisePropertyChanged("BackgroundImage");
            }
        }

        public Asset Background
        {
            get
            {
                return Asset.Load(_game.Table.Background);
            }
            set
            {
                _game.Table.Background = value?.FullPath;
                SetBackground();
                this.RaisePropertyChanged("Background");
            }
        }
        
        public string BackgroundStyle
        {
            get
            {
                return _game.Table.BackgroundStyle;
            }
            set
            {
                if (_game.Table.BackgroundStyle == value) return;
                _game.Table.BackgroundStyle = value;
                SetBackground();
                this.RaisePropertyChanged("BackgroundStyle");
            }
        }
        
        public int Width
        {
            get
            {
                return _game.Table.Width;
            }
            set
            {
                if (value > 3000) value = 3000;
                if (value < 100) value = 100;
                _game.Table.Width = value;

                this.RaisePropertyChanged("Width");
                CenterView();
            }
        }

        public int Height
        {
            get
            {
                return _game.Table.Height;
            }
            set
            {
                 if (value > 3000) value = 3000;
                 if (value < 100) value = 100;
                _game.Table.Height = value;

                this.RaisePropertyChanged("Height");
                CenterView();
            }
        }
        
        internal void SetBackground()
        {
            if (!DispatcherHelper.UIDispatcher.CheckAccess())
            {
                DispatcherHelper.UIDispatcher.Invoke(new Action(this.SetBackground));
                return;
            }
            if (ViewModelLocator.GameLoader.Game == null || ViewModelLocator.GameLoader.Game.Table == null) return;
            var bim = new BitmapImage();
            bim.BeginInit();
            bim.CacheOption = BitmapCacheOption.OnLoad;
            bim.UriSource = new Uri(_game.Table.Background);
            bim.EndInit();

            var backBrush = new ImageBrush(bim);
            if (!String.IsNullOrWhiteSpace(_game.Table.BackgroundStyle))
                switch (_game.Table.BackgroundStyle)
                {
                    case "tile":
                        backBrush.TileMode = TileMode.Tile;
                        backBrush.Viewport = new Rect(0, 0, backBrush.ImageSource.Width, backBrush.ImageSource.Height);
                        backBrush.ViewportUnits = BrushMappingMode.Absolute;
                        break;
                    case "uniform":
                        backBrush.Stretch = Stretch.Uniform;
                        break;
                    case "uniformToFill":
                        backBrush.Stretch = Stretch.UniformToFill;
                        break;
                    case "stretch":
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