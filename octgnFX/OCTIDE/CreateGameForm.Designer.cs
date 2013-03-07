namespace OCTIDE
{
    using System.Windows.Forms;

    partial class CreateGameForm
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
            this.textBoxGameName = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.textBoxLocation = new MetroFramework.Controls.MetroTextBox();
            this.buttonBrowse = new MetroFramework.Controls.MetroButton();
            this.metroButton1 = new MetroFramework.Controls.MetroButton();
            this.labelError = new System.Windows.Forms.Label();
            this.metroButton2 = new MetroFramework.Controls.MetroButton();
            this.SuspendLayout();
            // 
            // textBoxGameName
            // 
            this.textBoxGameName.CustomBackground = false;
            this.textBoxGameName.CustomForeColor = false;
            this.textBoxGameName.FontSize = MetroFramework.MetroTextBoxSize.Small;
            this.textBoxGameName.FontWeight = MetroFramework.MetroTextBoxWeight.Regular;
            this.textBoxGameName.Location = new System.Drawing.Point(121, 102);
            this.textBoxGameName.Multiline = false;
            this.textBoxGameName.Name = "textBoxGameName";
            this.textBoxGameName.SelectedText = "";
            this.textBoxGameName.Size = new System.Drawing.Size(327, 23);
            this.textBoxGameName.Style = MetroFramework.MetroColorStyle.Blue;
            this.textBoxGameName.StyleManager = null;
            this.textBoxGameName.TabIndex = 5;
            this.textBoxGameName.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textBoxGameName.UseStyleColors = false;
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.CustomBackground = false;
            this.metroLabel1.CustomForeColor = false;
            this.metroLabel1.FontSize = MetroFramework.MetroLabelSize.Medium;
            this.metroLabel1.FontWeight = MetroFramework.MetroLabelWeight.Light;
            this.metroLabel1.LabelMode = MetroFramework.Controls.MetroLabelMode.Default;
            this.metroLabel1.Location = new System.Drawing.Point(15, 102);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(84, 19);
            this.metroLabel1.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroLabel1.StyleManager = null;
            this.metroLabel1.TabIndex = 6;
            this.metroLabel1.Text = "Game Name";
            this.metroLabel1.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroLabel1.UseStyleColors = false;
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.CustomBackground = false;
            this.metroLabel2.CustomForeColor = false;
            this.metroLabel2.FontSize = MetroFramework.MetroLabelSize.Medium;
            this.metroLabel2.FontWeight = MetroFramework.MetroLabelWeight.Light;
            this.metroLabel2.LabelMode = MetroFramework.Controls.MetroLabelMode.Default;
            this.metroLabel2.Location = new System.Drawing.Point(15, 140);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(58, 19);
            this.metroLabel2.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroLabel2.StyleManager = null;
            this.metroLabel2.TabIndex = 8;
            this.metroLabel2.Text = "Location";
            this.metroLabel2.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroLabel2.UseStyleColors = false;
            // 
            // textBoxLocation
            // 
            this.textBoxLocation.CustomBackground = false;
            this.textBoxLocation.CustomForeColor = false;
            this.textBoxLocation.FontSize = MetroFramework.MetroTextBoxSize.Small;
            this.textBoxLocation.FontWeight = MetroFramework.MetroTextBoxWeight.Regular;
            this.textBoxLocation.Location = new System.Drawing.Point(121, 140);
            this.textBoxLocation.Multiline = false;
            this.textBoxLocation.Name = "textBoxLocation";
            this.textBoxLocation.SelectedText = "";
            this.textBoxLocation.Size = new System.Drawing.Size(327, 23);
            this.textBoxLocation.Style = MetroFramework.MetroColorStyle.Blue;
            this.textBoxLocation.StyleManager = null;
            this.textBoxLocation.TabIndex = 7;
            this.textBoxLocation.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textBoxLocation.UseStyleColors = false;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Highlight = false;
            this.buttonBrowse.Location = new System.Drawing.Point(466, 140);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(90, 23);
            this.buttonBrowse.Style = MetroFramework.MetroColorStyle.Blue;
            this.buttonBrowse.StyleManager = null;
            this.buttonBrowse.TabIndex = 9;
            this.buttonBrowse.Text = "Browse";
            this.buttonBrowse.Theme = MetroFramework.MetroThemeStyle.Light;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // metroButton1
            // 
            this.metroButton1.Highlight = false;
            this.metroButton1.Location = new System.Drawing.Point(23, 215);
            this.metroButton1.Name = "metroButton1";
            this.metroButton1.Size = new System.Drawing.Size(90, 32);
            this.metroButton1.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroButton1.StyleManager = null;
            this.metroButton1.TabIndex = 10;
            this.metroButton1.Text = "Create";
            this.metroButton1.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroButton1.Click += new System.EventHandler(this.metroButton1_Click);
            // 
            // labelError
            // 
            this.labelError.AutoSize = true;
            this.labelError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.labelError.Location = new System.Drawing.Point(39, 64);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(0, 13);
            this.labelError.TabIndex = 11;
            // 
            // metroButton2
            // 
            this.metroButton2.Highlight = false;
            this.metroButton2.Location = new System.Drawing.Point(466, 215);
            this.metroButton2.Name = "metroButton2";
            this.metroButton2.Size = new System.Drawing.Size(90, 32);
            this.metroButton2.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroButton2.StyleManager = null;
            this.metroButton2.TabIndex = 12;
            this.metroButton2.Text = "Cancel";
            this.metroButton2.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroButton2.Click += new System.EventHandler(this.metroButton2_Click);
            // 
            // CreateGameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 266);
            this.Controls.Add(this.metroButton2);
            this.Controls.Add(this.labelError);
            this.Controls.Add(this.metroButton1);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.metroLabel2);
            this.Controls.Add(this.textBoxLocation);
            this.Controls.Add(this.metroLabel1);
            this.Controls.Add(this.textBoxGameName);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateGameForm";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Text = "Create A Game";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroTextBox textBoxGameName;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroTextBox textBoxLocation;
        private MetroFramework.Controls.MetroButton buttonBrowse;
        private MetroFramework.Controls.MetroButton metroButton1;
        private System.Windows.Forms.Label labelError;
        private MetroFramework.Controls.MetroButton metroButton2;
    }
}