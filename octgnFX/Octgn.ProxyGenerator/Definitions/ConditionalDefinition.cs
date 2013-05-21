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
        private string NullConstant = "#NULL#";

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
                return(ResolveSwitch(values));
            }
            return (ret);
        }

        internal List<LinkDefinition> ResolveIf(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
            string property = ifNode.Attributes["property"].Value;
            string value = null;
            string contains = null;
            bool foundMatch = false;

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
                ret.AddRange(IfValue(values, property, value, out foundMatch));
            }
            
            if (foundMatch)
            {
                return (ret);
            }

            if(contains != null)
            {
                ret.AddRange(IfContains(values, property, contains, out foundMatch));
            }
 
            return (ret);
        }

        private List<LinkDefinition> IfValue(Dictionary<string, string> values, string property,string value, out bool foundMatch)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
            bool loadElse = false;
            foundMatch = false;
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
                    foundMatch = true;
                }
                else
                {
                    if (elseifNodeList.Count == 0 && ret.Count == 0)
                    {
                        loadElse = true;
                    }
                }
                if (value.Equals(NullConstant) && CheckNullConstant(values, property))
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
                    foundMatch = true;
                    loadElse = false;
                }
            }
            if (!foundMatch)
            {
                foreach (XmlNode elseIfNode in elseifNodeList)
                {
                    string elseIfValue = null;
                    if (elseIfNode.Attributes["value"] != null)
                    {
                        elseIfValue = elseIfNode.Attributes["value"].Value;
                    }
                    if (elseIfValue != null && !foundMatch)
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
                            }
                            loadElse = false;
                            foundMatch = true;
                        }
                        if (value.Equals(NullConstant) && CheckNullConstant(values, property))
                        {
                            foreach (XmlNode node in elseIfNode.ChildNodes)
                            {
                                if (TemplateDefinition.SkipNode(node))
                                {
                                    continue;
                                }
                                LinkDefinition link = LinkDefinition.LoadLink(node);
                                ret.Add(link);
                            }
                            foundMatch = true;
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
            return (ret);
        }

        private List<LinkDefinition> IfContains(Dictionary<string, string> values, string property, string contains, out bool foundMatch)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
            bool loadElse = false;
            foundMatch = false;
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
                    foundMatch = true;
                }
                else
                {
                    if (elseifNodeList.Count == 0 && ret.Count == 0)
                    {
                        loadElse = true;
                    }
                }
            }
            if (!foundMatch)
            {
                foreach (XmlNode elseIfNode in elseifNodeList)
                {
                    string elseIfContains = null;
                    if (elseIfNode.Attributes["contains"] != null)
                    {
                        elseIfContains = elseIfNode.Attributes["contains"].Value;
                    }
                    if (elseIfContains != null && !foundMatch)
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

                            }
                            loadElse = false;
                            foundMatch = true;
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

        internal bool CheckNullConstant(Dictionary<string, string> values, string property)
        {
            bool ret = false;

            if (!values.ContainsKey(property))
            {
                ret = true;
            }
            if (values.ContainsKey(property) && values[property] == null)
            {
                ret = true;
            }

            return (ret);
        }

        internal List<LinkDefinition> ResolveSwitch(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
            string property = switchNode.Attributes["property"].Value;

            bool currentBreak = true;
            bool foundMatch = false;
            foreach (XmlNode childNode in switchNode.ChildNodes)
            {
                if (TemplateDefinition.SkipNode(childNode))
                {
                    continue;
                }
                if (childNode.Name == "case")
                {
                    List<LinkDefinition> list = ResolveCase(values, childNode, property, out currentBreak);
                    foundMatch = (list.Count > 0);
                    ret.AddRange(list);
                }
                if ((!foundMatch || !currentBreak) && childNode.Name == "default")
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
                    foundMatch = true;
                    currentBreak = true;
                }
                if (foundMatch && currentBreak)
                {
                    break;
                }
            }

            return (ret);
        }

        private List<LinkDefinition> ResolveCase(Dictionary<string, string> values, XmlNode node, string property, out bool breakValue)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();

            string value = null;
            string contains = null;
            breakValue = true;
            if (node.Attributes["value"] != null)
            {
                value = node.Attributes["value"].Value;
            }
            if (node.Attributes["contains"] != null)
            {
                contains = node.Attributes["contains"].Value;
            }
            if (node.Attributes["break"] != null)
            {
                breakValue = bool.Parse(node.Attributes["break"].Value);
            }

            if (value != null)
            {

                if (values.ContainsKey(property) && values[property] == value)
                {
                    foreach (XmlNode subNode in node.ChildNodes)
                    {
                        if (TemplateDefinition.SkipNode(subNode))
                        {
                            continue;
                        }
                        LinkDefinition link = LinkDefinition.LoadLink(subNode);
                        ret.Add(link);
                    }
                }
                if (value.Equals(NullConstant) && CheckNullConstant(values, property))
                {
                    foreach (XmlNode subNode in node.ChildNodes)
                    {
                        if (TemplateDefinition.SkipNode(subNode))
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
                    foreach (XmlNode subNode in node.ChildNodes)
                    {
                        if (TemplateDefinition.SkipNode(subNode))
                        {
                            continue;
                        }
                        LinkDefinition link = LinkDefinition.LoadLink(subNode);
                        ret.Add(link);
                    }
                }
            }

            return (ret);
        }
    }
}
