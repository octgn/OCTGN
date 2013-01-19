using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using agsXMPP;
using agsXMPP.Collections;
using agsXMPP.protocol;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;

namespace MiniClient
{
    public partial class frmGroupChat : Form
    {
        #region << Constructors >>        
        public frmGroupChat(XmppClientConnection xmppCon, Jid roomJid, string Nickname)
        {
            InitializeComponent();   
            m_RoomJid = roomJid;
            m_XmppCon = xmppCon;
            m_Nickname = Nickname;

            Util.GroupChatForms.Add(roomJid.Bare.ToLower(), this);
            
            // Setup new Message Callback
            m_XmppCon.MessageGrabber.Add(roomJid, new BareJidComparer(), new MessageCB(MessageCallback), null);
            
            // Setup new Presence Callback
            m_XmppCon.PresenceGrabber.Add(roomJid, new BareJidComparer(), new PresenceCB(PresenceCallback), null);

        }
        #endregion

        private Jid                     m_RoomJid;
        private XmppClientConnection    m_XmppCon;
        private string                  m_Nickname;

        private void frmGroupChat_Load(object sender, EventArgs e)
        {
            if (m_RoomJid != null)
            {
                Presence pres = new Presence();

                Jid to = new Jid(m_RoomJid.ToString());
                to.Resource = m_Nickname;
                pres.To = to;
                m_XmppCon.Send(pres);
            }
        }

        private void frmGroupChat_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_RoomJid != null)
            {
                Presence pres = new Presence();
                pres.To = m_RoomJid;
                pres.Type = PresenceType.unavailable;
                m_XmppCon.Send(pres);
            }
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        private void MessageCallback(object sender, agsXMPP.protocol.client.Message msg, object data)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new MessageCB(MessageCallback), new object[] { sender, msg, data });
                return;
            }
            
            if (msg.Type == MessageType.groupchat)
                IncomingMessage(msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pres"></param>
        /// <param name="data"></param>
        private void PresenceCallback(object sender, agsXMPP.protocol.client.Presence pres, object data)
        {
            if (InvokeRequired)
            {
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke				
                BeginInvoke(new PresenceCB(PresenceCallback), new object[] { sender, pres, data });
                return;
            }

            ListViewItem lvi = FindListViewItem(pres.From);
            if (lvi != null)
            {
                if (pres.Type == PresenceType.unavailable)
                {
                    lvi.Remove();
                }
                else
                {
                    int imageIdx = Util.GetRosterImageIndex(pres);
                    lvi.ImageIndex = imageIdx;
                    lvi.SubItems[1].Text = (pres.Status == null ? "" : pres.Status);
                    User u = pres.SelectSingleElement(typeof(User)) as User;
                    if (u != null)
                    {
                        lvi.SubItems[2].Text = u.Item.Affiliation.ToString();
                        lvi.SubItems[3].Text = u.Item.Role.ToString();
                    }
                }
            }
            else
            {
                int imageIdx = Util.GetRosterImageIndex(pres);
                
                ListViewItem lv = new ListViewItem(pres.From.Resource);               

                lv.Tag = pres.From.ToString();
                lv.SubItems.Add(pres.Status == null ? "" : pres.Status);
                User u = pres.SelectSingleElement(typeof(User)) as User;
                if (u != null)
                {
                    lv.SubItems.Add(u.Item.Affiliation.ToString());
                    lv.SubItems.Add(u.Item.Role.ToString());
                }
                lv.ImageIndex = imageIdx;
                lvwRoster.Items.Add(lv);
            }
        }

        private ListViewItem FindListViewItem(Jid jid)
        {
            foreach (ListViewItem lvi in lvwRoster.Items)
            {
                if (jid.ToString().ToLower() == lvi.Tag.ToString().ToLower())
                    return lvi;
            }
            return null;
        }

        private void IncomingMessage(agsXMPP.protocol.client.Message msg)
        {
            if (msg.Type == MessageType.error)
            {
                //Handle errors here
                // we dont handle them in this example
                return;
            }

            if (msg.Subject != null)
            {
                txtSubject.Text = msg.Subject;

                rtfChat.SelectionColor = Color.DarkGreen;
                // The Nickname of the sender is in GroupChat in the Resource of the Jid
                rtfChat.AppendText(msg.From.Resource + " changed subject: ");
                rtfChat.SelectionColor = Color.Black;                
                rtfChat.AppendText(msg.Subject);
                rtfChat.AppendText("\r\n");
            }
            else
            {
                if (msg.Body == null)
                    return;

                rtfChat.SelectionColor = Color.Red;
                // The Nickname of the sender is in GroupChat in the Resource of the Jid
                rtfChat.AppendText(msg.From.Resource + " said: ");
                rtfChat.SelectionColor = Color.Black;
                rtfChat.AppendText(msg.Body);
                rtfChat.AppendText("\r\n");
            }
        }

        private void cmdSend_Click(object sender, EventArgs e)
        {
            // Make sure that the users send no empty messages
            if (rtfSend.Text.Length > 0)
            {
                agsXMPP.protocol.client.Message msg = new agsXMPP.protocol.client.Message();

                msg.Type = MessageType.groupchat;
                msg.To = m_RoomJid;
                msg.Body = rtfSend.Text;

                m_XmppCon.Send(msg);

                rtfSend.Text = "";
            }
        }

        /// <summary>
        /// Changing the subject in a chatroom
        /// in MUC rooms this could return an error when you are a normal user and not allowed
        /// to change the subject.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdChangeSubject_Click(object sender, EventArgs e)
        {
            agsXMPP.protocol.client.Message msg = new agsXMPP.protocol.client.Message();

            msg.Type = MessageType.groupchat;
            msg.To = m_RoomJid;
            msg.Subject = txtSubject.Text;

            m_XmppCon.Send(msg);
        }

        

    }
}