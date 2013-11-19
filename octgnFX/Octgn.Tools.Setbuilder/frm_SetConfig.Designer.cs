namespace Octgn.Tools.SetBuilder
{
    partial class frm_SetConfig
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
            this.chk_SaveAll = new System.Windows.Forms.CheckBox();
            this.checkList = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // chk_SaveAll
            // 
            this.chk_SaveAll.AutoSize = true;
            this.chk_SaveAll.Location = new System.Drawing.Point(13, 13);
            this.chk_SaveAll.Name = "chk_SaveAll";
            this.chk_SaveAll.Size = new System.Drawing.Size(214, 17);
            this.chk_SaveAll.TabIndex = 0;
            this.chk_SaveAll.Text = "Save All Properties when saving this set";
            this.chk_SaveAll.UseVisualStyleBackColor = true;
            this.chk_SaveAll.CheckedChanged += new System.EventHandler(this.chk_SaveAll_CheckedChanged);
            // 
            // checkList
            // 
            this.checkList.FormattingEnabled = true;
            this.checkList.Location = new System.Drawing.Point(13, 37);
            this.checkList.Name = "checkList";
            this.checkList.Size = new System.Drawing.Size(222, 259);
            this.checkList.TabIndex = 1;
            // 
            // frm_SetConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(247, 306);
            this.Controls.Add(this.checkList);
            this.Controls.Add(this.chk_SaveAll);
            this.MaximizeBox = false;
            this.Name = "frm_SetConfig";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Set Configuration";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frm_SetConfig_FormClosed);
            this.Load += new System.EventHandler(this.frm_SetConfig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chk_SaveAll;
        private System.Windows.Forms.CheckedListBox checkList;
    }
}