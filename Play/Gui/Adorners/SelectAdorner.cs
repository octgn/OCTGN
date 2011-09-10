using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Octgn.Play.Gui
{
	class SelectAdorner : Adorner
	{
		private Rectangle child;
		private Rect position;

		private static readonly Brush BorderBrush, FillBrush;

		static SelectAdorner()
		{
			Color color = Color.FromRgb(0x53, 0x9C, 0xD8);
			BorderBrush = new SolidColorBrush(color);
			BorderBrush.Freeze();
			color.A = 100;
			FillBrush = new SolidColorBrush(color);
			FillBrush.Freeze();
		}

		public SelectAdorner(UIElement adornedElement)
			: base(adornedElement)
		{
			child = new Rectangle() { StrokeThickness = 3, Stroke = BorderBrush, Fill = FillBrush, IsHitTestVisible = false };			
		}

		public Rect Rectangle
		{
			set 
			{
				position = value;
				InvalidateVisual(); 
			}
		}

		protected override Size MeasureOverride(Size constraint)
		{
			return position.Size;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			child.Arrange(position);
			return position.Size;
		}

		protected override System.Windows.Media.Visual GetVisualChild(int index)
		{ return child; }

		protected override int VisualChildrenCount
		{ get { return 1; } }
	}
}
