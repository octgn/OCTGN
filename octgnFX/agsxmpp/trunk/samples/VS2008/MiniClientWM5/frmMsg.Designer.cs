namespace MiniClient
{
	partial class frmMsg
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.MainMenu mainMenu1;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.mnuCancel = new System.Windows.Forms.MenuItem();
            this.mnuSend = new System.Windows.Forms.MenuItem();
            this.txtBody = new System.Windows.Forms.TextBox();
            this.txtSubject = new System.Windows.Forms.TextBox();
            this.lblSubject = new System.Windows.Forms.Label();
            this.lblJid = new System.Windows.Forms.Label();
            this.txtJid = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.mnuCancel);
            this.mainMenu1.MenuItems.Add(this.mnuSend);
            // 
            // mnuCancel
            // 
            this.mnuCancel.Text = "Cancel";
            this.mnuCancel.Click += new System.EventHandler(this.mnuCancel_Click);
            // 
            // mnuSend
            // 
            this.mnuSend.Text = "Send";
            this.mnuSend.Click += new System.EventHandler(this.mnuSend_Click);
            // 
            // txtBody
            // 
            this.txtBody.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBody.Location = new System.Drawing.Point(3, 57);
            this.txtBody.Multiline = true;
            this.txtBody.Name = "txtBody";
            this.txtBody.Size = new System.Drawing.Size(228, 208);
            this.txtBody.TabIndex = 4;
            // 
            // txtSubject
            // 
            this.txtSubject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSubject.Location = new System.Drawing.Point(65, 30);
            this.txtSubject.Name = "txtSubject";
            this.txtSubject.Size = new System.Drawing.Size(166, 21);
            this.txtSubject.TabIndex = 7;
            // 
            // lblSubject
            // 
            this.lblSubject.Location = new System.Drawing.Point(3, 30);
            this.lblSubject.Name = "lblSubject";
            this.lblSubject.Size = new System.Drawing.Size(46, 20);
            this.lblSubject.Text = "Subject:";
            // 
            // lblJid
            // 
            this.lblJid.Location = new System.Drawing.Point(3, 3);
            this.lblJid.Name = "lblJid";
            this.lblJid.Size = new System.Drawing.Size(56, 21);
            this.lblJid.Text = "From/To:";
            // 
            // txtJid
            // 
            this.txtJid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtJid.Location = new System.Drawing.Point(65, 3);
            this.txtJid.Name = "txtJid";
            this.txtJid.Size = new System.Drawing.Size(166, 21);
            this.txtJid.TabIndex = 2;
            // 
            // frmMsg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.lblSubject);
            this.Controls.Add(this.txtSubject);
            this.Controls.Add(this.txtBody);
            this.Controls.Add(this.txtJid);
            this.Controls.Add(this.lblJid);
            this.Menu = this.mainMenu1;
            this.Name = "frmMsg";
            this.Text = "Message";
            this.ResumeLayout(false);

		}

		

		#endregion

        private System.Windows.Forms.MenuItem mnuSend;
		private System.Windows.Forms.MenuItem mnuCancel;
		private System.Windows.Forms.TextBox txtBody;
		private System.Windows.Forms.TextBox txtSubject;
		private System.Windows.Forms.Label lblSubject;
        private System.Windows.Forms.Label lblJid;
        private System.Windows.Forms.TextBox txtJid;
	}
}