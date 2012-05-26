using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Octgn.Releaser
{
	public partial class page2 : Form
	{
		public page2()
		{
			InitializeComponent();
		}

		private void page2_Load(object sender, EventArgs e) 
		{ 
			tbOldVersion.Text = Program.OldVersion.ToString();
			Program.NewVersion = new Version(Program.OldVersion.Major , Program.OldVersion.Minor , Program.OldVersion.Build ,
			                                 Program.OldVersion.Revision + 1);
			tbNewVersion.Text = Program.NewVersion.ToString();
		}

		private void button1_Click(object sender, EventArgs e) 
		{ 
			Version v = null;
			if(Version.TryParse(tbNewVersion.Text,out v))
			{
				Program.NewVersion = v;
			}
		}

		private void page2_FormClosed(object sender, FormClosedEventArgs e) { Program.Form1.Close(); }
	}
}
