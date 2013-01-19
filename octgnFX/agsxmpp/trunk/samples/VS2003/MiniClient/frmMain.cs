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
using agsXMPP.protocol.client;
using agsXMPP.protocol.extensions.shim;

using agsXMPP.protocol.x;
using agsXMPP.protocol.x.data;

using agsXMPP.Xml;
using agsXMPP.Xml.Dom;

namespace MiniClient
{
	/// <summary>
	/// MainForm
	/// </summary>
	public class frmMain : System.Windows.Forms.Form
	{
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem mnuFileExit;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.MenuItem mnuFileConnect;
		private System.Windows.Forms.MenuItem mnuFileDisconnect;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ImageList ilsRoster;
		private System.Windows.Forms.ContextMenu ctxMnuRoster;
		private System.Windows.Forms.MenuItem ctxMnuRosterChat;
		private System.Windows.Forms.MenuItem ctxMnuRosterVcard;
		private System.Windows.Forms.MenuItem ctxMnuRosterDelete;
		private System.Windows.Forms.ToolBarButton tbbAdd;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabRoster;
		private System.Windows.Forms.ListView lvwRoster;
		private System.Windows.Forms.ComboBox cboStatus;
		private System.Windows.Forms.TabPage tabDebug;
		private System.Windows.Forms.RichTextBox rtfDebug;
		private System.Windows.Forms.ToolBar toolBar;
		private System.Windows.Forms.TabPage tabSocket;
		private System.Windows.Forms.RichTextBox rtfDebugSocket;
		internal System.Windows.Forms.ImageList ils16;
		
		private XmppClientConnection XmppCon;
		
		delegate void OnMessageDelegate	(object sender, agsXMPP.protocol.client.Message msg);
		delegate void OnPresenceDelegate(object sender, Presence pres);

