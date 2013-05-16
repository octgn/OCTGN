using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using agsXMPP;
using agsXMPP.protocol.client;

namespace MiniClient
{
	/// <summary>
	/// Zusammenfassung für frmSubscribe.
	/// </summary>
	public class frmSubscribe : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button cmdApprove;
		private System.Windows.Forms.Button cmdRefuse;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblFrom;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private XmppClientConnection	_connection;
		private Jid						_from;

		public frmSubscribe(XmppClientConnection con, Jid jid)
		{
			InitializeComponent();
            
			_connection = con;
			_from		= jid;

			lblFrom.Text	= jid.ToString();
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
		}

		#region Vom Windows Form-Designer generierter Code
		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			this.cmdApprove = new System.Windows.Forms.Button();
			this.cmdRefuse = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.lblFrom = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// cmdApprove
			// 
			this.cmdApprove.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdApprove.Location = new System.Drawing.Point(168, 56);
			this.cmdApprove.Name = "cmdApprove";
			this.cmdApprove.Size = new System.Drawing.Size(72, 24);
			this.cmdApprove.TabIndex = 0;
			this.cmdApprove.Text = "Approve";
			this.cmdApprove.Click += new System.EventHandler(this.cmdApprove_Click);
			// 
			// cmdRefuse
			// 
			this.cmdRefuse.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdRefuse.Location = new System.Drawing.Point(56, 56);
			this.cmdRefuse.Name = "cmdRefuse";
			this.cmdRefuse.Size = new System.Drawing.Size(72, 24);
			this.cmdRefuse.TabIndex = 1;
			this.cmdRefuse.Text = "Refuse";
			this.cmdRefuse.Click += new System.EventHandler(this.cmdRefuse_Click);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(8, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "From:";
			// 
			// lblFrom
			// 
			this.lblFrom.Location = new System.Drawing.Point(56, 16);
			this.lblFrom.Name = "lblFrom";
			this.lblFrom.Size = new System.Drawing.Size(224, 32);
			this.lblFrom.TabIndex = 3;
			this.lblFrom.Text = "jid";
			// 
			// frmSubscribe
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 88);
			this.Controls.Add(this.lblFrom);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cmdRefuse);
			this.Controls.Add(this.cmdApprove);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmSubscribe";
			this.Text = "Subscription Request";
			this.ResumeLayout(false);

		}
		#endregion

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
