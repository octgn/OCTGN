using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    public class TemplateDefinition
    {
        public List<LinkDefinition.LinkWrapper> OverlayBlocks = new List<LinkDefinition.LinkWrapper>();
        public List<LinkDefinition.LinkWrapper> TextBlocks = new List<LinkDefinition.LinkWrapper>();
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
                LinkDefinition.LinkWrapper wrapper = new LinkDefinition.LinkWrapper();
                if (overlayBlockNode.Name == "link")
                {
                    LinkDefinition link = LinkDefinition.LoadLink(overlayBlockNode);
                    wrapper.Link = link;
                }
                if (overlayBlockNode.Name == "conditional")
                {
                    ConditionalDefinition conditional = ConditionalDefinition.LoadConditional(overlayBlockNode);
                    wrapper.Conditional = conditional;
                }

                OverlayBlocks.Add(wrapper);
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
                LinkDefinition.LinkWrapper wrapper = new LinkDefinition.LinkWrapper();
                if (textBlocksNode.Name == "link")
                {
                    LinkDefinition link = LinkDefinition.LoadLink(textBlocksNode);
                    wrapper.Link = link;
                }
                if (textBlocksNode.Name == "conditional")
                {
                    ConditionalDefinition conditional = ConditionalDefinition.LoadConditional(textBlocksNode);
                    wrapper.Conditional = conditional;
                }

                TextBlocks.Add(wrapper);
            }
        }

        public List<LinkDefinition> GetOverLayBlocks(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();

            foreach (LinkDefinition.LinkWrapper wrapper in OverlayBlocks)
            {
                if (wrapper.Link != null)
                {
                    ret.Add(wrapper.Link);
                }
                if (wrapper.Conditional != null)
                {
                    ret.AddRange(wrapper.Conditional.ResolveConditional(values));
                }
            }

            return (ret);
        }

        public List<LinkDefinition> GetTextBlocks(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();

            foreach (LinkDefinition.LinkWrapper wrapper in TextBlocks)
            {
                if (wrapper.Link != null)
                {
                    ret.Add(wrapper.Link);
                }
                if (wrapper.Conditional != null)
                {
                    ret.AddRange(wrapper.Conditional.ResolveConditional(values));
                }
            }

            return (ret);
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
