namespace Octgn.Tools.SetBuilder
{
    partial class frm_ItemEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_ItemEditor));
            this.cmb_PropertyName = new System.Windows.Forms.ComboBox();
            this.txt_Field2 = new System.Windows.Forms.TextBox();
            this.btn_Accept = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.txt_Field3 = new System.Windows.Forms.TextBox();
            this.txt_Field1 = new System.Windows.Forms.TextBox();
            this.btn_GenerateGuid = new System.Windows.Forms.Button();
            this.txt_Field4 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // cmb_PropertyName
            // 
            this.cmb_PropertyName.FormattingEnabled = true;
            this.cmb_PropertyName.Location = new System.Drawing.Point(12, 11);
            this.cmb_PropertyName.Name = "cmb_PropertyName";
            this.cmb_PropertyName.Size = new System.Drawing.Size(156, 21);
            this.cmb_PropertyName.TabIndex = 1;
            // 
            // txt_Field2
            // 
            this.txt_Field2.Location = new System.Drawing.Point(12, 38);
            this.txt_Field2.Name = "txt_Field2";
            this.txt_Field2.Size = new System.Drawing.Size(156, 20);
            this.txt_Field2.TabIndex = 2;
            // 
            // btn_Accept
            // 
            this.btn_Accept.Location = new System.Drawing.Point(12, 115);
            this.btn_Accept.Name = "btn_Accept";
            this.btn_Accept.Size = new System.Drawing.Size(75, 23);
            this.btn_Accept.TabIndex = 6;
            this.btn_Accept.Text = "Ok";
            this.btn_Accept.UseVisualStyleBackColor = true;
            this.btn_Accept.Click += new System.EventHandler(this.btn_Accept_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(93, 115);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 7;
            this.btn_Cancel.Text = "Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // txt_Field3
            // 
            this.txt_Field3.Location = new System.Drawing.Point(12, 64);
            this.txt_Field3.Name = "txt_Field3";
            this.txt_Field3.Size = new System.Drawing.Size(156, 20);
            this.txt_Field3.TabIndex = 4;
            // 
            // txt_Field1
            // 
            this.txt_Field1.Location = new System.Drawing.Point(12, 12);
            this.txt_Field1.Name = "txt_Field1";
            this.txt_Field1.Size = new System.Drawing.Size(156, 20);
            this.txt_Field1.TabIndex = 0;
            // 
            // btn_GenerateGuid
            // 
            this.btn_GenerateGuid.Image = ((System.Drawing.Image)(resources.GetObject("btn_GenerateGuid.Image")));
            this.btn_GenerateGuid.Location = new System.Drawing.Point(174, 38);
            this.btn_GenerateGuid.Name = "btn_GenerateGuid";
            this.btn_GenerateGuid.Size = new System.Drawing.Size(20, 20);
            this.btn_GenerateGuid.TabIndex = 7;
            this.btn_GenerateGuid.UseVisualStyleBackColor = true;
            this.btn_GenerateGuid.Click += new System.EventHandler(this.btn_GenerateGuid_Click);
            // 
            // txt_Field4
            // 
            this.txt_Field4.Location = new System.Drawing.Point(12, 89);
            this.txt_Field4.Name = "txt_Field4";
            this.txt_Field4.Size = new System.Drawing.Size(156, 20);
            this.txt_Field4.TabIndex = 5;
            // 
            // frm_ItemEditor
            // 
            this.AcceptButton = this.btn_Accept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(194, 148);
            this.ControlBox = false;
            this.Controls.Add(this.txt_Field4);
            this.Controls.Add(this.btn_GenerateGuid);
            this.Controls.Add(this.txt_Field1);
            this.Controls.Add(this.txt_Field3);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Accept);
            this.Controls.Add(this.txt_Field2);
            this.Controls.Add(this.cmb_PropertyName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.Name = "frm_ItemEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frm_ItemEditor_FormClosed);
            this.Load += new System.EventHandler(this.frm_ItemEditor_Load);
            this.Shown += new System.EventHandler(this.frm_ItemEditor_VisibleChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frm_ItemEditor_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmb_PropertyName;
        private System.Windows.Forms.Button btn_Accept;
        private System.Windows.Forms.Button btn_Cancel;
        public System.Windows.Forms.TextBox txt_Field2;
        public System.Windows.Forms.TextBox txt_Field3;
        public System.Windows.Forms.TextBox txt_Field1;
        private System.Windows.Forms.Button btn_GenerateGuid;
        public System.Windows.Forms.TextBox txt_Field4;

    }
}