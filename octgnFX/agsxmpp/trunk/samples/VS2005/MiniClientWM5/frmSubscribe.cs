using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using agsXMPP;
using agsXMPP.protocol;
using agsXMPP.protocol.client;

namespace MiniClient
{
	/// <summary>
	/// 
	/// </summary>
	public partial class frmSubscribe : System.Windows.Forms.Form
    {	
		
		private XmppClientConnection _connection;
		private Jid _from;

		public frmSubscribe(XmppClientConnection con, Jid jid)
		{
			InitializeComponent();

			_connection = con;
			_from = jid;

			lblFrom.Text = jid.ToString();
		}        

		private void cmdRefuse_Click(object sender, System.EventArgs e)
		{
			PresenceManager pm = new PresenceManager(_connection);
			pm.RefuseSubscriptionRequest(_from);

			this.Close();
		}

		private void cmdApprove_Click(object sender, System.EventArgs e)
		{
			PresenceManager pm = new PresenceManager(_connection);
			pm.ApproveSubscriptionRequest(_from);

			this.Close();
		}
	}
}
