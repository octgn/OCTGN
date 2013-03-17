using Octgn.ProxyGenerator.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.ProxyGenerator
{
    public class TemplateSelector
    {
        private List<CardDefinition> templates = null;

        public string DefaultID { get; set; }

        public string TemplateMatchField { get; set; }

        public TemplateSelector()
        {
            templates = new List<CardDefinition>();
        }

        public void AddTemplate(CardDefinition cardDef)
        {
            if (templates.Contains(cardDef))
            {
                templates.Remove(cardDef);
            }
            templates.Add(cardDef);
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
    }
}
