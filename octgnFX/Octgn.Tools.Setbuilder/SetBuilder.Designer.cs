namespace Octgn.Tools.SetBuilder
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("No Game Definition Loaded");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.xmlPropertiesOpenDialog = new System.Windows.Forms.OpenFileDialog();
            this.proxydefOpenDialog = new System.Windows.Forms.OpenFileDialog();
            this.rootFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.definitionOpenDialog = new System.Windows.Forms.OpenFileDialog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tree_GameDef = new System.Windows.Forms.TreeView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadDefinitionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configureGameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.proxyPictureBox = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.proxyPictureBox)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // xmlPropertiesOpenDialog
            // 
            this.xmlPropertiesOpenDialog.Filter = "Properties file|properties.xml|Xml file|*.xml";
            // 
            // proxydefOpenDialog
            // 
            this.proxydefOpenDialog.Filter = "Proxydef XML file|*.xml";
            // 
            // rootFolderBrowserDialog
            // 
            this.rootFolderBrowserDialog.ShowNewFolderButton = false;
            // 
            // definitionOpenDialog
            // 
            this.definitionOpenDialog.Filter = "definition.xml|definition.xml";
            // 
            // splitContainer1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.splitContainer1, 2);
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tree_GameDef);
            this.splitContainer1.Panel1.Controls.Add(this.menuStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.proxyPictureBox);
            this.splitContainer1.Size = new System.Drawing.Size(678, 360);
            this.splitContainer1.SplitterDistance = 262;
            this.splitContainer1.SplitterWidth = 9;
            this.splitContainer1.TabIndex = 8;
            // 
            // tree_GameDef
            // 
            this.tree_GameDef.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tree_GameDef.Location = new System.Drawing.Point(0, 24);
            this.tree_GameDef.Name = "tree_GameDef";
            treeNode2.Name = "Node0";
            treeNode2.Text = "No Game Definition Loaded";
            this.tree_GameDef.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode2});
            this.tree_GameDef.Size = new System.Drawing.Size(262, 336);
            this.tree_GameDef.TabIndex = 1;
            this.tree_GameDef.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tree_GameDef_AfterSelect);
            this.tree_GameDef.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tree_GameDef_NodeMouseDoubleClick);
            this.tree_GameDef.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tree_GameDef_KeyDown);
            this.tree_GameDef.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tree_GameDef_MouseUp);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(262, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadDefinitionToolStripMenuItem,
            this.saveSetToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadDefinitionToolStripMenuItem
            // 
            this.loadDefinitionToolStripMenuItem.Name = "loadDefinitionToolStripMenuItem";
            this.loadDefinitionToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.loadDefinitionToolStripMenuItem.Text = "Load Definition";
            this.loadDefinitionToolStripMenuItem.Click += new System.EventHandler(this.loadDefinitionToolStripMenuItem_Click);
            // 
            // saveSetToolStripMenuItem
            // 
            this.saveSetToolStripMenuItem.Enabled = false;
            this.saveSetToolStripMenuItem.Name = "saveSetToolStripMenuItem";
            this.saveSetToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.saveSetToolStripMenuItem.Text = "Save Set";
            this.saveSetToolStripMenuItem.Click += new System.EventHandler(this.SaveSet_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configureGameMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // configureGameMenuItem
            // 
            this.configureGameMenuItem.Enabled = false;
            this.configureGameMenuItem.Name = "configureGameMenuItem";
            this.configureGameMenuItem.Size = new System.Drawing.Size(161, 22);
            this.configureGameMenuItem.Text = "Configure Game";
            this.configureGameMenuItem.Click += new System.EventHandler(this.configureGameMenuItem_Click);
            // 
            // proxyPictureBox
            // 
            this.proxyPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.proxyPictureBox.Location = new System.Drawing.Point(0, 0);
            this.proxyPictureBox.Name = "proxyPictureBox";
            this.proxyPictureBox.Size = new System.Drawing.Size(407, 360);
            this.proxyPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.proxyPictureBox.TabIndex = 6;
            this.proxyPictureBox.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(681, 366);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(681, 366);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "SetBuilder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.proxyPictureBox)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog xmlPropertiesOpenDialog;
        private System.Windows.Forms.OpenFileDialog proxydefOpenDialog;
        private System.Windows.Forms.FolderBrowserDialog rootFolderBrowserDialog;
        private System.Windows.Forms.OpenFileDialog definitionOpenDialog;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TreeView tree_GameDef;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadDefinitionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSetToolStripMenuItem;
        private System.Windows.Forms.PictureBox proxyPictureBox;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem configureGameMenuItem;
    }
}

