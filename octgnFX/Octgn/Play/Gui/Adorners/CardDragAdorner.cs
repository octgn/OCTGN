using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Octgn.Play.Gui
{
    internal class CardDragAdorner : Adorner, IDisposable
    {
        public readonly CardControl SourceCard;
        private Rectangle child = new Rectangle();
        private Vector offset = new Vector(), mouseOffset, mouseAdjustment = new Vector();
        private Point basePt;
        private Brush faceUpBrush, faceDownBrush;
        private SolidColorBrush lightRedBrush;
        private CardOrientation rot;
        private RotateTransform rot180Transform, rot90Transform;
        private TranslateTransform translate, offsetTransform;
        private ScaleTransform invertTransform;
        private static readonly Duration emptyDuration = TimeSpan.Zero;
        private static readonly Duration defaultAnimationDuration = TimeSpan.FromMilliseconds(200);
        private static readonly DoubleAnimation anim = new DoubleAnimation() { Duration = defaultAnimationDuration };
        internal bool onHoverRequestInverted;

        bool _canDrop = true, _faceUp;

        bool CanDrop
        {
            set
            {
                if (value == _canDrop) return;
                _canDrop = value;
                if (value)
                {
                    child.Fill = _faceUp ? faceUpBrush : faceDownBrush;
                    child.Stroke = null;
                }
                else
                {
                    child.Fill = lightRedBrush;
                    child.Stroke = Brushes.Red;
                }
            }
        }

        bool FaceUp
        {
            set
            {
                if (_faceUp == value) return;
                _faceUp = value;
                if (_canDrop)
                    child.Fill = value ? faceUpBrush : faceDownBrush;
            }
        }

        public CardDragAdorner(CardControl sourceCard, Vector mousePoint)
            : this(sourceCard, sourceCard, mousePoint)
        { }


        //fix MAINWINDOW bug
        public CardDragAdorner(CardControl anchor, CardControl sourceCard, Vector mousePoint)
            : base(Program.PlayWindow.Content as UIElement)
        {
            SourceCard = sourceCard;
            bool isCardInverted = anchor.IsOnTableCanvas && (Player.LocalPlayer.InvertedTable ^ anchor.IsInverted);
            Point cardOrigin;
            if (isCardInverted)
            {
                var cardDef = Program.Game.Definition.CardDefinition;
                cardOrigin = new Point(cardDef.Width, cardDef.Height);
                mouseOffset = new Vector(cardDef.Width - mousePoint.X, cardDef.Height - mousePoint.Y);
            }
            else
            {
                cardOrigin = new Point();
                mouseOffset = mousePoint;
            }
            //fix MAINWINDOW bug
            basePt = anchor.TranslatePoint(cardOrigin, Program.PlayWindow.Content as UIElement);

            _faceUp = sourceCard.IsAlwaysUp ? true : sourceCard.Card.FaceUp;
            lightRedBrush = Brushes.Red.Clone();
            faceDownBrush = new ImageBrush(sourceCard.Card.GetBitmapImage(false));
            faceUpBrush = _faceUp ?
                (Brush)new VisualBrush(sourceCard.GetCardVisual()) { Viewbox = new Rect(0, 0, sourceCard.ActualWidth, sourceCard.ActualHeight), ViewboxUnits = BrushMappingMode.Absolute } :
                (Brush)new ImageBrush(new BitmapImage(new Uri(Program.Game.Definition.CardDefinition.Front)));
            invertTransform = new ScaleTransform() { CenterX = 0.5, CenterY = 0.5 };
            faceUpBrush.RelativeTransform = invertTransform;
            if (faceUpBrush is VisualBrush)
                RenderOptions.SetCachingHint(faceUpBrush, CachingHint.Cache);

            child.BeginInit();
            child.Width = anchor.ActualWidth * CardControl.scaleFactor.Width;
            child.Height = anchor.ActualHeight * CardControl.scaleFactor.Height;
            child.Fill = _faceUp ? faceUpBrush : faceDownBrush;
            child.StrokeThickness = 3;

            var transforms = new TransformGroup();
            child.RenderTransform = transforms;
            rot = sourceCard.Card.Orientation;
            if ((rot & CardOrientation.Rot180) != 0)
            {
                rot180Transform = new RotateTransform(180, child.Width / 2, child.Height / 2);
                transforms.Children.Add(rot180Transform);
            }
            if ((rot & CardOrientation.Rot90) != 0)
            {
                rot90Transform = isCardInverted ?
                  new RotateTransform(90, child.Width / 2, child.Width / 2) :
                  new RotateTransform(90, child.Width / 2, child.Height - child.Width / 2);
                transforms.Children.Add(rot90Transform);
            }
            translate = new TranslateTransform();
            transforms.Children.Add(translate);

            child.IsHitTestVisible = false;
            child.EndInit();
            AddVisualChild(child);

            DoubleAnimation animation = new DoubleAnimation(0.55, 0.75, new Duration(TimeSpan.FromMilliseconds(500)));
            animation.AutoReverse = true;
            animation.RepeatBehavior = RepeatBehavior.Forever;
            animation.Freeze();

            faceUpBrush.BeginAnimation(Brush.OpacityProperty, animation);
            faceDownBrush.BeginAnimation(Brush.OpacityProperty, animation);
            lightRedBrush.BeginAnimation(Brush.OpacityProperty, animation);
        }

        public void SetState(double dx, double dy, bool canDrop, Size size, bool faceUp, bool isInverted)
        {
            CanDrop = canDrop; FaceUp = faceUp;
            offset.X = dx; offset.Y = dy;

            invertTransform.ScaleX = invertTransform.ScaleY = isInverted ? -1 : 1;

            if (size != Size.Empty && (size.Width != child.Width || size.Height != child.Height))
            {
                mouseAdjustment +=
                    new Vector(mouseOffset.X * (size.Width - child.Width) / Program.Game.Definition.CardDefinition.Width,
                                mouseOffset.Y * (size.Height - child.Height) / Program.Game.Definition.CardDefinition.Height);
                if (offsetTransform != null)
                {
                    offsetTransform.X *= size.Width / child.Width;
                    offsetTransform.Y *= size.Height / child.Height;
                }
                child.Width = size.Width; child.Height = size.Height;
                if (rot90Transform != null)
                {
                    rot90Transform.CenterX = child.Width / 2;
                    rot90Transform.CenterY = child.Height - child.Width / 2;
                }
                if (rot180Transform != null)
                {
                    rot180Transform.CenterX = child.Width / 2;
                    rot180Transform.CenterY = child.Height / 2;
                }
            }

            AdornerLayer layer = this.Parent as AdornerLayer;
            if (layer != null) layer.Update();
        }

        public void Dispose()
        {
            faceUpBrush.BeginAnimation(Brush.OpacityProperty, null);
            faceDownBrush.BeginAnimation(Brush.OpacityProperty, null);
            lightRedBrush.BeginAnimation(Brush.OpacityProperty, null);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            child.Measure(constraint);
            return child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            child.Arrange(new Rect(basePt + offset - mouseAdjustment, finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        { return child; }

        protected override int VisualChildrenCount
        { get { return 1; } }

        public void OffsetBy(double dx, double dy)
        {
            offsetTransform = new TranslateTransform(dx, dy);
            ((TransformGroup)child.RenderTransform).Children.Add(offsetTransform);
        }

        public void CollapseTo(double dx, double dy)
        {
            CollapseTo(dx, dy, true);
        }

        public void CollapseTo(double dx, double dy, bool animate)
        {
            if (Player.LocalPlayer.InvertedTable)
            { dx = -dx; dy = -dy; }
            if (offsetTransform != null)
            { dx -= offsetTransform.X; dy -= offsetTransform.Y; }

            if (!animate)
                anim.Duration = emptyDuration;
            anim.FillBehavior = FillBehavior.HoldEnd;
            anim.AccelerationRatio = 0.8; anim.To = 0;
            child.BeginAnimation(UIElement.OpacityProperty, anim);
            anim.AccelerationRatio = 0.3;
            anim.To = dx; translate.BeginAnimation(TranslateTransform.XProperty, anim);
            anim.To = dy; translate.BeginAnimation(TranslateTransform.YProperty, anim);
            if (!animate)
                anim.Duration = defaultAnimationDuration;
        }

        public void Expand()
        {
            anim.FillBehavior = FillBehavior.Stop; anim.AccelerationRatio = 0.3;
            anim.To = 1;
            child.BeginAnimation(UIElement.OpacityProperty, anim);
            anim.To = 0;
            translate.BeginAnimation(TranslateTransform.XProperty, anim);
            translate.BeginAnimation(TranslateTransform.YProperty, anim);
        }
    }
}