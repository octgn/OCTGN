namespace Octgn.ProxyGenerator
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Xml;

    using Octgn.ProxyGenerator.Definitions;
    using System.Drawing.Imaging;

    public class ProxyDefinition
    {
        public object Key { get; internal set; }
        public TemplateManager TemplateSelector { get; internal set; }
        internal XmlDocument Document;
        public string RootPath { get; internal set; }

        public ProxyDefinition(object key, string path, string rootPath)
        {
            RootPath = rootPath;
            Key = key;
            TemplateSelector = new TemplateManager();
            Load(path);
        }

        public Image GenerateProxyImage(Dictionary<string, string> values)
        {
            TemplateDefinition cardDef = TemplateSelector.GetTemplate(values);
            Image ret = ProxyGenerator.GenerateProxy(RootPath,cardDef, values);
            return (ret);
        }

        public bool SaveProxyImage(Dictionary<string, string> values, string path)
        {
            Image proxy = GenerateProxyImage(values);
            SaveProxyImage(proxy, path);
            return (File.Exists(path));
        }

        private void SaveProxyImage(Image proxy, string path)
        {
            proxy.Save(path, ImageFormat.Png);
            proxy.Dispose();
        }

        internal void Load(string path)
        {
            if (Document != null)
            {
                Document.RemoveAll();
                Document = null;
                TemplateSelector.ClearTemplates();
            }
            Document = new XmlDocument();
            Document.Load(path);
            LoadTemplates();
        }

        internal void LoadTemplates()
        {
            XmlNodeList blockList = Document.GetElementsByTagName("blocks");
            BlockManager.GetInstance().LoadBlocks(blockList[0]);

            XmlNodeList templateList = Document.GetElementsByTagName("template");
            foreach (XmlNode template in templateList)
            {
                TemplateDefinition templateDef = TemplateDefinition.LoadCardDefinition(template);
                templateDef.rootPath = RootPath;
                TemplateSelector.AddTemplate(templateDef);
            }
            
        }
    }
}