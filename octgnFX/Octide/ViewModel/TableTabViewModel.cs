namespace Octide.ViewModel
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using GalaSoft.MvvmLight.Threading;

    using Octgn.DataNew.Entities;

    using Octide.Messages;

    public class TableTabViewModel : ViewModelBase
    {
        private double angle;

        private double zoom;

        private Vector offset;

        private string boardImage;

        private double boardWidth;

        private double boardHeight;

        private Thickness boardMargin;

        private ImageBrush background;

        private double width;

        private double height;

        public double Angle
        {
            get
            {
                return this.angle;
            }
            set
            {
                if (value.Equals(this.angle))
                {
                    return;
                }
                this.angle = value;
                this.RaisePropertyChanged("Angle");
            }
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

        public string BoardImage
        {
            get
            {
                return this.boardImage;
            }
            set
            {
                this.boardImage = value;
                this.RaisePropertyChanged("BoardImage");
            }
        }

        public double BoardWidth
        {
            get
            {
                return this.boardWidth;
            }
            set
            {
                this.boardWidth = value;
                this.RaisePropertyChanged("BoardWidth");
            }
        }

        public double BoardHeight
        {
            get
            {
                return this.boardHeight;
            }
            set
            {
                this.boardHeight = value;
                this.RaisePropertyChanged("BoardHeight");
            }
        }

        public Thickness BoardMargin
        {
            get
            {
                return this.boardMargin;
            }
            set
            {
                this.boardMargin = value;
                this.RaisePropertyChanged("BoardMargin");
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

        public double Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
				this.RaisePropertyChanged("Width");
            }
        }

        public double Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
                this.RaisePropertyChanged("Height");
            }
        }

        public TableTabViewModel()
        {
            Zoom = 1;
            Messenger.Default.Register<PropertyChangedMessage<Game>>(this,x => this.RefreshValues());
            Messenger.Default.Register<MouseWheelTableZoom>(this, OnMouseWheelTableZoom);
			this.RefreshValues();
        }

        internal void RefreshValues()
        {
            var def = ViewModelLocator.GameLoader.Game;
            if (def.Table == null) return;
            BoardWidth = def.Table.BoardPosition.Width;
            BoardHeight = def.Table.BoardPosition.Height;
            var pos = new Rect(
                def.Table.BoardPosition.X,
                def.Table.BoardPosition.Y,
                def.Table.BoardPosition.Width,
                def.Table.BoardPosition.Height);
            BoardMargin = new Thickness(pos.Left, pos.Top, 0, 0);
            BoardImage = def.Table.Board;
            Width = def.Table.Width;
            Height = def.Table.Height;
			
            CenterView(def);
            if (def.Table.Background != null)
                DispatcherHelper.CheckBeginInvokeOnUI(()=>SetBackground(def.Table));
        }

        public void CenterView(Game game)
        {
            var tableDef = game.Table;
            Offset = new Vector(tableDef.Width / 2, tableDef.Height / 2);
        }

        internal void SetBackground(Group tableDef)
        {
            SetBackground(tableDef.Background, tableDef.BackgroundStyle);
        }

        internal void SetBackground(string url, string bs)
        {
            var bim = new BitmapImage();
            bim.BeginInit();
            bim.CacheOption = BitmapCacheOption.OnLoad;
            bim.UriSource = new Uri(url);
            bim.EndInit();

            var backBrush = new ImageBrush(bim);
            if (!String.IsNullOrWhiteSpace(bs))
                switch (bs)
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

        internal void OnMouseWheelTableZoom(MouseWheelTableZoom e)
        {
            double oldZoom = Zoom; // May be animated

            // Set the new zoom level
            if (e.Delta > 0)
                Zoom = oldZoom + 0.125;
            else if (oldZoom > 0.15)
                Zoom = oldZoom - 0.125;

            // Adjust the offset to center the zoom on the mouse pointer
            //double ratio = oldZoom - Zoom;
            //Offset += new Vector(e.Center.X * ratio, e.Center.Y * ratio);
        }
    }
}