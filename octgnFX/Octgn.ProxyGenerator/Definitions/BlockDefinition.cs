using Octgn.ProxyGenerator.Structs;

using System.Drawing;
using System.IO;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    public class BlockDefinition
    {
        public SectionStructs.Location location = new SectionStructs.Location { x = 0, y = 0 };
        public SectionStructs.WordWrap wordwrap = new SectionStructs.WordWrap { height = 0, width = 0 };
        public SectionStructs.Text text = new SectionStructs.Text { color = Color.White, size = 0 };
        public SectionStructs.Border border = new SectionStructs.Border { color = Color.White, size = 0 };

        public string id;
        public string type;
        public string src;

    }
}
