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
        public string Separator = " ";

        public static LinkDefinition LoadLink(XmlNode node)
        {
            LinkDefinition ret = new LinkDefinition();
            ret.Block = node.Attributes["block"].Value;
            if (node.Attributes["separator"] != null)
            {
                ret.Separator = node.Attributes["separator"].Value;
            }
            if (node.HasChildNodes)
            {
                foreach (XmlNode subNode in node.ChildNodes)
                {
                    if (TemplateDefinition.SkipNode(subNode))
                    {
                        continue;
                    }
                    Property prop = new Property();
                    prop.Name = subNode.Attributes["name"].Value;
                    prop.Value = string.Empty;
                    if (subNode.Attributes["format"] != null)
                    {
                        prop.Format = subNode.Attributes["format"].Value.Contains("{}") ? subNode.Attributes["format"].Value : "{}";
                    }
                    ret.NestedProperties.Add(prop);
                }
            }

            return (ret);
        }

        public class LinkWrapper
        {
            public LinkDefinition Link = null;
            public ConditionalDefinition Conditional = null;
        }
    }
}
