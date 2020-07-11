/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Octgn.Controls
{
    public class AnimatedWrapPanel : WrapPanel
    {
        // dependency property we attach to children to save their last arrange position
        private static readonly DependencyProperty SavedArrangePositionProperty
            = DependencyProperty.RegisterAttached("SavedArrangePosition", typeof (Point), typeof (AnimatedWrapPanel));

        // HACK: see comments inside AnimateLayout
        private static readonly DependencyProperty IsAnimationValidProperty
            = DependencyProperty.Register("IsAnimationValidProperty", typeof (int), typeof (AnimatedWrapPanel),
                                          new PropertyMetadata(0));

        private static readonly DependencyProperty SavedCurrentPositionProperty
            = DependencyProperty.RegisterAttached("SavedCurrentPosition", typeof (Point), typeof (AnimatedWrapPanel));

        private static readonly Duration AnimationDuration = new Duration(TimeSpan.FromMilliseconds(400));
        private static readonly TimeSpan CascadingDelay = TimeSpan.FromMilliseconds(60);
        private int _isAnimationValid;

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size result = base.ArrangeOverride(finalSize);
            AnimateLayout();
            return result;
        }

        private void AnimateLayout()
        {
            var startDelay = new TimeSpan();

            foreach (UIElement child in Children)
            {
                Point arrangePosition;
                Transform currentTransform = GetCurrentLayoutInfo(child, out arrangePosition);

                bool bypassTransform = _isAnimationValid != (int) GetValue(IsAnimationValidProperty);

                // If we had previously stored an arrange position, see if it has moved
                if (child.ReadLocalValue(SavedArrangePositionProperty) != DependencyProperty.UnsetValue)
                {
                    var savedArrangePosition = (Point) child.GetValue(SavedArrangePositionProperty);

                    // If the arrange position hasn't changed, then we've already set up animations, etc
                    // and don't need to do anything
                    if (!AreClose(savedArrangePosition, arrangePosition))
                    {
                        // If we apply the current transform to the saved arrange position, we'll see where
                        // it was last rendered
                        Point lastRenderPosition = currentTransform.Transform(savedArrangePosition);
                        if (bypassTransform)
                            lastRenderPosition = (Point) child.GetValue(SavedCurrentPositionProperty);
                        else
                            child.SetValue(SavedCurrentPositionProperty, lastRenderPosition);

                        // Transform the child from the new location back to the old position
                        var newTransform = new TranslateTransform();
                        SetLayout2LayoutTransform(child, newTransform);

                        // Decay the transformation with an animation
                        double startValue = lastRenderPosition.X - arrangePosition.X;
                        newTransform.BeginAnimation(TranslateTransform.XProperty,
                                                    MakeStaticAnimation(startValue, startDelay));
                        newTransform.BeginAnimation(TranslateTransform.XProperty, MakeAnimation(startValue, startDelay),
                                                    HandoffBehavior.Compose);
                        startValue = lastRenderPosition.Y - arrangePosition.Y;
                        newTransform.BeginAnimation(TranslateTransform.YProperty,
                                                    MakeStaticAnimation(startValue, startDelay));
                        newTransform.BeginAnimation(TranslateTransform.YProperty, MakeAnimation(startValue, startDelay),
                                                    HandoffBehavior.Compose);

                        // Next element starts to move a little later
                        startDelay = startDelay.Add(CascadingDelay);
                    }
                }

                // Save off the previous arrange position				
                child.SetValue(SavedArrangePositionProperty, arrangePosition);
            }
            // currently WPF doesn't allow me to read a value right after the call to BeginAnimation
            // this code enables me to trick it and know whether I can trust the current position or not.
            _isAnimationValid = (int) GetValue(IsAnimationValidProperty) + 1;
            BeginAnimation(IsAnimationValidProperty,
                           new Int32Animation(_isAnimationValid, _isAnimationValid, new Duration(), FillBehavior.HoldEnd));
        }

        protected virtual Transform GetCurrentLayoutInfo(UIElement element, out Point arrangePosition)
        {
            // Figure out where child actually is right now. This is a combination of where the
            // panel put it and any render transform currently applied
            Point currentPosition = element.TransformToAncestor(this).Transform(new Point());

            // See what transform is being applied
            Transform currentTransform = element.RenderTransform;

            // Compute where the panel actually arranged it to
            arrangePosition = currentPosition;
            if (currentTransform != null && currentTransform != Transform.Identity)
            {
                // Undo any transform we applied
                if (currentTransform.Inverse != null)
                    arrangePosition = currentTransform.Inverse.Transform(arrangePosition);
            }

            return currentTransform;
        }

        protected virtual void SetLayout2LayoutTransform(UIElement element, Transform transform)
        {
            element.RenderTransform = transform;
        }

        private static DoubleAnimation MakeAnimation(double start, TimeSpan startDelay)
        {
            return new DoubleAnimation(start, 0, AnimationDuration, FillBehavior.Stop)
                       {EasingFunction = new ExponentialEase(), BeginTime = startDelay};
        }

        private static DoubleAnimation MakeStaticAnimation(double value, TimeSpan duration)
        {
            return new DoubleAnimation(value, value, new Duration(duration), FillBehavior.Stop);
        }

        protected bool AreClose(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) < 2 && Math.Abs(p1.Y - p2.Y) < 2;
        }
    }
}