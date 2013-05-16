namespace MiniClient
{
	partial class frmChat
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
            this.txtSend = new System.Windows.Forms.TextBox();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.txtChat = new System.Windows.Forms.TextBox();
            this.mainMenu2 = new System.Windows.Forms.MainMenu();
            this.mnuSend = new System.Windows.Forms.MenuItem();
            this.mnuClose = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // txtSend
            // 
            this.txtSend.AcceptsReturn = true;
            this.txtSend.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtSend.Location = new System.Drawing.Point(0, 230);
            this.txtSend.Multiline = true;
            this.txtSend.Name = "txtSend";
            this.txtSend.Size = new System.Drawing.Size(240, 38);
            this.txtSend.TabIndex = 8;
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 225);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(240, 5);
            // 
            // txtChat
            // 
            this.txtChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtChat.Location = new System.Drawing.Point(0, 0);
            this.txtChat.Multiline = true;
            this.txtChat.Name = "txtChat";
            this.txtChat.Size = new System.Drawing.Size(240, 225);
            this.txtChat.TabIndex = 10;
            // 
            // mainMenu2
            // 
            this.mainMenu2.MenuItems.Add(this.mnuSend);
            this.mainMenu2.MenuItems.Add(this.mnuClose);
            // 
            // mnuSend
            // 
            this.mnuSend.Text = "Send";
            this.mnuSend.Click += new System.EventHandler(this.mnuSend_Click);
            // 
            // mnuClose
            // 
            this.mnuClose.Text = "Close";
            this.mnuClose.Click += new System.EventHandler(this.mnuClose_Click);
            // 
            // frmChat
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.txtChat);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.txtSend);
            this.Menu = this.mainMenu2;
            this.Name = "frmChat";
            this.Text = "frmChat";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.frmChat_Closing);
            this.ResumeLayout(false);

		}

		void mnuClose_Click(object sender, System.EventArgs e)
		{
			this.Close();
			//throw new System.Exception("The method or operation is not implemented.");
		}

	

		#endregion

		private System.Windows.Forms.MainMenu mainMenu2;
		private System.Windows.Forms.MenuItem mnuSend;
		private System.Windows.Forms.MenuItem mnuClose;
	}
}