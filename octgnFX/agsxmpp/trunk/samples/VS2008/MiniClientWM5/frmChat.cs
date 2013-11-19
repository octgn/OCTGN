using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using agsXMPP;
using agsXMPP.protocol;
using agsXMPP.protocol.client;
using agsXMPP.Collections;

namespace MiniClient
{
	/// <summary>
	/// 
	/// </summary>
	public partial class  frmChat : System.Windows.Forms.Form
	{      	

		private XmppClientConnection _connection;
        private Jid m_Jid;
		private System.Windows.Forms.TextBox txtSend;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.TextBox txtChat;
		private string _nickname;


		public frmChat(Jid jid, XmppClientConnection con, string nickname)
		{
			m_Jid = jid;
			_connection = con;
			_nickname = nickname;
                        
			InitializeComponent();

			this.Text = "Chat with " + nickname;

			Util.Forms.Add(m_Jid.Bare.ToLower(), this);

			// Setup new Message Callback
			con.MessageGrabber.Add(jid, new BareJidComparer(), new MessageCB(XmppCon_OnMessage), null);
		}

		public Jid Jid
		{
			get { return m_Jid; }
			set { m_Jid = value; }
		}


		private void OutgoingMessage(agsXMPP.protocol.client.Message msg)
		{
            txtChat.Text += "I said: " + msg.Body + "\r\n";
		}

		public void IncomingMessage(agsXMPP.protocol.client.Message msg)
		{
            txtChat.Text += _nickname + " said: " + msg.Body + "\r\n";
		}
        
		private void XmppCon_OnMessage(object sender, agsXMPP.protocol.client.Message msg, object data)
		{
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new MessageCB(XmppCon_OnMessage), new object[] { sender, msg, data });
                return;
            }            		
			
			if (msg.Type == MessageType.chat && msg.Body != null)
			{
                IncomingMessage(msg);				
			}			
		}	

		private void mnuSend_Click(object sender, System.EventArgs e)
		{
            if (txtSend.TextLength > 0)
            {
                agsXMPP.protocol.client.Message msg = new agsXMPP.protocol.client.Message();

                msg.Type = MessageType.chat;
                msg.To = m_Jid;
                msg.Body = txtSend.Text;

                // Send the chat-message
                _connection.Send(msg);
                // Display the message we sent right now
                OutgoingMessage(msg);
                // clear the chat window
                txtSend.Text = "";
            }
		}

        private void frmChat_Closing(object sender, CancelEventArgs e)
        {
            Util.Forms.Remove(m_Jid.Bare.ToLower());
            _connection.MessageGrabber.Remove(m_Jid);
            _connection = null;
        }
	}
}