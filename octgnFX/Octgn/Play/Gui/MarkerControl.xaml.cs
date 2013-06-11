using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Octgn.Play.Gui
{
    using System.Reflection;

    using log4net;

    public partial class MarkerControl
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MarkerControl()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this)) return;

            AddHandler(TableControl.TableKeyEvent, new EventHandler<TableKeyEventArgs>(TableKeyDown));
            int markerSize = Program.GameEngine.Definition.MarkerSize;
            img1.Height = markerSize;
            textBorder.Margin = new Thickness(markerSize*0.2, markerSize*0.04, 0, markerSize*0.04);
            text.Margin = new Thickness(markerSize*0.3, 0, markerSize*0.3, 0);
            text.FontSize = markerSize*0.8;
        }

        #region Key accelerators

        protected void TableKeyDown(object sender, TableKeyEventArgs te)
        {
            KeyEventArgs e = te.KeyEventArgs;
            switch (e.Key)
            {
                case Key.Add:
                    {
                        te.Handled = e.Handled = true;
                        var marker = DataContext as Marker;
                        if (marker != null) marker.Count++;
                    }
                    break;
                case Key.Subtract:
                    {
                        te.Handled = e.Handled = true;
                        var marker = DataContext as Marker;
                        if (marker != null) marker.Count--;
                    }
                    break;
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
        private static bool _isDragging, _isMouseDown, _takeAll;
        private static Point _mousePt, _mouseOffset;
        private static Adorners.DragAdorner _adorner;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            e.Handled = true;
            _isMouseDown = true;
            _mousePt = Mouse.GetPosition(this);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            e.Handled = true;
            Point pt = e.GetPosition(this);
            if (!_isDragging)
            {
                // Check if the button was pressed over the marker, and was not release on another control in the meantime 
                // (possible if the cursor is near the border of the marker)
                if (_isMouseDown && Mouse.LeftButton == MouseButtonState.Pressed &&
                    // Check if has moved enough to start a drag and drop
                    (Math.Abs(pt.X - _mousePt.X) > SystemParameters.MinimumHorizontalDragDistance ||
                     Math.Abs(pt.Y - _mousePt.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    e.Handled = true;
                    _isDragging = true;
                    DragStarted();
                    CaptureMouse();
                }
            }
            else
                DragDelta(pt.X - _mousePt.X, pt.Y - _mousePt.Y);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            e.Handled = true;
            _isMouseDown = false;
            if (!_isDragging) return;
            _isDragging = false;
            ReleaseMouseCapture();
            DragCompleted();
        }

        private void DragStarted()
        {
            // Hides the card view
            RaiseEvent(new CardEventArgs(CardControl.CardHoveredEvent, this));

            _takeAll = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            UIElement adorned = _takeAll ? this : (UIElement) img1;
            _mouseOffset = TranslatePoint(_mousePt, adorned);
            _mouseOffset.X -= adorned.DesiredSize.Width/2;
            _mouseOffset.Y -= adorned.DesiredSize.Height/2;
            _adorner = new Adorners.DragAdorner(adorned);
            AdornerLayer.GetAdornerLayer(_adorner.AdornedElement).Add(_adorner);
        }

        private static void DragDelta(double dx, double dy)
        {
            _adorner.LeftOffset = _mouseOffset.X + dx;
            _adorner.TopOffset = _mouseOffset.Y + dy;
        }

        private void DragCompleted()
        {
            // Hack, one of these ends up null and I'm not sure why
            try
            {
                if (_adorner == null || _adorner.Parent == null) return;
                ((AdornerLayer)_adorner.Parent).Remove(_adorner);
                _adorner = null;
                var marker = (Marker)DataContext;
                ushort count = _takeAll ? marker.Count : (ushort)1;
                var e = new MarkerEventArgs(this, marker, count);
                Mouse.DirectlyOver.RaiseEvent(e);
                if (Keyboard.IsKeyUp(Key.LeftAlt) && !e.Handled)
                    marker.Count -= count;

            }
            catch (Exception ex)
            {
                Log.Warn("DragCompleted",ex);
            }
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