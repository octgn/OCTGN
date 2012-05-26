using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Octgn.Releaser
{
	public partial class Form1 : Form
	{
		private string vFile = "";
		private string indexFile = "";
		private Version oldVersion = null;
		public Form1()
		{
			InitializeComponent();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			if(p1.Visible)
			{
				if(FirstPage())
				{
					Program.OldVersion = oldVersion;
					Program.OctgnDirectory = tbOctgnDirectory.Text;
					Program.SiteDirectory = tbSiteDirectory.Text;
					Program.VersionFile = vFile;
					page2 p = new page2();
					p.Show();
					this.Hide();
				}
			}
		}

		private Version CheckVersionFile()
		{
			using (var st = new FileStream(vFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using(var reader = XmlReader.Create(st))
				{
					while(reader.Read())
					{
						if(!reader.IsStartElement()) continue;
						if(reader.IsEmptyElement) continue;
						if(reader.Name == "Version")
						{
							if (reader.Read())
							{
								Version ret = null;
								if(Version.TryParse(reader.Value , out ret)) return ret;
								return null;
							}
						}
					}
				}
			}
			return null;
		}

		private bool FirstPage()
		{
			if (!Directory.Exists(tbOctgnDirectory.Text))
			{
				ErrorBox("Octgn Directory Doesn't Exist");
				return false;
			}
			if (!Directory.Exists(tbSiteDirectory.Text))
			{
				ErrorBox("gh-pages Directory Doesn't Exist.");
				return false;
			}
			vFile = Path.Combine(tbOctgnDirectory.Text , "currentversion.xml");
			if (!File.Exists(vFile))
			{
				ErrorBox("Octgn Directory is invalid.");
				return false;
			}
			indexFile = Path.Combine(tbSiteDirectory.Text , "index.html");
			if(!File.Exists(indexFile))
			{
				ErrorBox("gh-pages Directory is invalid.");
				return false;
			}
			oldVersion = CheckVersionFile();
			if(oldVersion == null)
			{
				ErrorBox("Invalid OCTGN directory, or bad 'currentversion.xml' file.");
				return false;
			}
			return true;
		}

		private void ErrorBox(string message) { MessageBox.Show(message , "Error" , MessageBoxButtons.OK , MessageBoxIcon.Error); }

		private void button1_Click(object sender, EventArgs e) 
		{ 
			FolderBrowserDialog f = new FolderBrowserDialog();
			var dr = f.ShowDialog();
			if(dr == DialogResult.OK)
			{
				tbOctgnDirectory.Text = f.SelectedPath;
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog f = new FolderBrowserDialog();
			var dr = f.ShowDialog();
			if (dr == DialogResult.OK)
			{
				tbSiteDirectory.Text = f.SelectedPath;
			}
		}
	}
}
