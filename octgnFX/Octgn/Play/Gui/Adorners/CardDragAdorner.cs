using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Octgn.Play.Gui.Adorners
{
    internal class CardDragAdorner : Adorner, IDisposable
    {
        private static readonly Duration EmptyDuration = TimeSpan.Zero;
        private static readonly Duration DefaultAnimationDuration = TimeSpan.FromMilliseconds(200);
        private static readonly DoubleAnimation Anim = new DoubleAnimation {Duration = DefaultAnimationDuration};
        public readonly CardControl SourceCard;
        private readonly Point _basePt;
        private readonly Rectangle _child = new Rectangle();
        private readonly Brush _faceDownBrush;
        private readonly Brush _faceUpBrush;
        private readonly ScaleTransform _invertTransform;
        private readonly SolidColorBrush _lightRedBrush;
        private readonly CardOrientation _rot;
        private readonly RotateTransform _rot180Transform;
        private readonly RotateTransform _rot90Transform;
        private readonly TranslateTransform _translate;
        internal bool OnHoverRequestInverted;

        private bool _canDrop = true, _faceUp;
        private Vector _mouseAdjustment;
        private Vector _mouseOffset;
        private Vector _offset;
        private TranslateTransform _offsetTransform;

        public CardDragAdorner(CardControl sourceCard, Vector mousePoint)
            : this(sourceCard, sourceCard, mousePoint)
        {
        }


        //fix MAINWINDOW bug
        public CardDragAdorner(CardControl anchor, CardControl sourceCard, Vector mousePoint)
            : base(WindowManager.PlayWindow.Content as UIElement)
        {
            SourceCard = sourceCard;
            bool isCardInverted = anchor.IsOnTableCanvas && (Player.LocalPlayer.InvertedTable ^ anchor.IsInverted);
            Point cardOrigin;
            if (isCardInverted)
            {
                cardOrigin = new Point(Program.GameEngine.Definition.CardWidth, Program.GameEngine.Definition.CardHeight);
                _mouseOffset = new Vector(Program.GameEngine.Definition.CardWidth - mousePoint.X, Program.GameEngine.Definition.CardHeight - mousePoint.Y);
            }
            else
            {
                cardOrigin = new Point();
                _mouseOffset = mousePoint;
            }
            //fix MAINWINDOW bug
            _basePt = anchor.TranslatePoint(cardOrigin, WindowManager.PlayWindow.Content as UIElement);

            _faceUp = sourceCard.IsAlwaysUp || sourceCard.Card.FaceUp;
            _lightRedBrush = Brushes.Red.Clone();
            _faceDownBrush = new ImageBrush(sourceCard.Card.GetBitmapImage(false));

            var bim = new BitmapImage();
            bim.BeginInit();
            bim.CacheOption = BitmapCacheOption.OnLoad;
            bim.UriSource = new Uri(Program.GameEngine.Definition.CardFront);
            bim.EndInit();

            _faceUpBrush = _faceUp
                               ? new VisualBrush(sourceCard.GetCardVisual())
                                     {
                                         Viewbox = new Rect(0, 0, sourceCard.ActualWidth, sourceCard.ActualHeight),
                                         ViewboxUnits = BrushMappingMode.Absolute
                                     }
                               : (Brush)
                                 new ImageBrush(bim);
            _invertTransform = new ScaleTransform {CenterX = 0.5, CenterY = 0.5};
            _faceUpBrush.RelativeTransform = _invertTransform;
            if (_faceUpBrush is VisualBrush)
                RenderOptions.SetCachingHint(_faceUpBrush, CachingHint.Cache);

            _child.BeginInit();
            _child.Width = anchor.ActualWidth*CardControl.ScaleFactor.Width;
            _child.Height = anchor.ActualHeight*CardControl.ScaleFactor.Height;
            _child.Fill = _faceUp ? _faceUpBrush : _faceDownBrush;
            _child.StrokeThickness = 3;

            var transforms = new TransformGroup();
            _child.RenderTransform = transforms;
            _rot = sourceCard.Card.Orientation;
            if ((_rot & CardOrientation.Rot180) != 0)
            {
                _rot180Transform = new RotateTransform(180, _child.Width/2, _child.Height/2);
                transforms.Children.Add(_rot180Transform);
            }
            if ((_rot & CardOrientation.Rot90) != 0)
            {
                _rot90Transform = isCardInverted
                                      ? new RotateTransform(90, _child.Width/2, _child.Width/2)
                                      : new RotateTransform(90, _child.Width/2, _child.Height - _child.Width/2);
                transforms.Children.Add(_rot90Transform);
            }
            _translate = new TranslateTransform();
            transforms.Children.Add(_translate);

            _child.IsHitTestVisible = false;
            _child.EndInit();
            AddVisualChild(_child);

            var animation = new DoubleAnimation(0.55, 0.75, new Duration(TimeSpan.FromMilliseconds(500)))
                                {AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever};
            animation.Freeze();

            _faceUpBrush.BeginAnimation(Brush.OpacityProperty, animation);
            _faceDownBrush.BeginAnimation(Brush.OpacityProperty, animation);
            _lightRedBrush.BeginAnimation(Brush.OpacityProperty, animation);
        }

        private bool CanDrop
        {
            set
            {
                if (value == _canDrop) return;
                _canDrop = value;
                if (value)
                {
                    _child.Fill = _faceUp ? _faceUpBrush : _faceDownBrush;
                    _child.Stroke = null;
                }
                else
                {
                    _child.Fill = _lightRedBrush;
                    _child.Stroke = Brushes.Red;
                }
            }
        }

        private bool FaceUp
        {
            set
            {
                if (_faceUp == value) return;
                _faceUp = value;
                if (_canDrop)
                    _child.Fill = value ? _faceUpBrush : _faceDownBrush;
            }
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _faceUpBrush.BeginAnimation(Brush.OpacityProperty, null);
            _faceDownBrush.BeginAnimation(Brush.OpacityProperty, null);
            _lightRedBrush.BeginAnimation(Brush.OpacityProperty, null);
        }

        #endregion

        public void SetState(double dx, double dy, bool canDrop, Size size, bool faceUp, bool isInverted)
        {
            CanDrop = canDrop;
            FaceUp = faceUp;
            _offset.X = dx;
            _offset.Y = dy;

            _invertTransform.ScaleX = _invertTransform.ScaleY = isInverted ? -1 : 1;

            if (size != Size.Empty &&
                (Math.Abs(size.Width - _child.Width) > double.Epsilon ||
                 Math.Abs(size.Height - _child.Height) > float.Epsilon))
            {
                _mouseAdjustment +=
                    new Vector(_mouseOffset.X*(size.Width - _child.Width)/Program.GameEngine.Definition.CardWidth,
                               _mouseOffset.Y*(size.Height - _child.Height)/
                               Program.GameEngine.Definition.CardHeight);
                if (_offsetTransform != null)
                {
                    _offsetTransform.X *= size.Width/_child.Width;
                    _offsetTransform.Y *= size.Height/_child.Height;
                }
                _child.Width = size.Width;
                _child.Height = size.Height;
                if (_rot90Transform != null)
                {
                    _rot90Transform.CenterX = _child.Width/2;
                    _rot90Transform.CenterY = _child.Height - _child.Width/2;
                }
                if (_rot180Transform != null)
                {
                    _rot180Transform.CenterX = _child.Width/2;
                    _rot180Transform.CenterY = _child.Height/2;
                }
            }

            var layer = Parent as AdornerLayer;
            if (layer != null) layer.Update();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _child.Measure(constraint);
            return _child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(_basePt + _offset - _mouseAdjustment, finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }

        public void OffsetBy(double dx, double dy)
        {
            _offsetTransform = new TranslateTransform(dx, dy);
            ((TransformGroup) _child.RenderTransform).Children.Add(_offsetTransform);
        }

        public void CollapseTo(double dx, double dy)
        {
            CollapseTo(dx, dy, true);
        }

        public void CollapseTo(double dx, double dy, bool animate)
        {
            if (Player.LocalPlayer.InvertedTable)
            {
                dx = -dx;
                dy = -dy;
            }
            if (_offsetTransform != null)
            {
                dx -= _offsetTransform.X;
                dy -= _offsetTransform.Y;
            }

            if (!animate)
                Anim.Duration = EmptyDuration;
            Anim.FillBehavior = FillBehavior.HoldEnd;
            Anim.AccelerationRatio = 0.8;
            Anim.To = 0;
            _child.BeginAnimation(OpacityProperty, Anim);
            Anim.AccelerationRatio = 0.3;
            Anim.To = dx;
            _translate.BeginAnimation(TranslateTransform.XProperty, Anim);
            Anim.To = dy;
            _translate.BeginAnimation(TranslateTransform.YProperty, Anim);
            if (!animate)
                Anim.Duration = DefaultAnimationDuration;
        }

        public void Expand()
        {
            Anim.FillBehavior = FillBehavior.Stop;
            Anim.AccelerationRatio = 0.3;
            Anim.To = 1;
            _child.BeginAnimation(OpacityProperty, Anim);
            Anim.To = 0;
            _translate.BeginAnimation(TranslateTransform.XProperty, Anim);
            _translate.BeginAnimation(TranslateTransform.YProperty, Anim);
        }
    }
}