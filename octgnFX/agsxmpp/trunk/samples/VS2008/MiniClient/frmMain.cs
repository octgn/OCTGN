using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using agsXMPP;
using agsXMPP.protocol;
using agsXMPP.protocol.iq;
using agsXMPP.protocol.iq.disco;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.iq.version;
using agsXMPP.protocol.iq.oob;
using agsXMPP.protocol.client;
using agsXMPP.protocol.extensions.shim;
using agsXMPP.protocol.extensions.si;
using agsXMPP.protocol.extensions.bytestreams;

using agsXMPP.protocol.x;
using agsXMPP.protocol.x.data;

using agsXMPP.Xml;
using agsXMPP.Xml.Dom;

using agsXMPP.sasl;

using agsXMPP.ui;
using agsXMPP.ui.roster;

using System.Security.Cryptography;
using System.Text;

using agsXMPP.protocol.stream.feature.compression;

namespace MiniClient
{
	/// <summary>
	/// MainForm
	/// </summary>
	public class frmMain : System.Windows.Forms.Form
    {
        private System.Windows.Forms.StatusBar statusBar1;
        private System.ComponentModel.IContainer components;
        
        
        private ContextMenuStrip contextMenuGC;        
        private ContextMenuStrip contextMenuStripRoster;
        private ToolStripMenuItem chatToolStripMenuItem;
        private ToolStripMenuItem vcardToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem connectToolStripMenuItem;
        private ToolStripMenuItem disconnectToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButtonAdd;
        private ToolStripMenuItem joinToolStripMenuItem;        
        
		private ToolStripMenuItem sendFileToolStripMenuItem;      
       
        private TabPage tabSocket;
        private RichTextBox rtfDebugSocket;
        private TabPage tabDebug;
        private RichTextBox rtfDebug;
        private TabPage tabGC;
        private TreeView treeGC;
        private ToolStrip ToolStripGC;
        private ToolStripButton toolStripButtonFindRooms;
        private ToolStripButton toolStripButtonFindPart;
        private ToolStripButton toolStripButtonSearch;
        private TabPage tabRoster;
        private ComboBox cboStatus;
        private TabControl tabControl1;
        private ImageList ils16;
        

        delegate void OnMessageDelegate(object sender, agsXMPP.protocol.client.Message msg);
		delegate void OnPresenceDelegate(object sender, Presence pres);

        const int IMAGE_PARTICIPANT = 3;
        const int IMAGE_CHATROOM = 4;
        private RosterControl rosterControl;
        const int IMAGE_SERVER      = 5;

        private XmppClientConnection XmppCon;
        //private DiscoHelper discoHelper;
        DiscoManager discoManager;

		public frmMain()
		{			
			InitializeComponent();

			treeGC.ContextMenuStrip = contextMenuGC;
	
			// initialize Combo Status
			cboStatus.Items.AddRange( new object[] {"offline",
													ShowType.away.ToString(),
													ShowType.xa.ToString(),
													ShowType.chat.ToString(),
													ShowType.dnd.ToString(),
													"online" });
			cboStatus.SelectedIndex = 0;

			// initilaize XmppConnection
            XmppCon = new XmppClientConnection();
            
            XmppCon.SocketConnectionType = agsXMPP.net.SocketConnectionType.Direct;

			XmppCon.OnReadXml		    += new XmlHandler(XmppCon_OnReadXml);
			XmppCon.OnWriteXml		    += new XmlHandler(XmppCon_OnWriteXml);
			
			XmppCon.OnRosterStart	    += new ObjectHandler(XmppCon_OnRosterStart);
			XmppCon.OnRosterEnd		    += new ObjectHandler(XmppCon_OnRosterEnd);
			XmppCon.OnRosterItem	    += new agsXMPP.XmppClientConnection.RosterHandler(XmppCon_OnRosterItem);

			XmppCon.OnAgentStart	    += new ObjectHandler(XmppCon_OnAgentStart);
			XmppCon.OnAgentEnd		    += new ObjectHandler(XmppCon_OnAgentEnd);
			XmppCon.OnAgentItem		    += new agsXMPP.XmppClientConnection.AgentHandler(XmppCon_OnAgentItem);

			XmppCon.OnLogin			    += new ObjectHandler(XmppCon_OnLogin);
			XmppCon.OnClose			    += new ObjectHandler(XmppCon_OnClose);
			XmppCon.OnError			    += new ErrorHandler(XmppCon_OnError);
			XmppCon.OnPresence		    += new PresenceHandler(XmppCon_OnPresence);
			XmppCon.OnMessage		    += new MessageHandler(XmppCon_OnMessage);
			XmppCon.OnIq			    += new IqHandler(XmppCon_OnIq);
			XmppCon.OnAuthError		    += new XmppElementHandler(XmppCon_OnAuthError);
            XmppCon.OnSocketError += new ErrorHandler(XmppCon_OnSocketError);

            XmppCon.OnReadSocketData    += new agsXMPP.net.BaseSocket.OnSocketDataHandler(ClientSocket_OnReceive);
            XmppCon.OnWriteSocketData   += new agsXMPP.net.BaseSocket.OnSocketDataHandler(ClientSocket_OnSend);

            XmppCon.ClientSocket.OnValidateCertificate += new System.Net.Security.RemoteCertificateValidationCallback(ClientSocket_OnValidateCertificate);
            
						
			XmppCon.OnXmppConnectionStateChanged		+= new XmppConnectionStateHandler(XmppCon_OnXmppConnectionStateChanged);
            XmppCon.OnSaslStart                         += new SaslEventHandler(XmppCon_OnSaslStart);

            discoManager = new DiscoManager(XmppCon);

            agsXMPP.Factory.ElementFactory.AddElementType("Login", null, typeof(Settings.Login));
            LoadChatServers();            
		}

