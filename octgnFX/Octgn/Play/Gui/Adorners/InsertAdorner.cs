using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Octgn.Play.Gui
{
    internal class InsertAdorner : Adorner
    {
        private readonly Polygon bottomTriangle;
        private readonly Polygon topTriangle;
        private Point position;

        public InsertAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
            topTriangle = new Polygon
                              {
                                  Points =
                                      new PointCollection(new[] {new Point(), new Point(6, -10), new Point(-6, -10)}),
                                  Fill = Brushes.White,
                                  Stroke = Brushes.Black,
                                  StrokeThickness = 1,
                                  Effect = new DropShadowEffect {ShadowDepth = 3}
                              };
            bottomTriangle = new Polygon
                                 {
                                     Points =
                                         new PointCollection(new[] {new Point(), new Point(6, 10), new Point(-6, 10)}),
                                     Fill = Brushes.White,
                                     Stroke = Brushes.Black,
                                     StrokeThickness = 1,
                                     Effect = new DropShadowEffect {ShadowDepth = 2}
                                 };
        }

        public FrameworkElement ClippingVisual { get; set; }

        protected override int VisualChildrenCount
        {
            get { return 2; }
        }

        public void MoveTo(Point to)
        {
            position = to;
            position.Offset(-3, -1);
            InvalidateArrange();

            // If we were given a clip visual, hide the triangle, which are totally outside of it
            if (ClippingVisual != null)
            {
                var clipping = new Rect(
                    ClippingVisual.PointToScreen(new Point(-4, -4)),
                    ClippingVisual.PointToScreen(new Point(ClippingVisual.ActualWidth + 4,
                                                           ClippingVisual.ActualHeight + 4)));
                Point testPt = PointToScreen(position);
                topTriangle.Visibility = clipping.Contains(testPt) ? Visibility.Visible : Visibility.Collapsed;
                testPt = PointToScreen(new Point(position.X, position.Y + Height + 2));
                bottomTriangle.Visibility = clipping.Contains(testPt) ? Visibility.Visible : Visibility.Collapsed;
            }

            var adornerLayer = Parent as AdornerLayer;
            if (adornerLayer != null) adornerLayer.Update();
        }

        protected override Visual GetVisualChild(int index)
        {
            return index == 0 ? topTriangle : bottomTriangle;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            topTriangle.Arrange(new Rect(position, finalSize));
            bottomTriangle.Arrange(
                new Rect(new Point(position.X, double.IsNaN(Height) ? finalSize.Height : position.Y + Height + 2),
                         finalSize));
            return finalSize;
        }
    }
}