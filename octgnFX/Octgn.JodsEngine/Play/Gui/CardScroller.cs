using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Octgn.Play.Gui
{
    public class CardScroller : System.Windows.Controls.ScrollViewer
    {
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            if (VisualTreeHelper.GetDescendantBounds(this).Contains(hitTestParameters.HitPoint))
            {
                return new PointHitTestResult(this, hitTestParameters.HitPoint);
            }

            return null;
        }

        protected override GeometryHitTestResult HitTestCore
                (GeometryHitTestParameters hitTestParameters)
        {
            var geometry = new RectangleGeometry(VisualTreeHelper.GetDescendantBounds(this));
            return new GeometryHitTestResult
             (this, geometry.FillContainsWithDetail(hitTestParameters.HitGeometry));
        }
    }
}
