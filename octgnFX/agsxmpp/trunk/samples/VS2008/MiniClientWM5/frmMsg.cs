using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using agsXMPP;
using agsXMPP.protocol.client;


namespace MiniClient
{
	public partial class frmMsg : Form
	{
        XmppClientConnection xmppCon = null;
        /// <summary>
        /// Constructor for Outgoing Messages
        /// </summary>
        public frmMsg(XmppClientConnection con)
		{
			InitializeComponent();
            xmppCon = con;
            lblJid.Text = "To:";
		}

        /// <summary>
        /// Constructor for Outgoing Messages
        /// </summary>
        public frmMsg(XmppClientConnection con, Jid to) : this(con)
        {
            xmppCon = con;         
            txtJid.Text = to.ToString();
        }

        /// <summary>
        /// Consrtctor for Incoming messages
        /// </summary>
        /// <param name="msg"></param>
        public frmMsg(Message msg)
        {
            InitializeComponent();

            lblJid.Text = "From:";
            DisplayIncomingMessage(msg);
            mnuSend.Enabled = false;
        }

		
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		public void DisplayIncomingMessage(Message msg)
		{	
            txtJid.Text     = msg.From.ToString();
			txtSubject.Text = msg.Subject;
			txtBody.Text    = msg.Body;
		}

        private void mnuCancel_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void mnuSend_Click(object sender, System.EventArgs e)
        {
            agsXMPP.protocol.client.Message msg = new agsXMPP.protocol.client.Message();

            msg.Type = MessageType.normal;
            
            msg.To = new Jid(txtJid.Text);
            msg.Subject = this.txtSubject.Text;
            msg.Body = txtBody.Text;

            xmppCon.Send(msg);

            this.Close();
        }

	}
}