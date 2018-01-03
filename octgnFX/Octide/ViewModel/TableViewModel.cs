using System.Collections.ObjectModel;

namespace Octide.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using GalaSoft.MvvmLight.Threading;

    using Octgn.DataNew.Entities;

    using Octide.Messages;

    public class TableViewModel : ViewModelBase
    {
        private Game _game;
        private double zoom;
        private Vector offset;
        private ImageBrush background;
        private readonly string[] backgroundStyles = new string[4] { "tile", "uniform", "uniformToFill", "stretch" };

        public string Name => "Table";

        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();
        
        public TableViewModel()
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
               
    

        public ImageBrush Background
        {
            get
            {
                return this.background;
            }
            set
            {
                this.background = value;
                this.RaisePropertyChanged("Background");
            }
        }

        public Asset BackgroundImageAsset
        {
            get
            {
				if(_game.Table.Background == null)
                    return new Asset();
                return Asset.Load(_game.Table.Background);
            }
            set
            {
                if (value == null) return;
                if (!File.Exists(value.FullPath)) return;
                _game.Table.Background = value.FullPath;
                SetBackground();
                this.RaisePropertyChanged("BackgroundImageAsset");
            }
        }
        
        public string BackgroundStyle
        {
            get
            {
                return BackgroundStyles.FirstOrDefault(x => x == _game.Table.BackgroundStyle);
            }
            set
            {
                _game.Table.BackgroundStyle = BackgroundStyles.FirstOrDefault(x => x == value);
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
        
        public string[] BackgroundStyles
        {
            get
            {
                return this.backgroundStyles;
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
            Background = backBrush;
        }


        public void CenterView()
        {
            Offset = new Vector(Width / 2, Height / 2);
        }
    }
}