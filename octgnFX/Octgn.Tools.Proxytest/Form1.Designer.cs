namespace Octgn.Tools.Proxytest
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
            this.propertiesDataGrid = new System.Windows.Forms.DataGridView();
            this.newPropertiesButton = new System.Windows.Forms.Button();
            this.loadPropertiesButton = new System.Windows.Forms.Button();
            this.savePropertiesButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.loadFromProxydefButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.rootDirTextBox = new System.Windows.Forms.TextBox();
            this.openDefinitionButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.proxydefPathTextBox = new System.Windows.Forms.TextBox();
            this.xmlPropertiesOpenDialog = new System.Windows.Forms.OpenFileDialog();
            this.proxydefOpenDialog = new System.Windows.Forms.OpenFileDialog();
            this.rootFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.definitionOpenDialog = new System.Windows.Forms.OpenFileDialog();
            this.generateProxyButton = new System.Windows.Forms.Button();
            this.proxyPictureBox = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.TimeGeneratedTextBox = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.propertiesDataGrid)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.proxyPictureBox)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertiesDataGrid
            // 
            this.propertiesDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.propertiesDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertiesDataGrid.Location = new System.Drawing.Point(3, 3);
            this.propertiesDataGrid.MultiSelect = false;
            this.propertiesDataGrid.Name = "propertiesDataGrid";
            this.propertiesDataGrid.Size = new System.Drawing.Size(522, 690);
            this.propertiesDataGrid.TabIndex = 0;
            // 
            // newPropertiesButton
            // 
            this.newPropertiesButton.Location = new System.Drawing.Point(110, 3);
            this.newPropertiesButton.Name = "newPropertiesButton";
            this.newPropertiesButton.Size = new System.Drawing.Size(75, 23);
            this.newPropertiesButton.TabIndex = 1;
            this.newPropertiesButton.Text = "New";
            this.newPropertiesButton.UseVisualStyleBackColor = true;
            this.newPropertiesButton.Click += new System.EventHandler(this.newPropertiesButton_Click);
            // 
            // loadPropertiesButton
            // 
            this.loadPropertiesButton.Location = new System.Drawing.Point(191, 3);
            this.loadPropertiesButton.Name = "loadPropertiesButton";
            this.loadPropertiesButton.Size = new System.Drawing.Size(75, 23);
            this.loadPropertiesButton.TabIndex = 2;
            this.loadPropertiesButton.Text = "Load";
            this.loadPropertiesButton.UseVisualStyleBackColor = true;
            this.loadPropertiesButton.Click += new System.EventHandler(this.loadPropertiesButton_Click);
            // 
            // savePropertiesButton
            // 
            this.savePropertiesButton.Location = new System.Drawing.Point(272, 3);
            this.savePropertiesButton.Name = "savePropertiesButton";
            this.savePropertiesButton.Size = new System.Drawing.Size(75, 23);
            this.savePropertiesButton.TabIndex = 3;
            this.savePropertiesButton.Text = "Save";
            this.savePropertiesButton.UseVisualStyleBackColor = true;
            this.savePropertiesButton.Click += new System.EventHandler(this.savePropertiesButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(536, 722);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.propertiesDataGrid);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(528, 696);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Properties";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.loadFromProxydefButton);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.rootDirTextBox);
            this.tabPage2.Controls.Add(this.openDefinitionButton);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.proxydefPathTextBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(528, 696);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Templatedef";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // loadFromProxydefButton
            // 
            this.loadFromProxydefButton.Location = new System.Drawing.Point(67, 167);
            this.loadFromProxydefButton.Name = "loadFromProxydefButton";
            this.loadFromProxydefButton.Size = new System.Drawing.Size(175, 23);
            this.loadFromProxydefButton.TabIndex = 6;
            this.loadFromProxydefButton.Text = "Load from proxydef";
            this.loadFromProxydefButton.UseVisualStyleBackColor = true;
            this.loadFromProxydefButton.Click += new System.EventHandler(this.loadFromProxydefButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(132, 137);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(18, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Or";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Rootdir:";
            // 
            // rootDirTextBox
            // 
            this.rootDirTextBox.Enabled = false;
            this.rootDirTextBox.Location = new System.Drawing.Point(67, 40);
            this.rootDirTextBox.Name = "rootDirTextBox";
            this.rootDirTextBox.Size = new System.Drawing.Size(175, 20);
            this.rootDirTextBox.TabIndex = 3;
            // 
            // openDefinitionButton
            // 
            this.openDefinitionButton.Location = new System.Drawing.Point(67, 97);
            this.openDefinitionButton.Name = "openDefinitionButton";
            this.openDefinitionButton.Size = new System.Drawing.Size(175, 23);
            this.openDefinitionButton.TabIndex = 2;
            this.openDefinitionButton.Text = "Load from definition.xml";
            this.openDefinitionButton.UseVisualStyleBackColor = true;
            this.openDefinitionButton.Click += new System.EventHandler(this.openDefinitionButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Proxydef: ";
            // 
            // proxydefPathTextBox
            // 
            this.proxydefPathTextBox.Enabled = false;
            this.proxydefPathTextBox.Location = new System.Drawing.Point(67, 13);
            this.proxydefPathTextBox.Name = "proxydefPathTextBox";
            this.proxydefPathTextBox.Size = new System.Drawing.Size(175, 20);
            this.proxydefPathTextBox.TabIndex = 0;
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
            // generateProxyButton
            // 
            this.generateProxyButton.Enabled = false;
            this.generateProxyButton.Location = new System.Drawing.Point(3, 3);
            this.generateProxyButton.Name = "generateProxyButton";
            this.generateProxyButton.Size = new System.Drawing.Size(101, 23);
            this.generateProxyButton.TabIndex = 5;
            this.generateProxyButton.Text = "Generate Proxy";
            this.generateProxyButton.UseVisualStyleBackColor = true;
            this.generateProxyButton.Click += new System.EventHandler(this.generateProxyButton_Click);
            // 
            // proxyPictureBox
            // 
            this.proxyPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.proxyPictureBox.Location = new System.Drawing.Point(0, 0);
            this.proxyPictureBox.Name = "proxyPictureBox";
            this.proxyPictureBox.Size = new System.Drawing.Size(557, 722);
            this.proxyPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.proxyPictureBox.TabIndex = 6;
            this.proxyPictureBox.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1108, 764);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // flowLayoutPanel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.generateProxyButton);
            this.flowLayoutPanel1.Controls.Add(this.newPropertiesButton);
            this.flowLayoutPanel1.Controls.Add(this.loadPropertiesButton);
            this.flowLayoutPanel1.Controls.Add(this.savePropertiesButton);
            this.flowLayoutPanel1.Controls.Add(this.TimeGeneratedTextBox);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 731);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1102, 30);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // TimeGeneratedTextBox
            // 
            this.TimeGeneratedTextBox.Enabled = false;
            this.TimeGeneratedTextBox.Location = new System.Drawing.Point(353, 3);
            this.TimeGeneratedTextBox.Name = "TimeGeneratedTextBox";
            this.TimeGeneratedTextBox.Size = new System.Drawing.Size(100, 20);
            this.TimeGeneratedTextBox.TabIndex = 6;
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
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.proxyPictureBox);
            this.splitContainer1.Size = new System.Drawing.Size(1102, 722);
            this.splitContainer1.SplitterDistance = 536;
            this.splitContainer1.SplitterWidth = 9;
            this.splitContainer1.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1108, 764);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Proxydef Tester";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.propertiesDataGrid)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.proxyPictureBox)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView propertiesDataGrid;
        private System.Windows.Forms.Button newPropertiesButton;
        private System.Windows.Forms.Button loadPropertiesButton;
        private System.Windows.Forms.Button savePropertiesButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.OpenFileDialog xmlPropertiesOpenDialog;
        private System.Windows.Forms.Button openDefinitionButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox proxydefPathTextBox;
        private System.Windows.Forms.OpenFileDialog proxydefOpenDialog;
        private System.Windows.Forms.Button loadFromProxydefButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox rootDirTextBox;
        private System.Windows.Forms.FolderBrowserDialog rootFolderBrowserDialog;
        private System.Windows.Forms.OpenFileDialog definitionOpenDialog;
        private System.Windows.Forms.Button generateProxyButton;
        private System.Windows.Forms.PictureBox proxyPictureBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox TimeGeneratedTextBox;
    }
}

