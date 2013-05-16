using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using agsXMPP;
using agsXMPP.protocol;
using agsXMPP.protocol.client;
using agsXMPP.protocol.Base;

namespace agsXMPP.ui.roster
{
    public partial class RosterControl : UserControl, IRosterControl
    {
        Tooltip tool = new Tooltip();

        #region << Constants >>
        private const int IMAGE_AWAY        = 0;
        private const int IMAGE_CHAT        = 1;
        private const int IMAGE_DND         = 2;
        private const int IMAGE_XA          = 3;
        private const int IMAGE_ONLINE      = 4;
        private const int IMAGE_OFFLINE     = 5;
        private const int IMAGE_EXPAND      = 6;
        private const int IMAGE_COLLAPSE    = 7;
        #endregion

        public event System.EventHandler SelectionChanged;
                
        #region << Properties >>
        private Dictionary <string,RosterData> m_Roster = new Dictionary <string, RosterData> ();

        private RosterNode      m_RootOnline;
        private RosterNode      m_RootOffline;
        private bool            m_HideEmptyGroups   = true;
        private string          m_DefaultGroupName  = "ungrouped";
        private Color           m_ColorRoot         = SystemColors.Highlight;
        private Color           m_ColorGroup        = SystemColors.Highlight;
        private Color           m_ColorResource     = SystemColors.ControlText;
        private Color           m_ColorRoster       = SystemColors.ControlText;

        private int             oldNodeIndex        = -1; 
        /// <summary>
        /// Dictionary collection of the Roster displayed in the control
        /// </summary>
        public Dictionary <string, RosterData> Roster
        {
            get { return m_Roster; }
        }

        public string DefaultGroupName
        {
            get { return m_DefaultGroupName; }
            set { m_DefaultGroupName = value; }
        }

        /// <summary>
        /// Should Empty groups be hidden? Default value = true
        /// </summary>
        public bool HideEmptyGroups
        {
            get { return m_HideEmptyGroups; }
            set { m_HideEmptyGroups = value; }
        }

        public Color ColorRoot
        {
            get { return m_ColorRoot; }
            set { m_ColorRoot = value; }
        }

        public Color ColorGroup
        {
            get { return m_ColorGroup; }
            set { m_ColorGroup = value; }
        }

        public Color ColorResource
        {
            get { return m_ColorResource; }
            set { m_ColorResource = value; }
        }

        public Color ColorRoster
        {
            get { return m_ColorRoster; }
            set { m_ColorRoster = value; }
        }
            

        #endregion

        public RosterControl()
        {
                        
            InitializeComponent();
                               
            // Sort Stuff
            this.treeView.TreeViewNodeSorter = new TreeViewComparer();

            InitImagelist();
            Clear();         
        }

        private void InitImagelist()
        {
            //public const int IMAGE_AWAY = 0;
            //public const int IMAGE_CHAT = 1;
            //public const int IMAGE_DND = 2;
            //public const int IMAGE_XA = 3;
            //public const int IMAGE_ONLINE = 4;
            //public const int IMAGE_OFFLINE = 5;
            //public const int IMAGE_EXPAND = 6;
            //public const int IMAGE_COLLAPSE = 6;
            
            this.ils16.Images.Clear();
            
            this.ils16.Images.Add(global::agsXMPP.ui.Properties.Resources.away);
            this.ils16.Images.Add(global::agsXMPP.ui.Properties.Resources.ffc);
            this.ils16.Images.Add(global::agsXMPP.ui.Properties.Resources.dnd);
            this.ils16.Images.Add(global::agsXMPP.ui.Properties.Resources.xa);
            this.ils16.Images.Add(global::agsXMPP.ui.Properties.Resources.online);
            this.ils16.Images.Add(global::agsXMPP.ui.Properties.Resources.offline);
            this.ils16.Images.Add(global::agsXMPP.ui.Properties.Resources.expand);
            this.ils16.Images.Add(global::agsXMPP.ui.Properties.Resources.collapse);            
        }

