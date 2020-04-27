using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator.Definitions
{
    using Octgn.Library.Exceptions;

    public class CaseDefinition
    {
        public string property = null;
        public string value = null;
        public string contains = null;
        public bool switchBreak = true;
        public List<LinkDefinition.LinkWrapper> linkList = new List<LinkDefinition.LinkWrapper>();

    }
    public class ConditionalDefinition
    {
        public CaseDefinition ifNode = null;
        public List<CaseDefinition> elseifNodeList = new List<CaseDefinition>();
        public CaseDefinition elseNode = null;
        public List<CaseDefinition> switchNodeList = new List<CaseDefinition>();
        public string switchProperty = null;
        private string NullConstant = "#NULL#";

        public List<LinkDefinition.LinkWrapper> ResolveConditional(Dictionary<string, string> values)
        {
            List<LinkDefinition.LinkWrapper> ret = new List<LinkDefinition.LinkWrapper>();
            if (ifNode != null)
            {
                return (ResolveIf(values));
            }
            if (switchProperty != null)
            {
                return(ResolveSwitch(values));
            }
            return (ret);
        }

        internal List<LinkDefinition.LinkWrapper> ResolveIf(Dictionary<string, string> values)
        {
            List<LinkDefinition.LinkWrapper> ret = new List<LinkDefinition.LinkWrapper>();

            ret.AddRange(ResolveIfValue(values));
            if (ret.Count > 0)
            {
                return (ret);
            }

            ret.AddRange(ResolveContainsValue(values));

            
            return (ret);
        }

        internal List<LinkDefinition.LinkWrapper> ResolveIfValue(Dictionary<string, string> values)
        {
            List<LinkDefinition.LinkWrapper> ret = new List<LinkDefinition.LinkWrapper>();
            bool found = false;
			found = IfValue(values, ifNode, out ret);
            if (found)
            {
                return (ret);
            }
            foreach (CaseDefinition caseDef in this.elseifNodeList)
            {
                found = this.IfValue(values, caseDef, out ret);
                if (found)
                {
                    return (ret);
                }
            }
            if (this.elseNode != null && ifNode.contains == null)
            {
                ret.AddRange(elseNode.linkList);
                return (ret);
            }
            return (ret);
        }

        internal bool IfValue(Dictionary<string, string> values, CaseDefinition caseDef, out List<LinkDefinition.LinkWrapper> links)
        {
            links = new List<LinkDefinition.LinkWrapper>();
            if (caseDef.property == null) return false;
            if (caseDef.value == null) return false;
            string property = caseDef.property;
            string value = caseDef.value;
            links.AddRange(IfValueList(values, caseDef, property, value));
            return (links.Count > 0);
        }

        internal List<LinkDefinition.LinkWrapper> IfValueList(Dictionary<string,string> values, CaseDefinition caseDef, string property, string value)
        {
            List<LinkDefinition.LinkWrapper> ret = new List<LinkDefinition.LinkWrapper>();
            if (values.ContainsKey(property) && values[property] == value)
            {
                return caseDef.linkList;
            }
            if (value.Equals(NullConstant) && CheckNullConstant(values, property))
            {
                return caseDef.linkList;
            }
            return (ret);
        }


        private List<LinkDefinition.LinkWrapper> ResolveContainsValue(Dictionary<string, string> values)
        {
            List<LinkDefinition.LinkWrapper> ret = new List<LinkDefinition.LinkWrapper>();
            bool found = false;
            if (ifNode.contains != null)
            {
                found = IfContains(values, ifNode, out ret);
            }
            if (found)
            {
                return (ret);
            }
            if (!found)
            {
                foreach (CaseDefinition caseDef in elseifNodeList)
                {
                    found = IfContains(values, caseDef, out ret);
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
                    ret.AddRange(elseNode.linkList);
                    return (ret);
                }
            }
            return (ret);
        }

        internal bool IfContains(Dictionary<string, string> values, CaseDefinition caseDef, out List<LinkDefinition.LinkWrapper> links)
        {
            links = new List<LinkDefinition.LinkWrapper>();
            string property = caseDef.property;
            if (caseDef.contains == null)
            {
                return (false);
            }
            string contains = caseDef.contains;
            links.AddRange(IfContainsList(values, caseDef, property, contains));
            return (links.Count > 0);
        }

        internal List<LinkDefinition.LinkWrapper> IfContainsList(Dictionary<string, string> values, CaseDefinition caseDef, string property, string contains)
        {
            List<LinkDefinition.LinkWrapper> ret = new List<LinkDefinition.LinkWrapper>();
            if (values.ContainsKey(property) && values[property].Contains(contains))
            {
                ret = caseDef.linkList;
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

        internal List<LinkDefinition.LinkWrapper> ResolveSwitch(Dictionary<string, string> values)
        {
            List<LinkDefinition.LinkWrapper> ret = new List<LinkDefinition.LinkWrapper>();

            bool currentBreak = true;
            bool foundMatch = false;
            foreach (CaseDefinition caseDef in switchNodeList)
            {
                List<LinkDefinition.LinkWrapper> list = ResolveCase(values, caseDef, switchProperty, out currentBreak);
                foundMatch = (list.Count > 0);
                ret.AddRange(list);

                if (foundMatch && currentBreak)
                {
                    break;
                }
            }

            if (!foundMatch && elseNode != null)
            {
                ret.AddRange(elseNode.linkList);
            }

            return (ret);
        }

        private List<LinkDefinition.LinkWrapper> ResolveCase(Dictionary<string, string> values, CaseDefinition caseDef, string property, out bool breakValue)
        {
            List<LinkDefinition.LinkWrapper> ret = new List<LinkDefinition.LinkWrapper>();

            breakValue = caseDef.switchBreak;

            if (caseDef.value != null)
            {

                if (values.ContainsKey(property) && values[property] == caseDef.value)
                {
                    ret.AddRange(caseDef.linkList);
                }
                if (caseDef.value.Equals(NullConstant) && CheckNullConstant(values, property))
                {
                    ret.AddRange(caseDef.linkList);
                }
            }
            if (caseDef.contains != null)
            {
                if (values.ContainsKey(property) && values[property].Contains(caseDef.contains))
                {
                    ret.AddRange(caseDef.linkList);
                }
            }

            return (ret);
        }
    }
}
