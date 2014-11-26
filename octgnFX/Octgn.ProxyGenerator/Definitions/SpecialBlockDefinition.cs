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

        public static BlockDefinition LoadSectionDefinition(BlockManager manager, XmlNode node)
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
            }
            return (ret);
        }
    }
}