        public void Clear()
        {
            m_Roster.Clear();

            treeView.Nodes.Clear();

            m_RootOnline = new RosterNode("Online");
            m_RootOnline.NodeType = RosterNodeType.RootNode;
            m_RootOnline.SelectedImageIndex = IMAGE_COLLAPSE;
            m_RootOnline.ImageIndex = IMAGE_COLLAPSE;

            m_RootOffline = new RosterNode("Offline");
            m_RootOffline.NodeType = RosterNodeType.RootNode;
            m_RootOffline.SelectedImageIndex = IMAGE_COLLAPSE;
            m_RootOffline.ImageIndex = IMAGE_COLLAPSE;

            treeView.Nodes.Add(m_RootOnline);
            treeView.Nodes.Add(m_RootOffline);     
        }

        public void BeginUpdate()
        {
            treeView.BeginUpdate();
        }

        public void EndUpdate()
        {
            treeView.EndUpdate();
        }

        public void ExpandAll()
        {
            treeView.ExpandAll();
        }

        public RosterNode GetRosterItem(Jid jid)
        {
            if (m_Roster.ContainsKey(jid.Bare))
            {
                RosterData d = m_Roster[jid.Bare];
                return d.RosterNode;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Remove a Rosteritem from the Roster
        /// </summary>
        /// <param name="ritem">The item to remove</param>
        /// <returns
        /// >returns true if the item was removed, false if it didn't exist
        /// and could not be removed
        /// </returns>
        public bool RemoveRosterItem(agsXMPP.protocol.iq.roster.RosterItem ritem)
        {
            return RemoveRosterItem(ritem.Jid);
        }

        /// <summary>
        /// Remove a RosterItem from the roster
        /// </summary>
        /// <param name="jid">The items Jid to remove</param>
        /// <returns
        /// >returns true if the item was removed, false if it didn't exist
        /// and could not be removed
        /// </returns>
        public bool RemoveRosterItem(Jid jid)
        {
            if (m_Roster.ContainsKey(jid.ToString()))
            {
                RosterData d = m_Roster[jid.ToString()];            
                TreeNode Parent = d.RosterNode.Parent;
                d.RosterNode.Remove();
                m_Roster.Remove(jid.ToString());

                if (m_HideEmptyGroups)
                {
                    if (Parent.Nodes.Count == 0)
                        Parent.Remove();
                }
                return true;
                
            }
            else
                return false;
        }

        public RosterNode SelectedItem()
        {
            return treeView.SelectedNode as RosterNode;
        }


        public RosterNode AddRosterItem(agsXMPP.protocol.iq.roster.RosterItem ritem)
        {
            string nodeText = ritem.Name != null ? ritem.Name : ritem.Jid.ToString();
            RosterNode node;

            if (!m_Roster.ContainsKey(ritem.Jid.ToString()))
            {
                node = new RosterNode();

                node.Text = nodeText;
                node.RosterItem = ritem;
                node.NodeType = RosterNodeType.RosterNode;
                node.ImageIndex = IMAGE_OFFLINE;
                node.SelectedImageIndex = IMAGE_OFFLINE;

                string groupname;
                if (ritem.GetGroups().Count > 0)
                {
                    Group g = (Group)ritem.GetGroups().Item(0);
                    groupname = g.Name;
                }
                else
                {
                    groupname = m_DefaultGroupName;
                }

                RosterNode group = GetGroup(m_RootOffline, groupname);
                if (group == null)
                    group = AddGroup(groupname, m_RootOffline);

                group.Nodes.Add(node);
                //group.Text = group.Name + " (" + group.Nodes.Count.ToString() + ")";
                m_Roster.Add(ritem.Jid.ToString(), new RosterData(node));

                return node;
            }
            else
            {
                if (m_Roster[ritem.Jid.ToString()].Online)
                {                   
                    // We upate a rosterItem which is online and has Presences
                    // So store this Presences before we remove this item and add them to the 
                    // new Node again                  
                    Presence[] pres = new Presence[m_Roster[ritem.Jid.ToString()].Presences.Count];
                    int i= 0;
                    Dictionary<string, PresenceData>.Enumerator myEnum = m_Roster[ritem.Jid.ToString()].Presences.GetEnumerator();
                    while (myEnum.MoveNext())
                    {
                       pres[i] = myEnum.Current.Value.Presence;
                       i++;
                    }
                    RemoveRosterItem(ritem);
                    node = AddRosterItem(ritem);
                    foreach (Presence p in pres)
                    {
                        SetPresence(p);
                    }
                    return node;
                }
                else
                {
                    RemoveRosterItem(ritem);
                    return AddRosterItem(ritem);
                }
            }
        }

        /// <summary>
        /// Adds a Group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private RosterNode AddGroup(string group, RosterNode rn)
        {
            RosterNode n = new RosterNode();

            n.NodeType = RosterNodeType.GroupNode;
            n.Text = group;
            // The groupname is stored in the the Name property
            n.Name = group;
            n.ImageIndex = IMAGE_COLLAPSE;
            n.SelectedImageIndex = IMAGE_COLLAPSE;
            rn.Nodes.Add(n);                      
                                    
            return n;
        }
                   

        /// <summary>
        /// This functions finds the rootnode of a leaf.
        /// In our case its either Online or Offline
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private TreeNode GetRootFromLeaf(TreeNode n)
        {
            if (n.Parent != null)
                return GetRootFromLeaf(n.Parent);
            else
                return n;                        
        }

        private RosterNode GetGroup(RosterNode node, string group)
        {
            foreach (RosterNode n in node.Nodes)
            {
                if (n.NodeType == RosterNodeType.GroupNode && n.Name == group)
                    return n;
            }
            return null;
        }

        /// <summary>
        /// Moves a Node from the online section to the offline section or vice versa
        /// </summary>
        /// <param name="rdata"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void MoveRoster(RosterData rdata, RosterNode from, RosterNode to)
        {
            RosterNode groupNode;

            string groupName = rdata.Group;

            groupNode = GetGroup(to, groupName);
            if (groupNode  == null)
                groupNode = AddGroup(groupName, to);

            RosterNode newNode = rdata.RosterNode.Clone() as RosterNode;
            groupNode.Nodes.Add(newNode);

            // Expand all groupes in the Online Node
            if (to == m_RootOnline)
            {
                groupNode.Expand();
                m_RootOnline.Expand();
            }

            TreeNode Parent = rdata.RosterNode.Parent;
            rdata.RosterNode.Remove();
            rdata.RosterNode = null;
            rdata.RosterNode = newNode;

            if (m_HideEmptyGroups)
            {
                if (Parent.Nodes.Count == 0)
                    Parent.Remove();
            }
        }

        public void SetPresence(Presence pres)
        {
            // Dont handle presence errors for now
            if (pres.Type == PresenceType.error)
                return;

            // We need a node all the time here
            RosterNode n;

            if (pres.Type == PresenceType.unavailable)
            {
                // The presence could also be a presence from a hroup chat.
                // So check here if its a presence from a rosterItem
                if (m_Roster.ContainsKey(pres.From.Bare))
                {
                    RosterData d = m_Roster[pres.From.Bare];
                    if (d != null)
                    {
                        if (d.Presences.ContainsKey(pres.From.ToString()))
                        {
                            PresenceData presData = d.Presences[pres.From.ToString()];
                            RosterNode parent = (RosterNode)presData.Node.Parent;

                            presData.Node.Remove();
                            d.Presences.Remove(pres.From.ToString());

                            // Last Presence goes Offline
                            if (d.Presences.Count == 0)
                            {
                                parent.ImageIndex = IMAGE_OFFLINE;
                                parent.SelectedImageIndex = IMAGE_OFFLINE;

                                // If Online Move it to the Offline Section
                                if (GetRootFromLeaf(d.RosterNode) == m_RootOnline)
                                {
                                    MoveRoster(d, m_RootOnline, m_RootOffline);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Online Presence
                // The presence could also be a presence from a hroup chat.
                // So check here if its a presence from a rosterItem
                if (m_Roster.ContainsKey(pres.From.Bare))
                {
                    RosterData d = m_Roster[pres.From.Bare];
                    // Check if the roster is in the online section, when not then move it from Offline to Online
                    if (!d.Online)
                    {
                        MoveRoster(d, m_RootOffline, m_RootOnline);                    
                    }

                    if (d != null)
                    {
                        int img = (int)pres.Show;
                        if (img == -1)
                            img = IMAGE_ONLINE;

                        if (d.Presences.ContainsKey(pres.From.ToString()))
                        {
                            // Presence for this item available
                            PresenceData presData = (PresenceData)d.Presences[pres.From.ToString()];

                            presData.Node.SelectedImageIndex    = img;
                            presData.Node.ImageIndex            = img;
                            presData.Node.Presence              = pres;

                            presData.Node.Parent.SelectedImageIndex = img;
                            presData.Node.Parent.ImageIndex = img;
                           
                            presData.Presence = pres;
                        }
                        else
                        {
                            // Presence not available yet, so add it
                            n = new RosterNode(pres.From.Resource, RosterNodeType.ResourceNode);
                                                    
                            n.SelectedImageIndex    = img;
                            n.ImageIndex            = img;
                            n.Presence              = pres;

                            d.RosterNode.Nodes.Add(n);

                            n.Parent.SelectedImageIndex = img;
                            n.Parent.ImageIndex = img;

                            n.Name = ((RosterNode)n.Parent).Name;
                           
                            d.Presences.Add(pres.From.ToString(), new PresenceData(n, pres));
                        }
                    }
                }
            }
        }

        private void treeView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            RosterNode rn = e.Node as RosterNode;
            if (rn.NodeType == RosterNodeType.RootNode ||
                rn.NodeType == RosterNodeType.GroupNode)
            {
                rn.ImageIndex = IMAGE_COLLAPSE;
                rn.SelectedImageIndex = IMAGE_COLLAPSE;
            }
        }

        private void treeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            RosterNode rn = e.Node as RosterNode;
            if (rn.NodeType == RosterNodeType.RootNode ||
                rn.NodeType == RosterNodeType.GroupNode)
            {
                rn.ImageIndex = IMAGE_EXPAND;
                rn.SelectedImageIndex = IMAGE_EXPAND;
            }
        }

       

        private void toolStripButtonClose_Click(object sender, EventArgs e)
        {
            toolStrip.Hide();
        }

        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
                toolStrip.Show();
        }

        private void toolStripButtonFindNext_Click(object sender, EventArgs e)
        {
            RosterNode node = treeView.SelectedNode as RosterNode;
            if (node == null)
                node = this.treeView.Nodes[0] as RosterNode;

            FindNext(node, this.toolStripTextBox.Text);
        }

        private void toolStripButtonFindPrevious_Click(object sender, EventArgs e)
        {
            RosterNode node = treeView.SelectedNode as RosterNode;
            if (node != null)
                FindPrevious(node, this.toolStripTextBox.Text);
        }

        /// <summary>
        /// Searching a contact downwards the treeview
        /// </summary>
        /// <param name="node"></param>
        /// <param name="pattern"></param>
        private void FindNext(RosterNode node, string pattern)
        {                 
                           
            if (
                (node.NodeType == RosterNodeType.RootNode || node.NodeType == RosterNodeType.GroupNode)
                && node.Nodes.Count > 0
                && !node.IsExpanded
                )
                node.Expand();

            if (node.NextVisibleNode != null)
            {
                node = node.NextVisibleNode as RosterNode;
                if ( node.NodeType == RosterNodeType.RosterNode
                     && node.Text.ToLower().Contains(pattern.ToLower()) )
                {
                    treeView.SelectedNode = node;
                    node.EnsureVisible();
                    treeView.Focus();
                }
                else
                    FindNext(node, pattern);
            }
            
        }

        /// <summary>
        /// Searching a contact upwards the treeview
        /// </summary>
        /// <param name="node"></param>
        /// <param name="pattern"></param>
        private void FindPrevious(RosterNode node, string pattern)
        {            
            if (node.PrevVisibleNode != null)
            {
                while (
                        (((RosterNode)node.PrevVisibleNode).NodeType == RosterNodeType.RootNode || ((RosterNode)node.PrevVisibleNode).NodeType == RosterNodeType.GroupNode)
                        && node.PrevVisibleNode.Nodes.Count > 0
                        && !node.PrevVisibleNode.IsExpanded
                    )
                    node.PrevVisibleNode.Expand();

                node = node.PrevVisibleNode as RosterNode;
                if ( node.NodeType == RosterNodeType.RosterNode
                     && node.Text.ToLower().Contains(pattern.ToLower()) )
                {
                    treeView.SelectedNode = node;
                    node.EnsureVisible();
                    treeView.Focus();
                }
                else
                    FindPrevious(node, pattern);
            }
        }

        private void toolStripTextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.toolStripTextBox.TextLength > 0)
            {
                toolStripButtonFindNext.Enabled = true;
                toolStripButtonFindPrevious.Enabled = true;
            }
            else
            {
                toolStripButtonFindNext.Enabled = false;
                toolStripButtonFindPrevious.Enabled = false;
            }
        }

        private void treeView_ContextMenuStripChanged(object sender, EventArgs e)
        {
            this.treeView.ContextMenuStrip = this.ContextMenuStrip;
        }

        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            TreeNode node = treeView.GetNodeAt(e.X, e.Y);
            if (node != null)
            {
                treeView.SelectedNode = node;                
            }
        }

        private void treeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            // colors that depend if the row is currently selected or not, 
            // assign to a system brush so should not be disposed
            // not selected colors
            RosterNode node = e.Node as RosterNode;
            
            Brush brushBack = SystemBrushes.Window;
            Brush brushText = null;

            switch (node.NodeType)
            {
                case RosterNodeType.GroupNode:
                    brushText = new SolidBrush(m_ColorGroup);
                    break;
                case RosterNodeType.RootNode:
                    brushText = new SolidBrush(m_ColorRoot);
                    break;
                case RosterNodeType.ResourceNode:
                    brushText = new SolidBrush(m_ColorResource);
                    break;
                case RosterNodeType.RosterNode:
                    brushText = new SolidBrush(m_ColorRoster);
                    break;
                default:
                    brushText = new SolidBrush(SystemColors.ControlText);
                    break;
            }
                        
            Font textFont;
            if (node.NodeType == RosterNodeType.GroupNode
                || node.NodeType == RosterNodeType.RootNode)
                textFont = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Bold);
            else
                textFont = this.Font;

