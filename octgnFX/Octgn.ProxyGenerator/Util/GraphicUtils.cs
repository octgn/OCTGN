using Octgn.ProxyGenerator.Definitions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            GraphicsPath path = null;
            if (section.wordwrap.width > 0 && section.wordwrap.height > 0)
            {
                path = GetTextPath(section.location.ToPoint(), section.text.size, value, section.wordwrap.ToSize());
            }
            else
            {
                path = GetTextPath(section.location.ToPoint(), section.text.size, value);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <param name="text"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        private static GraphicsPath GetTextPath(Point location, int size, string text, Size block)
        {
            GraphicsPath myPath = new GraphicsPath();
            FontFamily family = new FontFamily("Arial");
            int fontStyle = (int)FontStyle.Regular;
            StringFormat format = StringFormat.GenericDefault;
            Rectangle rect = new Rectangle(location, block);

            myPath.AddString(text,
                family,
                fontStyle,
                size,
                rect,
                format);

            return myPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static GraphicsPath GetTextPath(Point location, int size, string text)
        {
            GraphicsPath myPath = new GraphicsPath();
            FontFamily family = new FontFamily("Arial");
            int fontStyle = (int)FontStyle.Regular;
            StringFormat format = StringFormat.GenericDefault;

            myPath.AddString(text,
                family,
                fontStyle,
                size,
                location,
                format);

            return myPath;
        }
    }
}
