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

    public class TableTabViewModel : ViewModelBase
    {
        private double angle;

        private double zoom;

        private Vector offset;

        private string boardImage;

        private ImageBrush background;

        private readonly string[] backgroundStyles = new string[4] { "tile", "uniform", "uniformToFill", "stretch" };
        private ObservableCollection<CardViewModel> _cards;

        public List<Asset> Images
        {
            get
            {
                var ret = AssetManager.Instance.Assets.Where(x=>x.Type == AssetType.Image).ToList();
                RaisePropertyChanged("BackgroundImageAsset");
                return ret;
            }
        }

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

        public double BoardWidth
        {
            get
            {
                return ViewModelLocator.GameLoader.ValidGame ? ViewModelLocator.GameLoader.Game.Table.BoardPosition.Width : 200;
            }
            set
            {
                if (ViewModelLocator.GameLoader.ValidGame)
                {
                    if (value > 4000) value = 4000;
                    if (value < 5) value = 5;
                    ViewModelLocator.GameLoader.Game.Table.BoardPosition.Width = value;
                }

                this.RaisePropertyChanged("BoardWidth");
                this.RaisePropertyChanged("BoardMargin");
                if (ViewModelLocator.GameLoader.ValidGame)
                    CenterView(ViewModelLocator.GameLoader.Game);
            }
        }

        public double BoardHeight
        {
            get
            {
                return ViewModelLocator.GameLoader.ValidGame ? ViewModelLocator.GameLoader.Game.Table.BoardPosition.Height : 200;
            }
            set
            {
                if (ViewModelLocator.GameLoader.ValidGame)
                {
                    if (value > 4000) value = 4000;
                    if (value < 5) value = 5;
                    ViewModelLocator.GameLoader.Game.Table.BoardPosition.Height = value;
                }

                this.RaisePropertyChanged("BoardHeight");
                this.RaisePropertyChanged("BoardMargin");
                if (ViewModelLocator.GameLoader.ValidGame)
                    CenterView(ViewModelLocator.GameLoader.Game);
            }
        }

        public Thickness BoardMargin
        {
            get
            {
                var ret = new Rect();
                if (ViewModelLocator.GameLoader.ValidGame)
                {
                    ret = new Rect(
                        ViewModelLocator.GameLoader.Game.Table.BoardPosition.X,
                        ViewModelLocator.GameLoader.Game.Table.BoardPosition.Y,
                        ViewModelLocator.GameLoader.Game.Table.BoardPosition.Width,
                        ViewModelLocator.GameLoader.Game.Table.BoardPosition.Height);
                }
                return new Thickness(ret.Left, ret.Top, 0, 0);
            }
        }

        public double BoardX
        {
            get
            {
                return ViewModelLocator.GameLoader.ValidGame ? ViewModelLocator.GameLoader.Game.Table.BoardPosition.X : 0;
            }
            set
            {
                if (ViewModelLocator.GameLoader.ValidGame)
                {
                    if (value > 4000) value = 4000;
                    if (value < -4000) value = -4000;
                    ViewModelLocator.GameLoader.Game.Table.BoardPosition.X = value;
                }

                this.RaisePropertyChanged("BoardX");
                this.RaisePropertyChanged("BoardMargin");
                if (ViewModelLocator.GameLoader.ValidGame)
                    CenterView(ViewModelLocator.GameLoader.Game);
            }
        }

        public double BoardY
        {
            get
            {
                return ViewModelLocator.GameLoader.ValidGame ? ViewModelLocator.GameLoader.Game.Table.BoardPosition.Y : 0;
            }
            set
            {
                if (ViewModelLocator.GameLoader.ValidGame)
                {
                    if (value > 4000) value = 4000;
                    if (value < -4000) value = -4000;
                    ViewModelLocator.GameLoader.Game.Table.BoardPosition.Y = value;
                }

                this.RaisePropertyChanged("BoardY");
                this.RaisePropertyChanged("BoardMargin");
                if (ViewModelLocator.GameLoader.ValidGame)
                    CenterView(ViewModelLocator.GameLoader.Game);
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
                var gl = ViewModelLocator.GameLoader;
				if(!gl.ValidGame || gl.Game.Table == null)
                    return new Asset();
                var ret = Asset.Load(gl.Game.Table.Background);
                return ret;
            }
            set
            {
                if (value == null) return;
                if (!File.Exists(value.FullPath)) return;
                var def = ViewModelLocator.GameLoader.Game;
                if (def.Table == null) return;

                def.Table.Background = value.FullPath;
                SetBackground();
                this.RaisePropertyChanged("BackgroundImageAsset");
            }
        }

        public string BoardBackgroundImage
        {
            get
            {
                return BoardBackgroundImageAsset.FullPath;
            }
        }

        public Asset BoardBackgroundImageAsset
        {
            get
            {
                var gl = ViewModelLocator.GameLoader;
				if(!gl.ValidGame || gl.Game.Table == null)
                    return new Asset();
                var ret = Asset.Load(gl.Game.Table.Board);
                return ret;
            }
            set
            {
                if (value == null) return;
                if (!File.Exists(value.FullPath)) return;
                var def = ViewModelLocator.GameLoader.Game;
                if (def.Table == null) return;

                def.Table.Board = value.FullPath;
                SetBackground();
                this.RaisePropertyChanged("BoardBackgroundImageAsset");
                this.RaisePropertyChanged("BoardBackgroundImage");
            }
        }

        public string BackgroundStyle
        {
            get
            {
                if (ViewModelLocator.GameLoader.ValidGame == false) return String.Empty;
                return BackgroundStyles.FirstOrDefault(x => x == ViewModelLocator.GameLoader.Game.Table.BackgroundStyle);
            }
            set
            {
                if (BackgroundStyles.Any(x => x == value) && ViewModelLocator.GameLoader.ValidGame)
                {
                    ViewModelLocator.GameLoader.Game.Table.BackgroundStyle = BackgroundStyles.FirstOrDefault(x => x == value);
                    SetBackground();
                }
                this.RaisePropertyChanged("BackgroundStyle");
            }
        }

        public ObservableCollection<CardViewModel> Cards
        {
            get { return _cards; }
            set
            {
                if (value.Equals(_cards)) return;
                _cards = value;
                RaisePropertyChanged("Cards");
            }
        }

        public int Width
        {
            get
            {
                return ViewModelLocator.GameLoader.ValidGame ? ViewModelLocator.GameLoader.Game.Table.Width : 200;
            }
            set
            {
                if (ViewModelLocator.GameLoader.ValidGame)
                {
                    if (value > 4000) value = 4000;
                    if (value < 5) value = 5;
                    ViewModelLocator.GameLoader.Game.Table.Width = value;
                }

                this.RaisePropertyChanged("Width");
                if (ViewModelLocator.GameLoader.ValidGame)
                    CenterView(ViewModelLocator.GameLoader.Game);
            }
        }

        public int Height
        {
            get
            {
                return ViewModelLocator.GameLoader.ValidGame ? ViewModelLocator.GameLoader.Game.Table.Height : 200;
            }
            set
            {
                if (ViewModelLocator.GameLoader.ValidGame)
                {
                    if (value > 4000) value = 4000;
                    if (value < 5) value = 5;
                    ViewModelLocator.GameLoader.Game.Table.Height = value;
                }

                this.RaisePropertyChanged("Height");
                if (ViewModelLocator.GameLoader.ValidGame)
                    CenterView(ViewModelLocator.GameLoader.Game);
            }
        }

        public string CardBack
        {
            get
            {
                return ViewModelLocator.GameLoader.ValidGame ? ViewModelLocator.GameLoader.Game.CardBack : "";
            }
            set
            {
                if (ViewModelLocator.GameLoader.ValidGame)
                {
                    ViewModelLocator.GameLoader.Game.CardBack = value;
                }

                this.RaisePropertyChanged("CardBack");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public int CardWidth
        {
            get
            {
                return ViewModelLocator.GameLoader.ValidGame ? ViewModelLocator.GameLoader.Game.CardWidth : 50;
            }
            set
            {
                if (ViewModelLocator.GameLoader.ValidGame)
                {
                    if (value > 4000) value = 4000;
                    if (value < 5) value = 5;
                    ViewModelLocator.GameLoader.Game.CardWidth = value;
                }

                this.RaisePropertyChanged("CardWidth");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public int CardHeight
        {
            get
            {
                return ViewModelLocator.GameLoader.ValidGame ? ViewModelLocator.GameLoader.Game.CardHeight : 50;
            }
            set
            {
                if (ViewModelLocator.GameLoader.ValidGame)
                {
                    if (value > 4000) value = 4000;
                    if (value < 5) value = 5;
                    ViewModelLocator.GameLoader.Game.CardHeight = value;
                }

                this.RaisePropertyChanged("CardHeight");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public string[] BackgroundStyles
        {
            get
            {
                return this.backgroundStyles;
            }
        }

        public TableTabViewModel()
        {
            Zoom = 1;
            Cards = new ObservableCollection<CardViewModel>();
            Cards.Add(new CardViewModel());
            Messenger.Default.Register<PropertyChangedMessage<Game>>(this, x => this.RefreshValues());
            Messenger.Default.Register<AssetManagerUpdatedMessage>(this,
                x =>
                {
                    RaisePropertyChanged("Images");
                    RaisePropertyChanged("BackgroundImageAsset");
                });
            Messenger.Default.Register<MouseWheelTableZoom>(this, OnMouseWheelTableZoom);
            this.RefreshValues();
        }

        internal void RefreshValues()
        {
            var def = ViewModelLocator.GameLoader.Game;
            if (def.Table == null) return;
            BoardWidth = def.Table.BoardPosition.Width;
            BoardHeight = def.Table.BoardPosition.Height;
            BoardBackgroundImageAsset = Asset.Load(def.Table.Board);
            Width = def.Table.Width;
            Height = def.Table.Height;
            CardBack = def.CardBack;
            CardWidth = def.CardWidth;
            CardHeight = def.CardHeight;
            BackgroundImageAsset = Asset.Load(def.Table.Background);

            CenterView(def);
            SetBackground();
            RaisePropertyChanged("Images");
            RaisePropertyChanged("BackgroundImageAsset");
            RaisePropertyChanged("BackgroundStyle");
            RaisePropertyChanged("");
        }

        public void CenterView(Game game)
        {
            var tableDef = game.Table;
            Offset = new Vector(tableDef.Width / 2, tableDef.Height / 2);
        }

        internal void SetBackground()
        {
            if (!DispatcherHelper.UIDispatcher.CheckAccess())
            {
                DispatcherHelper.UIDispatcher.Invoke(new Action(this.SetBackground));
                return;
            }
            if (ViewModelLocator.GameLoader.Game == null || ViewModelLocator.GameLoader.Game.Table == null) return;
            var tableDef = ViewModelLocator.GameLoader.Game.Table;
            var bim = new BitmapImage();
            bim.BeginInit();
            bim.CacheOption = BitmapCacheOption.OnLoad;
            bim.UriSource = new Uri(tableDef.Background);
            bim.EndInit();

            var backBrush = new ImageBrush(bim);
            if (!String.IsNullOrWhiteSpace(tableDef.BackgroundStyle))
                switch (tableDef.BackgroundStyle)
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

        public void NewCard()
        {
            this.Cards.Add(new CardViewModel());
        }

        public void ResetCards()
        {
            this.Cards.Clear();
            this.Cards.Add(new CardViewModel());
        }
    }
}