using Octgn.ProxyGenerator.Definitions;
using Octgn.ProxyGenerator.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.ProxyGenerator
{
    public class TemplateManager
    {
        private List<CardDefinition> templates = null;

        public string DefaultID { get; set; }
        public bool UseMultiFieldMatching { get; set; }

        private List<TemplateMapping> templateMappings;

        public TemplateManager()
        {
            templates = new List<CardDefinition>();
            templateMappings = new List<TemplateMapping>();
        }

        public void AddTemplate(CardDefinition cardDef)
        {
            if (templates.Contains(cardDef))
            {
                templates.Remove(cardDef);
            }
            templates.Add(cardDef);
        }

        public CardDefinition GetTemplate(Dictionary<string,string> values)
        {
            string field = templateMappings[0].Name;
            if (field != null && values.ContainsKey(field))
            {
                return GetTemplate(values[field]);
            }
            return GetDefaultTemplate();
        }

        public CardDefinition GetTemplate(string ID)
        {
            CardDefinition ret = GetDefaultTemplate();
            foreach (CardDefinition cardDef in templates)
            {
                if (cardDef.id == ID)
                {
                    ret = cardDef;
                    break;
                }
            }
            return (ret);
        }

        public CardDefinition GetDefaultTemplate()
        {
            foreach (CardDefinition cardDef in templates)
            {
                if (cardDef.id == DefaultID)
                {
                    return cardDef;
                }
            }
            return null;
        }

        public void ClearTemplates()
        {
            templates.Clear();
        }

        public void AddMapping(string field)
        {
            AddMapping(field, string.Empty);
        }

        public void AddMapping(string field, string mapTo)
        {
            TemplateMapping mapping = new TemplateMapping()
            {
                Name = field,
                MapTo = mapTo
            };
            if (ContainsMapping(mapping))
            {
                templateMappings.Remove(mapping);
            }
            templateMappings.Add(mapping);
        }

        public bool ContainsMapping(string field)
        {
            TemplateMapping mapping = new TemplateMapping()
            {
                Name = field
            };
            return (ContainsMapping(mapping));
        }

        public bool ContainsMapping(TemplateMapping mapping)
        {
            foreach (TemplateMapping map in templateMappings)
            {
                if (map.Equals(mapping))
                {
                    return (true);
                }
            }
            return (false);
        }
    }
}
