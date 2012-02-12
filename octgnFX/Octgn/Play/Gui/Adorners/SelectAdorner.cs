using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Octgn.Play.Gui.Adorners
{
    internal class SelectAdorner : Adorner
    {
        private static readonly Brush BorderBrush, FillBrush;
        private readonly Rectangle _child;
        private Rect _position;

        static SelectAdorner()
        {
            var color = Color.FromRgb(0x53, 0x9C, 0xD8);
            BorderBrush = new SolidColorBrush(color);
            BorderBrush.Freeze();
            color.A = 100;
            FillBrush = new SolidColorBrush(color);
            FillBrush.Freeze();
        }

        public SelectAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            _child = new Rectangle
                         {StrokeThickness = 3, Stroke = BorderBrush, Fill = FillBrush, IsHitTestVisible = false};
        }

        public Rect Rectangle
        {
            set
            {
                _position = value;
                InvalidateVisual();
            }
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return _position.Size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(_position);
            return _position.Size;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }
    }
}