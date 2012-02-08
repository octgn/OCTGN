using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Octgn.Play.Gui
{
    public partial class MarkerControl
    {
        public MarkerControl()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this)) return;

            AddHandler(TableControl.TableKeyEvent, new EventHandler<TableKeyEventArgs>(TableKeyDown));
            int markerSize = Program.Game.Definition.MarkerSize;
            img1.Height = markerSize;
            textBorder.Margin = new Thickness(markerSize*0.2, markerSize*0.04, 0, markerSize*0.04);
            text.Margin = new Thickness(markerSize*0.3, 0, markerSize*0.3, 0);
            text.FontSize = markerSize*0.8;
        }

        #region Key accelerators

        protected void TableKeyDown(object sender, TableKeyEventArgs te)
        {
            KeyEventArgs e = te.KeyEventArgs;
            if (e.Key == Key.Add)
            {
                te.Handled = e.Handled = true;
                var marker = DataContext as Marker;
                if (marker != null) marker.Count++;
            }
            else if (e.Key == Key.Subtract)
            {
                te.Handled = e.Handled = true;
                var marker = DataContext as Marker;
                if (marker != null) marker.Count--;
            }
        }

        #endregion

        #region Drag and drop

        public static readonly RoutedEvent MarkerDroppedEvent = EventManager.RegisterRoutedEvent("MakerDropped",
                                                                                                 RoutingStrategy.Bubble,
                                                                                                 typeof (
                                                                                                     EventHandler
                                                                                                     <MarkerEventArgs>),
                                                                                                 typeof (CardControl));

        // One can only drag one marker at a time -> make everything static. It reduces memory usage.
        private static bool isDragging, isMouseDown, takeAll;
        private static Point mousePt, mouseOffset;
        private static DragAdorner adorner;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            e.Handled = true;
            isMouseDown = true;
            mousePt = Mouse.GetPosition(this);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            e.Handled = true;
            Point pt = e.GetPosition(this);
            if (!isDragging)
            {
                // Check if the button was pressed over the marker, and was not release on another control in the meantime 
                // (possible if the cursor is near the border of the marker)
                if (isMouseDown && Mouse.LeftButton == MouseButtonState.Pressed &&
                    // Check if has moved enough to start a drag and drop
                    (Math.Abs(pt.X - mousePt.X) > SystemParameters.MinimumHorizontalDragDistance ||
                     Math.Abs(pt.Y - mousePt.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    e.Handled = true;
                    isDragging = true;
                    DragStarted();
                    CaptureMouse();
                }
            }
            else
                DragDelta(pt.X - mousePt.X, pt.Y - mousePt.Y);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            e.Handled = true;
            isMouseDown = false;
            if (isDragging)
            {
                isDragging = false;
                ReleaseMouseCapture();
                DragCompleted();
            }
        }

        private void DragStarted()
        {
            // Hides the card view
            RaiseEvent(new CardEventArgs(CardControl.CardHoveredEvent, this));

            takeAll = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            UIElement adorned = takeAll ? this : (UIElement) img1;
            mouseOffset = TranslatePoint(mousePt, adorned);
            mouseOffset.X -= adorned.DesiredSize.Width/2;
            mouseOffset.Y -= adorned.DesiredSize.Height/2;
            adorner = new DragAdorner(adorned);
            AdornerLayer.GetAdornerLayer(adorner.AdornedElement).Add(adorner);
        }

        private void DragDelta(double dx, double dy)
        {
            adorner.LeftOffset = mouseOffset.X + dx;
            adorner.TopOffset = mouseOffset.Y + dy;
        }

        private void DragCompleted()
        {
            ((AdornerLayer) adorner.Parent).Remove(adorner);
            adorner = null;
            var marker = (Marker) DataContext;
            ushort count = takeAll ? marker.Count : (ushort) 1;
            var e = new MarkerEventArgs(this, marker, count);
            Mouse.DirectlyOver.RaiseEvent(e);
            if (Keyboard.IsKeyUp(Key.LeftAlt) && !e.Handled)
                marker.Count -= count;
        }

        #endregion
    }

    public class MarkerEventArgs : RoutedEventArgs
    {
        public readonly ushort Count;
        public readonly Marker Marker;

        public MarkerEventArgs(object source, Marker marker, ushort count)
            : base(MarkerControl.MarkerDroppedEvent, source)
        {
            Marker = marker;
            Count = count;
        }
    }

    //internal class VisibilityConverter : IValueConverter
    //{
    //  public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //  {
    //    return (ushort)value > System.Convert.ToUInt16(parameter) ?
    //        Visibility.Visible : Visibility.Collapsed;
    //  }

    //  public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //  { throw new NotImplementedException("The method or operation is not implemented."); }
    //}
}