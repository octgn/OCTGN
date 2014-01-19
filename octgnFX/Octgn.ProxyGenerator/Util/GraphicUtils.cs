using Octgn.ProxyGenerator.Definitions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;

namespace Octgn.ProxyGenerator.Util
{
    public class GraphicUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="overlay"></param>
        public static void MergeOverlay(Graphics graphics, BlockDefinition overlay)
        {
            using (Bitmap temp = GraphicUtils.LoadImage(overlay.src, PixelFormat.Format32bppArgb))
            {
                graphics.DrawImage(temp,overlay.location.x, overlay.location.y, temp.Width, temp.Height);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="section"></param>
        /// <param name="value"></param>
        public static void WriteString(Graphics graphics, BlockDefinition section, string value)
        {
            if (value == null || value == string.Empty)
            {
                return;
            }

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            value = System.Web.HttpUtility.HtmlDecode(value);
            GraphicsPath path = GetTextPath(section, value, graphics);

            if (section.location.flip)
            {
                using (Matrix matrix = new Matrix())
                {

                    matrix.Scale(-1, 1, MatrixOrder.Append);
                    path.Transform(matrix);
                }
                using (Matrix matrix = new Matrix())
                {
                    float min = path.PathPoints.Min(p => p.X);
                    matrix.Translate(((-min)+section.location.x), 0);
                    path.Transform(matrix);
                }
            }

            if (section.location.rotate > 0)
            {
                int rotateMod = section.location.rotate % 360;

                if (!section.location.altrotate)
                {
                    float centerX = 0;
                    float centerY = 0;
                    centerX = section.location.x;
                    centerY = section.location.y;

                    using (Matrix mat = new Matrix())
                    {
                        mat.RotateAt(rotateMod, new PointF(centerX, centerY), MatrixOrder.Append);
                        path.Transform(mat);
                    }
                }
                else
                {
                    using (Matrix matrix = new Matrix())
                    {
                        matrix.Rotate(rotateMod, MatrixOrder.Append);
                        path.Transform(matrix);
                    }
                    using (Matrix matrix = new Matrix())
                    {
                        float minX = path.PathPoints.Min(p => p.X);
                        float minY = path.PathPoints.Min(p => p.Y);
                        matrix.Translate(((-minX) + section.location.x), ((-minY) + section.location.y));
                        path.Transform(matrix);
                    }
                }
            }

            SolidBrush b = new SolidBrush(section.text.color);

            if (section.border.size > 0)
            {
                Pen p = new Pen(section.border.color, section.border.size);
                graphics.DrawPath(p, path);
                graphics.FillPath(b, path);
                p.Dispose();
            }
            else
            {
                graphics.FillPath(b, path);
            }
            b.Dispose();
            path.Dispose();
        }

        public static GraphicsPath GetTextPath(BlockDefinition section, string text, Graphics graphics)
        {
            GraphicsPath myPath = new GraphicsPath();
            FontFamily family = new FontFamily("Arial");
            FontStyle fontStyle = FontStyle.Regular;
            if (section.text.font != null)
            {
                PrivateFontCollection col = new PrivateFontCollection();
                col.AddFontFile(Path.Combine(section.Manager.RootPath, section.text.font));
                family = col.Families[0];
                bool fontStyleFound = false;
                foreach (var fontstyleEnum in Enum.GetValues(typeof(FontStyle)))
                {
                    if (family.IsStyleAvailable(((FontStyle)fontstyleEnum)))
                    {
                        fontStyle = (FontStyle)fontstyleEnum;
                        fontStyleFound = true;
                        break;
                    }
                    foreach (var secondStyle in Enum.GetValues(typeof(FontStyle)))
                    {
                        var combinedStyle = ((FontStyle)fontstyleEnum) | ((FontStyle)secondStyle);
                        if (family.IsStyleAvailable(combinedStyle))
                        {
                            fontStyle = combinedStyle;
                            fontStyleFound = true;
                            break;
                        }
                    }
                }
                if (!fontStyleFound)
                {
                    family = new FontFamily("Arial");
                }
            }
            int size = section.text.size;
            
            int minsize = 6;
            
            Point location = section.location.ToPoint();
            StringFormat format = StringFormat.GenericDefault;
            if (section.wordwrap.height > 0)
            {
                format = new StringFormat();
                format.Alignment = GetAlignment(section.wordwrap.align);
                format.LineAlignment = GetAlignment(section.wordwrap.valign);
                Size block = section.wordwrap.ToSize();
                Rectangle rect = new Rectangle(location, block);
                if (section.wordwrap.shrinkToFit)
                {
                    GraphicsUnit original = graphics.PageUnit;
                    graphics.PageUnit = GraphicsUnit.Point;  // Convert the PageUnit to Point just long enough to get an accurate measurement.
                    Font tempfont = new Font(family, size, fontStyle); // Create the font for the measurement.
                    SizeF unwrappedSize = graphics.MeasureString(text, tempfont);
                    if (unwrappedSize.Height > section.wordwrap.height)
                    {
                        if (unwrappedSize.Width > section.wordwrap.width)
                        {
                            int sizePerIncrement = (int)Math.Round((double)(unwrappedSize.Width / size), MidpointRounding.ToEven);
                            size = (int)Math.Round((double)(rect.Width / sizePerIncrement), MidpointRounding.ToEven);
                            size = (size < minsize) ? minsize : size;
                            tempfont = new Font(family, size, fontStyle);
                        }
                    }
                    else
                    {

                        float measuredHeight = graphics.MeasureString(text, tempfont, rect.Width, format).Height;
                        if (rect.Height < measuredHeight)
                        {
                             int sizePerIncrement = (int)Math.Round((double)(measuredHeight / unwrappedSize.Height), MidpointRounding.ToEven); //unwrappedSize.Height seems to be a better method of getting sizePerIncrement. 
                            size = (int)Math.Round((double)(rect.Height / sizePerIncrement), MidpointRounding.ToEven);
                            size = (size < minsize) ? minsize : size;
                            tempfont = new Font(family, size, fontStyle);
                        }
                    }
                    while (size > minsize && rect.Height < graphics.MeasureString(text, tempfont, rect.Width, format).Height) // Compare the height of the rendered text to the bounding box.  If it's larger
                    {
                        size -= 1; // Reduce the size, test again.
                        tempfont = new Font(family, size, fontStyle); // REcreate the font in the new size
                    }
                    // end addition
                    graphics.PageUnit = original; // Change the PageUnit back to display.
                }
                myPath.AddString(text,
                family,
                (int)fontStyle,
                size,
                rect,
                format);
            }
            else
            {
                myPath.AddString(text,
                family,
                (int)fontStyle,
                size,
                location,
                format);
            }
            return myPath;
        }

        private static StringAlignment GetAlignment(string alignment)
        {
            StringAlignment ret = StringAlignment.Near;

            if (alignment == null)
            {
                return (ret);
            }
            if (alignment == "center")
            {
                ret = StringAlignment.Center;
            }
            if (alignment == "far")
            {
                ret = StringAlignment.Far;
            }

            return (ret);
        }

        public static Bitmap LoadImage(string fileName, PixelFormat px = PixelFormat.DontCare)
        {
            Image img = Image.FromFile(fileName);
            Bitmap bmp = img as Bitmap;
            Graphics g = Graphics.FromImage(bmp);
            Bitmap bmpNew = new Bitmap(bmp);
            g.DrawImage(bmpNew, new Point(0, 0));
            g.Dispose();
            bmp.Dispose();
            img.Dispose();
            if (px != PixelFormat.DontCare)
            {
                Bitmap ret = bmpNew.Clone(new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), px);
                bmpNew.Dispose();
                return (ret);
            }
            return (bmpNew);
        }
    }
}
