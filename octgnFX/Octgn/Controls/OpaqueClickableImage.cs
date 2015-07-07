using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Octgn.Controls
{
    public class OpaqueClickableImage : Image
    {
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            var source = new FormatConvertedBitmap((BitmapSource) Source, PixelFormats.Bgra32, null, 0);

            // Get the pixel of the source that was hit
            var x = (int)(hitTestParameters.HitPoint.X / ActualWidth * source.PixelWidth);
            var y = (int)(hitTestParameters.HitPoint.Y / ActualHeight * source.PixelHeight);

            if (x == source.PixelWidth) x--;
            if (y == source.PixelHeight) y--;

            var pixelxy = new Int32Rect(x, y, 1, 1);
            var pixelbgra = new byte[4];

            source.CopyPixels(pixelxy, pixelbgra, source.PixelWidth * 4, 0);

            if (pixelbgra[3] < 5)
                return null;

            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }
    }
}
