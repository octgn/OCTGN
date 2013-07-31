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


namespace Octgn.Tools.SetBuilder
{
    public partial class Form1 : Form
    {
        private BindingSource bindingSource = new BindingSource();
        private BindingSource setsbindingSource = new BindingSource();
        private BindingSource cardsbindingSource = new BindingSource();
        private DataTable setList;
        private DataTable cardList;
        private DataTable dt;
        private Boolean setChanged = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void ResetDataTable()
        {
            dt = SetupDataTable();
            string[] values = new string[] { "Name", "test name" };
            dt.Rows.Add(values);
            bindingSource.DataSource = dt;
        }

        private void ResetSetList()
        {
            setList = SetupSetList();
            setsbindingSource.DataSource = setList;
            dgSetList.Columns[0].Width = 175;
            dgSetList.Columns[1].Width = 200;
            foreach (DataGridViewColumn column in dgSetList.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private DataTable SetupDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("Value");
            return (dt);
        }

        private DataTable SetupSetList()
        {
            DataTable setList = new DataTable();
            setList.Columns.Add("Name");
            setList.Columns.Add("GUID");
            return (setList);
        }

        private DataTable SetupCardList()
        {
            DataTable cardList = new DataTable();
            foreach (DataRow row in dt.Rows)
            {
                string tempstring = (string)row[0];
                cardList.Columns.Add(tempstring);
            }
            cardList.PrimaryKey = new DataColumn[] {cardList.Columns["GUID"]}; 
            return (cardList);
        }

        private void ResetCardList()
        {
            cardList = SetupCardList();
            cardsbindingSource.DataSource = cardList;
            foreach (DataGridViewColumn column in dgSetCards.Columns)
            {
               //column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            propertiesDataGrid.DataSource = bindingSource;
            dgSetList.DataSource = setsbindingSource;
            dgSetCards.DataSource = cardsbindingSource;
            ResetDataTable();
            ResetSetList();
        }

        private void GetSetList()
        {
            setList.Clear();
            DirectoryInfo tempSetPath = new DirectoryInfo(Path.Combine(rootDirTextBox.Text, "Sets"));
            DirectoryInfo[] dir = tempSetPath.GetDirectories();
            foreach (DirectoryInfo di in dir)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(Path.Combine(di.FullName, "set.xml"));
                    XmlNodeList setelements = doc.GetElementsByTagName("set");
                    string[] values = new string[] { setelements.Item(0).Attributes["name"].Value, setelements.Item(0).Attributes["id"].Value };
                    setList.Rows.Add(values);
                }
                catch
                {
                }
            }
        }

        private string GetExecutingDir()
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string dir = Path.GetDirectoryName(assemblyLocation);
            return (dir);
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

        private void generateProxy()
        {
            string tempImagePath = Path.Combine(GetExecutingDir(), "temp.png");
            if (File.Exists(tempImagePath))
            {
             //   File.Delete(tempImagePath);
            }
            ProxyDefinition def = GetProxyDef(proxydefPathTextBox.Text, rootDirTextBox.Text);
            def.SaveProxyImage(GetValues(), tempImagePath);
            proxyPictureBox.ImageLocation = tempImagePath;
            proxyPictureBox.Refresh();
        }

