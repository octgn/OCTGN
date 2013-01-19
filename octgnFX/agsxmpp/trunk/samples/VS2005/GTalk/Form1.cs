using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using agsXMPP;

namespace GTalk
{
    public partial class Form1 : Form
    {
        XmppClientConnection xmppCon = new XmppClientConnection();

        public Form1()
        {
            InitializeComponent();

            Init();
        }             

        private void Init()
        {
            listEvents.Items.Clear();

            // Subscribe to Events
            xmppCon.OnLogin         += new ObjectHandler(xmppCon_OnLogin);
            xmppCon.OnRosterStart   += new ObjectHandler(xmppCon_OnRosterStart);
            xmppCon.OnRosterEnd     += new ObjectHandler(xmppCon_OnRosterEnd);
            xmppCon.OnRosterItem    += new XmppClientConnection.RosterHandler(xmppCon_OnRosterItem);
            xmppCon.OnPresence      += new agsXMPP.protocol.client.PresenceHandler(xmppCon_OnPresence);
            xmppCon.OnAuthError     += new XmppElementHandler(xmppCon_OnAuthError);
            xmppCon.OnError         += new ErrorHandler(xmppCon_OnError);
            xmppCon.OnClose         += new ObjectHandler(xmppCon_OnClose);
            xmppCon.OnMessage       += new agsXMPP.protocol.client.MessageHandler(xmppCon_OnMessage);
        }

        void xmppCon_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            // ignore empty messages (events)
            if (msg.Body == null)
                return;

            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new agsXMPP.protocol.client.MessageHandler(xmppCon_OnMessage), new object[] { sender, msg });
                return;
            }
            
            listEvents.Items.Add(String.Format("OnMessage from:{0} type:{1}", msg.From.Bare, msg.Type.ToString() ));
            listEvents.Items.Add(msg.Body);
            listEvents.SelectedIndex = listEvents.Items.Count -1;
        }

        void xmppCon_OnClose(object sender)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new ObjectHandler(xmppCon_OnClose), new object[] { sender });
                return;
            }
            listEvents.Items.Add("OnClose Connection closed");
            listEvents.SelectedIndex = listEvents.Items.Count -1;
        }

        void xmppCon_OnError(object sender, Exception ex)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new ErrorHandler(xmppCon_OnError), new object[] { sender, ex });
                return;
            }
            listEvents.Items.Add("OnError");
            listEvents.SelectedIndex = listEvents.Items.Count -1;
        }

        void xmppCon_OnAuthError(object sender, agsXMPP.Xml.Dom.Element e)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new XmppElementHandler(xmppCon_OnAuthError), new object[] { sender, e });
                return;
            }
            listEvents.Items.Add("OnAuthError");
            listEvents.SelectedIndex = listEvents.Items.Count -1;
        }

        void xmppCon_OnPresence(object sender, agsXMPP.protocol.client.Presence pres)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new agsXMPP.protocol.client.PresenceHandler(xmppCon_OnPresence), new object[] { sender, pres });
                return;
            }
            listEvents.Items.Add(String.Format("Received Presence from:{0} show:{1} status:{2}", pres.From.ToString(), pres.Show.ToString(), pres.Status));
            listEvents.SelectedIndex = listEvents.Items.Count - 1;
        }

        void xmppCon_OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new XmppClientConnection.RosterHandler(xmppCon_OnRosterItem), new object[] { sender, item });
                return;
            }
            listEvents.Items.Add(String.Format("Received Contact {0}", item.Jid.Bare));
            listEvents.SelectedIndex = listEvents.Items.Count - 1;
        }

        void xmppCon_OnRosterEnd(object sender)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new ObjectHandler(xmppCon_OnRosterEnd), new object[] { sender });
                return;
            }
            listEvents.Items.Add("OnRosterEnd");
            listEvents.SelectedIndex = listEvents.Items.Count - 1;

            // Send our own presence to teh server, so other epople send us online
            // and the server sends us the presences of our contacts when they are
            // available
            xmppCon.SendMyPresence();
        }

        void xmppCon_OnRosterStart(object sender)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new ObjectHandler(xmppCon_OnRosterStart), new object[] { sender });
                return;
            }
            listEvents.Items.Add("OnRosterStart");
            listEvents.SelectedIndex = listEvents.Items.Count - 1;
        }

        void xmppCon_OnLogin(object sender)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new ObjectHandler(xmppCon_OnLogin), new object[] { sender });
                return;
            }
            listEvents.Items.Add("OnLogin");
            listEvents.SelectedIndex = listEvents.Items.Count - 1;
        }

        private void cmdLogin_Click(object sender, EventArgs e)
        {
            Jid jidUser = new Jid(txtJabberId.Text);
            
            xmppCon.Username = jidUser.User;
            xmppCon.Server = jidUser.Server;
            xmppCon.Password = txtPassword.Text;
            xmppCon.AutoResolveConnectServer = true;

            xmppCon.Open();
        }

        private void cmdLogout_Click(object sender, EventArgs e)
        {
            // close the xmpp connection
            xmppCon.Close();
        }

        private void cmdSend_Click(object sender, EventArgs e)
        {
            // Send a message
            agsXMPP.protocol.client.Message msg = new agsXMPP.protocol.client.Message();
            msg.Type = agsXMPP.protocol.client.MessageType.chat;
            msg.To = new Jid(txtJabberIdReceiver.Text);
            msg.Body = txtMessage.Text;

            xmppCon.Send(msg);

            txtMessage.Text = "";
        }
    }
}