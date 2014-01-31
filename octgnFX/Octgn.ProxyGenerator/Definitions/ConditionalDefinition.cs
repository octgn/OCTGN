using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    using Octgn.Library.Exceptions;

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

            ret.AddRange(ResolveIfValue(values));
            if (ret.Count > 0)
            {
                return (ret);
            }

            ret.AddRange(ResolveContainsValue(values));

            
            return (ret);
        }

        internal List<LinkDefinition> ResolveIfValue(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
            bool found = false;
			found = IfValue(values, ifNode, out ret);
            if (found)
            {
                return (ret);
            }
            foreach (XmlNode node in this.elseifNodeList)
            {
                found = this.IfValue(values, node, out ret);
                if (found)
                {
                    return (ret);
                }
            }
            if (this.elseNode != null)
            {
                ret.AddRange(this.LoadLinksFromNode(this.elseNode));
                return (ret);
            }
            return (ret);
        }

        internal bool IfValue(Dictionary<string, string> values, XmlNode node, out List<LinkDefinition> links)
        {
            links = new List<LinkDefinition>();
            if (node.Attributes["property"] == null) return false;
            if (node.Attributes["value"] == null) return false;
            string property = node.Attributes["property"].Value;
            string value = node.Attributes["value"].Value;
            links.AddRange(IfValueList(values, node, property, value));
            return (links.Count > 0);
        }

        internal List<LinkDefinition> IfValueList(Dictionary<string,string> values, XmlNode node, string property, string value)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
            if (values.ContainsKey(property) && values[property] == value)
            {
                return LoadLinksFromNode(node);
            }
            if (value.Equals(NullConstant) && CheckNullConstant(values, property))
            {
                return LoadLinksFromNode(node);
            }
            return (ret);
        }

        internal List<LinkDefinition> LoadLinksFromNode(XmlNode node)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();

            foreach (XmlNode subNode in node.ChildNodes)
            {
                if (TemplateDefinition.SkipNode(subNode))
                {
                    continue;
                }
                LinkDefinition link = LinkDefinition.LoadLink(subNode);
                ret.Add(link);
            }

            return (ret);
        }

        private List<LinkDefinition> ResolveContainsValue(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
            bool found = false;
            if (ifNode.Attributes["contains"] != null)
            {
                found = IfContains(values, ifNode, out ret);
            }
            if (found)
            {
                return (ret);
            }
            if (!found)
            {
                foreach (XmlNode node in elseifNodeList)
                {
                    found = IfContains(values, node, out ret);
                    if (found)
                    {
                        return (ret);
                    }
                }
            }
            if (!found)
            {
                if (elseNode != null)
                {
                    ret.AddRange(LoadLinksFromNode(elseNode));
                    return (ret);
                }
            }
            return (ret);
        }

        internal bool IfContains(Dictionary<string, string> values, XmlNode node, out List<LinkDefinition> links)
        {
            links = new List<LinkDefinition>();
            string property = node.Attributes["property"].Value;
            if (node.Attributes["contains"] == null)
            {
                return (false);
            }
            string contains = node.Attributes["contains"].Value;
            links.AddRange(IfContainsList(values, node, property, contains));
            return (links.Count > 0);
        }

        internal List<LinkDefinition> IfContainsList(Dictionary<string, string> values, XmlNode node, string property, string contains)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
            if (values.ContainsKey(property) && values[property].Contains(contains))
            {
                ret = LoadLinksFromNode(node);
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
            if (values.ContainsKey(property) && (values[property] == null || values[property].Length == 0))
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
                        LinkDefinition link = LinkDefinition.LoadLink(subNode);
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
