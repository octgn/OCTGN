using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using agsXMPP;
using agsXMPP.protocol;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.vcard;

namespace MiniClient
{
	/// <summary>
	/// Summary for frmVcard.
	/// </summary>
	public partial class frmVcard : System.Windows.Forms.Form
	{
		/// <summary>
		/// designer variables.
		/// </summary>				
		private System.Windows.Forms.TextBox txtFullname;
		private System.Windows.Forms.TextBox txtNickname;
		private System.Windows.Forms.TextBox txtBirthday;
		private System.Windows.Forms.Label lblPhoto;
        private System.Windows.Forms.PictureBox picPhoto;
		private System.Windows.Forms.TextBox txtDescription;
		
        string packetId = null;
        private XmppClientConnection _connection;

		public frmVcard(Jid jid, XmppClientConnection con)
		{

			InitializeComponent();

			_connection = con;

			this.Text = "Vcard from: " + jid.Bare;

			VcardIq viq = new VcardIq(IqType.get, new Jid(jid.Bare));
			packetId = viq.Id;
			con.IqGrabber.SendIq(viq, new IqCB(VcardResult), null);
		}


		private void VcardResult(object sender, IQ iq, object data)
		{
			if (InvokeRequired)
			{
				// Windows Forms are not Thread Safe, we need to invoke this :(
				// We're not in the UI thread, so we need to call BeginInvoke				
				BeginInvoke(new IqCB(VcardResult), new object[] { sender, iq, data });
				return;
			}

			if (iq.Type == IqType.result)
			{
				Vcard vcard = iq.Vcard;
				if (vcard != null)
				{
					txtFullname.Text = vcard.Fullname;
					txtNickname.Text = vcard.Nickname;
					txtBirthday.Text = vcard.Birthday.ToString();
					txtDescription.Text = vcard.Description;
                    Photo photo = vcard.Photo;
                    if (photo != null)
                        picPhoto.Image = vcard.Photo.Image;						
				}
			}
		}

        private void mnuClose_Click(object sender, EventArgs e)
        {
            _connection.IqGrabber.Remove(packetId);
            this.Close();
        }	
	}
}