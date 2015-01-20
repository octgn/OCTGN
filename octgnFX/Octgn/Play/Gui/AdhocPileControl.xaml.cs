using System;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Octgn.Play.Gui
{
    public partial class AdhocPileControl : INotifyPropertyChanged
    {
        private static readonly DoubleAnimation Anim = new DoubleAnimation
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(150)),
            AccelerationRatio = 0.3,
            DecelerationRatio = 0.3,
            FillBehavior = FillBehavior.HoldEnd
        };

        private double width;

        private double height;

        public double CardWidth
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
                OnPropertyChanged("CardWidth");
            }
        }

        public double CardHeight
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
                OnPropertyChanged("CardHeight");
            }
        }

        // One can only drag one marker at a time -> make everything static. It reduces memory usage.
        private static bool _isDragging, _isMouseDown;
        private static Point _mousePt, _mouseOffset;
        private static Adorners.DragAdorner _adorner;

        public AdhocPileControl()
        {
            InitializeComponent();
            CardWidth = Program.GameEngine.Definition.CardSize.Width + Program.GameEngine.Definition.CardSize.Width * .5;
            CardHeight = Program.GameEngine.Definition.CardSize.Height + Program.GameEngine.Definition.CardSize.Height * .5;
        }

        protected override void OnCardDropped(object sender, CardsEventArgs e)
        {
            e.Handled = e.CanDrop = true;
            if (group.TryToManipulate())
            {
                var cards = e.Cards.ToArray();
                Card.MoveCardsTo(group, cards, 
                    Enumerable.Repeat(e.FaceUp ?? false,cards.Length).ToArray()
                    ,Enumerable.Repeat(0,cards.Length).ToArray(),false);
            }
            //foreach (Card c in e.Cards)
            //    c.MoveTo(group, e.FaceUp != null && e.FaceUp.Value, 0, false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void MoveThumb_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            e.Handled = true;
            _isMouseDown = true;
            _mousePt = Mouse.GetPosition(this);
        }

        private void MoveThumb_OnMouseMove(object sender, MouseEventArgs e)
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

        private void MoveThumb_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
            //RaiseEvent(new CardEventArgs(CardControl.CardHoveredEvent, this));

            UIElement adorned = this;//_takeAll ? this : (UIElement)img1;
            _mouseOffset = TranslatePoint(_mousePt, adorned);
            _mouseOffset.X -= adorned.DesiredSize.Width / 2;
            _mouseOffset.Y -= adorned.DesiredSize.Height / 2;
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
                Canvas.SetLeft(this, _adorner.LeftOffset);
                Canvas.SetTop(this, _adorner.TopOffset);
                ((AdornerLayer)_adorner.Parent).Remove(_adorner);
                _adorner = null;
                //var marker = (Marker)DataContext;
                //ushort count = _takeAll ? marker.Count : (ushort)1;
                //var e = new MarkerEventArgs(this, marker, count);
                //Mouse.DirectlyOver.RaiseEvent(e);
                //if (Keyboard.IsKeyUp(Key.LeftAlt) && !e.Handled)
                //    marker.Count -= count;

            }
            catch (Exception ex)
            {
                Log.Warn("DragCompleted", ex);
            }
        }
    }
}
