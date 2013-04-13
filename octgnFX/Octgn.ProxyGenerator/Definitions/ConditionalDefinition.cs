﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    public class ConditionalDefinition
    {
        public XmlNode ifNode = null;
        public XmlNode elseNode = null;


        public static ConditionalDefinition LoadConditional(XmlNode node)
        {
            ConditionalDefinition ret = new ConditionalDefinition();

            foreach (XmlNode subNode in node.ChildNodes)
            {
                if (TemplateDefinition.SkipNode(subNode))
                {
                    continue;
                }
                if (ret.ifNode != null && subNode.Name == "if")
                {
                    break;
                }
                if (subNode.Name == "if")
                {
                    ret.ifNode = subNode;
                }
                if (subNode.Name == "else")
                {
                    ret.elseNode = subNode;
                }
            }
            return (ret);
        }

        public List<LinkDefinition> ResolveConditional(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
            string property = ifNode.Attributes["property"].Value;
            string value = null;
            string contains = null;

            if (ifNode.Attributes["value"] != null)
            {
                value = ifNode.Attributes["value"].Value;
            }
            if (ifNode.Attributes["contains"] != null)
            {
                contains = ifNode.Attributes["contains"].Value;
            }

            if (value != null)
            {
                if (values.ContainsKey(property) && values[property] == value)
                {
                    foreach (XmlNode node in ifNode.ChildNodes)
                    {
                        LinkDefinition link = LinkDefinition.LoadLink(node);
                        ret.Add(link);
                    }
                }
                else
                {
                    if (elseNode != null)
                    {
                        foreach (XmlNode node in elseNode.ChildNodes)
                        {
                            LinkDefinition link = LinkDefinition.LoadLink(node);
                            ret.Add(link);
                        }
                    }
                }
                return (ret);
            }

            if (contains != null)
            {
                if (values.ContainsKey(property) && values[property].Contains(contains))
                {
                    foreach (XmlNode node in ifNode.ChildNodes)
                    {
                        LinkDefinition link = LinkDefinition.LoadLink(node);
                        ret.Add(link);
                    }
                }
                else
                {
                    if (elseNode != null)
                    {
                        foreach (XmlNode node in elseNode.ChildNodes)
                        {
                            LinkDefinition link = LinkDefinition.LoadLink(node);
                            ret.Add(link);
                        }
                    }
                }
                return (ret);
            }


            return (ret);
        }
    }
}
