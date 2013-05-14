using Octgn.ProxyGenerator.Structs;

using System.Drawing;
using System.IO;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    public class BlockDefinition
    {
        public BlockManager Manager { get; set; }
        public SectionStructs.Location location = new SectionStructs.Location { x = 0, y = 0 };
        public SectionStructs.WordWrap wordwrap = new SectionStructs.WordWrap { height = 0, width = 0 };
        public SectionStructs.Text text = new SectionStructs.Text { color = Color.White, size = 0 };
        public SectionStructs.Border border = new SectionStructs.Border { color = Color.White, size = 0 };

        public string id;
        public string type;
        public string src;

        public static BlockDefinition LoadSectionDefinition(BlockManager manager, XmlNode node)
        {
            BlockDefinition ret = new BlockDefinition();
            ret.Manager = manager;

            ret.id = node.Attributes["id"].Value;
            ret.type = node.Attributes["type"].Value;
            if (node.Attributes["src"] != null)
            {
                ret.src = node.Attributes["src"].Value;
            }
            foreach (XmlNode prop in node.ChildNodes)
            {
                if (TemplateDefinition.SkipNode(prop))
                {
                    continue;
                }
                if (prop.Name.Equals("location"))
                {
                    ret.location.x = int.Parse(prop.Attributes["x"].Value);
                    ret.location.y = int.Parse(prop.Attributes["y"].Value);
                    if (prop.Attributes["rotate"] != null)
                    {
                        ret.location.rotate = int.Parse(prop.Attributes["rotate"].Value);
                    }
                    if (prop.Attributes["altrotate"] != null)
                    {
                        ret.location.altrotate = bool.Parse(prop.Attributes["altrotate"].Value);
                    }
                    if (prop.Attributes["flip"] != null)
                    {
                        ret.location.flip = bool.Parse(prop.Attributes["flip"].Value);
                    }
                }
                if (prop.Name.Equals("text"))
                {
                    ret.text.color = ColorTranslator.FromHtml(prop.Attributes["color"].Value);
                    ret.text.size = int.Parse(prop.Attributes["size"].Value);
                    if (prop.Attributes["font"] != null)
                    {
                        string relativePath = prop.Attributes["font"].Value;
                        string rootPath = manager.RootPath;
                        string combined = Path.Combine(rootPath, relativePath);
                        if (File.Exists(combined))
                        {
                            ret.text.font = relativePath;
                        }
                    }
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
                    if (prop.Attributes["align"] != null)
                    {
                        ret.wordwrap.align = prop.Attributes["align"].Value;
                    }
                    if (prop.Attributes["valign"] != null)
                    {
                        ret.wordwrap.valign = prop.Attributes["valign"].Value;
                    }
                    if (prop.Attributes["shrinktofit"] != null)
                    {
                        ret.wordwrap.shrinkToFit = bool.Parse(prop.Attributes["shrinktofit"].Value);
                    }
                }
            }

            return (ret);
        }
    }
}
