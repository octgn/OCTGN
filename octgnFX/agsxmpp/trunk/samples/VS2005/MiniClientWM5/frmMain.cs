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
	public partial class frmMain : System.Windows.Forms.Form
	{
	//	private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem mnuFileExit;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.MenuItem mnuFileConnect;
		private System.Windows.Forms.MenuItem mnuFileAddContact;
		private System.Windows.Forms.MenuItem mnuFileDisconnect;
		private System.Windows.Forms.MenuItem menuItem3;
		//private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ImageList ilsRoster;
		private System.Windows.Forms.ContextMenu ctxMnuRoster;
		private System.Windows.Forms.MenuItem ctxMnuRosterChat;
		private System.Windows.Forms.MenuItem ctxMnuRosterVcard;
		private System.Windows.Forms.MenuItem ctxMnuRosterDelete;
		private System.Windows.Forms.ImageList ilsToolbar;
		//		private System.Windows.Forms.ToolBarButton tbbAdd;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabRoster;
		private System.Windows.Forms.ListView lvwRoster;
		private System.Windows.Forms.ComboBox cboStatus;
		private System.Windows.Forms.TabPage tabDebug;
		private System.Windows.Forms.TextBox txtDebug;
		private System.Windows.Forms.ToolBar toolBar;
		private System.Windows.Forms.TabPage tabSocket;
		private System.Windows.Forms.TextBox txtSocketDebug;

		private XmppClientConnection XmppCon;

		delegate void OnMessageDelegate(object sender, agsXMPP.protocol.client.Message msg);
		delegate void OnPresenceDelegate(object sender, Presence pres);
		delegate void OnMessageDelegate1(agsXMPP.protocol.client.Message msg);
		

		public frmMain()
		{

			InitializeComponent();

			
			// initialize Combo Status			
            InitStatusCombo();

			// initilaize XmppConnection and setup all event handlers
			XmppCon = new XmppClientConnection();

			XmppCon.OnReadXml += new XmlHandler(XmppCon_OnReadXml);
			XmppCon.OnWriteXml += new XmlHandler(XmppCon_OnWriteXml);

			XmppCon.OnRosterStart += new ObjectHandler(XmppCon_OnRosterStart);
			XmppCon.OnRosterEnd += new ObjectHandler(XmppCon_OnRosterEnd);
			XmppCon.OnRosterItem += new agsXMPP.XmppClientConnection.RosterHandler(XmppCon_OnRosterItem);

			XmppCon.OnAgentStart += new ObjectHandler(XmppCon_OnAgentStart);
			XmppCon.OnAgentEnd += new ObjectHandler(XmppCon_OnAgentEnd);
			XmppCon.OnAgentItem += new agsXMPP.XmppClientConnection.AgentHandler(XmppCon_OnAgentItem);

			XmppCon.OnLogin += new ObjectHandler(XmppCon_OnLogin);
			XmppCon.OnClose += new ObjectHandler(XmppCon_OnClose);
			XmppCon.OnError += new ErrorHandler(XmppCon_OnError);
			XmppCon.OnPresence += new PresenceHandler(XmppCon_OnPresence);
			XmppCon.OnMessage += new MessageHandler(XmppCon_OnMessage);
			XmppCon.OnIq += new IqHandler(XmppCon_OnIq);

			
			XmppCon.ClientSocket.OnReceive += new agsXMPP.net.ClientSocket.OnSocketDataHandler(ClientSocket_OnReceive);
			XmppCon.ClientSocket.OnSend += new agsXMPP.net.ClientSocket.OnSocketDataHandler(ClientSocket_OnSend);
			
		}

        private void InitStatusCombo()
        {
            cboStatus.Items.Add("offline");
            cboStatus.Items.Add(ShowType.away.ToString());
            cboStatus.Items.Add(ShowType.xa.ToString());
            cboStatus.Items.Add(ShowType.chat.ToString());
            cboStatus.Items.Add(ShowType.dnd.ToString());
            cboStatus.Items.Add("online");
            cboStatus.SelectedIndex = 0;
        }

		/// <summary>
		/// Entry Point
		/// </summary>
		[MTAThread]
		static void Main()
		{			
			Application.Run(new frmMain());
		}

		#region << XmppConnection events >>
		private void XmppCon_OnReadXml(object sender, string xml)
		{
			if (InvokeRequired)
			{
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new XmlHandler(XmppCon_OnReadXml), new object[] { sender, xml });
				return;
			}
            
            txtDebug.SelectionStart = txtDebug.Text.Length;
            txtDebug.Text += ("RECV:" + xml + "\r\n");
            txtDebug.SelectionStart = txtDebug.Text.Length;
            txtDebug.ScrollToCaret();
		}

		private void XmppCon_OnWriteXml(object sender, string xml)
		{
			if (InvokeRequired)
			{
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new XmlHandler(XmppCon_OnWriteXml), new object[] { sender, xml });
				return;
			}
            txtDebug.SelectionStart = txtDebug.Text.Length;
            txtDebug.Text += ("SEND:" + xml + "\r\n");
            txtDebug.SelectionStart = txtDebug.Text.Length;
            txtDebug.ScrollToCaret();
		}

		private void XmppCon_OnRosterStart(object sender)
		{
			if (InvokeRequired)
			{
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new ObjectHandler(XmppCon_OnRosterStart), new object[] { this });
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
				BeginInvoke(new ObjectHandler(XmppCon_OnRosterEnd), new object[] { this });
				return;
			}
			// enable redraw again
			lvwRoster.EndUpdate();

			// We received the rosterm now its time to send our presence
			cboStatus.SelectedIndex = 5;			
		}

		private void XmppCon_OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item)
		{
			if (InvokeRequired)
			{
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new agsXMPP.XmppClientConnection.RosterHandler(XmppCon_OnRosterItem), new object[] { this, item });
				return;
			}
			if (item.Subscription != SubscriptionType.remove)
			{
				ListViewItem li;

				li = FindRosterListViewItem(item.Jid);
				if (li == null)
				{
					li = new ListViewItem(item.Name != null ? item.Name : item.Jid.ToString());

					li.Tag = item.Jid.ToString();                    					
					li.SubItems.Add("");
					li.SubItems.Add("");
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
				BeginInvoke(new agsXMPP.ObjectHandler(XmppCon_OnLogin), new object[] { sender });
				return;
			}
			mnuFileConnect.Enabled = false;
			mnuFileDisconnect.Enabled = true;
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
				BeginInvoke(new OnPresenceDelegate(XmppCon_OnPresence), new object[] { sender, pres });
				return;
			}

			if (pres.Type == PresenceType.subscribe)
			{
				frmSubscribe f = new frmSubscribe(XmppCon, pres.From);
				f.Show();
			}
			else if (pres.Type == PresenceType.subscribed)
			{

			}
			else if (pres.Type == PresenceType.unsubscribe)
			{

			}
			else if (pres.Type == PresenceType.unsubscribed)
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

		private void XmppCon_OnIq(object sender, IQ iq)
		{
			if (InvokeRequired)
			{
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new IqHandler(XmppCon_OnIq), new object[] { sender, iq });
				return;
			}
            			
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

			// check for xData Message
			Element e = msg.SelectSingleElement(typeof(Data));
			if (e != null)
			{
				Data xdata = e as Data;
				if (xdata.Type == XDataFormType.form)
				{
					// This is not supported by the WM5 MiniClient sample
				}
			}
			else
			{
                if (msg.Type == MessageType.chat)
                {
                    if (!Util.Forms.ContainsKey(msg.From.Bare))
                    {
                        ListViewItem itm = FindRosterListViewItem(msg.From);
                        string nick = itm == null ? msg.From.Bare : itm.Text;

                        frmChat f = new frmChat(msg.From, XmppCon, nick);
                        f.Show();
                        f.IncomingMessage(msg);
                    }
                }
                else if (msg.Type == MessageType.normal)
                {
                    frmMsg fMsg = new frmMsg(msg);
                    fMsg.Show();
                }
                else if (msg.Type == MessageType.headline)
                {
                    // not handeled in this example
                }
			}
		}

		private void XmppCon_OnClose(object sender)
		{
			if (InvokeRequired)
			{
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new agsXMPP.ObjectHandler(XmppCon_OnClose), new object[] { sender });
				return;
			}
			Console.WriteLine("OnClose");
			mnuFileConnect.Enabled = true;
			mnuFileDisconnect.Enabled = false;

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

		private void mnuFileAddContact_Click(object sender, System.EventArgs e)
		{
			frmAddRoster f = new frmAddRoster(XmppCon);
			f.Show();
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
					XmppCon.Show = (ShowType)Enum.Parse(typeof(ShowType), cboStatus.Text, true);
				}
				XmppCon.SendMyPresence();
			}
		}

		private ListViewItem FindRosterListViewItem(Jid jid)
		{
			foreach (ListViewItem lvi in lvwRoster.Items)
			{
				if (jid.Bare.ToLower() == lvi.Tag.ToString().ToLower())
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

			lvwRoster.Columns.Add("name", 100, HorizontalAlignment.Left);
			lvwRoster.Columns.Add("status", 75, HorizontalAlignment.Left);
			lvwRoster.Columns.Add("resource", 75, HorizontalAlignment.Left);
		}

		private ListViewItem GetSelectedListViewItem()
		{
            if (lvwRoster.SelectedIndices.Count > 0)
                return lvwRoster.Items[lvwRoster.SelectedIndices[0]];
            else
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
				frmVcard f = new frmVcard(new Jid(itm.Tag.ToString()), XmppCon);
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
					frmChat f = new frmChat(new Jid((string)itm.Tag), XmppCon, itm.Text);
					f.Show();
				}
			}
		}

        private void ctxMnuRosterMessage_Click(object sender, EventArgs e)
        {
            ListViewItem itm = GetSelectedListViewItem();
            if (itm != null)
            {
                if (!Util.Forms.ContainsKey(itm.Tag))
                {
                    frmMsg fMsg = new frmMsg(XmppCon, new Jid((string)itm.Tag));
                    fMsg.Show();
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

				XmppCon.RosterManager.RemoveRosterItem(new Jid(itm.Tag as string));
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
				BeginInvoke(new agsXMPP.net.ClientSocket.OnSocketDataHandler(ClientSocket_OnReceive), new object[] { sender, data, count });
				return;
			}
            string text = System.Text.Encoding.Default.GetString(data, 0, count);

            txtSocketDebug.SelectionStart = txtSocketDebug.TextLength;
            txtSocketDebug.Text += ("RECV:" + text + "\r\n");
            txtSocketDebug.SelectionStart = txtSocketDebug.TextLength;
            txtSocketDebug.ScrollToCaret();
		}

		private void ClientSocket_OnSend(object sender, byte[] data, int count)
		{
			if (InvokeRequired)
			{
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new agsXMPP.net.ClientSocket.OnSocketDataHandler(ClientSocket_OnSend), new object[] { sender, data, count });
				return;
            }
            string text = System.Text.Encoding.Default.GetString(data, 0, count);

            txtSocketDebug.SelectionStart = txtSocketDebug.TextLength;
            txtSocketDebug.Text += ("SEND:" + text + "\r\n");
            txtSocketDebug.SelectionStart = txtSocketDebug.TextLength;
            txtSocketDebug.ScrollToCaret();
		}

        private void mnuSendMessage_Click(object sender, EventArgs e)
        {
            frmMsg f = new frmMsg(this.XmppCon);
            f.Show();
        }    

	}
}