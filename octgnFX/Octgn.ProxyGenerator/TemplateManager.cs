using Octgn.ProxyGenerator.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.ProxyGenerator
{
    public class TemplateManager
    {
        private List<TemplateDefinition> templates = null;

        public TemplateManager()
        {
            templates = new List<TemplateDefinition>();
        }

        public void AddTemplate(TemplateDefinition cardDef)
        {
            if (templates.Contains(cardDef))
            {
                templates.Remove(cardDef);
            }
            templates.Add(cardDef);
        }

        public TemplateDefinition GetTemplate(Dictionary<string,string> values)
        {
            TemplateDefinition ret = MatchTemplate(values);
            
            
            return (ret);
        }


        public TemplateDefinition GetDefaultTemplate()
        {
            foreach (TemplateDefinition templateDef in templates)
            {
                if (templateDef.defaultTemplate)
                {
                    return templateDef;
                }
            }
            return null;
        }

        public void ClearTemplates()
        {
            templates.Clear();
        }

        private TemplateDefinition MatchTemplate(Dictionary<string,string> values)
        {
            TemplateDefinition ret = GetDefaultTemplate();
            foreach(TemplateDefinition template in templates)
            {
                bool match = TemplateMatch(values, template);
                if(match)
                {
                    return (template);
                }
            }
            return (ret);
        }

        private bool TemplateMatch(Dictionary<string, string> values, TemplateDefinition template)
        {
            bool ret = false;
            int found = 0;
            foreach (Property prop in template.Matches)
            {
                if (values.ContainsKey(prop.Name))
                {
                    if (values[prop.Name] == prop.Value)
                    {
                        found++;
                    }
                }
            }
            if (found == (template.Matches.Count))
            {
                ret = true;
            }

            return(ret);
        }

    }
}
