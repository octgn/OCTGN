using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Documents;

namespace Octgn.Play.Gui
{
  public class FanPanel : Panel
  {
    #region Private vars

    private UIElement mouseOverElement = null;
    private static readonly IEasingFunction expoEasing = new ExponentialEase();
    private static readonly DoubleAnimation animation = new DoubleAnimation() { EasingFunction = expoEasing, Duration = TimeSpan.FromMilliseconds(300) };
    private static readonly DoubleAnimation staticAnimation = new DoubleAnimation() { EasingFunction = expoEasing, FillBehavior = FillBehavior.Stop };
    private InsertAdorner insertAdorner = null;
    private UIElement spacedItem1 = null, spacedItem2 = null;
    private const int SpacingWidth = 8;
    private const double LayoutAnimationDelay = 0.1;
    private double itemSkipSize;

    #endregion

    #region Dependency properties

    private static double GetXPosition(DependencyObject obj)
    { return (double)obj.GetValue(XPositionProperty); }

    private static void SetXPosition(DependencyObject obj, double value)
    { obj.SetValue(XPositionProperty, value); }

    private static readonly DependencyProperty XPositionProperty =
        DependencyProperty.RegisterAttached("XPositionProperty", typeof(double), typeof(FanPanel));

    #endregion

    public FanPanel()
    {
      //  Wire events
      this.PreviewMouseMove += new MouseEventHandler(FanPanel_MouseMove);
      this.MouseEnter += new MouseEventHandler(MouseRequiresArrange);
      this.MouseLeave += new MouseEventHandler(MouseRequiresArrange);
    }

    #region Insertion related

    public int GetIndexFromPoint(Point position)
    {
      if (itemSkipSize == 0) return 0;
      int idx = position.X < 0 ? 0 : (int)(position.X / itemSkipSize + 0.5);
      if (idx > Children.Count) idx = Children.Count;
      return idx;
    }

    public void DisplayInsertIndicator(Card source, int idx)
    {
      // If the index corresponds to the card being dragged, it's a NOP so don't provide any visual feedback
      if (idx < Children.Count)
      {
        if (((FrameworkElement)Children[idx]).DataContext == source)
        { HideInsertIndicator(); return; }
        if (idx > 0 && ((FrameworkElement)Children[idx - 1]).DataContext == source)
        { HideInsertIndicator(); return; }
      }

      // Create an adorner if it doesn't exist yet
      if (insertAdorner == null)
      {
        insertAdorner = new InsertAdorner(this);
        var layer = AdornerLayer.GetAdornerLayer(this);
        layer.Add(insertAdorner);
      }

      // Position the insert adorner correctly
      insertAdorner.MoveTo(new Point(idx * itemSkipSize, 0));

      // Cancel previous spacing
      CancelSpacing();

      // Space neighbors
      if (idx < Children.Count)
      {
        spacedItem2 = Children[idx];
        SetSpacing(spacedItem2, SpacingWidth);
        if (idx > 0)
        {
          spacedItem1 = Children[idx - 1];
          SetSpacing(spacedItem1, -SpacingWidth);
        }
      }
    }

    public void HideInsertIndicator()
    {
      if (insertAdorner == null) return;
      var layer = AdornerLayer.GetAdornerLayer(this);
      layer.Remove(insertAdorner);
      insertAdorner = null;
      CancelSpacing();
    }

    private void CancelSpacing()
    {
      if (spacedItem1 != null)
      {
        CancelSpacing(spacedItem1);
        spacedItem1 = null;
      }
      if (spacedItem2 != null)
      {
        CancelSpacing(spacedItem2);
        spacedItem2 = null;
      }
    }

    private void CancelSpacing(UIElement element)
    {
      var group = (TransformGroup)element.RenderTransform;
      var translate = (TranslateTransform)group.Children[1];
      animation.To = 0; animation.FillBehavior = FillBehavior.Stop;
      translate.BeginAnimation(TranslateTransform.XProperty, animation);
    }

    private void SetSpacing(UIElement element, int value)
    {
      var group = (TransformGroup)element.RenderTransform;
      var translate = (TranslateTransform)group.Children[1];
      animation.To = value; animation.FillBehavior = FillBehavior.HoldEnd;
      translate.BeginAnimation(TranslateTransform.XProperty, animation);
    }

    #endregion

    #region Mouse related

