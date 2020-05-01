using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Octgn.DataNew.Entities;

namespace Octgn.Play.Gui
{
    public class PilePanel : Panel
    {
        // this derived panel handles the resizing of the expanded (hand) piles so they fill up the entire width of the screen.
        // it is copied from the DockPanel base code, so there may be redundant sections.
        protected override Size MeasureOverride(Size constraint)
        {
            UIElementCollection children = InternalChildren;

            double accumulatedWidth = 0.0;

            foreach (UIElement child in children)
            {
                if (child == null) continue;

                Size childConstraint = new Size(Math.Max(0.0, constraint.Width - accumulatedWidth), constraint.Height);

                child.Measure(childConstraint);
                Size childDesiredSize = child.DesiredSize;

                accumulatedWidth += childDesiredSize.Width;

            }

            return new Size(accumulatedWidth, constraint.Height);
        }


        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElementCollection children = InternalChildren;

            double accumulatedWidth = 0.0;
            double pileTotalWidth = 0.0;
            double expandedPileMinimumWidth = 0.0;

            foreach (ContentPresenter child in children)
            {
                if (child == null) continue;
                Size childDesiredSize = child.DesiredSize;

                if (child.Content is Pile pile && pile.ViewState == GroupViewState.Pile)
                {
                    pileTotalWidth += childDesiredSize.Width;
                }
                else
                {
                    expandedPileMinimumWidth += childDesiredSize.Width;
                }
            }

            if (expandedPileMinimumWidth == 0) // if there's no expanded piles, then offset the accumulatedwidth so the piles align to the right side
                accumulatedWidth += arrangeSize.Width - pileTotalWidth;

            int zIndex = Children.Count;

            foreach (ContentPresenter child in children)
            {
                if (child == null) continue;
                Size childDesiredSize = child.DesiredSize;
                Rect rcChild = new Rect(
                    accumulatedWidth,
                    0.0,
                    Math.Max(0.0, arrangeSize.Width - accumulatedWidth),
                    Math.Max(0.0, arrangeSize.Height)
                    );
                if (child.Content is Pile pile && pile.ViewState == GroupViewState.Pile)
                {
                    accumulatedWidth += childDesiredSize.Width;
                    rcChild.Width = childDesiredSize.Width;
                }
                else
                {
                    double adjustedExpandedPileWith = childDesiredSize.Width / expandedPileMinimumWidth * (arrangeSize.Width - pileTotalWidth);
                    accumulatedWidth += adjustedExpandedPileWith;
                    rcChild.Width = adjustedExpandedPileWith;
                }
                child.Arrange(rcChild);
                SetZIndex(child, zIndex--);
            }

            return arrangeSize;
        }
    }
}