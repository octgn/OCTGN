namespace Octgn.Play.Gui.Adorners
{
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    public class NoteAdorner : Adorner, IDisposable
    {
        private bool focused = false;
        public NoteAdorner(NoteControl adornedElement)
            : base(adornedElement)
        {

            AdornedElement.MouseEnter += OnMouseEnter;
            AdornedElement.MouseLeave += OnMouseLeave;
            this.MouseLeave += OnMouseLeave;
            visualChildren = new VisualCollection(this);

            // Call a helper method to initialize the Thumbs
            // with a customized cursors.
            BuildAdornerCorner(ref bottomRight, Cursors.SizeNWSE);

            // Add handlers for resizing.
            bottomRight.DragDelta += new DragDeltaEventHandler(HandleBottomRight);
            adornedElement.MoveThumb.DragDelta += HandleTop;
        }

        private void OnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            lock (this)
            {
                if (this.IsMouseOver) return;
                if (AdornedElement.IsMouseOver) return;
                if (bottomRight.IsMouseOver) return;
                if (focused == false) return;
                focused = false;
            }
            Thread.Sleep(200);
            DoubleAnimation da = new DoubleAnimation();
            da.From = .8;
            da.To = 0;
            da.Duration = new Duration(TimeSpan.FromSeconds(.5));
            da.AutoReverse = false;
            //da.RepeatBehavior=new RepeatBehavior(3);
            bottomRight.BeginAnimation(OpacityProperty, da);
        }

        void OnMouseEnter(object sender, RoutedEventArgs e)
        {
            if (focused == true) return;
            focused = true;
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = .8;
            da.Duration = new Duration(TimeSpan.FromSeconds(.5));
            da.AutoReverse = false;
            //da.RepeatBehavior=new RepeatBehavior(3);
            bottomRight.BeginAnimation(OpacityProperty, da);
            var a = bottomRight.IsHitTestVisible;
        }
        // Resizing adorner uses Thumbs for visual elements.  
        // The Thumbs have built-in mouse input handling.
        Thumb bottomRight;

        // To store and manage the adorner's visual children.
        VisualCollection visualChildren;

        // Handler for resizing from the bottom-right.
        void HandleBottomRight(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;
            FrameworkElement parentElement = adornedElement.Parent as FrameworkElement;

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(adornedElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger 
            // than the width or height of an adorner, respectively.
            adornedElement.Width = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
            adornedElement.Height = Math.Max(args.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);
        }

        void HandleTop(object sender, DragDeltaEventArgs args)
        {
            var item = this.AdornedElement as Control;

            if (item != null)
            {
                double left = Canvas.GetLeft(item);
                double top = Canvas.GetTop(item);

                Canvas.SetLeft(item, left + args.HorizontalChange);
                Canvas.SetTop(item, top + args.VerticalChange);
            }
        }

        // Arrange the Adorners.
        protected override Size ArrangeOverride(Size finalSize)
        {
            // desiredWidth and desiredHeight are the width and height of the element that's being adorned.  
            // These will be used to place the ResizingAdorner at the corners of the adorned element.  
            double desiredWidth = AdornedElement.DesiredSize.Width;
            double desiredHeight = AdornedElement.DesiredSize.Height;
            // adornerWidth & adornerHeight are used for placement as well.
            double adornerWidth = this.DesiredSize.Width;
            double adornerHeight = this.DesiredSize.Height;
            bottomRight.Arrange(new Rect(desiredWidth - adornerWidth/1.91, desiredHeight - adornerHeight/1.91, adornerWidth, adornerHeight));

            // Return the final size.
            return finalSize;
        }

        // Helper method to instantiate the corner Thumbs, set the Cursor property, 
        // set some appearance properties, and add the elements to the visual tree.
        void BuildAdornerCorner(ref Thumb cornerThumb, Cursor customizedCursor)
        {
            if (cornerThumb != null) return;

            cornerThumb = new Thumb();

            cornerThumb.Style = FindResource("RoundThumb") as Style;
            // Set some arbitrary visual characteristics.
            cornerThumb.Cursor = customizedCursor;
            cornerThumb.Height = cornerThumb.Width = 10;
            cornerThumb.Opacity = 0.00;
            cornerThumb.SetValue(Canvas.ZIndexProperty, 2);
            cornerThumb.MouseLeave += OnMouseLeave;

            visualChildren.Add(cornerThumb);
        }

        // This method ensures that the Widths and Heights are initialized.  Sizing to content produces
        // Width and Height values of Double.NaN.  Because this Adorner explicitly resizes, the Width and Height
        // need to be set first.  It also sets the maximum size of the adorned element.
        void EnforceSize(FrameworkElement adornedElement)
        {
            if (adornedElement.Width.Equals(Double.NaN))
                adornedElement.Width = adornedElement.DesiredSize.Width;
            if (adornedElement.Height.Equals(Double.NaN))
                adornedElement.Height = adornedElement.DesiredSize.Height;

            FrameworkElement parent = adornedElement.Parent as FrameworkElement;
            if (parent != null)
            {
                adornedElement.MaxHeight = parent.ActualHeight;
                adornedElement.MaxWidth = parent.ActualWidth;
            }
        }
        // Override the VisualChildrenCount and GetVisualChild properties to interface with 
        // the adorner's visual collection.
        protected override int VisualChildrenCount { get { return visualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return visualChildren[index]; }

        public void Dispose()
        {
            AdornedElement.MouseEnter -= OnMouseEnter;
            AdornedElement.MouseLeave -= OnMouseLeave;
            this.MouseLeave -= OnMouseLeave;
            this.MouseLeave -= OnMouseLeave;
            this.bottomRight.MouseLeave += OnMouseLeave;
            bottomRight.DragDelta -= new DragDeltaEventHandler(HandleBottomRight);
            (this.AdornedElement as NoteControl).MoveThumb.DragDelta -= HandleTop;
            //(this.AdornedElement as NoteControl).TextBox.GotKeyboardFocus += TextBox_GotKeyboardFocus;
            //(this.AdornedElement as NoteControl).LostKeyboardFocus += TextBox_LostKeyboardFocus;
        }
    }
}