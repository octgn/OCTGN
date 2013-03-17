using Octgn.ProxyGenerator.Definitions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Octgn.ProxyGenerator
{
    public class ProxyManager
    {
        #region Singleton
        private static ProxyManager Context { get; set; }
        private static object locker = new object();
        public static ProxyManager Get()
        {
            lock (locker) return Context ?? (Context = new ProxyManager());
        }
        internal ProxyManager()
        {
            templateSelector = new TemplateSelector();
            fieldMapper = new FieldMapper();
        }
        #endregion Singleton

        private TemplateSelector templateSelector = null;
        private FieldMapper fieldMapper = null;
        private XmlDocument doc = null;

        public bool LoadDefinition(string path)
        {
            if (File.Exists(path))
            {
                LoadXML(path);
                return true;
            }
            return false;
        }

        private void LoadXML(string path)
        {
            if (doc != null)
            {
                doc.RemoveAll();
                doc = null;
                templateSelector.ClearTemplates();
                fieldMapper.ClearMappings();
            }
            doc = new XmlDocument();
            doc.Load(path);
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            XmlNodeList cardList = doc.GetElementsByTagName("card");
            foreach (XmlNode card in cardList)
            {
                CardDefinition cardDef = CardDefinition.LoadCardDefinition(card);
                templateSelector.AddTemplate(cardDef);
            }
        }

        public Image GenerateProxyImage(Dictionary<string, string> values)
        {
            values = GetFieldMapper().RemapDictionary(values);
            CardDefinition cardDef = GetTemplateSelector().GetTemplate(values);
            Image ret = ProxyGenerator.GenerateProxy(cardDef, values);
            return (ret);
        }

        public Image GenerateProxyImage(string templateID, Dictionary<string, string> values)
        {
            values = GetFieldMapper().RemapDictionary(values);
            CardDefinition cardDef = templateSelector.GetTemplate(templateID);
            Image ret = ProxyGenerator.GenerateProxy(cardDef, values);
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

        public FieldMapper GetFieldMapper()
        {
            return (fieldMapper);
        }

        public TemplateSelector GetTemplateSelector()
        {
            return (templateSelector);
        }
    }
}
