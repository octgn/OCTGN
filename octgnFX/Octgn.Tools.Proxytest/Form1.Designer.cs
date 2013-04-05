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
            ((System.ComponentModel.ISupportInitialize)(this.propertiesDataGrid)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.proxyPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // propertiesDataGrid
            // 
            this.propertiesDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.propertiesDataGrid.Location = new System.Drawing.Point(0, 1);
            this.propertiesDataGrid.MultiSelect = false;
            this.propertiesDataGrid.Name = "propertiesDataGrid";
            this.propertiesDataGrid.Size = new System.Drawing.Size(244, 323);
            this.propertiesDataGrid.TabIndex = 0;
            // 
            // newPropertiesButton
            // 
            this.newPropertiesButton.Location = new System.Drawing.Point(3, 330);
            this.newPropertiesButton.Name = "newPropertiesButton";
            this.newPropertiesButton.Size = new System.Drawing.Size(75, 23);
            this.newPropertiesButton.TabIndex = 1;
            this.newPropertiesButton.Text = "New";
            this.newPropertiesButton.UseVisualStyleBackColor = true;
            this.newPropertiesButton.Click += new System.EventHandler(this.newPropertiesButton_Click);
            // 
            // loadPropertiesButton
            // 
            this.loadPropertiesButton.Location = new System.Drawing.Point(84, 330);
            this.loadPropertiesButton.Name = "loadPropertiesButton";
            this.loadPropertiesButton.Size = new System.Drawing.Size(75, 23);
            this.loadPropertiesButton.TabIndex = 2;
            this.loadPropertiesButton.Text = "Load";
            this.loadPropertiesButton.UseVisualStyleBackColor = true;
            this.loadPropertiesButton.Click += new System.EventHandler(this.loadPropertiesButton_Click);
            // 
            // savePropertiesButton
            // 
            this.savePropertiesButton.Location = new System.Drawing.Point(165, 330);
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
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(256, 386);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.propertiesDataGrid);
            this.tabPage1.Controls.Add(this.savePropertiesButton);
            this.tabPage1.Controls.Add(this.newPropertiesButton);
            this.tabPage1.Controls.Add(this.loadPropertiesButton);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(248, 360);
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
            this.tabPage2.Size = new System.Drawing.Size(248, 360);
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
            this.generateProxyButton.Location = new System.Drawing.Point(275, 160);
            this.generateProxyButton.Name = "generateProxyButton";
            this.generateProxyButton.Size = new System.Drawing.Size(136, 23);
            this.generateProxyButton.TabIndex = 5;
            this.generateProxyButton.Text = "Generate Proxy";
            this.generateProxyButton.UseVisualStyleBackColor = true;
            this.generateProxyButton.Click += new System.EventHandler(this.generateProxyButton_Click);
            // 
            // proxyPictureBox
            // 
            this.proxyPictureBox.Location = new System.Drawing.Point(417, 12);
            this.proxyPictureBox.Name = "proxyPictureBox";
            this.proxyPictureBox.Size = new System.Drawing.Size(273, 382);
            this.proxyPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.proxyPictureBox.TabIndex = 6;
            this.proxyPictureBox.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(702, 412);
            this.Controls.Add(this.proxyPictureBox);
            this.Controls.Add(this.generateProxyButton);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Proxydef Tester";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.propertiesDataGrid)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.proxyPictureBox)).EndInit();
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
    }
}

