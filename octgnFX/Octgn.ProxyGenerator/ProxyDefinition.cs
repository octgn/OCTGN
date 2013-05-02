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
        public BlockManager BlockManager { get; internal set; }
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
            Image ret = ProxyGenerator.GenerateProxy(BlockManager,RootPath,cardDef, values);
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
            BlockManager = new BlockManager(RootPath);
            BlockManager.LoadBlocks(blockList[0]);

            XmlNodeList templateList = Document.GetElementsByTagName("template");
            foreach (XmlNode template in templateList)
            {
                TemplateDefinition templateDef = TemplateDefinition.LoadCardDefinition(template);
                templateDef.rootPath = RootPath;
                TemplateSelector.AddTemplate(templateDef);
            }
            
        }

        public static Dictionary<string, string> GetBlockSources(string path)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            foreach (XmlNode node in doc.GetElementsByTagName("block"))
            {
                string id = node.Attributes["id"].Value;
                if (node.Attributes["src"] != null)
                {
                    ret.Add(id, node.Attributes["src"].Value);
                }
                string fontPath = GetFontPath(node);
                if (fontPath != string.Empty)
                {
                    ret.Add(id, fontPath);
                }
            }

            doc.RemoveAll();
            doc = null;

            return (ret);
        }

        private static string GetFontPath(XmlNode node)
        {
            string ret = string.Empty;
            foreach (XmlNode subNode in node.ChildNodes)
            {
                if (TemplateDefinition.SkipNode(subNode))
                {
                    continue;
                }
                if (subNode.Name == "text")
                {
                    if (subNode.Attributes["font"] != null)
                    {
                        ret = subNode.Attributes["font"].Value;
                        break;
                    }
                }
            }
            return (ret);
        }

        public static List<string> GetTemplateSources(string path)
        {
            List<string> ret = new List<string>();

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            foreach (XmlNode node in doc.GetElementsByTagName("template"))
            {
                ret.Add(node.Attributes["src"].Value);
            }

            doc.RemoveAll();
            doc = null;

            return (ret);
        }
    }
}