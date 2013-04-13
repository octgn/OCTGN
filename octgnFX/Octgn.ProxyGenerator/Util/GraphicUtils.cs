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
            GraphicsPath path = null;
            if (section.wordwrap.width > 0 && section.wordwrap.height > 0)
            {
                if (section.text.font == null)
                {
                    path = GetTextPath(section.location.ToPoint(), section.text.size, value, section.wordwrap.ToSize());
                }
                else
                {
                    path = GetTextPath(section.location.ToPoint(), section.text.size, value, section.wordwrap.ToSize(), section.text.font);
                }
            }
            else
            {
                if (section.text.font == null)
                {
                    path = GetTextPath(section.location.ToPoint(), section.text.size, value);
                }
                else
                {
                    path = GetTextPath(section.location.ToPoint(), section.text.size, value, section.text.font);
                }
            }

            if (section.location.flip)
            {
                using (Matrix matrix = new Matrix(1,0,0,-1,0,0))
                {
                    //matrix.Translate(section.location.x, section.location.y);
                    path.Transform(matrix);
                }
                using (Matrix matrix = new Matrix())
                {
                    PointF p = path.PathData.Points[0];
                    int y = (int)p.Y;
                    int ny = (section.location.y - y);
                    float newY = (p.Y+ny)+(~((int)p.Y)+1) + (path.GetBounds().Height/2);
                    matrix.Translate(0, newY);
                    path.Transform(matrix);
                }
            }

            if (section.location.rotate > 0)
            {
                float height = path.GetBounds().Height;
                using (Matrix matrix = new Matrix())
                {
                    matrix.Rotate(section.location.rotate);
                    path.Transform(matrix);
                }
                using (Matrix matrix = new Matrix())
                {
                    PointF p = path.PathData.Points[0];
                    float x = 0;
                    float y = 0;
                    if (p.X < 0)
                    {
                        int tx = (int)p.X;
                        int nx = (section.location.x - tx);
                        x = (p.X + nx) + (~((int)p.X) + 1);
                    }
                    else
                    {
                        x = (section.location.x - p.X) + (height/2);
                    }
                    if (p.Y < 0)
                    {
                        int ty = (int)p.Y;
                        int ny = (section.location.y - ty);
                        y = (p.Y + ny) + (~((int)p.Y) + 1);
                    }
                    else
                    {
                        y = (section.location.y - p.Y) + (height/2);
                    }
                    matrix.Translate(x, y);
                    path.Transform(matrix);
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

        private static GraphicsPath GetTextPath(Point location, int size, string text, Size block, string font)
        {
            GraphicsPath myPath = new GraphicsPath();
            PrivateFontCollection col = new PrivateFontCollection();
            col.AddFontFile(Path.Combine(BlockManager.GetInstance().rootPath, font));
            FontFamily family = col.Families[0];
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

        public static GraphicsPath GetTextPath(Point location, int size, string text, string font)
        {
            GraphicsPath myPath = new GraphicsPath();
            PrivateFontCollection col = new PrivateFontCollection();
            col.AddFontFile(Path.Combine(BlockManager.GetInstance().rootPath, font));
            FontFamily family = col.Families[0];
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
