using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator
{
    public class SectionDefinition
    {
        public SectionStructs.Location location;
        public SectionStructs.Block block;
        public SectionStructs.Text text;
        public SectionStructs.Border border;

        public string id;

        public static SectionDefinition LoadSectionDefinition(XmlNode node)
        {
            SectionDefinition ret = new SectionDefinition();

            ret.id = node.Attributes["id"].Value;
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
                if (prop.Name.Equals("block"))
                {
                    ret.block.height = int.Parse(prop.Attributes["height"].Value);
                    ret.block.width = int.Parse(prop.Attributes["width"].Value);
                }
            }

            return (ret);
        }
    }
}
