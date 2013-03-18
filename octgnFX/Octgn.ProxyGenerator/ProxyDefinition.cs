namespace Octgn.ProxyGenerator
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Xml;

    using Octgn.ProxyGenerator.Definitions;

    public class ProxyDefinition
    {
        public object Key { get; internal set; }
        public TemplateSelector TemplateSelector { get; internal set; }
        public FieldMapper FieldMapper { get; internal set; }
        internal XmlDocument Document;
        public string RootPath { get; internal set; }

        public ProxyDefinition(object key, string path, string rootPath)
        {
            RootPath = rootPath;
            Key = key;
            TemplateSelector = new TemplateSelector();
            FieldMapper = new FieldMapper();
            FieldMapper.TemplateSelector = TemplateSelector;
            Load(path);
        }

        public Image GenerateProxyImage(Dictionary<string, string> values)
        {
            values = FieldMapper.RemapDictionary(values);
            CardDefinition cardDef = TemplateSelector.GetTemplate(values);
            Image ret = ProxyGenerator.GenerateProxy(RootPath,cardDef, values);
            return (ret);
        }

        public Image GenerateProxyImage(string templateID, Dictionary<string, string> values)
        {
            values = FieldMapper.RemapDictionary(values);
            CardDefinition cardDef = TemplateSelector.GetTemplate(templateID);
            Image ret = ProxyGenerator.GenerateProxy(RootPath,cardDef, values);
            return (ret);
        }

        public bool SaveProxyImage(Dictionary<string, string> values, string path)
        {
            Image proxy = GenerateProxyImage(values);
            proxy.Save(path);
            proxy.Dispose();
            return (File.Exists(path));
        }

        public bool SaveProxyImage(string templateID, Dictionary<string, string> values, string path)
        {
            Image proxy = GenerateProxyImage(templateID, values);
            proxy.Save(path);
            proxy.Dispose();
            return (File.Exists(path));
        }

        internal void Load(string path)
        {
            if (Document != null)
            {
                Document.RemoveAll();
                Document = null;
                TemplateSelector.ClearTemplates();
                FieldMapper.ClearMappings();
            }
            Document = new XmlDocument();
            Document.Load(path);
            LoadTemplates();
        }

        internal void LoadTemplates()
        {
            XmlNodeList cardList = Document.GetElementsByTagName("card");
            foreach (XmlNode card in cardList)
            {
                CardDefinition cardDef = CardDefinition.LoadCardDefinition(card);
                TemplateSelector.AddTemplate(cardDef);
            }
        }
    }
}