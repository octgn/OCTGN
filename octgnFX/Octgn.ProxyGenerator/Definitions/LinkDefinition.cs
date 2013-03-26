using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    public class LinkDefinition
    {
        public string Block { get; set; }
        public List<Property> NestedProperties = new List<Property>();
        public string Concentate = string.Empty;

        public static LinkDefinition LoadLink(XmlNode node)
        {
            LinkDefinition ret = new LinkDefinition();
            ret.Block = node.Attributes["block"].Value;
            if (node.Attributes["concentate"] != null)
            {
                ret.Concentate = node.Attributes["concentate"].Value;
            }
            if (node.HasChildNodes)
            {
                foreach (XmlNode subNode in node.ChildNodes)
                {
                    Property prop = new Property();
                    prop.Name = subNode.Attributes["name"].Value;
                    prop.Value = string.Empty;
                    ret.NestedProperties.Add(prop);
                }
            }

            return (ret);
        }
    }
}