    private void FanPanel_MouseMove(object sender, MouseEventArgs e)
    {
      if (mouseOverElement == null)
      {
        if (!IsMouseDirectlyOver) InvalidateArrange();
      }
      else
        if (!mouseOverElement.IsMouseOver) InvalidateArrange();
    }

    private void MouseRequiresArrange(object sender, MouseEventArgs e)
    {
      InvalidateArrange();
    }

    #endregion

    #region Layout

    protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
    {
      base.OnVisualChildrenChanged(visualAdded, visualRemoved);
      // Set up the transformations
      if (visualAdded != null)
      {
        var child = (UIElement)visualAdded;
        child.RenderTransformOrigin = new Point(0, 0);
        var group = new TransformGroup();
        child.RenderTransform = group;
        group.Children.Add(new ScaleTransform());       // Used for hover effects
        group.Children.Add(new TranslateTransform());   // Y used for hover effects, X for drag and drop
        group.Children.Add(new TranslateTransform());   // X used for layout to layout animations
        SetXPosition(child, double.NaN);
      }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
      Size idealSize = new Size(0, 0);

      // Allow children as much room as they want - then scale them
      Size size = new Size(Double.PositiveInfinity, Double.PositiveInfinity);
      foreach (UIElement child in Children)
      {
        child.Measure(size);
        idealSize.Width += child.DesiredSize.Width;
        idealSize.Height = Math.Max(idealSize.Height, child.DesiredSize.Height);
      }

      // EID calls us with infinity, but framework doesn't like us to return infinity
      if (double.IsInfinity(availableSize.Height) || double.IsInfinity(availableSize.Width))
        return idealSize;
      else
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      if (Children == null || Children.Count == 0)
        return finalSize;

      double totalChildWidth = 0;

      foreach (UIElement child in this.Children)
      {
        child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));
        totalChildWidth += child.DesiredSize.Width;
      }

      // Assume all children have the same width
      if (Children[0].DesiredSize.Height == 0) return finalSize;
      double ratio = finalSize.Height / Children[0].DesiredSize.Height;
      double childWidth = Children[0].DesiredSize.Width * ratio;

      // Check if enough space is available
      itemSkipSize = childWidth + 2;
      if (Children.Count > 1 && Children.Count * itemSkipSize > finalSize.Width)
        itemSkipSize = (finalSize.Width - childWidth) / (Children.Count - 1);

      double xposition = 0;
      double animationDelay = 0;
      UIElement newMouseOverElement = null;

      foreach (UIElement child in Children)
      {
        var group = (TransformGroup)child.RenderTransform;
        var scale = (ScaleTransform)group.Children[0];
        var translate = (TranslateTransform)group.Children[1];
        scale.ScaleX = scale.ScaleY = ratio;
        if (child.IsMouseOver)
        {
          newMouseOverElement = child;
          if (child != mouseOverElement)
          {
            animation.To = 1.3 * ratio; animation.FillBehavior = FillBehavior.HoldEnd;
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            animation.To = -0.4 * finalSize.Height;
            translate.BeginAnimation(TranslateTransform.YProperty, animation);
          }
        }
        else
        {
          if (child == mouseOverElement)
          {
            animation.To = 1; animation.FillBehavior = FillBehavior.Stop;
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            animation.To = 0;
            translate.BeginAnimation(TranslateTransform.YProperty, animation);
          }
        }

        child.Arrange(new Rect(new Point(xposition, 0), child.DesiredSize));

        // Perform layout to layout animation                
        double oldPos = GetXPosition(child);
        if (!double.IsNaN(oldPos) && Math.Abs(xposition - oldPos) > 2)
        {
          var translate2 = (TranslateTransform)group.Children[2];
          var delay = TimeSpan.FromSeconds(animationDelay);
          animation.FillBehavior = FillBehavior.Stop;
          animation.From = oldPos - xposition;
          animation.To = 0; animation.BeginTime = delay;
          staticAnimation.To = staticAnimation.From = animation.From;
          staticAnimation.Duration = delay;
          if (animationDelay > 0)
            translate2.BeginAnimation(TranslateTransform.XProperty, staticAnimation);
          translate2.BeginAnimation(TranslateTransform.XProperty, animation, HandoffBehavior.Compose);
          animation.From = null; animation.BeginTime = TimeSpan.Zero;
          animationDelay += LayoutAnimationDelay;
        }
        SetXPosition(child, xposition);

        xposition += itemSkipSize;
      }

      mouseOverElement = newMouseOverElement;
      return finalSize;
    }

    #endregion
  }
}
