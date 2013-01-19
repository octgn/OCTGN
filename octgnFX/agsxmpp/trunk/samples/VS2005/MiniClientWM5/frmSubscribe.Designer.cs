namespace MiniClient
{
	partial class frmSubscribe
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblFrom = new System.Windows.Forms.Label();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.mnuRefuse = new System.Windows.Forms.MenuItem();
            this.mnuApprove = new System.Windows.Forms.MenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(8, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 17);
            this.label1.Text = "From:";
            // 
            // lblFrom
            // 
            this.lblFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFrom.Location = new System.Drawing.Point(56, 11);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(169, 17);
            this.lblFrom.Text = "jid";
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.mnuRefuse);
            this.mainMenu1.MenuItems.Add(this.mnuApprove);
            // 
            // mnuRefuse
            // 
            this.mnuRefuse.Text = "Refuse";
            this.mnuRefuse.Click += new System.EventHandler(this.cmdRefuse_Click);
            // 
            // mnuApprove
            // 
            this.mnuApprove.Text = "Approve";
            this.mnuApprove.Click += new System.EventHandler(this.cmdApprove_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(11, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(214, 171);
            this.label2.Text = "You got a subscription request.\r\n\r\nPress Approve to allow this person to add you " +
                "to the contact list.\r\n\r\nPress Refuse to deny the request.\r\n\r\n";
            // 
            // frmSubscribe
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblFrom);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Menu = this.mainMenu1;
            this.MinimizeBox = false;
            this.Name = "frmSubscribe";
            this.Text = "Subscription Request";
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem mnuRefuse;
        private System.Windows.Forms.MenuItem mnuApprove;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblFrom;
	}
}