		public frmMain()
		{
			
			InitializeComponent();

					
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
			
			XmppCon.OnReadXml		+= new XmlHandler(XmppCon_OnReadXml);
			XmppCon.OnWriteXml		+= new XmlHandler(XmppCon_OnWriteXml);
			
			XmppCon.OnRosterStart	+= new ObjectHandler(XmppCon_OnRosterStart);
			XmppCon.OnRosterEnd		+= new ObjectHandler(XmppCon_OnRosterEnd);
			XmppCon.OnRosterItem	+= new agsXMPP.XmppClientConnection.RosterHandler(XmppCon_OnRosterItem);

			XmppCon.OnAgentStart	+= new ObjectHandler(XmppCon_OnAgentStart);
			XmppCon.OnAgentEnd		+= new ObjectHandler(XmppCon_OnAgentEnd);
			XmppCon.OnAgentItem		+= new agsXMPP.XmppClientConnection.AgentHandler(XmppCon_OnAgentItem);

			XmppCon.OnLogin			+= new ObjectHandler(XmppCon_OnLogin);
			XmppCon.OnClose			+= new ObjectHandler(XmppCon_OnClose);
			XmppCon.OnError			+= new ErrorHandler(XmppCon_OnError);
			XmppCon.OnPresence		+= new PresenceHandler(XmppCon_OnPresence);
			XmppCon.OnMessage		+= new MessageHandler(XmppCon_OnMessage);
			XmppCon.OnIq			+= new IqHandler(XmppCon_OnIq);
			
			XmppCon.ClientSocket.OnValidateCertificate	+= new agsXMPP.net.BaseSocket.CertificateValidationCallback(ClientSocket_OnValidateCertificate);
			XmppCon.ClientSocket.OnReceive				+= new agsXMPP.net.ClientSocket.OnSocketDataHandler(ClientSocket_OnReceive);
			XmppCon.ClientSocket.OnSend					+= new agsXMPP.net.ClientSocket.OnSocketDataHandler(ClientSocket_OnSend);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.mnuFileConnect = new System.Windows.Forms.MenuItem();
			this.mnuFileDisconnect = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.mnuFileExit = new System.Windows.Forms.MenuItem();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.ctxMnuRoster = new System.Windows.Forms.ContextMenu();
			this.ctxMnuRosterChat = new System.Windows.Forms.MenuItem();
			this.ctxMnuRosterVcard = new System.Windows.Forms.MenuItem();
			this.ctxMnuRosterDelete = new System.Windows.Forms.MenuItem();
			this.ilsRoster = new System.Windows.Forms.ImageList(this.components);
			this.toolBar = new System.Windows.Forms.ToolBar();
			this.tbbAdd = new System.Windows.Forms.ToolBarButton();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabRoster = new System.Windows.Forms.TabPage();
			this.lvwRoster = new System.Windows.Forms.ListView();
			this.cboStatus = new System.Windows.Forms.ComboBox();
			this.tabDebug = new System.Windows.Forms.TabPage();
			this.rtfDebug = new System.Windows.Forms.RichTextBox();
			this.tabSocket = new System.Windows.Forms.TabPage();
			this.rtfDebugSocket = new System.Windows.Forms.RichTextBox();
			this.ils16 = new System.Windows.Forms.ImageList(this.components);
			this.tabControl1.SuspendLayout();
			this.tabRoster.SuspendLayout();
			this.tabDebug.SuspendLayout();
			this.tabSocket.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem1});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.mnuFileConnect,
																					  this.mnuFileDisconnect,
																					  this.menuItem3,
																					  this.mnuFileExit});
			this.menuItem1.Text = "File";
			// 
			// mnuFileConnect
			// 
			this.mnuFileConnect.Index = 0;
			this.mnuFileConnect.Text = "Connect";
			this.mnuFileConnect.Click += new System.EventHandler(this.mnuFileConnect_Click);
			// 
			// mnuFileDisconnect
			// 
			this.mnuFileDisconnect.Enabled = false;
			this.mnuFileDisconnect.Index = 1;
			this.mnuFileDisconnect.Text = "Disconnect";
			this.mnuFileDisconnect.Click += new System.EventHandler(this.mnuFileDisconnect_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 2;
			this.menuItem3.Text = "-";
			// 
			// mnuFileExit
			// 
			this.mnuFileExit.Index = 3;
			this.mnuFileExit.Text = "&Exit";
			this.mnuFileExit.Click += new System.EventHandler(this.mnuFileExit_Click);
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 361);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(280, 24);
			this.statusBar1.TabIndex = 1;
			this.statusBar1.Text = "Offline";
			// 
			// ctxMnuRoster
			// 
			this.ctxMnuRoster.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.ctxMnuRosterChat,
																						 this.ctxMnuRosterVcard,
																						 this.ctxMnuRosterDelete});
			// 
			// ctxMnuRosterChat
			// 
			this.ctxMnuRosterChat.Index = 0;
			this.ctxMnuRosterChat.Text = "Chat";
			this.ctxMnuRosterChat.Click += new System.EventHandler(this.ctxMnuRosterChat_Click);
			// 
			// ctxMnuRosterVcard
			// 
			this.ctxMnuRosterVcard.Index = 1;
			this.ctxMnuRosterVcard.Text = "Vcard";
			this.ctxMnuRosterVcard.Click += new System.EventHandler(this.ctxMnuRosterVcard_Click);
			// 
			// ctxMnuRosterDelete
			// 
			this.ctxMnuRosterDelete.Index = 2;
			this.ctxMnuRosterDelete.Text = "delete";
			this.ctxMnuRosterDelete.Click += new System.EventHandler(this.ctxMnuRosterDelete_Click);
			// 
			// ilsRoster
			// 
			this.ilsRoster.ImageSize = new System.Drawing.Size(16, 16);
			this.ilsRoster.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilsRoster.ImageStream")));
			this.ilsRoster.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// toolBar
			// 
			this.toolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																					   this.tbbAdd});
			this.toolBar.DropDownArrows = true;
			this.toolBar.ImageList = this.ils16;
			this.toolBar.Location = new System.Drawing.Point(0, 0);
			this.toolBar.Name = "toolBar";
			this.toolBar.ShowToolTips = true;
			this.toolBar.Size = new System.Drawing.Size(280, 28);
			this.toolBar.TabIndex = 3;
			this.toolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar_ButtonClick);
			// 
			// tbbAdd
			// 
			this.tbbAdd.ImageIndex = 0;
			this.tbbAdd.ToolTipText = "add contact";
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabRoster);
			this.tabControl1.Controls.Add(this.tabDebug);
			this.tabControl1.Controls.Add(this.tabSocket);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.ImageList = this.ils16;
			this.tabControl1.Location = new System.Drawing.Point(0, 28);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(280, 333);
			this.tabControl1.TabIndex = 4;
			// 
			// tabRoster
			// 
			this.tabRoster.Controls.Add(this.lvwRoster);
			this.tabRoster.Controls.Add(this.cboStatus);
			this.tabRoster.ImageIndex = 1;
			this.tabRoster.Location = new System.Drawing.Point(4, 23);
			this.tabRoster.Name = "tabRoster";
			this.tabRoster.Size = new System.Drawing.Size(272, 306);
			this.tabRoster.TabIndex = 0;
			this.tabRoster.Text = "Roster";
			// 
			// lvwRoster
			// 
			this.lvwRoster.ContextMenu = this.ctxMnuRoster;
			this.lvwRoster.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvwRoster.Location = new System.Drawing.Point(0, 21);
			this.lvwRoster.Name = "lvwRoster";
			this.lvwRoster.Size = new System.Drawing.Size(272, 285);
			this.lvwRoster.SmallImageList = this.ilsRoster;
			this.lvwRoster.TabIndex = 1;
			this.lvwRoster.View = System.Windows.Forms.View.Details;
			// 
			// cboStatus
			// 
			this.cboStatus.Dock = System.Windows.Forms.DockStyle.Top;
			this.cboStatus.Location = new System.Drawing.Point(0, 0);
			this.cboStatus.Name = "cboStatus";
			this.cboStatus.Size = new System.Drawing.Size(272, 21);
			this.cboStatus.TabIndex = 0;
			this.cboStatus.SelectedValueChanged += new System.EventHandler(this.cboStatus_SelectedValueChanged);
			// 
			// tabDebug
			// 
			this.tabDebug.Controls.Add(this.rtfDebug);
			this.tabDebug.ImageIndex = 2;
			this.tabDebug.Location = new System.Drawing.Point(4, 23);
			this.tabDebug.Name = "tabDebug";
			this.tabDebug.Size = new System.Drawing.Size(272, 306);
			this.tabDebug.TabIndex = 1;
			this.tabDebug.Text = "Debug";
			this.tabDebug.Visible = false;
			// 
			// rtfDebug
			// 
			this.rtfDebug.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rtfDebug.Location = new System.Drawing.Point(0, 0);
			this.rtfDebug.Name = "rtfDebug";
			this.rtfDebug.ReadOnly = true;
			this.rtfDebug.Size = new System.Drawing.Size(272, 306);
			this.rtfDebug.TabIndex = 0;
			this.rtfDebug.Text = "";
			// 
			// tabSocket
			// 
			this.tabSocket.Controls.Add(this.rtfDebugSocket);
			this.tabSocket.ImageIndex = 2;
			this.tabSocket.Location = new System.Drawing.Point(4, 23);
			this.tabSocket.Name = "tabSocket";
			this.tabSocket.Size = new System.Drawing.Size(272, 306);
			this.tabSocket.TabIndex = 2;
			this.tabSocket.Text = "Socket Debug";
			// 
			// rtfDebugSocket
			// 
			this.rtfDebugSocket.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rtfDebugSocket.Location = new System.Drawing.Point(0, 0);
			this.rtfDebugSocket.Name = "rtfDebugSocket";
			this.rtfDebugSocket.ReadOnly = true;
			this.rtfDebugSocket.Size = new System.Drawing.Size(272, 306);
			this.rtfDebugSocket.TabIndex = 1;
			this.rtfDebugSocket.Text = "";
			// 
			// ils16
			// 
			this.ils16.ImageSize = new System.Drawing.Size(16, 16);
			this.ils16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ils16.ImageStream")));
			this.ils16.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// frmMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(280, 385);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.toolBar);
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

		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
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
			rtfDebug.SelectionStart		= rtfDebug.Text.Length;
			rtfDebug.SelectionLength	= 0;
			rtfDebug.SelectionColor		= Color.Blue;
			rtfDebug.AppendText("SEND: ");
			rtfDebug.SelectionColor		= Color.Black;
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
			lvwRoster.BeginUpdate();
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
			lvwRoster.EndUpdate();

			// We received the rosterm now its time to send our presence			
