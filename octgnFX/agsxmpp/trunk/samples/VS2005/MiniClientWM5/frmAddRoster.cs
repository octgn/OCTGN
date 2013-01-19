using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using agsXMPP;

namespace MiniClient
{
	/// <summary>
	/// Summary description for femAddRoster.
	/// </summary>
	public partial class frmAddRoster : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox txtNickname;
		private System.Windows.Forms.TextBox txtJid;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		       	

		private XmppClientConnection _connection;

		public frmAddRoster(XmppClientConnection con)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			_connection = con;
		}
        
		private void menuItem1_Click(object sender, System.EventArgs e)
		{

			Jid jid = new Jid(txtJid.Text);

			// Add the Rosteritem using the Rostermanager			
			if (txtNickname.Text.Length > 0)
				_connection.RosterManager.AddRosterItem(jid, txtNickname.Text);
			else
				_connection.RosterManager.AddRosterItem(jid);

			// Ask for subscription now
			_connection.PresenceManager.Subcribe(jid);

			this.Close();
		}
	}
}
