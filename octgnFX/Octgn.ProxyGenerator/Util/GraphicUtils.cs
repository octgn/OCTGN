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
            using (Image temp = Image.FromFile(overlay.src))
            {
                Bitmap b = ((Bitmap)temp).Clone(new Rectangle(0, 0, temp.Width, temp.Height), PixelFormat.Format32bppArgb);
                //b.MakeTransparent();
                graphics.DrawImage(b,overlay.location.x, overlay.location.y, b.Width, b.Height);
                b.Dispose();
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
            value = System.Web.HttpUtility.HtmlDecode(value);
            GraphicsPath path = GetTextPath(section, value);

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
            }
            else
            {
                graphics.FillPath(b, path);
            }
        }

        public static GraphicsPath GetTextPath(BlockDefinition section, string text)
        {
            GraphicsPath myPath = new GraphicsPath();
            FontFamily family = new FontFamily("Arial");
            int fontStyle = (int)FontStyle.Regular;
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
                        fontStyle = (int)fontstyleEnum;
                        fontStyleFound = true;
                        break;
                    }
                    foreach (var secondStyle in Enum.GetValues(typeof(FontStyle)))
                    {
                        var combinedStyle = ((FontStyle)fontstyleEnum) | ((FontStyle)secondStyle);
                        if (family.IsStyleAvailable(combinedStyle))
                        {
                            fontStyle = (int)combinedStyle;
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
            
            Point location = section.location.ToPoint();
            StringFormat format = StringFormat.GenericDefault;
            if (section.wordwrap.height > 0)
            {
                format = new StringFormat();
                format.Alignment = GetAlignment(section.wordwrap.align);
                format.LineAlignment = GetAlignment(section.wordwrap.valign);
                Size block = section.wordwrap.ToSize();
                Rectangle rect = new Rectangle(location, block);

                myPath.AddString(text,
                family,
                fontStyle,
                size,
                rect,
                format);
            }
            else
            {
                myPath.AddString(text,
                family,
                fontStyle,
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
    }
}
