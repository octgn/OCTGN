namespace Octgn.Tools.O8buildgui
{
    partial class O8buildGUIForm
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
            this.components = new System.ComponentModel.Container();
            this.LayoutSplitContainer = new System.Windows.Forms.SplitContainer();
            this.ConsoleOutput = new System.Windows.Forms.ListBox();
            this.useTesto8buildCheckbox = new System.Windows.Forms.CheckBox();
            this.ActionLabel = new System.Windows.Forms.Label();
            this.ActionComboBox = new System.Windows.Forms.ComboBox();
            this.RunButton = new System.Windows.Forms.Button();
            this.LoadDirButton = new System.Windows.Forms.Button();
            this.PathLabel = new System.Windows.Forms.Label();
            this.PathTextBox = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.ListBoxContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copySelectedLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.LayoutSplitContainer)).BeginInit();
            this.LayoutSplitContainer.Panel1.SuspendLayout();
            this.LayoutSplitContainer.Panel2.SuspendLayout();
            this.LayoutSplitContainer.SuspendLayout();
            this.ListBoxContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // LayoutSplitContainer
            // 
            this.LayoutSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LayoutSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.LayoutSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.LayoutSplitContainer.Name = "LayoutSplitContainer";
            this.LayoutSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // LayoutSplitContainer.Panel1
            // 
            this.LayoutSplitContainer.Panel1.Controls.Add(this.ConsoleOutput);
            // 
            // LayoutSplitContainer.Panel2
            // 
            this.LayoutSplitContainer.Panel2.Controls.Add(this.useTesto8buildCheckbox);
            this.LayoutSplitContainer.Panel2.Controls.Add(this.ActionLabel);
            this.LayoutSplitContainer.Panel2.Controls.Add(this.ActionComboBox);
            this.LayoutSplitContainer.Panel2.Controls.Add(this.RunButton);
            this.LayoutSplitContainer.Panel2.Controls.Add(this.LoadDirButton);
            this.LayoutSplitContainer.Panel2.Controls.Add(this.PathLabel);
            this.LayoutSplitContainer.Panel2.Controls.Add(this.PathTextBox);
            this.LayoutSplitContainer.Size = new System.Drawing.Size(1008, 730);
            this.LayoutSplitContainer.SplitterDistance = 616;
            this.LayoutSplitContainer.TabIndex = 0;
            // 
            // ConsoleOutput
            // 
            this.ConsoleOutput.ContextMenuStrip = this.ListBoxContextMenuStrip;
            this.ConsoleOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsoleOutput.FormattingEnabled = true;
            this.ConsoleOutput.Location = new System.Drawing.Point(0, 0);
            this.ConsoleOutput.Name = "ConsoleOutput";
            this.ConsoleOutput.ScrollAlwaysVisible = true;
            this.ConsoleOutput.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.ConsoleOutput.Size = new System.Drawing.Size(1008, 616);
            this.ConsoleOutput.TabIndex = 0;
            // 
            // useTesto8buildCheckbox
            // 
            this.useTesto8buildCheckbox.AutoSize = true;
            this.useTesto8buildCheckbox.Location = new System.Drawing.Point(54, 60);
            this.useTesto8buildCheckbox.Name = "useTesto8buildCheckbox";
            this.useTesto8buildCheckbox.Size = new System.Drawing.Size(102, 17);
            this.useTesto8buildCheckbox.TabIndex = 6;
            this.useTesto8buildCheckbox.Text = "Use test o8build";
            this.useTesto8buildCheckbox.UseVisualStyleBackColor = true;
            // 
            // ActionLabel
            // 
            this.ActionLabel.AutoSize = true;
            this.ActionLabel.Location = new System.Drawing.Point(13, 36);
            this.ActionLabel.Name = "ActionLabel";
            this.ActionLabel.Size = new System.Drawing.Size(40, 13);
            this.ActionLabel.TabIndex = 5;
            this.ActionLabel.Text = "Action:";
            // 
            // ActionComboBox
            // 
            this.ActionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ActionComboBox.FormattingEnabled = true;
            this.ActionComboBox.Location = new System.Drawing.Point(54, 33);
            this.ActionComboBox.Name = "ActionComboBox";
            this.ActionComboBox.Size = new System.Drawing.Size(331, 21);
            this.ActionComboBox.Sorted = true;
            this.ActionComboBox.TabIndex = 4;
            // 
            // RunButton
            // 
            this.RunButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RunButton.Location = new System.Drawing.Point(921, 5);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(75, 93);
            this.RunButton.TabIndex = 3;
            this.RunButton.Text = "Run";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // LoadDirButton
            // 
            this.LoadDirButton.Location = new System.Drawing.Point(391, 6);
            this.LoadDirButton.Name = "LoadDirButton";
            this.LoadDirButton.Size = new System.Drawing.Size(75, 23);
            this.LoadDirButton.TabIndex = 2;
            this.LoadDirButton.Text = "Load Dir";
            this.LoadDirButton.UseVisualStyleBackColor = true;
            this.LoadDirButton.Click += new System.EventHandler(this.LoadDirButton_Click);
            // 
            // PathLabel
            // 
            this.PathLabel.AutoSize = true;
            this.PathLabel.Location = new System.Drawing.Point(13, 9);
            this.PathLabel.Name = "PathLabel";
            this.PathLabel.Size = new System.Drawing.Size(32, 13);
            this.PathLabel.TabIndex = 1;
            this.PathLabel.Text = "Path:";
            // 
            // PathTextBox
            // 
            this.PathTextBox.Enabled = false;
            this.PathTextBox.Location = new System.Drawing.Point(54, 6);
            this.PathTextBox.Name = "PathTextBox";
            this.PathTextBox.Size = new System.Drawing.Size(331, 20);
            this.PathTextBox.TabIndex = 0;
            // 
            // ListBoxContextMenuStrip
            // 
            this.ListBoxContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copySelectedLineToolStripMenuItem});
            this.ListBoxContextMenuStrip.Name = "ListBoxContextMenuStrip";
            this.ListBoxContextMenuStrip.Size = new System.Drawing.Size(184, 26);
            // 
            // copySelectedLineToolStripMenuItem
            // 
            this.copySelectedLineToolStripMenuItem.Name = "copySelectedLineToolStripMenuItem";
            this.copySelectedLineToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.copySelectedLineToolStripMenuItem.Text = "Copy selected line(s)";
            this.copySelectedLineToolStripMenuItem.Click += new System.EventHandler(this.copySelectedLineToolStripMenuItem_Click);
            // 
            // O8buildGUIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.LayoutSplitContainer);
            this.Name = "O8buildGUIForm";
            this.Text = "o8buildGUI";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.LayoutSplitContainer.Panel1.ResumeLayout(false);
            this.LayoutSplitContainer.Panel2.ResumeLayout(false);
            this.LayoutSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LayoutSplitContainer)).EndInit();
            this.LayoutSplitContainer.ResumeLayout(false);
            this.ListBoxContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer LayoutSplitContainer;
        private System.Windows.Forms.Button RunButton;
        private System.Windows.Forms.Button LoadDirButton;
        private System.Windows.Forms.Label PathLabel;
        private System.Windows.Forms.TextBox PathTextBox;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ListBox ConsoleOutput;
        private System.Windows.Forms.Label ActionLabel;
        private System.Windows.Forms.ComboBox ActionComboBox;
        private System.Windows.Forms.CheckBox useTesto8buildCheckbox;
        private System.Windows.Forms.ContextMenuStrip ListBoxContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem copySelectedLineToolStripMenuItem;
    }
}

