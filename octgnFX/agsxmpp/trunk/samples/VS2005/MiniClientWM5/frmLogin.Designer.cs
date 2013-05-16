namespace MiniClient
{
	partial class frmLogin
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
            this.lblJid = new System.Windows.Forms.Label();
            this.txtJid = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.numPriority = new System.Windows.Forms.NumericUpDown();
            this.lblPriority = new System.Windows.Forms.Label();
            this.lblResource = new System.Windows.Forms.Label();
            this.txtResource = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.chkSSL = new System.Windows.Forms.CheckBox();
            this.chkRegister = new System.Windows.Forms.CheckBox();
            this.mainMenu2 = new System.Windows.Forms.MainMenu();
            this.mnuCancel = new System.Windows.Forms.MenuItem();
            this.mnuLogin = new System.Windows.Forms.MenuItem();
            this.PriorityUpDown = new System.Windows.Forms.NumericUpDown();
            this.SuspendLayout();
            // 
            // lblJid
            // 
            this.lblJid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblJid.Location = new System.Drawing.Point(8, 8);
            this.lblJid.Name = "lblJid";
            this.lblJid.Size = new System.Drawing.Size(72, 16);
            this.lblJid.Text = "Jabber ID:";
            // 
            // txtJid
            // 
            this.txtJid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtJid.Location = new System.Drawing.Point(80, 8);
            this.txtJid.Name = "txtJid";
            this.txtJid.Size = new System.Drawing.Size(144, 21);
            this.txtJid.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(80, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(144, 16);
            this.label2.Text = "user@server.org";
            // 
            // txtPassword
            // 
            this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPassword.Location = new System.Drawing.Point(80, 56);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(144, 21);
            this.txtPassword.TabIndex = 1;
            // 
            // lblPassword
            // 
            this.lblPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblPassword.Location = new System.Drawing.Point(8, 56);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(64, 16);
            this.lblPassword.Text = "Password:";
            // 
            // numPriority
            // 
            this.numPriority.Location = new System.Drawing.Point(0, 0);
            this.numPriority.Name = "numPriority";
            this.numPriority.Size = new System.Drawing.Size(100, 22);
            this.numPriority.TabIndex = 0;
            // 
            // lblPriority
            // 
            this.lblPriority.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblPriority.Location = new System.Drawing.Point(8, 88);
            this.lblPriority.Name = "lblPriority";
            this.lblPriority.Size = new System.Drawing.Size(56, 16);
            this.lblPriority.Text = "Priority:";
            // 
            // lblResource
            // 
            this.lblResource.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblResource.Location = new System.Drawing.Point(8, 120);
            this.lblResource.Name = "lblResource";
            this.lblResource.Size = new System.Drawing.Size(64, 16);
            this.lblResource.Text = "Resource:";
            // 
            // txtResource
            // 
            this.txtResource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtResource.Location = new System.Drawing.Point(80, 120);
            this.txtResource.Name = "txtResource";
            this.txtResource.Size = new System.Drawing.Size(144, 21);
            this.txtResource.TabIndex = 4;
            this.txtResource.Text = "MiniClient";
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(138, 88);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 16);
            this.label6.Text = "Port:";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(176, 88);
            this.txtPort.MaxLength = 5;
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(48, 21);
            this.txtPort.TabIndex = 3;
            this.txtPort.Text = "5222";
            // 
            // chkSSL
            // 
            this.chkSSL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chkSSL.Enabled = false;
            this.chkSSL.Location = new System.Drawing.Point(61, 154);
            this.chkSSL.Name = "chkSSL";
            this.chkSSL.Size = new System.Drawing.Size(160, 16);
            this.chkSSL.TabIndex = 5;
            this.chkSSL.Text = "use SSL (old style SSL)";
            // 
            // chkRegister
            // 
            this.chkRegister.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chkRegister.Location = new System.Drawing.Point(61, 178);
            this.chkRegister.Name = "chkRegister";
            this.chkRegister.Size = new System.Drawing.Size(144, 16);
            this.chkRegister.TabIndex = 11;
            this.chkRegister.Text = "register new account";
            // 
            // mainMenu2
            // 
            this.mainMenu2.MenuItems.Add(this.mnuCancel);
            this.mainMenu2.MenuItems.Add(this.mnuLogin);
            // 
            // mnuCancel
            // 
            this.mnuCancel.Text = "Cancel";
            this.mnuCancel.Click += new System.EventHandler(this.mnuCancel_Click);
            // 
            // mnuLogin
            // 
            this.mnuLogin.Text = "Login";
            this.mnuLogin.Click += new System.EventHandler(this.mnuLogin_Click);
            // 
            // PriorityUpDown
            // 
            this.PriorityUpDown.Location = new System.Drawing.Point(80, 87);
            this.PriorityUpDown.Name = "PriorityUpDown";
            this.PriorityUpDown.Size = new System.Drawing.Size(52, 22);
            this.PriorityUpDown.TabIndex = 18;
            this.PriorityUpDown.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // frmLogin
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.PriorityUpDown);
            this.Controls.Add(this.chkRegister);
            this.Controls.Add(this.chkSSL);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtResource);
            this.Controls.Add(this.lblResource);
            this.Controls.Add(this.lblPriority);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtJid);
            this.Controls.Add(this.lblJid);
            this.MaximizeBox = false;
            this.Menu = this.mainMenu2;
            this.Name = "frmLogin";
            this.Text = "Login";
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.MainMenu mainMenu2;
		private System.Windows.Forms.MenuItem mnuCancel;
        private System.Windows.Forms.NumericUpDown PriorityUpDown;
        private System.Windows.Forms.MenuItem mnuLogin;
	}
}