        private void XmppCon_OnSocketError(object sender, Exception ex)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new ErrorHandler(XmppCon_OnSocketError), new object[] { sender, ex });
                return;
            }

            MessageBox.Show("Socket Error\r\n" + ex.Message + "\r\n" + ex.InnerException);            
        }           

        private void XmppCon_OnSaslStart(object sender, SaslEventArgs args)
        {
            // You can define the SASL mechanism here when needed, or implement your own SASL mechanisms
            // for authentication

            //args.Auto = false;
            //args.Mechanism = agsXMPP.protocol.sasl.Mechanism.GetMechanismName(agsXMPP.protocol.sasl.MechanismType.PLAIN);            
        }

        private void LoadChatServers()
        {
            treeGC.TreeViewNodeSorter = new TreeNodeSorter();
            
            string fileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            fileName += @"\chatservers.xml";

            Document doc = new Document();
            doc.LoadFile(fileName);
                        
            // Get Servers
            ElementList servers = doc.RootElement.SelectElements("Server");
            foreach (Element server in servers)
            {
                TreeNode n = new TreeNode(server.Value);
                n.Tag           = "server";
                n.ImageIndex    = n.SelectedImageIndex = IMAGE_SERVER;

                this.treeGC.Nodes.Add(n);
            }
        }
		/// <summary>
		/// 
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Vom Windows Form-Designer generierter Code
		
		/// <summary>
		/// 
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.contextMenuGC = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.joinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripRoster = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.chatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vcardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonAdd = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSearch = new System.Windows.Forms.ToolStripButton();
            this.tabSocket = new System.Windows.Forms.TabPage();
            this.rtfDebugSocket = new System.Windows.Forms.RichTextBox();
            this.tabDebug = new System.Windows.Forms.TabPage();
            this.rtfDebug = new System.Windows.Forms.RichTextBox();
            this.tabGC = new System.Windows.Forms.TabPage();
            this.treeGC = new System.Windows.Forms.TreeView();
            this.ils16 = new System.Windows.Forms.ImageList(this.components);
            this.ToolStripGC = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonFindRooms = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFindPart = new System.Windows.Forms.ToolStripButton();
            this.tabRoster = new System.Windows.Forms.TabPage();
            this.rosterControl = new agsXMPP.ui.roster.RosterControl();
            this.cboStatus = new System.Windows.Forms.ComboBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.contextMenuGC.SuspendLayout();
            this.contextMenuStripRoster.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabSocket.SuspendLayout();
            this.tabDebug.SuspendLayout();
            this.tabGC.SuspendLayout();
            this.ToolStripGC.SuspendLayout();
            this.tabRoster.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 438);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(347, 24);
            this.statusBar1.TabIndex = 1;
            this.statusBar1.Text = "Offline";
            // 
            // contextMenuGC
            // 
            this.contextMenuGC.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.joinToolStripMenuItem});
            this.contextMenuGC.Name = "contextMenuGC";
            this.contextMenuGC.Size = new System.Drawing.Size(105, 26);
            // 
            // joinToolStripMenuItem
            // 
            this.joinToolStripMenuItem.Name = "joinToolStripMenuItem";
            this.joinToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.joinToolStripMenuItem.Text = "Join";
            this.joinToolStripMenuItem.Click += new System.EventHandler(this.joinToolStripMenuItem_Click);
            // 
            // contextMenuStripRoster
            // 
            this.contextMenuStripRoster.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chatToolStripMenuItem,
            this.vcardToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.sendFileToolStripMenuItem});
            this.contextMenuStripRoster.Name = "contextMenuStripRoster";
            this.contextMenuStripRoster.Size = new System.Drawing.Size(129, 92);
            // 
            // chatToolStripMenuItem
            // 
            this.chatToolStripMenuItem.Image = global::MiniClient.Properties.Resources.comment;
            this.chatToolStripMenuItem.Name = "chatToolStripMenuItem";
            this.chatToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.chatToolStripMenuItem.Text = "chat";
            this.chatToolStripMenuItem.Click += new System.EventHandler(this.chatToolStripMenuItem_Click);
            // 
            // vcardToolStripMenuItem
            // 
            this.vcardToolStripMenuItem.Image = global::MiniClient.Properties.Resources.vcard;
            this.vcardToolStripMenuItem.Name = "vcardToolStripMenuItem";
            this.vcardToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.vcardToolStripMenuItem.Text = "vcard";
            this.vcardToolStripMenuItem.Click += new System.EventHandler(this.vcardToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::MiniClient.Properties.Resources.user_delete;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.deleteToolStripMenuItem.Text = "delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // sendFileToolStripMenuItem
            // 
            this.sendFileToolStripMenuItem.Image = global::MiniClient.Properties.Resources.package;
            this.sendFileToolStripMenuItem.Name = "sendFileToolStripMenuItem";
            this.sendFileToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.sendFileToolStripMenuItem.Text = "Send File";
            this.sendFileToolStripMenuItem.ToolTipText = "Send File to Buddy";
            this.sendFileToolStripMenuItem.Click += new System.EventHandler(this.sendFileToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(347, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.disconnectToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Image = global::MiniClient.Properties.Resources.connect;
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.connectToolStripMenuItem.Text = "Connect";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.connectToolStripMenuItem_Click);
            // 
            // disconnectToolStripMenuItem
            // 
            this.disconnectToolStripMenuItem.Image = global::MiniClient.Properties.Resources.disconnect;
            this.disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            this.disconnectToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.disconnectToolStripMenuItem.Text = "Disconnect";
            this.disconnectToolStripMenuItem.Click += new System.EventHandler(this.disconnectToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(134, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::MiniClient.Properties.Resources.door_in;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonAdd,
            this.toolStripButtonSearch});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(347, 25);
            this.toolStrip1.TabIndex = 8;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonAdd
            // 
            this.toolStripButtonAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonAdd.Image = global::MiniClient.Properties.Resources.user_add;
            this.toolStripButtonAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonAdd.Name = "toolStripButtonAdd";
            this.toolStripButtonAdd.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonAdd.Text = "toolStripButton1";
            this.toolStripButtonAdd.ToolTipText = "Add User";
            this.toolStripButtonAdd.Click += new System.EventHandler(this.toolStripButtonAdd_Click);
            // 
            // toolStripButtonSearch
            // 
            this.toolStripButtonSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSearch.Image = global::MiniClient.Properties.Resources.zoom;
            this.toolStripButtonSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSearch.Name = "toolStripButtonSearch";
            this.toolStripButtonSearch.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonSearch.Text = "toolStripButton1";
            this.toolStripButtonSearch.ToolTipText = "User Search";
            this.toolStripButtonSearch.Click += new System.EventHandler(this.toolStripButtonSearch_Click);
            // 
            // tabSocket
            // 
            this.tabSocket.Controls.Add(this.rtfDebugSocket);
            this.tabSocket.ImageIndex = 0;
            this.tabSocket.Location = new System.Drawing.Point(4, 23);
            this.tabSocket.Name = "tabSocket";
            this.tabSocket.Size = new System.Drawing.Size(339, 362);
            this.tabSocket.TabIndex = 2;
            this.tabSocket.Text = "Socket Debug";
            this.tabSocket.UseVisualStyleBackColor = true;
            // 
            // rtfDebugSocket
            // 
            this.rtfDebugSocket.BackColor = System.Drawing.SystemColors.Window;
            this.rtfDebugSocket.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfDebugSocket.Location = new System.Drawing.Point(0, 0);
            this.rtfDebugSocket.Name = "rtfDebugSocket";
            this.rtfDebugSocket.ReadOnly = true;
            this.rtfDebugSocket.Size = new System.Drawing.Size(339, 362);
            this.rtfDebugSocket.TabIndex = 1;
            this.rtfDebugSocket.Text = "";
            // 
            // tabDebug
            // 
            this.tabDebug.Controls.Add(this.rtfDebug);
            this.tabDebug.ImageIndex = 0;
            this.tabDebug.Location = new System.Drawing.Point(4, 23);
            this.tabDebug.Name = "tabDebug";
            this.tabDebug.Size = new System.Drawing.Size(339, 362);
            this.tabDebug.TabIndex = 1;
            this.tabDebug.Text = "Debug";
            this.tabDebug.UseVisualStyleBackColor = true;
            this.tabDebug.Visible = false;
            // 
            // rtfDebug
            // 
            this.rtfDebug.BackColor = System.Drawing.SystemColors.Window;
            this.rtfDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfDebug.Location = new System.Drawing.Point(0, 0);
            this.rtfDebug.Name = "rtfDebug";
            this.rtfDebug.ReadOnly = true;
            this.rtfDebug.Size = new System.Drawing.Size(339, 362);
            this.rtfDebug.TabIndex = 0;
            this.rtfDebug.Text = "";
            // 
            // tabGC
            // 
            this.tabGC.Controls.Add(this.treeGC);
            this.tabGC.Controls.Add(this.ToolStripGC);
            this.tabGC.ImageIndex = 2;
            this.tabGC.Location = new System.Drawing.Point(4, 23);
            this.tabGC.Name = "tabGC";
            this.tabGC.Padding = new System.Windows.Forms.Padding(3);
            this.tabGC.Size = new System.Drawing.Size(339, 362);
            this.tabGC.TabIndex = 3;
            this.tabGC.Text = "GroupChat";
            this.tabGC.UseVisualStyleBackColor = true;
            // 
            // treeGC
            // 
            this.treeGC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeGC.ImageIndex = 0;
            this.treeGC.ImageList = this.ils16;
            this.treeGC.Location = new System.Drawing.Point(3, 28);
            this.treeGC.Name = "treeGC";
            this.treeGC.SelectedImageIndex = 0;
            this.treeGC.Size = new System.Drawing.Size(333, 331);
            this.treeGC.TabIndex = 20;
            // 
            // ils16
            // 
            this.ils16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ils16.ImageStream")));
            this.ils16.TransparentColor = System.Drawing.Color.Transparent;
            this.ils16.Images.SetKeyName(0, "application_xp_terminal.png");
            this.ils16.Images.SetKeyName(1, "folder_user.png");
            this.ils16.Images.SetKeyName(2, "group.png");
            this.ils16.Images.SetKeyName(3, "user.png");
            this.ils16.Images.SetKeyName(4, "comments.png");
            this.ils16.Images.SetKeyName(5, "server.png");
            // 
            // ToolStripGC
            // 
            this.ToolStripGC.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonFindRooms,
            this.toolStripButtonFindPart});
            this.ToolStripGC.Location = new System.Drawing.Point(3, 3);
            this.ToolStripGC.Name = "ToolStripGC";
            this.ToolStripGC.Size = new System.Drawing.Size(333, 25);
            this.ToolStripGC.TabIndex = 19;
            this.ToolStripGC.Text = "toolStrip2";
            // 
            // toolStripButtonFindRooms
            // 
            this.toolStripButtonFindRooms.Image = global::MiniClient.Properties.Resources.comments;
            this.toolStripButtonFindRooms.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonFindRooms.Name = "toolStripButtonFindRooms";
            this.toolStripButtonFindRooms.Size = new System.Drawing.Size(82, 22);
            this.toolStripButtonFindRooms.Text = "Find Rooms";
            this.toolStripButtonFindRooms.Click += new System.EventHandler(this.toolStripButtonFindRooms_Click);
            // 
            // toolStripButtonFindPart
            // 
            this.toolStripButtonFindPart.Image = global::MiniClient.Properties.Resources.group;
            this.toolStripButtonFindPart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonFindPart.Name = "toolStripButtonFindPart";
            this.toolStripButtonFindPart.Size = new System.Drawing.Size(106, 22);
            this.toolStripButtonFindPart.Text = "Find Participants";
            this.toolStripButtonFindPart.Click += new System.EventHandler(this.toolStripButtonFindPart_Click);
            // 
            // tabRoster
            // 
            this.tabRoster.Controls.Add(this.rosterControl);
            this.tabRoster.Controls.Add(this.cboStatus);
            this.tabRoster.ImageIndex = 3;
            this.tabRoster.Location = new System.Drawing.Point(4, 23);
            this.tabRoster.Name = "tabRoster";
            this.tabRoster.Size = new System.Drawing.Size(339, 362);
            this.tabRoster.TabIndex = 0;
            this.tabRoster.Text = "Roster";
            this.tabRoster.UseVisualStyleBackColor = true;
            // 
            // rosterControl
            // 
            this.rosterControl.ColorGroup = System.Drawing.SystemColors.Highlight;
            this.rosterControl.ColorResource = System.Drawing.SystemColors.ControlText;
            this.rosterControl.ColorRoot = System.Drawing.SystemColors.Highlight;
            this.rosterControl.ColorRoster = System.Drawing.SystemColors.ControlText;
            this.rosterControl.DefaultGroupName = "ungrouped";
            this.rosterControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rosterControl.HideEmptyGroups = true;
            this.rosterControl.Location = new System.Drawing.Point(0, 21);
            this.rosterControl.Name = "rosterControl";
            this.rosterControl.Size = new System.Drawing.Size(339, 341);
            this.rosterControl.TabIndex = 13;
            this.rosterControl.SelectionChanged += new System.EventHandler(this.rosterControl_SelectionChanged);
            // 
            // cboStatus
            // 
            this.cboStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboStatus.Location = new System.Drawing.Point(0, 0);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Size = new System.Drawing.Size(339, 21);
            this.cboStatus.TabIndex = 10;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabRoster);
            this.tabControl1.Controls.Add(this.tabGC);
            this.tabControl1.Controls.Add(this.tabDebug);
            this.tabControl1.Controls.Add(this.tabSocket);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.ImageList = this.ils16;
            this.tabControl1.Location = new System.Drawing.Point(0, 49);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(347, 389);
            this.tabControl1.TabIndex = 9;
            // 
            // frmMain
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(347, 462);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusBar1);
            this.Name = "frmMain";
            this.Text = "Mini Client";
            this.contextMenuGC.ResumeLayout(false);
            this.contextMenuStripRoster.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabSocket.ResumeLayout(false);
            this.tabDebug.ResumeLayout(false);
            this.tabGC.ResumeLayout(false);
            this.tabGC.PerformLayout();
            this.ToolStripGC.ResumeLayout(false);
            this.ToolStripGC.PerformLayout();
            this.tabRoster.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.EnableVisualStyles();
			Application.DoEvents();
			Application.Run(new frmMain());
		}

		#region << XmppConnection events >>
		private void XmppCon_OnReadXml(object sender, string xml)
		{			
			if (InvokeRequired)
			{	
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new XmlHandler(XmppCon_OnReadXml), new object[]{sender, xml});
				return;
			}
			rtfDebug.SelectionStart		= rtfDebug.Text.Length;
			rtfDebug.SelectionLength	= 0;
			rtfDebug.SelectionColor		= Color.Red;
			rtfDebug.AppendText("RECV: ");
			rtfDebug.SelectionColor		= Color.Black;
			rtfDebug.AppendText(xml);
			rtfDebug.AppendText("\r\n"); 
		}

		private void XmppCon_OnWriteXml(object sender, string xml)
		{
			if (InvokeRequired)
			{	
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new XmlHandler(XmppCon_OnWriteXml), new object[]{sender, xml});
				return;
			}
            rtfDebug.SelectionStart = rtfDebug.Text.Length;
            rtfDebug.SelectionLength = 0;
            rtfDebug.SelectionColor = Color.Blue;
            rtfDebug.AppendText("SEND: ");
            rtfDebug.SelectionColor = Color.Black;
            rtfDebug.AppendText(xml);
            rtfDebug.AppendText("\r\n");
		}

		private void XmppCon_OnRosterStart(object sender)
		{
			if (InvokeRequired)
			{	
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new ObjectHandler(XmppCon_OnRosterStart), new object[]{this});
				return;
			}
			// Disable redraw for faster updating
			rosterControl.BeginUpdate();
		}
		
		private void XmppCon_OnRosterEnd(object sender)
		{
			if (InvokeRequired)
			{	
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new ObjectHandler(XmppCon_OnRosterEnd), new object[]{this});
				return;
			}
			// enable redraw again
            rosterControl.EndUpdate();
            rosterControl.ExpandAll();

            
            //// Send our Online Presence now, this is done in the cboStatus SelectionChanges event
            //// after the next line
            //cboStatus.SelectedIndex = 5;
			// since 0.97 we don't need this anymore ==> AutoPresence property
            
            cboStatus.Text = "online";
            this.cboStatus.SelectedValueChanged += new System.EventHandler(this.cboStatus_SelectedValueChanged);
		}
		
		private void XmppCon_OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item)
		{
			if (InvokeRequired)
			{	
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new agsXMPP.XmppClientConnection.RosterHandler(XmppCon_OnRosterItem), new object[]{this, item});
				return;
			}

			if (item.Subscription != SubscriptionType.remove)
			{
                rosterControl.AddRosterItem(item);
			}
			else
			{                
                rosterControl.RemoveRosterItem(item);
			}			
		}
		
		private void XmppCon_OnAgentStart(object sender)
		{

		}

		private void XmppCon_OnAgentEnd(object sender)
		{

		}

		private void XmppCon_OnAgentItem(object sender, agsXMPP.protocol.iq.agent.Agent agent)
		{

		}

		private void XmppCon_OnLogin(object sender)
		{
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new ObjectHandler(XmppCon_OnLogin), new object[] { sender});
                return;
            }
			connectToolStripMenuItem.Enabled	= false;
			disconnectToolStripMenuItem.Enabled	= true;
            statusBar1.Text = "Online";

            DiscoServer();
		}

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void XmppCon_OnAuthError(object sender, Element e)
		{
			if (InvokeRequired)
			{	
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new XmppElementHandler(XmppCon_OnAuthError), new object[]{sender, e});
				return;
			}
			
			if (XmppCon.XmppConnectionState != XmppConnectionState.Disconnected)
                XmppCon.Close();

			MessageBox.Show("Authentication Error!\r\nWrong password or username.", 
				"Error", 
				MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation,
				MessageBoxDefaultButton.Button1);
            
		}
        
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="pres"></param>
		private void XmppCon_OnPresence(object sender, Presence pres)
		{
			if (InvokeRequired)
			{	
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new OnPresenceDelegate(XmppCon_OnPresence), new object[]{sender, pres});
				return;
			}

			if (pres.Type == PresenceType.subscribe)
			{
				frmSubscribe f = new frmSubscribe(XmppCon, pres.From);
				f.Show();
			}
			else if(pres.Type == PresenceType.subscribed)
			{

			}
			else if(pres.Type == PresenceType.unsubscribe)
			{

			}
			else if(pres.Type == PresenceType.unsubscribed)
			{

			}
			else
			{
                try
                {
                    rosterControl.SetPresence(pres);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
			}

		}

        private void XmppCon_OnIq(object sender, agsXMPP.protocol.client.IQ iq)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new IqHandler(XmppCon_OnIq), new object[] { sender, iq });
                return;
            }
                       

            if (iq != null)
            {
                // No Iq with query
                if (iq.HasTag(typeof(agsXMPP.protocol.extensions.si.SI)))
                {
                    if (iq.Type == IqType.set)
                    {
                        agsXMPP.protocol.extensions.si.SI si = iq.SelectSingleElement(typeof(agsXMPP.protocol.extensions.si.SI)) as agsXMPP.protocol.extensions.si.SI;

                        agsXMPP.protocol.extensions.filetransfer.File file = si.File;
                        if (file != null)
                        {
                            // somebody wants to send a file to us
                            Console.WriteLine(file.Size.ToString());
                            Console.WriteLine(file.Name);
                            frmFileTransfer frmFile = new frmFileTransfer(XmppCon, iq);
                            frmFile.Show();
                        }
                    }
                }                
                else
                {
                    Element query = iq.Query;

                    if (query != null)
                    {
                        if (query.GetType() == typeof(agsXMPP.protocol.iq.version.Version))
                        {
                            // its a version IQ VersionIQ
                            agsXMPP.protocol.iq.version.Version version = query as agsXMPP.protocol.iq.version.Version;
                            if (iq.Type == IqType.get)
                            {
                                // Somebody wants to know our client version, so send it back
                                iq.SwitchDirection();
                                iq.Type = IqType.result;

                                version.Name = "MiniClient";
                                version.Ver = "0.5";
                                version.Os = Environment.OSVersion.ToString();

                                XmppCon.Send(iq);
                            }
                        }                        
                    }
                }
            }
        }

        /// <summary>
		/// We received a message
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="msg"></param>
		private void XmppCon_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
		{
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new OnMessageDelegate(XmppCon_OnMessage), new object[] { sender, msg });
                return;
            }

            // Dont handle GroupChat Messages here, they have their own callbacks in the
            // GroupChat Form
            if (msg.Type == MessageType.groupchat)
                return;

            if (msg.Type == MessageType.error)
            {
                //Handle errors here
                // we dont handle them in this example
                return;
            }			

			// check for xData Message
			
			if (msg.HasTag(typeof(Data)))
			{	
                Element e = msg.SelectSingleElement(typeof(Data));                
				Data xdata = e as Data;
				if (xdata.Type == XDataFormType.form)
				{
					frmXData fXData = new frmXData(xdata);
					fXData.Text = "xData Form from " + msg.From.ToString();
					fXData.Show();
				}
			}
            else if(msg.HasTag(typeof(agsXMPP.protocol.extensions.ibb.Data)))
            {
                // ignore IBB messages
                return;
            }
			else
			{
                if (msg.Body != null)
                {
                    if (!Util.ChatForms.ContainsKey(msg.From.Bare))
                    {
                        RosterNode rn = rosterControl.GetRosterItem(msg.From);
                        string nick = msg.From.Bare;
                        if (rn != null)
                            nick = rn.Text;

                        frmChat f = new frmChat(msg.From, XmppCon, nick);
                        f.Show();
                        f.IncomingMessage(msg);
                    }
                }
			}
		}

		private void XmppCon_OnClose(object sender)
		{
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new ObjectHandler(XmppCon_OnClose), new object[] {sender});
                return;
            }
            			
			connectToolStripMenuItem.Enabled	= true;
			disconnectToolStripMenuItem.Enabled	= false;
            cboStatus.SelectedValueChanged -= new System.EventHandler(this.cboStatus_SelectedValueChanged);

			cboStatus.Text = "offline";
            statusBar1.Text = "OffLine";
            rosterControl.Clear();

		}
		
		private void XmppCon_OnError(object sender, Exception ex)
		{

		}
		#endregion

        private bool ClientSocket_OnValidateCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
			return true;
		}

		private void ClientSocket_OnReceive(object sender, byte[] data, int count)
		{
			if (InvokeRequired)
			{	
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new agsXMPP.net.ClientSocket.OnSocketDataHandler(ClientSocket_OnReceive), new object[]{sender, data, count});
				return;
			}

            try
            {
                rtfDebugSocket.SelectionStart = rtfDebug.Text.Length;
                rtfDebugSocket.SelectionLength = 0;
                rtfDebugSocket.SelectionColor = Color.Red;
                rtfDebugSocket.AppendText("RECV: ");

                rtfDebugSocket.SelectionStart = rtfDebug.Text.Length;
                rtfDebugSocket.SelectionLength = 0;
                rtfDebugSocket.SelectionColor = Color.Black;
                rtfDebugSocket.AppendText(System.Text.Encoding.Default.GetString(data, 0, count));
                rtfDebugSocket.AppendText("\r\n\r\n");
            }
            catch { }
		}

		private void ClientSocket_OnSend(object sender, byte[] data, int count)
		{
			if (InvokeRequired)
			{	
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new agsXMPP.net.ClientSocket.OnSocketDataHandler(ClientSocket_OnSend), new object[]{sender, data, count});
				return;
			}

            try
            {
                rtfDebugSocket.SelectionStart = rtfDebug.Text.Length;
                rtfDebugSocket.SelectionLength = 0;
                rtfDebugSocket.SelectionColor = Color.Blue;
                rtfDebugSocket.AppendText("SEND: ");

                rtfDebugSocket.SelectionStart = rtfDebug.Text.Length;
                rtfDebugSocket.SelectionLength = 0;
                rtfDebugSocket.SelectionColor = Color.Black;
                rtfDebugSocket.AppendText(System.Text.Encoding.Default.GetString(data, 0, count));
                rtfDebugSocket.AppendText("\r\n\r\n");
            }
            catch { }
		}
        
		private void XmppCon_OnXmppConnectionStateChanged(object sender, XmppConnectionState state)
		{
			Console.WriteLine("OnXmppConnectionStateChanged: " + state.ToString());
		}
		
		private void OnBrowseIQ(object sender, IQ iq, object data)
		{			
			Element s = iq.SelectSingleElement(typeof(agsXMPP.protocol.iq.browse.Service));
			if (s!=null)
			{
				agsXMPP.protocol.iq.browse.Service service = s as agsXMPP.protocol.iq.browse.Service;
				string[] ns = service.GetNamespaces();
			}			
		}

        #region << RequestDiscover >>
        public void RequestDiscovery()
        {
            //DiscoItemsIq discoIq = new DiscoItemsIq(IqType.get);
            ////TreeNode node = treeGC.SelectedNode;
            ////discoIq.To = new Jid(this.XmppCon.Server);
            //discoIq.To = new Jid("amessage.info");
            //this.XmppCon.IqGrabber.SendIq(discoIq, new IqCB(OnGetDiscovery), null);
        }

        /// <summary>
        /// Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="iq"></param>
        /// <param name="data"></param>
        private void OnGetDiscovery(object sender, IQ iq, object data)
        {
            DiscoItems items = iq.Query as DiscoItems;
            if (items == null)
                return;

            DiscoItem[] rooms = items.GetDiscoItems();
            foreach (DiscoItem item in rooms)
            {
                Console.WriteLine(item.Name);
            }
        }
        #endregion

        #region << lookup chatrooms on a chatserver using service discovery >>
        /// <summary>
        /// Discover chatromms of a chat server using disco (service discovery)
        /// </summary>
        private void FindChatRooms()
        {
            TreeNode node = treeGC.SelectedNode;
            if (node == null || node.Level != 0)
                return;

            DiscoItemsIq discoIq = new DiscoItemsIq(IqType.get);
            discoIq.To = new Jid(node.Text);
            this.XmppCon.IqGrabber.SendIq(discoIq, new IqCB(OnGetChatRooms), node);
        }

        /// <summary>
        /// Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="iq"></param>
        /// <param name="data"></param>
        private void OnGetChatRooms(object sender, IQ iq, object data)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new IqCB(OnGetChatRooms), new object[] { sender, iq, data });
                return;
            }

            TreeNode node = data as TreeNode;
            node.Nodes.Clear();

            DiscoItems items = iq.Query as DiscoItems;
            if (items == null)
                return;

            DiscoItem[] rooms = items.GetDiscoItems();
            foreach (DiscoItem item in rooms)
            {
                TreeNode n = new TreeNode(item.Name);
                n.Tag           = item.Jid.ToString();
                n.ImageIndex    = n.SelectedImageIndex  = IMAGE_CHATROOM;
                node.Nodes.Add(n);
            }
        }

        private void FindParticipants()
        {
            TreeNode node = treeGC.SelectedNode;
            if (node == null && node.Level != 1)
                return;

            DiscoItemsIq discoIq = new DiscoItemsIq(IqType.get);
            discoIq.To = new Jid((string) node.Tag);
            this.XmppCon.IqGrabber.SendIq(discoIq, new IqCB(OnGetParticipants), node);
        }

        private void OnGetParticipants(object sender, IQ iq, object data)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new IqCB(OnGetParticipants), new object[] { sender, iq, data });
                return;
            }

            TreeNode node = data as TreeNode;
            node.Nodes.Clear();

            DiscoItems items = iq.Query as DiscoItems;
            if (items == null)
                return;

            DiscoItem[] rooms = items.GetDiscoItems();
            foreach (DiscoItem item in rooms)
            {
                TreeNode n = new TreeNode(item.Jid.Resource);
                n.Tag           = item.Jid.ToString();
                n.ImageIndex    = n.SelectedImageIndex  = IMAGE_PARTICIPANT;
                node.Nodes.Add(n);
            }
        }
        #endregion

        private void chatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RosterNode node = rosterControl.SelectedItem();
            if (node != null)
            {
                if (!Util.ChatForms.ContainsKey(node.RosterItem.Jid.ToString()))
                {
                    frmChat f = new frmChat(node.RosterItem.Jid, XmppCon, node.Text);
                    f.Show();
                }
            }			
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RosterNode node = rosterControl.SelectedItem();
            if (node != null)
            {
                RosterIq riq = new RosterIq();
                riq.Type = IqType.set;

                XmppCon.RosterManager.RemoveRosterItem(node.RosterItem.Jid);
            }
        }

        private void sendFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RosterNode node = rosterControl.SelectedItem();

            if (node != null)
            {
                if (node.Nodes.Count > 0)
                {
                    Jid jid = node.RosterItem.Jid;
                    jid.Resource = node.FirstNode.Text;
                    frmFileTransfer ft = new frmFileTransfer(XmppCon, jid);
                    ft.Show();
                }               
            }
        }

        private void vcardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RosterNode node = rosterControl.SelectedItem();
            if (node != null)
            {
                frmVcard f = new frmVcard(node.RosterItem.Jid, XmppCon);
                f.Show();
            }
        }

        private void rosterControl_SelectionChanged(object sender, EventArgs e)
        {
            RosterNode node = rosterControl.SelectedItem();
            if (node != null)
            {
                if (node.NodeType == RosterNodeType.RosterNode)
                    rosterControl.ContextMenuStrip = this.contextMenuStripRoster;
                else if (node.NodeType == RosterNodeType.GroupNode)
                    rosterControl.ContextMenuStrip = null;    // Add Group context menu here
                else if (node.NodeType == RosterNodeType.RootNode)
                    rosterControl.ContextMenuStrip = null;    // Add RootNode context menu here
                else if (node.NodeType == RosterNodeType.ResourceNode)
                    rosterControl.ContextMenuStrip = null;    // Add Resource Context Menu here
            }
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmLogin f = new frmLogin(XmppCon);

            if (f.ShowDialog() == DialogResult.OK)
            {               
                XmppCon.Open();
                // for compression debug statistics
                // XmppCon.ClientSocket.OnIncomingCompressionDebug += new agsXMPP.net.BaseSocket.OnSocketCompressionDebugHandler(ClientSocket_OnIncomingCompressionDebug);
                // XmppCon.ClientSocket.OnOutgoingCompressionDebug += new agsXMPP.net.BaseSocket.OnSocketCompressionDebugHandler(ClientSocket_OnOutgoingCompressionDebug);
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XmppCon.Close();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolStripButtonAdd_Click(object sender, EventArgs e)
        {
            frmAddRoster f = new frmAddRoster(XmppCon);
            f.Show();    
        }

        private void cboStatus_SelectedValueChanged(object sender, EventArgs e)
        {
            if (XmppCon != null && XmppCon.Authenticated)
			{
				if (cboStatus.Text == "online")
				{
					XmppCon.Show = ShowType.NONE;
				}
                else if (cboStatus.Text == "offline")
                {
                    XmppCon.Close(); 
                }
                else
                {
                    XmppCon.Show = (ShowType)Enum.Parse(typeof(ShowType), cboStatus.Text);
                }
				XmppCon.SendMyPresence();
			}		
        }       

        private void joinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Join a ChatRoom
            if (XmppCon.XmppConnectionState == XmppConnectionState.Disconnected)
                return;

            TreeNode node = this.treeGC.SelectedNode;
            if (node != null && node.Level == 1)
            {
                // Ask for the Nickname for this GroupChat
                frmInputBox input = new frmInputBox("Enter your Nickname for the chatroom", "Nickname", "Nickname");
                if (input.ShowDialog() == DialogResult.OK)
                {
                    Jid jid = new Jid((string)node.Tag);
                    string nickname = input.Result;
                    frmGroupChat gc = new frmGroupChat(this.XmppCon, jid, nickname);
                    gc.Show();
                }
            }
        }             

        private void toolStripButtonFindRooms_Click(object sender, EventArgs e)
        {
            if (XmppCon.XmppConnectionState == XmppConnectionState.Disconnected)
                return;

            FindChatRooms(); 
        }

        private void toolStripButtonFindPart_Click(object sender, EventArgs e)
        {
            if (XmppCon.XmppConnectionState == XmppConnectionState.Disconnected)
                return;

            FindParticipants();
        }

        #region << Disco Server >>
        // Sending Disco request to the server we are connected to for discovering
        // the services runing on our server
        private void DiscoServer()
        {           
            discoManager.DiscoverItems(new Jid(XmppCon.Server), new IqCB(OnDiscoServerResult), null);            
        }

        /// <summary>
        /// Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="iq"></param>
        /// <param name="data"></param>
        private void OnDiscoServerResult(object sender, IQ iq, object data)
        {
            if (iq.Type == IqType.result)
            {
                Element query = iq.Query;
                if (query != null && query.GetType() == typeof(DiscoItems))
                {
                    DiscoItems items = query as DiscoItems;
                    DiscoItem[] itms = items.GetDiscoItems();
                    
                    foreach (DiscoItem itm in itms)
                    {
                        if (itm.Jid != null)
                            discoManager.DiscoverInformation(itm.Jid, new IqCB(OnDiscoInfoResult), itm);
                    }
                }
            }
        }

        private void OnDiscoInfoResult(object sender, IQ iq, object data)
        {
            // <iq from='proxy.cachet.myjabber.net' to='gnauck@jabber.org/Exodus' type='result' id='jcl_19'>
            //  <query xmlns='http://jabber.org/protocol/disco#info'>
            //      <identity category='proxy' name='SOCKS5 Bytestreams Service' type='bytestreams'/>
            //      <feature var='http://jabber.org/protocol/bytestreams'/>
            //      <feature var='http://jabber.org/protocol/disco#info'/>
            //  </query>
            // </iq>
            if (iq.Type == IqType.result)
            {
                if (iq.Query is DiscoInfo)
                {
                    DiscoInfo di = iq.Query as DiscoInfo;
                    if (di.HasFeature(agsXMPP.Uri.IQ_SEARCH))
                    {
                        Jid jid = iq.From;
                        if (!Util.Services.Search.Contains(jid))
                            Util.Services.Search.Add(jid);
                    }
                    else if (di.HasFeature(agsXMPP.Uri.BYTESTREAMS))
                    {
                        Jid jid = iq.From;
                        if (!Util.Services.Proxy.Contains(jid))
                            Util.Services.Proxy.Add(jid);
                    }                    
                }
            }
        }
        #endregion

        private void toolStripButtonSearch_Click(object sender, EventArgs e)
        {
            frmSearch fSearch = new frmSearch(this.XmppCon);
            fSearch.Show();
        }

       
    }
}