using Octgn.ProxyGenerator.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    public class MultiMatchDefinition
    {
        public List<string> MultiMatchMappings = new List<string>();

        public static MultiMatchDefinition LoadMultiMatchDefinition(XmlNode node)
        {
            MultiMatchDefinition ret = new MultiMatchDefinition();
            foreach (XmlNode subNode in node.ChildNodes)
            {
                ret.MultiMatchMappings.Add(subNode.Attributes["id"].Value);
            }
            return (ret);
        }
    }
}
