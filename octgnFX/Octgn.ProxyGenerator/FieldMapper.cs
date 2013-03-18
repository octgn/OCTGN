using Octgn.ProxyGenerator.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.ProxyGenerator
{
    public class FieldMapper
    {
        private List<FieldMapping> mappingList = null;
        public TemplateSelector TemplateSelector { get; set; }

        public FieldMapper()
        {
            mappingList = new List<FieldMapping>();
        }

        public void AddMapping(FieldMapping mapping)
        {
            if (mappingList.Contains(mapping))
            {
                mappingList.Remove(mapping);
            }
            mappingList.Add(mapping);
        }

        public void AddMapping(string name, string mapTo)
        {
            FieldMapping mapping = new FieldMapping()
            {
                Name = name,
                MapTo = mapTo
            };
            AddMapping(mapping);
        }

        public void ClearMappings()
        {
            mappingList.Clear();
        }

        public bool ContainsMapping(string field)
        {
            foreach (FieldMapping mapping in mappingList)
            {
                if (mapping.Name == field)
                {
                    return true;
                }
            }
            return false;
        }

        public string MapField(string field)
        {
            string ret = null;
            foreach (FieldMapping mapping in mappingList)
            {
                if (mapping.Name == field)
                {
                    return (mapping.MapTo);
                }
                if (TemplateSelector.ContainsMapping(field))
                {
                    return field;
                }
            }
            return (ret);
        }

        public Dictionary<string, string> RemapDictionary(Dictionary<string, string> oldDict)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> kvi in oldDict)
            {
                string newKey = MapField(kvi.Key);
                if (newKey != null)
                {
                    ret.Add(newKey, kvi.Value);
                }
            }

            return (ret);
        }

        public void DictionaryToMapping(Dictionary<string, string> dict)
        {
            foreach (KeyValuePair<string, string> kvi in dict)
            {
                AddMapping(kvi.Key, kvi.Value);
            }
        }
    }
}
