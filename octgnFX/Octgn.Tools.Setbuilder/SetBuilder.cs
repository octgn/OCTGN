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
        public List<string> propertyList = new List<string> { };
        ContextMenu cmGame = new ContextMenu();
        ContextMenu cmSet = new ContextMenu();
        ContextMenu cmCard = new ContextMenu();
        ContextMenu cmProperty = new ContextMenu();
        ContextMenu cmAlternate = new ContextMenu();
        ContextMenu cmPack = new ContextMenu();
        ContextMenu cmOptions = new ContextMenu();
        ContextMenu cmOption = new ContextMenu();
        ContextMenu cmPick = new ContextMenu();


        string rootDir;
        string proxydefPath;
        string gameVersion;
        public setConfig configfile = new setConfig();
        public gameSettings config;



        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setupContextMenus();
        }

        private void setupContextMenus()
        {
            cmGame.MenuItems.Add("Add Set", new EventHandler(AddItem_Click));

            cmSet.MenuItems.Add("Add Card", new EventHandler(AddItem_Click));
            cmSet.MenuItems.Add("Add Pack", new EventHandler(AddItem_Click));
            cmSet.MenuItems.Add("Save Set", new EventHandler(SaveSet_Click));
            cmSet.MenuItems.Add("Remove Set", new EventHandler(DeleteItem_Click));
            
            cmCard.MenuItems.Add("Add Property", new EventHandler(AddItem_Click));
            cmCard.MenuItems.Add("Add Alternate", new EventHandler(AddItem_Click));
            cmCard.MenuItems.Add("Duplicate Card", new EventHandler(DuplicateCard_Click));
            cmCard.MenuItems.Add("Remove Card", new EventHandler(DeleteItem_Click));
            
            cmAlternate.MenuItems.Add("Add Property", new EventHandler(AddItem_Click));
            cmAlternate.MenuItems.Add("Remove Alternate", new EventHandler(DeleteItem_Click));
            
            cmProperty.MenuItems.Add("Remove Property", new EventHandler(DeleteItem_Click));

            cmPack.MenuItems.Add("Add Options", new EventHandler(AddItem_Click));
            cmPack.MenuItems.Add("Add Pick", new EventHandler(AddItem_Click));
            cmPack.MenuItems.Add("Remove Pack", new EventHandler(DeleteItem_Click));

            cmOptions.MenuItems.Add("Add Option", new EventHandler(AddItem_Click));
            cmOptions.MenuItems.Add("Remove Options", new EventHandler(DeleteItem_Click));

            cmOption.MenuItems.Add("Add Pick", new EventHandler(AddItem_Click));
            cmOption.MenuItems.Add("Add Options", new EventHandler(AddItem_Click));
            cmOption.MenuItems.Add("Remove Option", new EventHandler(DeleteItem_Click));

            cmPick.MenuItems.Add("Remove Pick", new EventHandler(DeleteItem_Click));
        }
        private void AddItem_Click(object sender, EventArgs e)
        {
            MenuItem orig = (MenuItem)sender;
            TreeNode newNode = new TreeNode();
            if (orig.Text == "Add Set")
            {
                setObject newSet = new setObject(Guid.Parse(tree_GameDef.Nodes[0].Tag.ToString()));
                newSet.version = gameVersion;
                newSet.gameVersion = gameVersion;
                newSet.changed = true;
                newNode.Text = newSet.name + " (" + newSet.id + ")";
                newNode.Tag = newSet;
                newNode.ContextMenu = cmSet;

            }
            else if (orig.Text == "Add Card")
            {
                cardObject newCard = new cardObject("New Card", Guid.NewGuid(),config.propertyList);
                newCard.changed = true;
                newNode.Text = newCard.name + " (" + newCard.id + ")";
                newNode.Tag = newCard;
                foreach (cardProperty prop in newCard.cardProperties)
                {
                    TreeNode subNode = new TreeNode();
                    subNode.Text = prop.name + ": " + prop.value;
                    subNode.Tag = prop;
                    subNode.ContextMenu = cmProperty;
                    newNode.Nodes.Add(subNode);
                }
                newNode.ContextMenu = cmCard;
                int cardsNodeIndex = -1;
                foreach (TreeNode node in tree_GameDef.SelectedNode.Nodes)
                {
                    if (node.Text == "cards") cardsNodeIndex = node.Index;
                }
                if (cardsNodeIndex == -1)
                {
                    TreeNode cardsNode = new TreeNode("cards");
                    tree_GameDef.SelectedNode.Nodes.Add(cardsNode);
                    tree_GameDef.SelectedNode = cardsNode;
                }
                else tree_GameDef.SelectedNode = tree_GameDef.SelectedNode.Nodes[cardsNodeIndex];
            }
            else if (orig.Text == "Add Property")
            {
                cardProperty newProperty = new cardProperty("Select a Property Type", "");
                newProperty.changed = true;
                if (tree_GameDef.SelectedNode.Tag is cardObject)
                {
                    cardObject currentCard = (cardObject)tree_GameDef.SelectedNode.Tag;
                    currentCard.cardProperties.Add(newProperty);
                }
                else if (tree_GameDef.SelectedNode.Tag is cardAlternate)
                {
                    cardAlternate currentCard = (cardAlternate)tree_GameDef.SelectedNode.Tag;
                    currentCard.alternateProperties.Add(newProperty);
                }
                newNode.Text = newProperty.name + ": " + newProperty.value;
                newNode.Tag = newProperty;
                newNode.ContextMenu = cmProperty;
            }
            else if (orig.Text == "Add Alternate")
            {
                cardObject currentCard = (cardObject)tree_GameDef.SelectedNode.Tag;
                cardAlternate newAlternate = new cardAlternate("New Alternate", "", config.propertyList);
                newAlternate.changed = true;
                currentCard.cardAlternates.Add(newAlternate);
                newNode.Text = newAlternate.name + " (" + newAlternate.type + ")";
                newNode.Tag = newAlternate;
                foreach (cardProperty prop in newAlternate.alternateProperties)
                {
                    TreeNode subNode = new TreeNode();
                    subNode.Text = prop.name + ": " + prop.value;
                    subNode.Tag = prop;
                    subNode.ContextMenu = cmProperty;
                    newNode.Nodes.Add(subNode);
                }

                newNode.ContextMenu = cmAlternate;
            }
            else if (orig.Text == "Add Pack")
            {
                packObject pack = new packObject("New Pack");
                pack.changed = true;
                newNode.Text = pack.name + " (" + pack.id + ")";
                newNode.Tag = pack;
                newNode.ContextMenu = cmPack;
                int packagingNodeIndex = -1;
                if (tree_GameDef.SelectedNode.Text == "packaging") packagingNodeIndex = tree_GameDef.SelectedNode.Index;
                else
                {
                    foreach (TreeNode node in tree_GameDef.SelectedNode.Nodes)
                    {
                        if (node.Text == "packaging") packagingNodeIndex = node.Index;
                    }
                }
                if (packagingNodeIndex == -1)
                {
                    TreeNode packagingNode = new TreeNode("packaging");
                    tree_GameDef.SelectedNode.Nodes.Add(packagingNode);
                    tree_GameDef.SelectedNode = packagingNode;
                }
                else tree_GameDef.SelectedNode = tree_GameDef.SelectedNode.Nodes[packagingNodeIndex];

            }
            else if (orig.Text == "Add Options")
            {
                optionsObject options = new optionsObject();
                options.changed = true;
                newNode.Text = "options";
                newNode.Tag = options;
                newNode.ContextMenu = cmOptions;
            }
            else if (orig.Text == "Add Option")
            {
                optionObject option = new optionObject("0");
                option.changed = true;
                newNode.Text = "Probability: " + option.probability;
                newNode.Tag = option;
                newNode.ContextMenu = cmOption;
            }
            else if (orig.Text == "Add Pick")
            {
                pickObject pick = new pickObject("0","key", "value");
                pick.changed = true;
                newNode.Text = "Qty: " + pick.qty + " | Key: " + pick.key + " | Value: " + pick.value;
                newNode.Tag = pick;
                newNode.ContextMenu = cmPick;
            }
            else return;
            tree_GameDef.SelectedNode.Nodes.Add(newNode);
            tree_GameDef.SelectedNode = newNode;
            updateNode(tree_GameDef.SelectedNode);
        }

        private void DuplicateItem_Click(object sender, EventArgs e)
        {
        }

        private void DeleteItem_Click(object sender, EventArgs e)
        {
            MenuItem orig = (MenuItem)sender;
            TreeNode currentNode = tree_GameDef.SelectedNode;
            gameItem currentItem = (gameItem)tree_GameDef.SelectedNode.Tag;
            DialogResult res = MessageBox.Show("Are you sure you want to remove the " + currentItem.objectType + ": " + currentItem.name + "?", "Remove " + currentItem.objectType + "?", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes)
            {
                if (orig.Text == "Remove Set")
                {
                    setObject set = (setObject)currentItem;
                    string setID = set.id.ToString();
                    string savePath = Path.Combine(rootDir, "Sets", setID);
                    if (Directory.Exists(savePath)) Directory.Delete(savePath, true);
                }
                else if (orig.Text == "Remove Card")
                {
                    cardObject card = (cardObject)currentItem;
                }
                else if (orig.Text == "Remove Alternate")
                {
                    cardAlternate alt = (cardAlternate)currentItem;
                    cardObject card = (cardObject)tree_GameDef.SelectedNode.Parent.Tag;
                    card.cardAlternates.Remove(alt);
                }
                else if (orig.Text == "Remove Property")
                {
                    cardProperty prop = (cardProperty)currentItem;
                    gameItem propParent = (gameItem)tree_GameDef.SelectedNode.Parent.Tag;
                    if (propParent is cardObject)
                    {
                        cardObject card = (cardObject)propParent;
                        card.cardProperties.Remove(prop);
                    }
                    else if (propParent is cardAlternate)
                    {
                        cardAlternate card = (cardAlternate)propParent;
                        card.alternateProperties.Remove(prop);
                    }
                }
                else if (orig.Text == "Remove Pack")
                {
                    packObject pack = (packObject)currentItem;
                    packagingObject packParent = (packagingObject)currentNode.Parent.Tag;
                    packParent.packList.Remove(pack);
                }
                else if (orig.Text == "Remove Options")
                {
                    optionsObject options = (optionsObject)currentItem;
                    gameItem optionsParent = (gameItem)currentNode.Parent.Tag;
                    if (optionsParent is packObject)
                    {
                        packObject parent = (packObject)optionsParent;
                        parent.optionsList.Remove(options);
                    }
                    else if (optionsParent is optionObject)
                    {
                        optionObject parent = (optionObject)optionsParent;
                        parent.optionsList.Remove(options);
                    }
                }
                else if (orig.Text == "Remove Option")
                {
                    optionObject option = (optionObject)currentItem;
                    optionsObject parent = (optionsObject)currentNode.Parent.Tag;
                    parent.optionList.Remove(option);
                }
                else if (orig.Text == "Remove Pick")
                {
                    pickObject pick = (pickObject)currentItem;
                    gameItem pickParent = (gameItem)currentNode.Parent.Tag;
                    if (pickParent is packObject)
                    {
                        packObject parent = (packObject)pickParent;
                        parent.pickList.Remove(pick);
                    }
                    else if (pickParent is optionObject)
                    {
                        optionObject parent = (optionObject)pickParent;
                        parent.pickList.Remove(pick);
                    }
                }
                else return;
                currentNode.Remove();
            }
        }

        private void DuplicateCard_Click(object sender, EventArgs e)
        {
            cardObject currentCard = (cardObject)tree_GameDef.SelectedNode.Tag;
            cardObject newCard = new cardObject("Copy of " + currentCard.name);
            newCard.changed = true;
            TreeNode newNode = new TreeNode(newCard.name + " (" + newCard.id + ")");
            newNode.Tag = newCard;
            newNode.ContextMenu = cmCard;
            tree_GameDef.SelectedNode.Parent.Nodes.Add(newNode);
            tree_GameDef.SelectedNode = newNode;
            foreach (cardProperty prop in currentCard.cardProperties)
            {
                cardProperty newProp = newCard.AddProperty(prop.name, prop.value);
                newProp.changed = true;
                TreeNode subNode = new TreeNode(prop.name + ": " + prop.value);
                subNode.Tag = newProp;
                subNode.ContextMenu = cmProperty;
                newNode.Nodes.Add(subNode);
                updateNode(subNode);
            }
            foreach (cardAlternate alt in currentCard.cardAlternates)
            {
                cardAlternate newAlt = new cardAlternate(alt.name, alt.type);
                newAlt.changed = true;
                TreeNode subNode = new TreeNode(newAlt.name + " (" + newAlt.type + ")");
                subNode.Tag = newAlt;
                foreach (cardProperty prop in alt.alternateProperties)
                {
                    cardProperty newProp = newAlt.AddProperty(prop.name, prop.value);
                    newProp.changed = true;
                    TreeNode subSubNode = new TreeNode(prop.name + ": " + prop.value);
                    subSubNode.Tag = newProp;
                    subSubNode.ContextMenu = cmProperty;
                    subNode.Nodes.Add(subSubNode);
                    updateNode(subSubNode);
                }
                newCard.AddAlternate(newAlt);
                newNode.Nodes.Add(subNode);
                updateNode(subNode);
            }
            updateNode(newNode);
            generateProxy();
        }

        private void SaveSet_Click(object sender, EventArgs e)
        {
            TreeNode currentNode = tree_GameDef.SelectedNode;
            if (currentNode.Tag is setObject)
            {
                saveSetChanges(currentNode, true);
            }
        }
        
        private void GetSetList(TreeNode tree, Guid gameGuid)
        {
            DirectoryInfo tempSetPath = new DirectoryInfo(Path.Combine(rootDir, "Sets"));
            DirectoryInfo[] dir = tempSetPath.GetDirectories();
            foreach (DirectoryInfo di in dir)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(Path.Combine(di.FullName, "set.xml"));
                    tree.Nodes.Add(loadSet(doc, gameGuid));
               }
                catch
                {
                }
            }
        }

        public TreeNode loadSet(XmlDocument doc, Guid gameGuid)
        {
            XmlNodeList setelements = doc.GetElementsByTagName("set");
            setObject set = readSet(doc.GetElementsByTagName("set"), gameGuid);
            TreeNode mNode = new TreeNode();
            mNode.Text = set.name + " (" + set.id.ToString() + ")";
            mNode.Tag = set;
            mNode.ContextMenu = cmSet;
            if (set.setPackaging.packList.Count > 0)
            {
                TreeNode packagingNode = new TreeNode("packaging");
                foreach (packObject pack in set.setPackaging.packList)
                {
                    packagingNode.Nodes.Add(appendPackItem(pack));
                }
                mNode.Nodes.Add(packagingNode);
            }
            if (set.setMarkers.Count > 0)
            {
                TreeNode markerNode = new TreeNode("markers");

                foreach (markerObject marker in set.setMarkers)
                {
                    TreeNode newMarkerNode = new TreeNode();
                    newMarkerNode.Text = marker.name + " (" + marker.id.ToString() + ")";
                    newMarkerNode.Tag = marker;
                    markerNode.Nodes.Add(newMarkerNode);
                }
                mNode.Nodes.Add(markerNode);
            }
            if (set.setCards.Count > 0)
            {
                TreeNode cardsNode = new TreeNode();
                cardsNode.Text = "cards";
                foreach (cardObject card in set.setCards)
                {
                    TreeNode cNode = new TreeNode();
                    cNode.Text = card.name + " (" + card.id.ToString() + ")";
                    cNode.Tag = card;
                    cNode.ContextMenu = cmCard;
                    foreach (cardProperty property in card.cardProperties)
                    {
                        TreeNode pNode = new TreeNode();
                        pNode.Text = property.name + ": " + property.value.ToString();
                        pNode.Tag = property;
                        pNode.ContextMenu = cmProperty;
                        cNode.Nodes.Add(pNode);
                    }
                    foreach (cardAlternate alternate in card.cardAlternates)
                    {
                        TreeNode aNode = new TreeNode();
                        aNode.Text = alternate.name + " (" + alternate.type + ")";
                        aNode.Tag = alternate;
                        aNode.ContextMenu = cmAlternate;
                        foreach (cardProperty property in alternate.alternateProperties)
                        {
                            TreeNode apNode = new TreeNode();
                            apNode.Text = property.name + ": " + property.value.ToString();
                            apNode.Tag = property;
                            apNode.ContextMenu = cmProperty;
                            aNode.Nodes.Add(apNode);
                        }
                        cNode.Nodes.Add(aNode);
                    }
                    cardsNode.Nodes.Add(cNode);
                }
                mNode.Nodes.Add(cardsNode);
            }
            return mNode;
        }

        public setObject readSet(XmlNodeList setelements,Guid gameGuid)
        {
            setObject set = new setObject(gameGuid, setelements.Item(0).Attributes["name"].Value, Guid.Parse(setelements.Item(0).Attributes["id"].Value));
            set.SetGameVersion(setelements.Item(0).Attributes["gameVersion"].Value);
            set.SetVersion(setelements.Item(0).Attributes["version"].Value);
            foreach (XmlNode childnode in setelements.Item(0).ChildNodes)
            {
                if (childnode.Name == "cards") readCards(set, childnode.ChildNodes);
                else if (childnode.Name == "packaging") readPackaging(set, childnode.ChildNodes);
                else if (childnode.Name == "markers")
                {
                    foreach (XmlNode child in childnode.ChildNodes)
                    {
                        if (child.Name == "marker")
                        {
                            set.AddMarker(new markerObject(child.Attributes["name"].Value, Guid.Parse(child.Attributes["id"].Value)));
                        }
                    }
                }
            }
            return set;
        }

        private void readCards(setObject set, XmlNodeList nodes)
        {
            foreach (XmlNode childnode in nodes)
            {
                if (childnode.Name == "card")
                {
                    cardObject card = new cardObject(childnode.Attributes["name"].Value, Guid.Parse(childnode.Attributes["id"].Value));
                    foreach (XmlNode subnode in childnode.ChildNodes)
                    {
                        if (subnode.Name == "property")
                        {
                            card.AddProperty(subnode.Attributes["name"].Value, subnode.Attributes["value"].Value);
                        }
                        else if (subnode.Name == "alternate")
                        {
                            cardAlternate altcard = new cardAlternate(subnode.Attributes["name"].Value, subnode.Attributes["type"].Value);
                            foreach (XmlNode altprop in subnode.ChildNodes)
                            {
                                if (altprop.Name == "property")
                                {
                                    altcard.AddProperty(altprop.Attributes["name"].Value, altprop.Attributes["value"].Value);
                                }
                            }
                            card.AddAlternate(altcard);
                        }
                    }
                    set.AddCard(card);
                }
            }
        }

        private void readPackaging(setObject set, XmlNodeList nodes)
        {
            packagingObject packaging = set.setPackaging;
            foreach (XmlNode childnode in nodes)
            {
                if (childnode.Name == "pack")
                {
                    gameItem getPack = readPackItem(childnode);
                    packObject pack = (packObject)getPack;
                    packaging.packList.Add(pack);                   
                }
            }
        }

        private gameItem readPackItem(XmlNode node)
        {
            if (node.Name == "pack")
            {
                packObject pack = new packObject(node.Attributes["name"].Value);
                pack.id = Guid.Parse(node.Attributes["id"].Value);
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (childNode.Name == "pick")
                    {
                        gameItem getPick = readPackItem(childNode);
                        pickObject pick = (pickObject)getPick;
                        pack.pickList.Add(pick);
                    }
                    else if (childNode.Name == "options")
                    {
                        gameItem getOptions = readPackItem(childNode);
                        optionsObject options = (optionsObject)getOptions;
                        pack.optionsList.Add(options);
                    }
                }
                return pack;
            }
            else if (node.Name == "pick")
            {
                pickObject pick = new pickObject(node.Attributes["qty"].Value,node.Attributes["key"].Value, node.Attributes["value"].Value);
                return pick;
            }
            else if (node.Name == "options")
            {
                optionsObject options = new optionsObject();
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (childNode.Name == "option")
                    {
                        gameItem getOption = readPackItem(childNode);
                        optionObject option = (optionObject)getOption;
                        options.optionList.Add(option);
                    }
                }
                return options;
            }
            else if (node.Name == "option")
            {
                optionObject option = new optionObject(node.Attributes["probability"].Value);
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (childNode.Name == "pick")
                    {
                        gameItem getPick = readPackItem(childNode);
                        pickObject pick = (pickObject)getPick;
                        option.pickList.Add(pick);

                    }
                    else if (childNode.Name == "options")
                    {
                        gameItem getOptions = readPackItem(childNode);
                        optionsObject options = (optionsObject)getOptions;
                        option.optionsList.Add(options);
                    }
                }
                return option;
            }
            else
            {
                gameItem badObject = new gameItem();
                return badObject;
            }
        }

        private TreeNode appendPackItem(gameItem item)
        {
            TreeNode newNode = new TreeNode();
            if (item is packObject)
            {
                packObject pack = (packObject)item;
                newNode.Text = pack.name + " (" + pack.id.ToString() + ")";
                newNode.ContextMenu = cmPack;
                newNode.Tag = pack;
                foreach (optionsObject options in pack.optionsList) newNode.Nodes.Add(appendPackItem(options));
                foreach (pickObject pick in pack.pickList) newNode.Nodes.Add(appendPackItem(pick));
            }
            else if (item is optionsObject)
            {
                optionsObject options = (optionsObject)item;
                newNode.Text = "options";
                newNode.Tag = options;
                newNode.ContextMenu = cmOptions;
                foreach (optionObject option in options.optionList) newNode.Nodes.Add(appendPackItem(option));
            }
            else if (item is optionObject)
            {
                optionObject option = (optionObject)item;
                newNode.Text = "Probability: " + option.probability;
                newNode.Tag = option;
                newNode.ContextMenu = cmOption;
                foreach (optionsObject options in option.optionsList) newNode.Nodes.Add(appendPackItem(options));
                foreach (pickObject pick in option.pickList) newNode.Nodes.Add(appendPackItem(pick));
            }
            else if (item is pickObject)
            {
                pickObject pick = (pickObject)item;
                newNode.Text = "Qty: " + pick.qty + " | Key: " + pick.key + " | Value: " + pick.value;
                newNode.Tag = pick;
                newNode.ContextMenu = cmPick;
            }
            return newNode;
        }

        private string GetExecutingDir()
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string dir = Path.GetDirectoryName(assemblyLocation);
            return (dir);
        }

        private bool ValidateTemplatePaths()
        {
            Dictionary<string, string> blockSources = ProxyDefinition.GetBlockSources(proxydefPath);
            foreach (KeyValuePair<string, string> kvi in blockSources)
            {
                string path = Path.Combine(rootDir, kvi.Value);
                if (!File.Exists(path))
                {
                    return (false);
                }
            }
            List<string> templateSource = ProxyDefinition.GetTemplateSources(proxydefPath);
            foreach (string s in templateSource)
            {
                string path = Path.Combine(rootDir, s);
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

        private Dictionary<string, string> GetValues(cardObject card)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            ret.Add("Name", card.name);
            foreach (cardProperty row in card.cardProperties)
            {
                string key = (string)row.name;
                string value = (string)row.value;
                ret.Add(key, value);
            }

            return (ret);
        }

        private Dictionary<string, string> GetValues(cardAlternate alternate)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            ret.Add("Name", alternate.name);
            foreach (cardProperty row in alternate.alternateProperties)
            {
                string key = (string)row.name;
                string value = (string)row.value;
                ret.Add(key, value);
            }

            return (ret);
        }

        private void generateProxy()
        {
            if (tree_GameDef.SelectedNode.Tag is cardObject) generateProxy((cardObject)tree_GameDef.SelectedNode.Tag);
            else if (tree_GameDef.SelectedNode.Tag is cardAlternate) generateProxy((cardAlternate)tree_GameDef.SelectedNode.Tag);
            else if (tree_GameDef.SelectedNode.Parent != null)
            {
                if (tree_GameDef.SelectedNode.Parent.Tag is cardObject) generateProxy((cardObject)tree_GameDef.SelectedNode.Parent.Tag);
                else if (tree_GameDef.SelectedNode.Parent.Tag is cardAlternate) generateProxy((cardAlternate)tree_GameDef.SelectedNode.Parent.Tag);
            }
        }

        private void generateProxy(cardObject card)
        {
            string tempImagePath = Path.Combine(GetExecutingDir(), "temp.png");
            if (File.Exists(tempImagePath))
            {
             //   File.Delete(tempImagePath);
            }
            ProxyDefinition def = GetProxyDef(proxydefPath, rootDir);
            def.SaveProxyImage(GetValues(card), tempImagePath);
            proxyPictureBox.ImageLocation = tempImagePath;
            proxyPictureBox.Refresh();
        }

        private void generateProxy(cardAlternate alternate)
        {
            string tempImagePath = Path.Combine(GetExecutingDir(), "temp.png");
            if (File.Exists(tempImagePath))
            {
                //   File.Delete(tempImagePath);
            }
            ProxyDefinition def = GetProxyDef(proxydefPath, rootDir);
            def.SaveProxyImage(GetValues(alternate), tempImagePath);
            proxyPictureBox.ImageLocation = tempImagePath;
            proxyPictureBox.Refresh();
        }

        private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
      {
         XmlNode xNode;
         TreeNode tNode;
         XmlNodeList nodeList;
         int i;

         // Loop through the XML nodes until the leaf is reached.
         // Add the nodes to the TreeView during the looping process.
         if (inXmlNode.HasChildNodes)
         {
            nodeList = inXmlNode.ChildNodes;
            for(i = 0; i<=nodeList.Count - 1; i++)
            {
               xNode = inXmlNode.ChildNodes[i];
               XmlNode attrName = xNode.Attributes["name"];
               if (attrName != null)
                   inTreeNode.Nodes.Add(new TreeNode(xNode.Name + " - " + attrName.Value));
               else
                   inTreeNode.Nodes.Add(new TreeNode(xNode.Name));
               tNode = inTreeNode.Nodes[i];
               AddNode(xNode, tNode);
            }
         }
         else
         {
            // Here you need to pull the data from the XmlNode based on the
            // type of node, whether attribute values are required, and so forth.
            inTreeNode.Text = (inXmlNode.OuterXml).Trim();
         }
      }                  
   
        private void loadDefinitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (definitionOpenDialog.ShowDialog() != DialogResult.Cancel)
            {
                checkSetChanges();
                configfile.Load("settings.xml");
                tree_GameDef.Nodes.Clear();
                tree_GameDef.Nodes.Add("No Game Definition Loaded");
                string def = definitionOpenDialog.FileName;
                rootDir = Path.GetDirectoryName(def);
                XmlDocument doc = new XmlDocument();
                doc.Load(def);
                XmlNodeList list = doc.GetElementsByTagName("proxygen");
                string relPath = list.Item(0).Attributes["definitionsrc"].Value;
                proxydefPath = Path.Combine(rootDir, relPath);
                XmlNodeList gameelements = doc.GetElementsByTagName("game");
                tree_GameDef.Nodes[0].Tag = gameelements.Item(0).Attributes["id"].Value;
                tree_GameDef.Nodes[0].Text = gameelements.Item(0).Attributes["name"].Value;
                gameVersion = gameelements.Item(0).Attributes["version"].Value;
                tree_GameDef.Nodes[0].ContextMenu = cmGame;
                Guid tempGuid = Guid.Parse(gameelements.Item(0).Attributes["id"].Value);
                GetSetList(tree_GameDef.Nodes[0],tempGuid);
                propertyList.Clear();
                foreach (XmlNode node in gameelements.Item(0).ChildNodes)
                {
                    if (node.Name == "card")
                    {
                        foreach (XmlNode prop in node.ChildNodes)
                        {
                            if (prop.Name == "property") propertyList.Add(prop.Attributes["name"].Value);
                        }
                    }
                }
                if (configfile.HasSet(tempGuid))
                {
                    config = configfile.GetSet(tempGuid);
                    config.UpdateSet(propertyList);
                }
                else config = configfile.AddSet(tempGuid, propertyList);
                configfile.Save("settings.xml");
                configureGameMenuItem.Enabled = true;
                doc = null;
                if (!ValidateTemplatePaths())
                {
                    proxydefPath = string.Empty;
                    rootDir = string.Empty;
                    MessageBox.Show("Template contains an invalid image path");
                }
            }

        }

        private void checkSetChanges()
        {
            foreach (TreeNode child in tree_GameDef.Nodes[0].Nodes)
            {
                Boolean changed = false;
                if (child.Tag is gameItem)
                {
                    changed = scanNode(child, changed);
                }
                if (changed)
                {
                    gameItem set = (gameItem)child.Tag;
                    DialogResult res = MessageBox.Show(set.name + " has changes pending.  Save them?","Changes pending",MessageBoxButtons.YesNoCancel);
                    if (res == DialogResult.Yes) saveSetChanges(child);
                    else if (res == DialogResult.Cancel) break;
                }
            }
        }

        private Boolean scanNode(TreeNode node, Boolean changed)
        {
            if (node.Tag is gameItem)
            {
                gameItem tempItem = (gameItem)node.Tag;
                if (tempItem.changed) return true;
                else
                {
                    foreach (TreeNode child in node.Nodes)
                    {
                        changed = scanNode(child, changed);
                    }
                    return changed;
                }
            }
            else if ((node.Text == "packaging") || (node.Text == "markers") || (node.Text == "cards"))
            {
                foreach (TreeNode child in node.Nodes)
                {
                    changed = scanNode(child, changed);
                }
                return changed;
            }
            else return changed; 
        }

        private void saveSetChanges(TreeNode node)
        {
            saveSetChanges(node, false);
        }

        private void saveSetChanges(TreeNode node, Boolean reload)
        {
            if (node.Tag is setObject)
            {
                setObject set = (setObject)node.Tag;
                string setID = set.id.ToString();
                string savePath = Path.Combine(rootDir, "Sets", setID);
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                XmlDocument doc = new XmlDocument();
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", "yes");
                doc.AppendChild(dec);
                addXmlNode(doc,node);
                string file = Path.Combine(savePath, "set.xml");
                if (File.Exists(file))
                {
                    DialogResult res = MessageBox.Show("Are you sure you want to overwrite " + file + "?", "Overwrite File?", MessageBoxButtons.YesNo);
                    if (res == DialogResult.Yes)
                    {
                        File.Delete(file);
                    }
                }
                if (File.Exists(file)) return;
                doc.Save(file);
                doc.RemoveAll();
                doc = null;
                if (reload)
                {
                    doc = new XmlDocument();
                    doc.Load(file);
                    TreeNode parent = node.Parent;
                    int index = parent.Nodes.IndexOf(node);
                    parent.Nodes.RemoveAt(index);
                    parent.Nodes.Insert(index, loadSet(doc, Guid.Parse(tree_GameDef.Nodes[0].Tag.ToString())));
                }
            }
        }

        private void addXmlNode(XmlDocument doc, TreeNode node)
        {
            XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "set", null);
            addXmlNode(doc, node, newNode);
            doc.AppendChild(newNode);
        }

        private void addXmlNode(XmlDocument doc, TreeNode node, XmlNode parent)
        {
            if (node.Tag is setObject)
            {
                setObject set = (setObject)node.Tag;
                XmlNode setName = doc.CreateNode(XmlNodeType.Attribute, "name", null);
                setName.Value = set.name;
                parent.Attributes.SetNamedItem(setName);
                XmlNode setGuid = doc.CreateNode(XmlNodeType.Attribute, "id", null);
                setGuid.Value = set.id.ToString();
                parent.Attributes.SetNamedItem(setGuid);
                XmlNode setGameGuid = doc.CreateNode(XmlNodeType.Attribute, "gameId", null);
                setGameGuid.Value = tree_GameDef.Nodes[0].Tag.ToString();
                parent.Attributes.SetNamedItem(setGameGuid);
                XmlNode setGameVer = doc.CreateNode(XmlNodeType.Attribute, "gameVersion", null);
                setGameVer.Value = set.gameVersion;
                parent.Attributes.SetNamedItem(setGameVer);
                XmlNode setVersion = doc.CreateNode(XmlNodeType.Attribute, "version", null);
                setVersion.Value = set.version;
                parent.Attributes.SetNamedItem(setVersion);
                XmlNode packagingNode = doc.CreateNode(XmlNodeType.Element, "packaging", null);
                // Save Packaging Information
                XmlNode markersNode = doc.CreateNode(XmlNodeType.Element, "markers", null);
                // Save Markers Information
                XmlNode cardsNode = doc.CreateNode(XmlNodeType.Element, "cards", null);
                foreach (TreeNode child in node.Nodes)
                {
                    foreach (TreeNode subChild in child.Nodes)
                    {
                        if (subChild.Tag is cardObject) addXmlNode(doc, subChild, cardsNode);
                        else if (subChild.Tag is markerObject) addXmlNode(doc, subChild, markersNode);
                        else if (subChild.Tag is packObject) addXmlNode(doc, subChild, packagingNode);
                    }
                }
                parent.AppendChild(packagingNode);
                parent.AppendChild(markersNode);
                parent.AppendChild(cardsNode);
            }
            else if (node.Tag is cardObject)
            {
                cardObject card = (cardObject)node.Tag;
                XmlNode cardNode = doc.CreateNode(XmlNodeType.Element, "card", null);
                XmlNode cardName = doc.CreateNode(XmlNodeType.Attribute, "name", null);
                cardName.Value = card.name;
                cardNode.Attributes.SetNamedItem(cardName);
                XmlNode cardId = doc.CreateNode(XmlNodeType.Attribute, "id", null);
                cardId.Value = card.id.ToString();
                cardNode.Attributes.SetNamedItem(cardId);
                foreach (gameProperty prop in config.propertyList)
                {
                    Boolean found = false;
                    foreach (TreeNode child in node.Nodes)
                    {
                        gameItem obj = (gameItem)child.Tag;
                        if ((child.Tag is cardProperty) && (obj.name == prop.property))
                        {
                            addXmlNode(doc, child, cardNode);
                            found = true;
                        }
                    }
                    if ((!found) && ((config.saveAllProperties) || (prop.alwaysSave)))
                    {
                        addXmlEmpty(doc, prop.property, cardNode);
                    }
                }
                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Tag is cardAlternate) addXmlNode(doc, child, cardNode);
                }
                parent.AppendChild(cardNode);
            }
            else if (node.Tag is cardAlternate)
            {
                cardAlternate alt = (cardAlternate)node.Tag;
                XmlNode altNode = doc.CreateNode(XmlNodeType.Element, "alternate", null);
                XmlNode altName = doc.CreateNode(XmlNodeType.Attribute, "name", null);
                altName.Value = alt.name;
                altNode.Attributes.SetNamedItem(altName);
                XmlNode altType = doc.CreateNode(XmlNodeType.Attribute, "type", null);
                altType.Value = alt.type;
                altNode.Attributes.SetNamedItem(altType);
                foreach (gameProperty prop in config.propertyList)
                {
                    Boolean found = false;
                    foreach (TreeNode child in node.Nodes)
                    {
                        gameItem obj = (gameItem)child.Tag;
                        if ((child.Tag is cardProperty) && (obj.name == prop.property))
                        {
                            addXmlNode(doc, child, altNode);
                            found = true;
                        }
                    }
                    if ((!found) && ((config.saveAllProperties) || (prop.alwaysSave)))
                    {
                        addXmlEmpty(doc, prop.property, altNode);
                    }
                }
                parent.AppendChild(altNode);
            }
            else if (node.Tag is cardProperty)
            {
                cardProperty prop = (cardProperty)node.Tag;
                XmlNode propNode = doc.CreateNode(XmlNodeType.Element, "property", null);
                XmlNode propName = doc.CreateNode(XmlNodeType.Attribute, "name", null);
                propName.Value = prop.name;
                propNode.Attributes.SetNamedItem(propName);
                XmlNode propValue = doc.CreateNode(XmlNodeType.Attribute, "value", null);
                propValue.Value = prop.value;
                propNode.Attributes.SetNamedItem(propValue);
                if ((prop.value != "") || ((config.saveAllProperties) || (config.GetProperty(prop.name).alwaysSave))) parent.AppendChild(propNode);
            }
            else if (node.Tag is markerObject)
            {
                markerObject marker = (markerObject)node.Tag;
                XmlNode markerNode = doc.CreateNode(XmlNodeType.Element, "marker", null);
                XmlNode markerName = doc.CreateNode(XmlNodeType.Attribute, "name", null);
                markerName.Value = marker.name;
                markerNode.Attributes.SetNamedItem(markerName);
                XmlNode markerId = doc.CreateNode(XmlNodeType.Attribute, "id", null);
                markerId.Value = marker.id.ToString();
                markerNode.Attributes.SetNamedItem(markerId);
                parent.AppendChild(markerNode);
            }
            else if (node.Tag is packObject)
            {
                packObject pack = (packObject)node.Tag;
                XmlNode packNode = doc.CreateNode(XmlNodeType.Element, "pack", null);
                XmlNode packName = doc.CreateNode(XmlNodeType.Attribute, "name", null);
                packName.Value = pack.name;
                packNode.Attributes.SetNamedItem(packName);
                XmlNode packId = doc.CreateNode(XmlNodeType.Attribute, "id", null);
                packId.Value = pack.id.ToString();
                packNode.Attributes.SetNamedItem(packId);
                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Tag is optionsObject) addXmlNode(doc, child, packNode);                   
                }
                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Tag is pickObject) addXmlNode(doc, child, packNode);                   
                }
                parent.AppendChild(packNode);
            }
            else if (node.Tag is optionsObject)
            {
                optionsObject options = (optionsObject)node.Tag;
                XmlNode optionsNode = doc.CreateNode(XmlNodeType.Element, "options", null);
                foreach (TreeNode child in node.Nodes) addXmlNode(doc, child, optionsNode);
                parent.AppendChild(optionsNode);
            }
            else if (node.Tag is optionObject)
            {
                optionObject option = (optionObject)node.Tag;
                XmlNode optionNode = doc.CreateNode(XmlNodeType.Element, "option", null);
                XmlNode optionProbability = doc.CreateNode(XmlNodeType.Attribute, "probability", null);
                optionProbability.Value = option.probability;
                optionNode.Attributes.SetNamedItem(optionProbability);
                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Tag is optionsObject) addXmlNode(doc, child, optionNode);
                }
                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Tag is pickObject) addXmlNode(doc, child, optionNode);
                }
                parent.AppendChild(optionNode);
            }
            else if (node.Tag is pickObject)
            {
                pickObject pick = (pickObject)node.Tag;
                XmlNode pickNode = doc.CreateNode(XmlNodeType.Element, "pick", null);
                XmlNode pickQty = doc.CreateNode(XmlNodeType.Attribute, "qty", null);
                XmlNode pickKey = doc.CreateNode(XmlNodeType.Attribute, "key", null);
                XmlNode pickValue = doc.CreateNode(XmlNodeType.Attribute, "value", null);
                pickQty.Value = pick.qty;
                pickKey.Value = pick.key;
                pickValue.Value = pick.value;
                pickNode.Attributes.SetNamedItem(pickQty);
                pickNode.Attributes.SetNamedItem(pickKey);
                pickNode.Attributes.SetNamedItem(pickValue);
                parent.AppendChild(pickNode);
            }
        }
        private void addXmlEmpty(XmlDocument doc, string pName, XmlNode parent)
        {
            XmlNode propNode = doc.CreateNode(XmlNodeType.Element, "property", null);
            XmlNode propName = doc.CreateNode(XmlNodeType.Attribute, "name", null);
            propName.Value = pName;
            propNode.Attributes.SetNamedItem(propName);
            XmlNode propValue = doc.CreateNode(XmlNodeType.Attribute, "value", null);
            propValue.Value = "";
            propNode.Attributes.SetNamedItem(propValue);
            parent.AppendChild(propNode);
        }

        private void tree_GameDef_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tree_GameDef.SelectedNode.Tag is setObject) saveSetToolStripMenuItem.Enabled = true;
            else saveSetToolStripMenuItem.Enabled = false;
            generateProxy();
        }

        private void tree_GameDef_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (tree_GameDef.SelectedNode != null)
            {
                if (!(tree_GameDef.SelectedNode.Tag is string))
                {
                    openEditor();
                }
            }
        }

        public void updateNode()
        {
            updateNode(tree_GameDef.SelectedNode);
        }

        public void updateNode(TreeNode node)
        {
            gameItem item = (gameItem)node.Tag;
            if (item.changed)
            {
                node.NodeFont = new Font(tree_GameDef.Font, FontStyle.Bold);
                generateProxy();
            }
            if (node.Tag is cardProperty)
            {
                cardProperty tempObj = (cardProperty)node.Tag;
                node.Text = tempObj.name + ": " + tempObj.value;
            }
            else if (node.Tag is cardAlternate)
            {
                cardAlternate tempObj = (cardAlternate)node.Tag;
                node.Text = tempObj.name + " (" + tempObj.type + ")";
            }
            else if (node.Tag is cardObject)
            {
                cardObject tempObj = (cardObject)node.Tag;
                node.Text = tempObj.name + " (" + tempObj.id.ToString() + ")";
            }
            else if (node.Tag is setObject)
            {
                setObject tempObj = (setObject)node.Tag;
                node.Text = tempObj.name + " (" + tempObj.id.ToString() + ")";
            }
            else if (node.Tag is markerObject)
            {
                markerObject tempObj = (markerObject)node.Tag;
                node.Text = tempObj.name + " (" + tempObj.id.ToString() + ")";
            }
            else if (node.Tag is packObject)
            {
                packObject tempObj = (packObject)node.Tag;
                node.Text = tempObj.name + " (" + tempObj.id.ToString() + ")";
            }
            else if (node.Tag is optionObject)
            {
                optionObject tempObj = (optionObject)node.Tag;
                node.Text = "Probability: " + tempObj.probability;
            }
            else if (node.Tag is pickObject)
            {
                pickObject tempObj = (pickObject)node.Tag;
                node.Text = "Qty: " + tempObj.qty + " | Key:" + tempObj.key + " | Value: " + tempObj.value;
            }
        }

        public void openEditor()
        {
            frm_ItemEditor frm_Editor = new frm_ItemEditor(this);
            frm_Editor.item = tree_GameDef.SelectedNode.Tag;
            frm_Editor.Show();
            frm_Editor.Location = PointToScreen(new Point(tree_GameDef.SelectedNode.Bounds.X, tree_GameDef.SelectedNode.Bounds.Y));
            this.Enabled = false;
        }

        public void openConfig()
        {
            frm_SetConfig frm_Config = new frm_SetConfig(this);
            frm_Config.Show();
            this.Enabled = false;
        }

        public void openNode(string card, string position)
        {
            object currentItem = tree_GameDef.SelectedNode.Tag;
            if (card != "same")
            {
                if ((tree_GameDef.SelectedNode.Parent.Tag is cardObject) || (tree_GameDef.SelectedNode.Parent.Tag is cardAlternate) || (currentItem is setObject) || (tree_GameDef.SelectedNode.Parent.Text == "cards"))
                {
                    TreeNode currentNode;
                    TreeNode parentNode;
                    if ((tree_GameDef.SelectedNode.Parent.Tag is cardObject) || (tree_GameDef.SelectedNode.Parent.Tag is cardAlternate))
                    {
                        currentNode = tree_GameDef.SelectedNode.Parent;
                        parentNode = tree_GameDef.SelectedNode.Parent.Parent;
                    }
                    else
                    {   
                        currentNode = tree_GameDef.SelectedNode;
                        parentNode = tree_GameDef.SelectedNode.Parent;
                    }
                    if ((card == "next") && (currentNode == parentNode.LastNode)) tree_GameDef.SelectedNode = parentNode.FirstNode;
                    else if ((card == "prev") && (currentNode == parentNode.FirstNode)) tree_GameDef.SelectedNode = parentNode.LastNode;
                    else if (card == "prev") tree_GameDef.SelectedNode = currentNode.PrevNode;
                    else if (card == "next") tree_GameDef.SelectedNode = currentNode.NextNode;
                    if (tree_GameDef.SelectedNode != currentNode) currentNode.Collapse();
                }
            }
            TreeNode node = tree_GameDef.SelectedNode;
            Boolean found = true;
            if ((position == "same") && ((node.Tag is cardObject) || (node.Tag is cardAlternate)) && (!(currentItem is cardObject)))
            {
                found = false;
                foreach (TreeNode child in node.Nodes)
                {
                    if (currentItem.GetType() == child.Tag.GetType())
                    {
                        if (currentItem is cardProperty)
                        {
                            cardProperty item = (cardProperty)currentItem;
                            cardProperty childItem = (cardProperty)child.Tag;
                            if (item.name == childItem.name)
                            {
                                found = true;
                                tree_GameDef.SelectedNode = child;
                            }
                        }
                        if (currentItem is cardAlternate)
                        {
                            cardAlternate altitem = (cardAlternate)currentItem;
                            cardAlternate altchildItem = (cardAlternate)child.Tag;
                            if (altitem.type == altchildItem.type)
                            {
                                found = true;
                                tree_GameDef.SelectedNode = child;
                            }
                        }
                    }
                }
            }
            else if ((position == "next") && (node == node.Parent.LastNode)) tree_GameDef.SelectedNode = node.Parent.FirstNode;
            else if ((position == "prev") && (node == node.Parent.FirstNode)) tree_GameDef.SelectedNode = node.Parent.LastNode;
            else if (position == "next") tree_GameDef.SelectedNode = node.NextNode;
            else if (position == "prev") tree_GameDef.SelectedNode = node.PrevNode;
            if (!found)
            {
                TreeNode newNode = new TreeNode();
                if (currentItem is cardProperty)
                {
                    cardProperty oldProp = (cardProperty)currentItem;
                    cardProperty newProp = new cardProperty(oldProp.name,"");
                    newNode.Text = newProp.name + ": " + newProp.value;
                    newNode.Tag = newProp;
                }
                else if (currentItem is cardAlternate)
                {
                    cardAlternate oldAlt = (cardAlternate)currentItem;
                    cardAlternate newAlt = new cardAlternate("Alternate Name", oldAlt.type);
                    newNode.Text = newAlt.name + " (" + newAlt.type + ")";
                    newNode.Tag = newAlt;
                }
                int index = tree_GameDef.SelectedNode.Nodes.Add(newNode);
                tree_GameDef.SelectedNode = tree_GameDef.SelectedNode.Nodes[index];                        
            }
            openEditor();
            }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            checkSetChanges();
        }

        private void tree_GameDef_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Select the clicked node
                tree_GameDef.SelectedNode = tree_GameDef.GetNodeAt(e.X, e.Y);

                //if (tree_GameDef.SelectedNode != null)
                //{
                //    myContextMenuStrip.Show(treeView1, e.Location);
                //}
            }
        }

        private void tree_GameDef_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.F2) || (e.KeyCode == Keys.Enter))       // Ctrl-Right Arrow
            {
                openEditor();
                e.SuppressKeyPress = true;  // stops bing! also sets handeled which stop event bubbling
            }
        }

        private void configureGameMenuItem_Click(object sender, EventArgs e)
        {
            openConfig();
        }
    }
}
