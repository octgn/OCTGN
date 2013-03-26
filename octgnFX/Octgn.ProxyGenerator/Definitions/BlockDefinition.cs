using Octgn.ProxyGenerator.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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

        public static BlockDefinition LoadSectionDefinition(XmlNode node)
        {
            BlockDefinition ret = new BlockDefinition();

            ret.id = node.Attributes["id"].Value;
            ret.type = node.Attributes["type"].Value;
            if (node.Attributes["src"] != null)
            {
                ret.src = node.Attributes["src"].Value;
            }
            foreach (XmlNode prop in node.ChildNodes)
            {
                if (prop.Name.Equals("location"))
                {
                    ret.location.x = int.Parse(prop.Attributes["x"].Value);
                    ret.location.y = int.Parse(prop.Attributes["y"].Value);
                }
                if (prop.Name.Equals("text"))
                {
                    ret.text.color = ColorTranslator.FromHtml(prop.Attributes["color"].Value);
                    ret.text.size = int.Parse(prop.Attributes["size"].Value);
                }
                if (prop.Name.Equals("border"))
                {
                    ret.border.color = ColorTranslator.FromHtml(prop.Attributes["color"].Value);
                    ret.border.size = int.Parse(prop.Attributes["size"].Value);
                }
                if (prop.Name.Equals("wordwrap"))
                {
                    ret.wordwrap.height = int.Parse(prop.Attributes["height"].Value);
                    ret.wordwrap.width = int.Parse(prop.Attributes["width"].Value);
                }
            }

            return (ret);
        }
    }
}
