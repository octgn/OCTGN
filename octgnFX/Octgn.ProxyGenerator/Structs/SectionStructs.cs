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

            public Location(int x, int y) 
            {
                this.x = x;
                this.y = y;
                this.rotate = 0;
                this.flip = false;
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

            public Text(string color, int size)
            {
                this.color = ColorTranslator.FromHtml(color);
                this.size = size;
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

            public WordWrap(int width, int height) 
            {
                this.width = width;
                this.height = height; 
            }

            public Size ToSize()
            {
                return (new Size(this.width, this.height));
            }
        }
    }
}
