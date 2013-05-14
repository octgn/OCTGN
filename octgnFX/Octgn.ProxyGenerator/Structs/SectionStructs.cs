using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Octgn.ProxyGenerator.Structs
{
    public class SectionStructs
    {
        public struct Location
        {
            public int x;
            public int y;
            public int rotate;
            public bool flip;
            public bool altrotate;

            public Location(int x, int y) 
            {
                this.x = x;
                this.y = y;
                this.rotate = 0;
                this.flip = false;
                this.altrotate = false;
            }

            public Point ToPoint()
            {
                return (new Point(this.x, this.y));
            }
        }

        public struct Text
        {
            public Color color;
            public int size;
            public string font;

            public Text(string color, int size)
            {
                this.color = ColorTranslator.FromHtml(color);
                this.size = size;
                this.font = null;
            }
        }

        public struct Border
        {
            public Color color;
            public int size;

            public Border(string color, int size)
            {
                this.color = ColorTranslator.FromHtml(color);
                this.size = size;
            }
        }

        public struct WordWrap
        {
            public int width;
            public int height;
            public string align;
            public string valign;
            public Boolean shrinkToFit;

            public WordWrap(int width, int height) 
            {
                this.width = width;
                this.height = height;
                this.align = "near";
                this.valign = "near";
                shrinkToFit = false;
            }

            public Size ToSize()
            {
                return (new Size(this.width, this.height));
            }
        }
    }
}
