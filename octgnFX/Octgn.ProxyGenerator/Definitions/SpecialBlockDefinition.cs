using Octgn.ProxyGenerator.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    public class SpecialBlockDefinition : BlockDefinition
    {

        public static new BlockDefinition LoadSectionDefinition(BlockManager manager, XmlNode node)
        {
            BlockDefinition ret = new BlockDefinition();
            ret.Manager = manager;

            ret.type = "overlay";

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
                    //if (prop.Attributes["rotate"] != null)
                    //{
                    //    ret.location.rotate = int.Parse(prop.Attributes["rotate"].Value);
                    //}
                    //if (prop.Attributes["altrotate"] != null)
                    //{
                    //    ret.location.altrotate = bool.Parse(prop.Attributes["altrotate"].Value);
                    //}
                    //if (prop.Attributes["flip"] != null)
                    //{
                    //    ret.location.flip = bool.Parse(prop.Attributes["flip"].Value);
                    //}
                }
                if(prop.Name.Equals("size"))
                {
                    ret.wordwrap.height = int.Parse(prop.Attributes["height"].Value);
                    ret.wordwrap.width = int.Parse(prop.Attributes["width"].Value);
                }
            }
            return (ret);
        }
    }
}
