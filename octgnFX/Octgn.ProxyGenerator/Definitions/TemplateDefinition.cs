using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    public class TemplateDefinition
    {
        public List<LinkDefinition> OverlayBlocks = new List<LinkDefinition>();
        public List<LinkDefinition> TextBlocks = new List<LinkDefinition>();
        public List<Property> Matches = new List<Property>();

        public string src;
        public string rootPath;
        public bool defaultTemplate = false;

        public static TemplateDefinition LoadCardDefinition(XmlNode node)
        {
            TemplateDefinition ret = new TemplateDefinition();
            ret.src = node.Attributes["src"].Value;
            if (node.Attributes["default"] != null)
            {
                ret.defaultTemplate = bool.Parse(node.Attributes["default"].Value);
            }
            foreach (XmlNode subNode in node)
            {
                if (SkipNode(subNode))
                {
                    continue;
                }
                if (subNode.Name.Equals("textblocks"))
                {
                    ret.LoadTextBlocks(subNode);
                }
                if (subNode.Name.Equals("overlayblocks"))
                {
                    ret.LoadOverlayBlocks(subNode);
                }
                if (subNode.Name.Equals("matches"))
                {
                    ret.LoadMatches(subNode);
                }
            }

            return (ret);
        }

        private void LoadMatches(XmlNode node)
        {
            foreach (XmlNode match in node.ChildNodes)
            {
                if (SkipNode(match))
                {
                    continue;
                }
                Property prop = new Property();
                prop.Name = match.Attributes["name"].Value;
                prop.Value = match.Attributes["value"].Value;
                Matches.Add(prop);
            }
        }

        private void LoadOverlayBlocks(XmlNode node)
        {
            foreach (XmlNode overlayBlockNode in node.ChildNodes)
            {
                if (SkipNode(overlayBlockNode))
                {
                    continue;
                }
                LinkDefinition link = LinkDefinition.LoadLink(overlayBlockNode);
                OverlayBlocks.Add(link);
            }
        }

        private void LoadTextBlocks(XmlNode node)
        {
            foreach (XmlNode textBlocksNode in node.ChildNodes)
            {
                if (SkipNode(textBlocksNode))
                {
                    continue;
                }
                LinkDefinition link = LinkDefinition.LoadLink(textBlocksNode);
                TextBlocks.Add(link);
            }
        }

        public static bool SkipNode(XmlNode node)
        {
            if (node.NodeType == XmlNodeType.Comment)
            {
                return (true);
            }
            return (false);
        }

    }
}
