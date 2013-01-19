using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using agsXMPP;
using agsXMPP.Xml;
using agsXMPP.Xml.Dom;


namespace MiniClient
{
	/// <summary>
	/// Summary for frmLogin.
	/// </summary>
	public partial class frmLogin : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label lblJid;
        private System.Windows.Forms.TextBox txtJid;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Label lblPassword;
		private System.Windows.Forms.Label lblPriority;
		private System.Windows.Forms.Label lblResource;
		
		private System.Windows.Forms.TextBox txtResource;
		private System.Windows.Forms.NumericUpDown numPriority;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.CheckBox chkSSL;
		private System.Windows.Forms.CheckBox chkRegister;

		private XmppClientConnection _connection;

		public frmLogin(XmppClientConnection con)
		{
			InitializeComponent();

			LoadSettings();
			this.DialogResult = DialogResult.Cancel;
			_connection = con;
		}

		private void mnuCancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void mnuLogin_Click(object sender, System.EventArgs e)
		{
			Jid jid = new Jid(txtJid.Text);

			_connection.Server          = jid.Server;
			_connection.Username        = jid.User;
			_connection.Password        = txtPassword.Text;
			_connection.Resource        = txtResource.Text;
            _connection.Priority        = (int) PriorityUpDown.Value;

			_connection.Port            = int.Parse(txtPort.Text);

            _connection.UseCompression  = false;

            // SSL is not supported by the CF yet
            //_connection.UseSSL = false;
            //_connection.UseStartTLS = true;

            // Use this property if the connection host is different the the server part in the Jid
            // e.g. GTalk is doing this
            //_connection.ConnectServer = "talk.google.com";                                                  

			_connection.RegisterAccount = chkRegister.Checked;

			this.DialogResult = DialogResult.OK;

			SaveSettings();

			this.Close();
		}

		private string SettingsFilename
		{
			get
			{                
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                string path = System.IO.Path.Combine(appData, "MiniClient");
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);
				
                return System.IO.Path.Combine(path, "settings.xml");
			}
		}

		private void LoadSettings()
		{
			if (System.IO.File.Exists(SettingsFilename))
			{
				Document doc = new Document();
				doc.LoadFile(SettingsFilename);
				Element Login = doc.RootElement.SelectSingleElement("Login");

				txtJid.Text             = Login.GetTag("Jid");
				txtPassword.Text        = Login.GetTag("Password");
				txtResource.Text        = Login.GetTag("Resource");
				PriorityUpDown.Value	= Login.GetTagInt("Priority");
				
			}
		}

		private void SaveSettings()
		{
			Document doc = new Document();
			Element Settings = new Element("Settings");

			Element Login = new Element("Login");

			Login.ChildNodes.Add(new Element("Jid", txtJid.Text));
			Login.ChildNodes.Add(new Element("Password", txtPassword.Text));
			Login.ChildNodes.Add(new Element("Resource", txtResource.Text));
			Login.ChildNodes.Add(new Element("Priority", numPriority.Value.ToString()));
			Login.ChildNodes.Add(new Element("Port", txtPort.Text));
			Login.ChildNodes.Add(new Element("Ssl", chkSSL.Checked));

			doc.ChildNodes.Add(Settings);
			Settings.ChildNodes.Add(Login);
			
			doc.Save(SettingsFilename);
		}		
	}
}
