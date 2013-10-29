namespace MiniClient
{
	partial class frmMain
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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.mnuFileConnect = new System.Windows.Forms.MenuItem();
            this.mnuFileAddContact = new System.Windows.Forms.MenuItem();
            this.mnuSendMessage = new System.Windows.Forms.MenuItem();
            this.mnuFileDisconnect = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.mnuFileExit = new System.Windows.Forms.MenuItem();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.ctxMnuRoster = new System.Windows.Forms.ContextMenu();
            this.ctxMnuRosterChat = new System.Windows.Forms.MenuItem();
            this.ctxMnuRosterVcard = new System.Windows.Forms.MenuItem();
            this.ctxMnuRosterDelete = new System.Windows.Forms.MenuItem();
            this.ilsRoster = new System.Windows.Forms.ImageList();
            this.toolBar = new System.Windows.Forms.ToolBar();
            this.ilsToolbar = new System.Windows.Forms.ImageList();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabRoster = new System.Windows.Forms.TabPage();
            this.lvwRoster = new System.Windows.Forms.ListView();
            this.cboStatus = new System.Windows.Forms.ComboBox();
            this.tabDebug = new System.Windows.Forms.TabPage();
            this.txtDebug = new System.Windows.Forms.TextBox();
            this.tabSocket = new System.Windows.Forms.TabPage();
            this.txtSocketDebug = new System.Windows.Forms.TextBox();
            this.ctxMnuRosterMessage = new System.Windows.Forms.MenuItem();
            this.tabControl1.SuspendLayout();
            this.tabRoster.SuspendLayout();
            this.tabDebug.SuspendLayout();
            this.tabSocket.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuItem1);
            // 
            // menuItem1
            // 
            this.menuItem1.MenuItems.Add(this.mnuFileConnect);
            this.menuItem1.MenuItems.Add(this.mnuFileAddContact);
            this.menuItem1.MenuItems.Add(this.mnuSendMessage);
            this.menuItem1.MenuItems.Add(this.mnuFileDisconnect);
            this.menuItem1.MenuItems.Add(this.menuItem3);
            this.menuItem1.MenuItems.Add(this.mnuFileExit);
            this.menuItem1.Text = "File";
            // 
            // mnuFileConnect
            // 
            this.mnuFileConnect.Text = "Connect";
            this.mnuFileConnect.Click += new System.EventHandler(this.mnuFileConnect_Click);
            // 
            // mnuFileAddContact
            // 
            this.mnuFileAddContact.Text = "Add Contact";
            this.mnuFileAddContact.Click += new System.EventHandler(this.mnuFileAddContact_Click);
            // 
            // mnuSendMessage
            // 
            this.mnuSendMessage.Text = "Send Message";
            this.mnuSendMessage.Click += new System.EventHandler(this.mnuSendMessage_Click);
            // 
            // mnuFileDisconnect
            // 
            this.mnuFileDisconnect.Enabled = false;
            this.mnuFileDisconnect.Text = "Disconnect";
            this.mnuFileDisconnect.Click += new System.EventHandler(this.mnuFileDisconnect_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Text = "-";
            // 
            // mnuFileExit
            // 
            this.mnuFileExit.Text = "&Exit";
            this.mnuFileExit.Click += new System.EventHandler(this.mnuFileExit_Click);
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 246);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(240, 22);
            this.statusBar1.Text = "Offline";
            // 
            // ctxMnuRoster
            // 
            this.ctxMnuRoster.MenuItems.Add(this.ctxMnuRosterChat);
            this.ctxMnuRoster.MenuItems.Add(this.ctxMnuRosterMessage);
            this.ctxMnuRoster.MenuItems.Add(this.ctxMnuRosterVcard);
            this.ctxMnuRoster.MenuItems.Add(this.ctxMnuRosterDelete);
            // 
            // ctxMnuRosterChat
            // 
            this.ctxMnuRosterChat.Text = "Chat";
            this.ctxMnuRosterChat.Click += new System.EventHandler(this.ctxMnuRosterChat_Click);
            // 
            // ctxMnuRosterVcard
            // 
            this.ctxMnuRosterVcard.Text = "Vcard";
            this.ctxMnuRosterVcard.Click += new System.EventHandler(this.ctxMnuRosterVcard_Click);
            // 
            // ctxMnuRosterDelete
            // 
            this.ctxMnuRosterDelete.Text = "delete";
            this.ctxMnuRosterDelete.Click += new System.EventHandler(this.ctxMnuRosterDelete_Click);
            // 
            // ilsRoster
            // 
            this.ilsRoster.ImageSize = new System.Drawing.Size(16, 16);
            // 
            // toolBar
            // 
            this.toolBar.Name = "toolBar";
            // 
            // ilsToolbar
            // 
            this.ilsToolbar.ImageSize = new System.Drawing.Size(16, 16);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabRoster);
            this.tabControl1.Controls.Add(this.tabDebug);
            this.tabControl1.Controls.Add(this.tabSocket);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(240, 246);
            this.tabControl1.TabIndex = 4;
            // 
            // tabRoster
            // 
            this.tabRoster.Controls.Add(this.lvwRoster);
            this.tabRoster.Controls.Add(this.cboStatus);
            this.tabRoster.Location = new System.Drawing.Point(0, 0);
            this.tabRoster.Name = "tabRoster";
            this.tabRoster.Size = new System.Drawing.Size(240, 223);
            this.tabRoster.Text = "Roster";
            // 
            // lvwRoster
            // 
            this.lvwRoster.ContextMenu = this.ctxMnuRoster;
            this.lvwRoster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvwRoster.Location = new System.Drawing.Point(0, 22);
            this.lvwRoster.Name = "lvwRoster";
            this.lvwRoster.Size = new System.Drawing.Size(240, 201);
            this.lvwRoster.SmallImageList = this.ilsRoster;
            this.lvwRoster.TabIndex = 1;
            this.lvwRoster.View = System.Windows.Forms.View.Details;
            // 
            // cboStatus
            // 
            this.cboStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboStatus.Location = new System.Drawing.Point(0, 0);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Size = new System.Drawing.Size(240, 22);
            this.cboStatus.TabIndex = 0;
            this.cboStatus.SelectedValueChanged += new System.EventHandler(this.cboStatus_SelectedValueChanged);
            // 
            // tabDebug
            // 
            this.tabDebug.Controls.Add(this.txtDebug);
            this.tabDebug.Location = new System.Drawing.Point(0, 0);
            this.tabDebug.Name = "tabDebug";
            this.tabDebug.Size = new System.Drawing.Size(232, 220);
            this.tabDebug.Text = "Debug";
            // 
            // txtDebug
            // 
            this.txtDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDebug.Location = new System.Drawing.Point(0, 0);
            this.txtDebug.Multiline = true;
            this.txtDebug.Name = "txtDebug";
            this.txtDebug.ReadOnly = true;
            this.txtDebug.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDebug.Size = new System.Drawing.Size(232, 220);
            this.txtDebug.TabIndex = 0;
            // 
            // tabSocket
            // 
            this.tabSocket.Controls.Add(this.txtSocketDebug);
            this.tabSocket.Location = new System.Drawing.Point(0, 0);
            this.tabSocket.Name = "tabSocket";
            this.tabSocket.Size = new System.Drawing.Size(232, 220);
            this.tabSocket.Text = "Socket Debug";
            // 
            // txtSocketDebug
            // 
            this.txtSocketDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSocketDebug.Location = new System.Drawing.Point(0, 0);
            this.txtSocketDebug.Multiline = true;
            this.txtSocketDebug.Name = "txtSocketDebug";
            this.txtSocketDebug.ReadOnly = true;
            this.txtSocketDebug.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtSocketDebug.Size = new System.Drawing.Size(232, 220);
            this.txtSocketDebug.TabIndex = 1;
            // 
            // ctxMnuRosterMessage
            // 
            this.ctxMnuRosterMessage.Text = "Message";
            this.ctxMnuRosterMessage.Click += new System.EventHandler(this.ctxMnuRosterMessage_Click);
            // 
            // frmMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusBar1);
            this.Menu = this.mainMenu1;
            this.Name = "frmMain";
            this.Text = "Mini Client";
            this.tabControl1.ResumeLayout(false);
            this.tabRoster.ResumeLayout(false);
            this.tabDebug.ResumeLayout(false);
            this.tabSocket.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		

		#endregion

        private System.Windows.Forms.MenuItem mnuSendMessage;
        private System.Windows.Forms.MenuItem ctxMnuRosterMessage;

	}
}