        private void loadDefinitionToolStripMenuItem_Click(object sender, EventArgs e)
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
                XmlNodeList gameelements = doc.GetElementsByTagName("game");
                txt_GameName.Text = gameelements.Item(0).Attributes["name"].Value;
                txt_GameGUID.Text = gameelements.Item(0).Attributes["id"].Value;
                txt_GameGUID.Enabled = false;
                txt_GameMinVersion.Text = gameelements.Item(0).Attributes["octgnVersion"].Value;
                txt_GameVersion.Text = gameelements.Item(0).Attributes["version"].Value;
                txt_GameTags.Text = gameelements.Item(0).Attributes["tags"].Value;
                txt_GameDescription.Text = gameelements.Item(0).Attributes["description"].Value;
                GetSetList();
                dt = SetupDataTable();
                newSetDataToolStripMenuItem.Enabled = true;
                saveSetDataToolStripMenuItem.Enabled = true;
                XmlNodeList carddefs = doc.GetElementsByTagName("card");
                string[] firstvalue = new string[] { "GUID", "" };
                dt.Rows.Add(firstvalue);
                firstvalue = new string[] { "Name", "" };
                dt.Rows.Add(firstvalue);
                foreach (XmlNode node in carddefs)
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        string[] values = new string[2];
                        values[0] = child.Attributes["name"].Value;
                        values[1] = "";
                        dt.Rows.Add(values);
                    }
                }
                propertiesDataGrid.Columns[0].Width = 175;
                propertiesDataGrid.Columns[1].Width = 200;
                foreach (DataGridViewColumn column in propertiesDataGrid.Columns)
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                propertiesDataGrid.Columns[0].ReadOnly = true;
                bindingSource.DataSource = dt;
                ResetCardList();
                doc.RemoveAll();
                doc = null;
                if (!ValidateTemplatePaths())
                {
                    proxydefPathTextBox.Text = string.Empty;
                    rootDirTextBox.Text = string.Empty;
                    MessageBox.Show("Template contains an invalid image path");
                }
            }

        }

        private void dgSetList_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (setChanged)
            {
                DialogResult saveChanges = MessageBox.Show("Do you want to save the changes to set: " + txt_SetName.Text, "Save changes", MessageBoxButtons.YesNoCancel);
                if (saveChanges == DialogResult.Cancel) 
                {
                    return;
                }
                else if (saveChanges == DialogResult.Yes)
                {
                    saveSetChanges();
                }
            }
            ResetCardList();
            string folder = setList.Rows[e.RowIndex][1].ToString();
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(rootDirTextBox.Text, "Sets", folder, "set.xml"));
            XmlNodeList setelements = doc.GetElementsByTagName("set");
            txt_SetName.Text = setelements.Item(0).Attributes["name"].Value;
            txt_SetGUID.Text = setelements.Item(0).Attributes["id"].Value;
            txt_SetGUID.Enabled = false;
            txt_MinGameVersion.Text = setelements.Item(0).Attributes["gameVersion"].Value;
            txt_SetVersion.Text = setelements.Item(0).Attributes["version"].Value;
            XmlNodeList cards = doc.GetElementsByTagName("card");

            foreach (XmlNode card in cards)
            {
                DataRow tempdata = cardList.NewRow();
                tempdata["name"] = card.Attributes["name"].Value;
                tempdata["guid"] = card.Attributes["id"].Value;
                
                foreach (XmlNode cardproperty in card.ChildNodes)
                {
                    tempdata[cardproperty.Attributes["name"].Value] = cardproperty.Attributes["value"].Value;
                }
                cardList.Rows.Add(tempdata);
            }
            setChanged = false;
        }

        private void dgSetCards_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            foreach (DataRow line in dt.Rows)
            {
                line["Value"] = dgSetCards.Rows[e.RowIndex].Cells[line.Field<String>(0)].Value;
                //MessageBox.Show(line.Field<string>(0));
            }
            generateProxy();
        }

        private void saveSetChanges()
        {
            DataTable dt = (DataTable)cardsbindingSource.DataSource;
            string savePath = Path.Combine(rootDirTextBox.Text, "Sets", txt_SetGUID.Text);
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0","utf-8","yes");
            doc.AppendChild(dec);
            XmlNode setNode = doc.CreateNode(XmlNodeType.Element,"set",null);
            XmlNode setName = doc.CreateNode(XmlNodeType.Attribute,"name",null);
            setName.Value = txt_SetName.Text;
            XmlNode setGuid = doc.CreateNode(XmlNodeType.Attribute,"id",null);
            setGuid.Value = txt_SetGUID.Text;
            XmlNode gameGuid = doc.CreateNode(XmlNodeType.Attribute, "gameId", null);
            gameGuid.Value = txt_GameGUID.Text;
            XmlNode setGameVer = doc.CreateNode(XmlNodeType.Attribute, "gameVersion", null);
            setGameVer.Value = txt_MinGameVersion.Text;
            XmlNode setVersion = doc.CreateNode(XmlNodeType.Attribute,"version",null);
            setVersion.Value = txt_SetVersion.Text;
            setNode.Attributes.SetNamedItem(setName);
            setNode.Attributes.SetNamedItem(setGuid);
            setNode.Attributes.SetNamedItem(gameGuid);
            setNode.Attributes.SetNamedItem(setGameVer);
            setNode.Attributes.SetNamedItem(setVersion);
            XmlNode cards = doc.CreateNode(XmlNodeType.Element, "cards", null);
            foreach (DataRow row in dt.Rows)
            {
                object[] values = row.ItemArray;
                XmlNode card = doc.CreateNode(XmlNodeType.Element, "card", null);
                XmlNode cardID = doc.CreateNode(XmlNodeType.Attribute,"id",null);
                cardID.Value = row.Field<string>("guid");
                XmlNode cardName = doc.CreateNode(XmlNodeType.Attribute,"name",null);
                cardName.Value = row.Field<string>("name");
                card.Attributes.SetNamedItem(cardID);
                card.Attributes.SetNamedItem(cardName);
                foreach (DataColumn field in dt.Columns)
                {
                    if (field.ColumnName == "GUID" || field.ColumnName == "Name") 
                    {
                    }
                    else
                    {
                        XmlNode prop = doc.CreateNode(XmlNodeType.Element,"property", null);
                        XmlNode propName = doc.CreateNode(XmlNodeType.Attribute, "name", null);
                        XmlNode propValue = doc.CreateNode(XmlNodeType.Attribute, "value", null);
                        propName.Value = field.ColumnName;
                        propValue.Value = (row[field] == DBNull.Value) ? string.Empty : (string)row[field];
                        prop.Attributes.SetNamedItem(propName);
                        prop.Attributes.SetNamedItem(propValue);
                        card.AppendChild(prop);
                    }
                }
                cards.AppendChild(card);
            }
            setNode.AppendChild(cards);
            doc.AppendChild(setNode);
            string file = Path.Combine(savePath, "Set.xml");
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            doc.Save(file);
            doc.RemoveAll();
            doc = null;
            setChanged = false;
        }

        private void saveSetDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveSetChanges();
        }

        private void newSetDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (setChanged)
            {
                DialogResult saveChanges = MessageBox.Show("Do you want to save the changes to set: " + txt_SetName.Text, "Save changes", MessageBoxButtons.YesNoCancel);
                if (saveChanges == DialogResult.Cancel)
                {
                    return;
                }
                else if (saveChanges == DialogResult.Yes)
                {
                    saveSetChanges();
                }
            }
            ResetCardList();
            Guid tempGuid = Guid.NewGuid();
            txt_SetGUID.Text = tempGuid.ToString();
            txt_SetGUID.Enabled = false;
        }

        private void btn_AddCard_Click(object sender, EventArgs e)
        {
            DataRow tempData = cardList.NewRow();
            Guid tempGuid = Guid.NewGuid();
            tempData["guid"] = tempGuid;
            tempData["name"] = "Card Name";
            cardList.Rows.Add(tempData);
            foreach (DataRow line in dt.Rows)
            {
                line["Value"] = tempData[line[0].ToString()];
            }
            generateProxy();

        }

        private void propertiesDataGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            string tempGuid = "";
            foreach (DataRow line in dt.Rows)
            {
                if (line["Name"].ToString() == "GUID") tempGuid = line["Value"].ToString();
            }
            DataRow tempData = cardList.Rows.Find(tempGuid);
            foreach (DataRow line in dt.Rows)
            {
                tempData[line["Name"].ToString()] = line["Value"].ToString();
            }
            tempData.AcceptChanges();
            generateProxy();
        }

        private void propertiesDataGrid_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (propertiesDataGrid.CurrentRow.Cells[e.ColumnIndex].ReadOnly)
            {
                SendKeys.Send("{tab}");
            }
        }

        private void btn_Duplicate_Click(object sender, EventArgs e)
        {
            string tempGuid = Guid.NewGuid().ToString();
            DataRow tempData = cardList.NewRow();
            foreach (DataRow line in dt.Rows)
            {
                tempData[line["Name"].ToString()] = line["Value"].ToString();
            }
            tempData["GUID"] = tempGuid;
            cardList.Rows.Add(tempData);
            foreach (DataRow line in dt.Rows)
            {
                line["Value"] = tempData[line[0].ToString()];
            }
            generateProxy();
        }


    }
}
