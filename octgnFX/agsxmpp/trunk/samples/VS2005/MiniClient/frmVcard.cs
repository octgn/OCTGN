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
	/// Zusammenfassung für frmVcard.
	/// </summary>
	public class frmVcard : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private XmppClientConnection _connection;
		private System.Windows.Forms.TextBox txtFullname;
		private System.Windows.Forms.TextBox txtNickname;
		private System.Windows.Forms.TextBox txtBirthday;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.PictureBox picPhoto;
		private System.Windows.Forms.Button cmdClose;
		private System.Windows.Forms.TextBox txtDescription;
		string	packetId;

		public frmVcard(Jid jid, XmppClientConnection con)
		{
			
			InitializeComponent();
			
			_connection = con;

			this.Text = "Vcard from: " + jid.Bare;

			VcardIq viq = new VcardIq(IqType.get, new Jid(jid.Bare));
			packetId = viq.Id;
			con.IqGrabber.SendIq(viq, new IqCB(VcardResult), null);			
		}

		/// <summary>
		/// Die verwendeten Ressourcen bereinigen.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
			// Remove the packet from the Tracker in case we Close the from before getting
			// the result. The callback yould cause a nullpointer exception
			if (_connection.IqGrabber != null)
				_connection.IqGrabber.Remove(packetId);
		}

		#region Vom Windows Form-Designer generierter Code
		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.txtFullname = new System.Windows.Forms.TextBox();
			this.txtBirthday = new System.Windows.Forms.TextBox();
			this.txtDescription = new System.Windows.Forms.TextBox();
			this.txtNickname = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.picPhoto = new System.Windows.Forms.PictureBox();
			this.cmdClose = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 24);
			this.label1.TabIndex = 0;
			this.label1.Text = "Full Name:";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(16, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(104, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "Birthday:";
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(16, 120);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(104, 16);
			this.label3.TabIndex = 2;
			this.label3.Text = "Description:";
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(16, 48);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(96, 16);
			this.label4.TabIndex = 3;
			this.label4.Text = "Nick Name:";
			// 
			// txtFullname
			// 
			this.txtFullname.Location = new System.Drawing.Point(128, 16);
			this.txtFullname.Name = "txtFullname";
			this.txtFullname.ReadOnly = true;
			this.txtFullname.Size = new System.Drawing.Size(176, 20);
			this.txtFullname.TabIndex = 0;
			this.txtFullname.Text = "";
			// 
			// txtBirthday
			// 
			this.txtBirthday.Location = new System.Drawing.Point(128, 80);
			this.txtBirthday.Name = "txtBirthday";
			this.txtBirthday.ReadOnly = true;
			this.txtBirthday.Size = new System.Drawing.Size(176, 20);
			this.txtBirthday.TabIndex = 2;
			this.txtBirthday.Text = "";
			// 
			// txtDescription
			// 
			this.txtDescription.Location = new System.Drawing.Point(128, 112);
			this.txtDescription.Multiline = true;
			this.txtDescription.Name = "txtDescription";
			this.txtDescription.ReadOnly = true;
			this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtDescription.Size = new System.Drawing.Size(176, 64);
			this.txtDescription.TabIndex = 3;
			this.txtDescription.Text = "";
			// 
			// txtNickname
			// 
			this.txtNickname.Location = new System.Drawing.Point(128, 48);
			this.txtNickname.Name = "txtNickname";
			this.txtNickname.ReadOnly = true;
			this.txtNickname.Size = new System.Drawing.Size(176, 20);
			this.txtNickname.TabIndex = 1;
			this.txtNickname.Text = "";
			// 
			// label5
			// 
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label5.Location = new System.Drawing.Point(16, 192);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(104, 16);
			this.label5.TabIndex = 8;
			this.label5.Text = "Photo:";
			// 
			// picPhoto
			// 
			this.picPhoto.Location = new System.Drawing.Point(128, 192);
			this.picPhoto.Name = "picPhoto";
			this.picPhoto.Size = new System.Drawing.Size(176, 104);
			this.picPhoto.TabIndex = 9;
			this.picPhoto.TabStop = false;
			// 
			// cmdClose
			// 
			this.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdClose.Location = new System.Drawing.Point(120, 304);
			this.cmdClose.Name = "cmdClose";
			this.cmdClose.Size = new System.Drawing.Size(96, 24);
			this.cmdClose.TabIndex = 4;
			this.cmdClose.Text = "Close";
			this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
			// 
			// frmVcard
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(328, 336);
			this.Controls.Add(this.cmdClose);
			this.Controls.Add(this.picPhoto);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.txtNickname);
			this.Controls.Add(this.txtDescription);
			this.Controls.Add(this.txtBirthday);
			this.Controls.Add(this.txtFullname);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "frmVcard";
			this.Text = "frmVcard";
			this.ResumeLayout(false);

		}
		#endregion


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
				if (vcard!=null)
				{
					txtFullname.Text	= vcard.Fullname;
					txtNickname.Text	= vcard.Nickname;
					txtBirthday.Text	= vcard.Birthday.ToString();
					txtDescription.Text = vcard.Description;
					Photo photo = vcard.Photo;
					if (photo != null)
						picPhoto.Image		= vcard.Photo.Image;
				}
                
                
			}
		}

		private void cmdClose_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
