// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.ProxyGenerator.Definitions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Octgn.ProxyGenerator
{
    class ProxyDeserializer
    {
        public static BlockDefinition DeserializeBlock(string rootPath, XmlNode node)
        {
            BlockDefinition ret = new BlockDefinition();

            ret.id = node.Attributes["id"].Value;
            ret.type = node.Attributes["type"].Value;
            if (node.Attributes["src"] != null)
            {
                //todo: make sure the src exists and figure out what to do if it doesn't
                ret.src = Path.Combine(rootPath, node.Attributes["src"].Value);
            }
            foreach (XmlNode prop in node.ChildNodes)
            {
                if (SkipNode(prop))
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
                        string combined = Path.Combine(rootPath, prop.Attributes["font"].Value);
                        if (File.Exists(combined))
                        {
                            ret.text.font = combined;
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
        public static BlockDefinition DeserializeArtOverlayBlock(XmlNode node)
        {
            BlockDefinition ret = new BlockDefinition();

            ret.type = "overlay";

            foreach (XmlNode prop in node.ChildNodes)
            {
                if (SkipNode(prop))
                {
                    continue;
                }
                if (prop.Name.Equals("location"))
                {
                    ret.location.x = int.Parse(prop.Attributes["x"].Value);
                    ret.location.y = int.Parse(prop.Attributes["y"].Value);
                }
                if (prop.Name.Equals("size"))
                {
                    ret.wordwrap.height = int.Parse(prop.Attributes["height"].Value);
                    ret.wordwrap.width = int.Parse(prop.Attributes["width"].Value);
                }
            }
            return (ret);
        }

        public static TemplateDefinition DeserializeTemplate(string rootPath, XmlNode node)
        {
            TemplateDefinition ret = new TemplateDefinition();
            //todo: make sure the src exists and figure out what to do if it doesn't
            ret.src = Path.Combine(rootPath, node.Attributes["src"].Value);
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
                if (subNode.Name.Equals("matches"))
                {
                    foreach (XmlNode match in subNode.ChildNodes)
                    {
                        if (SkipNode(match))
                        {
                            continue;
                        }
                        Property prop = new Property();
                        prop.Name = match.Attributes["name"].Value;
                        prop.Value = match.Attributes["value"].Value;
                        ret.Matches.Add(prop);
                    }
                }
                if (subNode.Name.Equals("overlayblocks"))
                {
                    foreach (XmlNode overlayBlockNode in subNode.ChildNodes)
                    {
                        if (SkipNode(node))
                        {
                            continue;
                        }

                        var wrapper = DeserializeLinkWrapper(overlayBlockNode, false);

                        ret.OverlayBlocks.Add(wrapper);
                    }
                }
                if (subNode.Name.Equals("textblocks"))
                {
                    foreach (XmlNode textBlocksNode in subNode.ChildNodes)
                    {
                        if (SkipNode(textBlocksNode))
                        {
                            continue;
                        }
                        var wrapper = DeserializeLinkWrapper(textBlocksNode, true);

                        ret.TextBlocks.Add(wrapper);
                    }
                }
            }

            return (ret);
        }

        public static LinkDefinition.LinkWrapper DeserializeLinkWrapper(XmlNode node, bool isTextBlock)
        {
            var ret = new LinkDefinition.LinkWrapper();
            if (node.Name == "link")
            {
                LinkDefinition link = DeserializeLink(node, isTextBlock);
                ret.Link = link;
            }
            if (node.Name == "conditional")
            {
                ConditionalDefinition conditional = DeserializeConditional(node, isTextBlock);
                ret.Conditional = conditional;
            }
            if (node.Name == "artoverlay")
            {
                BlockDefinition b = DeserializeArtOverlayBlock(node);
                ret.CardArtCrop = b;
            }
            return ret;
        }

        public static ConditionalDefinition DeserializeConditional(XmlNode node, bool isTextBlock)
        {
            ConditionalDefinition ret = new ConditionalDefinition();

            foreach (XmlNode subNode in node.ChildNodes)
            {
                if (SkipNode(subNode))
                {
                    continue;
                }
                if (subNode.Name == "switch")
                {
                    ret.switchProperty = subNode.Attributes["property"].Value;
                    foreach (XmlNode switchNode in subNode.ChildNodes)
                    {

                        if (SkipNode(switchNode))
                        {
                            continue;
                        }
                        if (switchNode.Name == "default")
                        {
                            ret.elseNode = DeserializeCase(switchNode, isTextBlock);
                        }
                        if (switchNode.Name == "case")
                        {
                            CaseDefinition switchCase = DeserializeCase(switchNode, isTextBlock);
                            ret.switchNodeList.Add(switchCase);
                        }
                    }
                }
                else
                {
                    if (ret.ifNode != null && subNode.Name == "if")
                    {
                        //when there are two if cases, ignore all but the first
                        //TODO: properly split apart conditional from switch in the XSD to enforce 1 minimum if
                        continue;
                    }
                    if (ret.ifNode == null && subNode.Name == "elseif")
                    {
                        //when there's no if case, convert the first elif to an if
                        ret.ifNode = DeserializeCase(subNode, isTextBlock);
                        continue;
                    }
                    if (subNode.Name == "if")
                    {
                        ret.ifNode = DeserializeCase(subNode, isTextBlock);
                    }
                    if (subNode.Name == "elseif")
                    {
                        ret.elseifNodeList.Add(DeserializeCase(subNode, isTextBlock));
                    }
                    if (subNode.Name == "else")
                    {
                        ret.elseNode = DeserializeCase(subNode, isTextBlock);
                    }
                }
            }
            return (ret);
        }

        public static CaseDefinition DeserializeCase(XmlNode node, bool isTextBlock)
        {
            var ret = new CaseDefinition();
            if (node.Attributes["contains"] != null)
            {
                ret.contains = node.Attributes["contains"].Value;
            }
            if (node.Attributes["value"] != null)
            {
                ret.value = node.Attributes["value"].Value;
            }
            if (node.Attributes["break"] != null)
            {
                ret.switchBreak = bool.Parse(node.Attributes["break"].Value);
            }
            if (node.Attributes["property"] != null)
            {
                ret.property = node.Attributes["property"].Value;
            }
            foreach (XmlNode linkNode in node.ChildNodes)
            {
                if (SkipNode(linkNode))
                {
                    continue;
                }
                var wrapper = DeserializeLinkWrapper(linkNode, isTextBlock);
                ret.linkList.Add(wrapper);
            }
            return ret;
        }

        public static LinkDefinition DeserializeLink(XmlNode node, bool isTextBlock)
        {
            LinkDefinition ret = new LinkDefinition();
            ret.IsTextLink = isTextBlock;
            ret.Block = node.Attributes["block"].Value;
            if (node.Attributes["separator"] != null)
            {
                ret.Separator = node.Attributes["separator"].Value;
            }
            if (node.HasChildNodes)
            {
                foreach (XmlNode subNode in node.ChildNodes)
                {
                    if (SkipNode(subNode))
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
