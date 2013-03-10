using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Octgn.ProxyGenerator
{
    public class SectionStructs
    {
        public struct Location
        {
            public int x = 0;
            public int y = 0;

            public Location(int x, int y) 
            {
                this.x = x;
                this.y = y; 
            }

            public Point ToPoint()
            {
                return (new Point(this.x, this.y));
            }
        }

        public struct Text
        {
            public Color color = Color.White;
            public int size = 0;

            public Text(string color, int size)
            {
                this.color = ColorTranslator.FromHtml(color);
                this.size = size;
            }
        }

        public struct Border
        {
            public Color color = Color.White;
            public int size = 0;

            public Border(string color, int size)
            {
                this.color = ColorTranslator.FromHtml(color);
                this.size = size;
            }
        }

        public struct Block
        {
            public int width = 0;
            public int height = 0;

            public Block(int width, int height) 
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
