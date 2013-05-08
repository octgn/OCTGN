using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    public class ConditionalDefinition
    {
        public XmlNode ifNode = null;
        public List<XmlNode> elseifNodeList = new List<XmlNode>();
        public XmlNode elseNode = null;
        public XmlNode switchNode = null;


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
                if (subNode.Name == "elseif")
                {
                    ret.elseifNodeList.Add(subNode);
                }
                if (subNode.Name == "else")
                {
                    ret.elseNode = subNode;
                }
                if (subNode.Name == "switch")
                {
                    ret.switchNode = subNode;
                }
            }
            return (ret);
        }

        public List<LinkDefinition> ResolveConditional(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
            if (ifNode != null)
            {
                return (ResolveIf(values));
            }
            if (switchNode != null)
            {
                ResolveSwitch(values);
            }
            return (ret);
        }

        internal List<LinkDefinition> ResolveIf(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
            string property = ifNode.Attributes["property"].Value;
            string value = null;
            string contains = null;
            bool loadElse = false;

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
                        if (TemplateDefinition.SkipNode(node))
                        {
                            continue;
                        }
                        LinkDefinition link = LinkDefinition.LoadLink(node);
                        ret.Add(link);
                    }
                }
                else
                {
                    if (elseifNodeList.Count == 0 && ret.Count == 0)
                    {
                        loadElse = true;
                    }
                }
            }

            foreach (XmlNode elseIfNode in elseifNodeList)
            {
                string elseIfValue = null;
                if (elseIfNode.Attributes["value"] != null)
                {
                    elseIfValue = elseIfNode.Attributes["value"].Value;
                }
                if (elseIfValue != null)
                {
                    if (values.ContainsKey(property) && values[property] == elseIfValue)
                    {
                        foreach (XmlNode node in elseIfNode.ChildNodes)
                        {
                            if (TemplateDefinition.SkipNode(node))
                            {
                                continue;
                            }
                            LinkDefinition link = LinkDefinition.LoadLink(node);
                            ret.Add(link);
                            loadElse = false;
                        }
                    }
                }
            }

            if (value != null && loadElse)
            {
                if (elseNode != null)
                {
                    foreach (XmlNode node in elseNode.ChildNodes)
                    {
                        if (TemplateDefinition.SkipNode(node))
                        {
                            continue;
                        }
                        LinkDefinition link = LinkDefinition.LoadLink(node);
                        ret.Add(link);
                    }
                }

            }

            if (contains != null)
            {
                if (values.ContainsKey(property) && values[property].Contains(contains))
                {
                    foreach (XmlNode node in ifNode.ChildNodes)
                    {
                        if (TemplateDefinition.SkipNode(node))
                        {
                            continue;
                        }
                        LinkDefinition link = LinkDefinition.LoadLink(node);
                        ret.Add(link);
                    }
                }
                else
                {
                    if (elseifNodeList.Count == 0 && ret.Count == 0)
                    {
                        loadElse = true;
                    }
                }
            }

            foreach (XmlNode elseIfNode in elseifNodeList)
            {
                string elseIfContains = null;
                if (elseIfNode.Attributes["value"] != null)
                {
                    elseIfContains = elseIfNode.Attributes["value"].Value;
                }
                if (elseIfContains != null)
                {
                    if (values.ContainsKey(property) && values[property].Contains(elseIfContains))
                    {
                        foreach (XmlNode node in elseIfNode.ChildNodes)
                        {
                            if (TemplateDefinition.SkipNode(node))
                            {
                                continue;
                            }
                            LinkDefinition link = LinkDefinition.LoadLink(node);
                            ret.Add(link);
                            loadElse = false;
                        }
                    }
                }
            }

            if (contains != null && loadElse)
            {
                if (elseNode != null)
                {
                    foreach (XmlNode node in elseNode.ChildNodes)
                    {
                        if (TemplateDefinition.SkipNode(node))
                        {
                            continue;
                        }
                        LinkDefinition link = LinkDefinition.LoadLink(node);
                        ret.Add(link);
                    }
                }
            }
            return (ret);
        }

        internal List<LinkDefinition> ResolveSwitch(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
            string property = switchNode.Attributes["property"].Value;

            bool fallThrough = false;
            foreach (XmlNode childNode in switchNode.ChildNodes)
            {
                if (TemplateDefinition.SkipNode(childNode))
                {
                    continue;
                }
                if (childNode.Name == "case")
                {
                    string value = null;
                    string contains = null;
                    bool breakValue = true;
                    if (childNode.Attributes["value"] != null)
                    {
                        value = childNode.Attributes["value"].Value;
                    }
                    if (childNode.Attributes["contains"] != null)
                    {
                        contains = childNode.Attributes["contains"].Value;
                    }
                    if (childNode.Attributes["break"] != null)
                    {
                        breakValue = bool.Parse(childNode.Attributes["break"].Value);
                    }
                    if (fallThrough)
                    {
                        if (value != null)
                        {

                            if (values.ContainsKey(property) && values[property] == value)
                            {
                                foreach (XmlNode node in childNode.ChildNodes)
                                {
                                    if (TemplateDefinition.SkipNode(node))
                                    {
                                        continue;
                                    }
                                    LinkDefinition link = LinkDefinition.LoadLink(node);
                                    ret.Add(link);
                                }
                            }

                            if (!breakValue)
                            {
                                fallThrough = true;
                                continue;
                            }
                        }
                        if (contains != null)
                        {

                            if (values.ContainsKey(property) && values[property].Contains(contains))
                            {
                                foreach (XmlNode node in childNode.ChildNodes)
                                {
                                    if (TemplateDefinition.SkipNode(node))
                                    {
                                        continue;
                                    }
                                    LinkDefinition link = LinkDefinition.LoadLink(node);
                                    ret.Add(link);
                                }
                            }

                            if (!breakValue)
                            {
                                fallThrough = true;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return (ret);
        }
    }
}
