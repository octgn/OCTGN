namespace MiniClient
{
	partial class frmAddRoster
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
            this.txtNickname = new System.Windows.Forms.TextBox();
            this.txtJid = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.mainMenu2 = new System.Windows.Forms.MainMenu();
            this.mnuAdd = new System.Windows.Forms.MenuItem();
            this.mnuCancel = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // txtNickname
            // 
            this.txtNickname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNickname.Location = new System.Drawing.Point(80, 58);
            this.txtNickname.Name = "txtNickname";
            this.txtNickname.Size = new System.Drawing.Size(157, 21);
            this.txtNickname.TabIndex = 9;
            // 
            // txtJid
            // 
            this.txtJid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtJid.Location = new System.Drawing.Point(80, 15);
            this.txtJid.Name = "txtJid";
            this.txtJid.Size = new System.Drawing.Size(157, 21);
            this.txtJid.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(3, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 16);
            this.label3.Text = "Nickname:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(80, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(160, 16);
            this.label2.Text = "user@server.org";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(3, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 16);
            this.label1.Text = "Jabber ID:";
            // 
            // mainMenu2
            // 
            this.mainMenu2.MenuItems.Add(this.mnuCancel);
            this.mainMenu2.MenuItems.Add(this.mnuAdd);
            // 
            // mnuAdd
            // 
            this.mnuAdd.Text = "Add";
            this.mnuAdd.Click += new System.EventHandler(this.menuItem1_Click);
            // 
            // mnuCancel
            // 
            this.mnuCancel.Text = "Cancel";
            this.mnuCancel.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // frmAddRoster
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.txtNickname);
            this.Controls.Add(this.txtJid);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Menu = this.mainMenu2;
            this.MinimizeBox = false;
            this.Name = "frmAddRoster";
            this.Text = "Add Contact";
            this.ResumeLayout(false);

		}

		void menuItem2_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

	

		#endregion

		private System.Windows.Forms.MainMenu mainMenu2;
		private System.Windows.Forms.MenuItem mnuAdd;
		private System.Windows.Forms.MenuItem mnuCancel;
	}
}