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
        public List<LinkDefinition> linkList = new List<LinkDefinition>();

    }
    public class ConditionalDefinition
    {
        public CaseDefinition ifNode = null;
        public List<CaseDefinition> elseifNodeList = new List<CaseDefinition>();
        public CaseDefinition elseNode = null;
        public List<CaseDefinition> switchNodeList = new List<CaseDefinition>();
        public string switchProperty = null;
        private string NullConstant = "#NULL#";

        public List<LinkDefinition> ResolveConditional(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
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

        internal bool IfValue(Dictionary<string, string> values, CaseDefinition caseDef, out List<LinkDefinition> links)
        {
            links = new List<LinkDefinition>();
            if (caseDef.property == null) return false;
            if (caseDef.value == null) return false;
            string property = caseDef.property;
            string value = caseDef.value;
            links.AddRange(IfValueList(values, caseDef, property, value));
            return (links.Count > 0);
        }

        internal List<LinkDefinition> IfValueList(Dictionary<string,string> values, CaseDefinition caseDef, string property, string value)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
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


        private List<LinkDefinition> ResolveContainsValue(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
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

        internal bool IfContains(Dictionary<string, string> values, CaseDefinition caseDef, out List<LinkDefinition> links)
        {
            links = new List<LinkDefinition>();
            string property = caseDef.property;
            if (caseDef.contains == null)
            {
                return (false);
            }
            string contains = caseDef.contains;
            links.AddRange(IfContainsList(values, caseDef, property, contains));
            return (links.Count > 0);
        }

        internal List<LinkDefinition> IfContainsList(Dictionary<string, string> values, CaseDefinition caseDef, string property, string contains)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();
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

        internal List<LinkDefinition> ResolveSwitch(Dictionary<string, string> values)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();

            bool currentBreak = true;
            bool foundMatch = false;
            foreach (CaseDefinition caseDef in switchNodeList)
            {
                List<LinkDefinition> list = ResolveCase(values, caseDef, switchProperty, out currentBreak);
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

        private List<LinkDefinition> ResolveCase(Dictionary<string, string> values, CaseDefinition caseDef, string property, out bool breakValue)
        {
            List<LinkDefinition> ret = new List<LinkDefinition>();

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
