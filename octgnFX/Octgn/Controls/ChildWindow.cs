using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.Windows.Media;

namespace Octgn.Controls
{
    [TemplatePart(Name = "PART_CloseBtn", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MoveThumb", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_ResizeThumb", Type = typeof(Thumb))]
    public class ChildWindow : ContentControl
    {
        private static readonly Duration FadeOutDuration = new Duration(TimeSpan.FromMilliseconds(220));

        static ChildWindow()
        {
            // This should allow me to easily use subclasses of ChildWindow, but it doesn't seem to work
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChildWindow), new FrameworkPropertyMetadata(typeof(ChildWindow)));
        }

        public ChildWindow()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            try 
            {            
                RenderTransform = new ScaleTransform();
                // HACK: This shouldn't be required with the code in the cctor, but I can't get it to work otherwise
                Style = (Style)FindResource(typeof(ChildWindow));
            }
            catch(Exception e)
            {
                Trace.WriteLine("[Octgn.Controls.ChildWindow]Error: " + e.Message);
            }

        }

        #region Title property

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ChildWindow), new UIPropertyMetadata(""));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var thumb = (Thumb)GetTemplateChild("PART_MoveThumb");
            if (thumb != null)
            {
                thumb.DragStarted += WindowBeginDrag;
                thumb.DragDelta += WindowDragged;
                thumb.DragCompleted += WindowDropped;
            }

            thumb = (Thumb)GetTemplateChild("PART_ResizeThumb");
            if (thumb != null)
                thumb.DragDelta += WindowResizing;

            var btn = (Button)GetTemplateChild("PART_CloseButton");
            if (btn != null)
                btn.Click += WindowClose;
        }

        public void Close()
        {
            OnClose();

            // Prevent any interaction from happening during the animation
            IsHitTestVisible = false;

            var scale = (ScaleTransform)RenderTransform;
            scale.CenterX = ActualWidth / 2; scale.CenterY = ActualHeight / 2;
            var fadeOut = new DoubleAnimation(0, FadeOutDuration);
            var shrink = new DoubleAnimation(0.8, FadeOutDuration);
            fadeOut.Completed += delegate
            {
                var manager = (ChildWindowManager)Parent;
                if (manager == null) {return;}
                manager.Hide(this);
            };
            RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, shrink);
            RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, shrink);
            BeginAnimation(OpacityProperty, fadeOut, HandoffBehavior.SnapshotAndReplace);
        }

        private double _moveX, _moveY;

        private void WindowBeginDrag(object sender, DragStartedEventArgs e)
        {
            _moveX = Canvas.GetLeft(this);
            _moveY = Canvas.GetTop(this);
        }

        private void WindowDragged(object sender, DragDeltaEventArgs e)
        {
            _moveX += e.HorizontalChange; _moveY += e.VerticalChange;
            Canvas.SetLeft(this, _moveX);
            Canvas.SetTop(this, _moveY);
        }

        private void WindowDropped(object sender, DragCompletedEventArgs e)
        {
            Canvas canvas = (Canvas)Parent;
            _moveX = _moveX < 0 ? 0 : _moveX + Width > canvas.ActualWidth ? canvas.ActualWidth - Width : _moveX;
            _moveY = _moveY < 0 ? 0 : _moveY + Height > canvas.ActualHeight ? canvas.ActualHeight - Height : _moveY;
            SetValue(Canvas.LeftProperty, _moveX);
            SetValue(Canvas.TopProperty, _moveY);
        }

        private void WindowResizing(object sender, DragDeltaEventArgs e)
        {
            var newWidth = Math.Max(MinWidth, Width + e.HorizontalChange);
            var newHeight = Math.Max(MinHeight, Height + e.VerticalChange);
            Width = newWidth; Height = newHeight;
        }

        private void WindowClose(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        protected virtual void OnClose()
        { }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            // The window is added to the visual tree -> perform a small animation
            var scale = (ScaleTransform)RenderTransform;
            scale.CenterX = Width / 2; scale.CenterY = Height / 2;
            var fadeIn = new DoubleAnimation(0, 1, FadeOutDuration, FillBehavior.Stop);
            var expand = new DoubleAnimation(0.8, 1, FadeOutDuration, FillBehavior.Stop);
            RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, expand);
            RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, expand);
            BeginAnimation(OpacityProperty, fadeIn);
            base.OnVisualParentChanged(oldParent);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            ((ChildWindowManager)Parent).Activate(this);
        }
    }
}