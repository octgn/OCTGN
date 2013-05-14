using Octgn.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Octgn.Tools.Proxytest
{
    public partial class Form1 : Form
    {
        private BindingSource bindingSource = new BindingSource();
        private string xmlPropertyFileName = "properties.xml";

        public Form1()
        {
            InitializeComponent();
        }

        private void newPropertiesButton_Click(object sender, EventArgs e)
        {
            ResetDataTable();
        }

        private void ResetDataTable()
        {
            DataTable dt = SetupDataTable();
            string[] values = new string[] { "Name", "test name" };
            dt.Rows.Add(values);
            bindingSource.DataSource = dt;
        }

        private DataTable SetupDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("Value");
            return (dt);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            propertiesDataGrid.DataSource = bindingSource;
            ResetDataTable();
            LoadXML();
        }

        private void savePropertiesButton_Click(object sender, EventArgs e)
        {
            SaveAsXML();
        }

        private void SaveAsXML()
        {
            DataTable dt = (DataTable)bindingSource.DataSource;
            XmlDocument doc = new XmlDocument();
            XmlNode rootNode = doc.CreateNode(XmlNodeType.Element,"root", null);
            foreach (DataRow row in dt.Rows)
            {
                object[] values = row.ItemArray;
                XmlNode prop = doc.CreateNode(XmlNodeType.Element, "propertyset", null);
                XmlNode name = doc.CreateNode(XmlNodeType.Element, "name", null);
                XmlNode value = doc.CreateNode(XmlNodeType.Element, "value", null);
                name.InnerText = (string)values[0];
                value.InnerText = (string)values[1];
                prop.AppendChild(name);
                prop.AppendChild(value);
                rootNode.AppendChild(prop);
            }
            doc.AppendChild(rootNode);
            string file = Path.Combine(GetExecutingDir(), xmlPropertyFileName);
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            doc.Save(file);
            doc.RemoveAll();
            doc = null;
        }

        private void LoadXML(string path)
        {
            DataTable dt = SetupDataTable();
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList list = doc.GetElementsByTagName("propertyset");
            foreach (XmlNode node in list)
            {
                string[] values = new string[2];
                values[0] = node.ChildNodes[0].InnerText;
                values[1] = node.ChildNodes[1].InnerText;
                dt.Rows.Add(values);
            }
            bindingSource.DataSource = dt;
            doc.RemoveAll();
            doc = null;
        }

        private void LoadXML()
        {
            
            string propFile = Path.Combine(GetExecutingDir(), xmlPropertyFileName);
            if (File.Exists(propFile))
            {
                LoadXML(propFile);
            }
        }

        private string GetExecutingDir()
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string dir = Path.GetDirectoryName(assemblyLocation);
            return (dir);
        }

        private void loadPropertiesButton_Click(object sender, EventArgs e)
        {
            if (xmlPropertiesOpenDialog.ShowDialog() != DialogResult.Cancel)
            {
                LoadXML(xmlPropertiesOpenDialog.FileName);
            }
        }


        private bool ValidateTemplatePaths()
        {
            Dictionary<string, string> blockSources = ProxyDefinition.GetBlockSources(proxydefPathTextBox.Text);
            foreach (KeyValuePair<string, string> kvi in blockSources)
            {
                string path = Path.Combine(rootDirTextBox.Text, kvi.Value);
                if (!File.Exists(path))
                {
                    return (false);
                }
            }
            List<string> templateSource = ProxyDefinition.GetTemplateSources(proxydefPathTextBox.Text);
            foreach (string s in templateSource)
            {
                string path = Path.Combine(rootDirTextBox.Text, s);
                if (!File.Exists(path))
                {
                    return (false);
                }
            }
            return (true);
        }

        private ProxyDefinition GetProxyDef(string path, string rootPath)
        {
            ProxyDefinition ret = new ProxyDefinition(null, path, rootPath);
            return (ret);
        }

        private void loadFromProxydefButton_Click(object sender, EventArgs e)
        {
            if (proxydefOpenDialog.ShowDialog() != DialogResult.Cancel)
            {
                proxydefPathTextBox.Text = proxydefOpenDialog.FileName;
                if (rootFolderBrowserDialog.ShowDialog() != DialogResult.Cancel)
                {
                    rootDirTextBox.Text = rootFolderBrowserDialog.SelectedPath;
                    if (!ValidateTemplatePaths())
                    {
                        proxydefPathTextBox.Text = string.Empty;
                        rootDirTextBox.Text = string.Empty;
                        MessageBox.Show("Template contains an invalid image path");
                    }
                    else
                    {
                        generateProxyButton.Enabled = true;
                    }
                }
            }
        }

        private void openDefinitionButton_Click(object sender, EventArgs e)
        {
            if (definitionOpenDialog.ShowDialog() != DialogResult.Cancel)
            {
                string def = definitionOpenDialog.FileName;
                rootDirTextBox.Text = Path.GetDirectoryName(def);
                XmlDocument doc = new XmlDocument();
                doc.Load(def);
                XmlNodeList list = doc.GetElementsByTagName("proxygen");
                string relPath = list.Item(0).Attributes["definitionsrc"].Value;
                proxydefPathTextBox.Text = Path.Combine(rootDirTextBox.Text, relPath);
                doc.RemoveAll();
                doc = null;
                if (!ValidateTemplatePaths())
                {
                    proxydefPathTextBox.Text = string.Empty;
                    rootDirTextBox.Text = string.Empty;
                    MessageBox.Show("Template contains an invalid image path");
                }
                else
                {
                    generateProxyButton.Enabled = true;
                }
            }
        }

        private Dictionary<string, string> GetValues()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            DataTable dt = (DataTable)bindingSource.DataSource;
            foreach (DataRow row in dt.Rows)
            {
                string key = (string)row.ItemArray[0];
                string value = string.Empty;
                if (!row.IsNull(1))
                {
                    value = (string)row.ItemArray[1];
                }
                ret.Add(key, value);
            }

            return (ret);
        }

        private void generateProxyButton_Click(object sender, EventArgs e)
        {
            string tempImagePath = Path.Combine(GetExecutingDir(), "temp.png");
            if (File.Exists(tempImagePath))
            {
                File.Delete(tempImagePath);
            }
            ProxyDefinition def = GetProxyDef(proxydefPathTextBox.Text, rootDirTextBox.Text);
            DateTime startTime = DateTime.Now;
            def.SaveProxyImage(GetValues(), tempImagePath);
            DateTime endTime = DateTime.Now;
            proxyPictureBox.ImageLocation = tempImagePath;
            proxyPictureBox.Refresh();
            TimeGeneratedTextBox.Text = string.Format("Generated in {0}ms", (endTime - startTime).Milliseconds);
        }

 
    }
}