//			XmppCon.Show	= ShowType.NONE;
//			XmppCon.Status	= "Online";
//			XmppCon.SendMyPresence();

			cboStatus.SelectedIndex = 5;
			//cboStatus.Text = "online";
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
				ListViewItem li;
				
				li = FindRosterListViewItem(item.Jid);
				if (li == null)
				{
					li = new ListViewItem(item.Name != null ? item.Name : item.Jid.ToString(), 0);
			
					li.Tag = item.Jid.ToString();
					li.SubItems.AddRange(new string[]{"", ""});
					lvwRoster.Items.Add(li);
				}
				else
				{
					li.Text = item.Name != null ? item.Name : item.Jid.ToString();
				}
			}
			else
			{
				ListViewItem li = FindRosterListViewItem(item.Jid);
				if (li!=null)
					li.Remove();
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
				BeginInvoke(new agsXMPP.ObjectHandler(XmppCon_OnLogin), new object[]{sender});
				return;
			}
			mnuFileConnect.Enabled		= false;
			mnuFileDisconnect.Enabled	= true;			
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
				ListViewItem lvi = FindRosterListViewItem(pres.From);
				if (lvi != null)
				{
					int imageIdx = GetRosterImageIndex(pres);
					lvi.ImageIndex = imageIdx;
					lvi.SubItems[1].Text = pres.Status;
					lvi.SubItems[2].Text = pres.From.Resource;
				}
			}

		}

		private void XmppCon_OnIq(object sender, agsXMPP.protocol.client.IQ iq)
		{
			if (InvokeRequired)
			{	
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new IqHandler(XmppCon_OnIq), new object[]{sender, iq});
				return;
			}
			
			Element query = iq.Query;
			
			if (query !=null)
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

						version.Name	= "MiniClient";					
						version.Ver		= "0.5";
						version.Os		= Environment.OSVersion.ToString();
					
						XmppCon.Send(iq);
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
				BeginInvoke(new OnMessageDelegate(XmppCon_OnMessage), new object[]{sender, msg});
				return;
			}

			// check for xData Message
			Element e = msg.SelectSingleElement(typeof(Data));
			if (e != null)
			{	
				Data xdata = e as Data;
				if (xdata.Type == XDataFormType.form)
				{
					frmXData fXData = new frmXData(xdata);
					fXData.Text = "xData Form from " + msg.From.ToString();
					fXData.Show();
				}
			}
			else
			{
				if (!Util.Forms.ContainsKey(msg.From.Bare))
				{
					ListViewItem itm = FindRosterListViewItem(msg.From);
					string nick =  itm == null ? msg.From.Bare : itm.Text;
				
					frmChat f = new frmChat(msg.From, XmppCon, nick);
					f.Show();
					f.IncomingMessage(msg);
				}
			}
		}

		private void XmppCon_OnClose(object sender)
		{
			if (InvokeRequired)
			{	
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new agsXMPP.ObjectHandler(XmppCon_OnClose), new object[]{sender});
				return;
			}
			Console.WriteLine("OnClose");
			mnuFileConnect.Enabled		= true;
			mnuFileDisconnect.Enabled	= false;

			cboStatus.Text = "offline";
			InitRosterListView();
		}
		
		private void XmppCon_OnError(object sender, Exception ex)
		{

		}
		#endregion

		#region << Menu Events >>
		private void mnuFileDisconnect_Click(object sender, System.EventArgs e)
		{
			XmppCon.Close();
		}

		private void mnuFileConnect_Click(object sender, System.EventArgs e)
		{
			frmLogin f = new frmLogin(XmppCon);
			
			if (f.ShowDialog() == DialogResult.OK)
			{
				InitRosterListView();
				XmppCon.Open();
			}
		}

		private void mnuFileExit_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
		#endregion
		
		private void cboStatus_SelectedValueChanged(object sender, System.EventArgs e)
		{
			if (XmppCon != null && XmppCon.Authenticated)
			{
				if (cboStatus.Text == "online")
				{
					XmppCon.Show = ShowType.NONE;
				}
				else
				{
					XmppCon.Show = (ShowType) Enum.Parse(typeof(ShowType), cboStatus.Text);
				}
				XmppCon.SendMyPresence();
			}
		}

		private ListViewItem FindRosterListViewItem(Jid jid)
		{
			foreach (ListViewItem lvi in lvwRoster.Items)
			{
				if( jid.Bare.ToLower() == lvi.Tag.ToString().ToLower())
					return lvi;
			}
			return null;
		}

		/// <summary>
		/// Initialize Roster Listview
		/// </summary>
		private void InitRosterListView()
		{
			lvwRoster.Clear();
			lvwRoster.MultiSelect = false;
			
			lvwRoster.Columns.Add("name", 100, HorizontalAlignment.Left);
			lvwRoster.Columns.Add("status", 75, HorizontalAlignment.Left);			
			lvwRoster.Columns.Add("resource", 75, HorizontalAlignment.Left);
		}

		private ListViewItem GetSelectedListViewItem()
		{
			if (lvwRoster.SelectedItems.Count > 0)
			{
				return lvwRoster.SelectedItems[0];
			}
			return null;
		}

		private int GetRosterImageIndex(Presence pres)
		{			
			if (pres.Type == PresenceType.unavailable)
			{
				return 0;
			}
			else if (pres.Type == PresenceType.error)
			{
				// presence error, we dont care in the miniclient here
			}
			else
			{
				switch (pres.Show)
				{
					case ShowType.NONE:
						return 1;						
					case ShowType.away:
						return 2;						
					case ShowType.chat:
						return 4;						
					case ShowType.xa:
						return 3;						
					case ShowType.dnd:
						return 5;						
				}
			}

			return 0;
		}

		private void ctxMnuRosterVcard_Click(object sender, System.EventArgs e)
		{
			ListViewItem itm = GetSelectedListViewItem();
			if (itm != null)
			{				 
				frmVcard f = new frmVcard( new Jid(itm.Tag.ToString()), XmppCon);
				f.Show();
			}
		}

		private void ctxMnuRosterChat_Click(object sender, System.EventArgs e)
		{
			ListViewItem itm = GetSelectedListViewItem();
			if (itm != null)
			{
				if (!Util.Forms.ContainsKey(itm.Tag))
				{				
					frmChat f = new frmChat(new Jid( (string) itm.Tag ), XmppCon, itm.Text);
					f.Show();					
				}		
			}			
		}

		private void ctxMnuRosterDelete_Click(object sender, System.EventArgs e)
		{
//			<iq from='juliet@example.com/balcony' type='set' id='roster_4'>
//				<query xmlns='jabber:iq:roster'>
//					<item jid='nurse@example.com' subscription='remove'/>
//				</query>
//			</iq>
			ListViewItem itm = GetSelectedListViewItem();
			if (itm != null)
			{
				RosterIq riq = new RosterIq();
				riq.Type = IqType.set;
				
				XmppCon.RosterManager.RemoveRosterItem( new Jid(itm.Tag as string) );				
			}
		}

		private void toolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if (e.Button == tbbAdd)
			{
				frmAddRoster f = new frmAddRoster(XmppCon);
				f.Show();
			}
		}

		private bool ClientSocket_OnValidateCertificate(System.Security.Cryptography.X509Certificates.X509Certificate certificate, int[] certificateErrors)
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
			rtfDebugSocket.SelectionStart		= rtfDebug.Text.Length;
			rtfDebugSocket.SelectionLength	= 0;
			rtfDebugSocket.SelectionColor		= Color.Red;
			rtfDebugSocket.AppendText("RECV: ");
			rtfDebugSocket.SelectionColor		= Color.Black;
			rtfDebugSocket.AppendText(System.Text.Encoding.Default.GetString(data));
			rtfDebugSocket.AppendText("\r\n");
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
			rtfDebugSocket.SelectionStart		= rtfDebug.Text.Length;
			rtfDebugSocket.SelectionLength	= 0;
			rtfDebugSocket.SelectionColor		= Color.Blue;
			rtfDebugSocket.AppendText("SEND: ");
			rtfDebugSocket.SelectionColor		= Color.Black;
			rtfDebugSocket.AppendText(System.Text.Encoding.Default.GetString(data));
			rtfDebugSocket.AppendText("\r\n");
		}

	

		
	}
}