            if ((e.State & TreeNodeStates.Selected) != 0)
            {
                if ((e.State & TreeNodeStates.Focused) != 0)
                {
                    // selected and has focus
                    brushBack = SystemBrushes.Highlight;
                    brushText = SystemBrushes.HighlightText;
                }
                else
                {
                    // selected and does not have focus
                    brushBack = SystemBrushes.Control;
                }
            }

            SizeF textSize = e.Graphics.MeasureString(e.Node.Text, textFont);
            RectangleF rc = new RectangleF(e.Bounds.X, e.Bounds.Y, textSize.Width + 2, e.Bounds.Height);
            
            // draw node background            
            e.Graphics.FillRectangle(brushBack, rc);
        
            if (rc.Width > 0 && rc.Height > 0)
            {               
                rc = new RectangleF(e.Bounds.X, e.Bounds.Y + (int)(e.Bounds.Height - this.Font.Height) / 2,
                                    textSize.Width, e.Bounds.Height);
                e.Graphics.DrawString(e.Node.Text, textFont, brushText, rc);
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, new EventArgs());
        }        

        private void treeView_MouseMove(object sender, MouseEventArgs e)
        {
            RosterNode rn = this.treeView.GetNodeAt(e.X, e.Y) as RosterNode;
            if (rn != null && rn.NodeType == RosterNodeType.RosterNode)
            {
                int currentNodeIndex = rn.Index;
                if (currentNodeIndex != oldNodeIndex)
                {
                    // Node Changed                    
                    tool.Active = false;               

                    oldNodeIndex = currentNodeIndex;

                    if (rn.RosterItem != null)
                    {
                        if (m_Roster.ContainsKey(rn.RosterItem.Jid.Bare))
                        {
                            tool.Tooltiptext = rn.RosterItem.Jid.ToString();
                            tool.Tooltiptext += "\r\nSubscription: " + rn.RosterItem.Subscription.ToString();                       
                        
                            RosterData d = m_Roster[rn.RosterItem.Jid.Bare];
                            if (d != null)
                            {
                                foreach (PresenceData p in d.Presences.Values)
                                {
                                    tool.Tooltiptext += "\r\n\r\n" + "Resource: " + p.Presence.From.Resource;
                                    if (p.Presence.Status != null)
                                        tool.Tooltiptext += "\r\n" + "Status: " + p.Presence.Status;
                                }
                            }
                            tool.Active = true;
                        }
                    }
                }
            } 
        }
     

        private void treeView_MouseLeave(object sender, EventArgs e)
        {
            tool.Active = false;
        }

            

    
        
    }
}
