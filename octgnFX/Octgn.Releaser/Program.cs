using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Octgn.Releaser
{
	public static class Program
	{
		public static Version OldVersion { get; set; }
		public static Version NewVersion { get; set; }
		public static string OctgnDirectory { get; set; }
		public static string SiteDirectory { get; set; }
		public static string VersionFile { get; set; }
		public static Form1 Form1 { get; set; }

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Form1 = new Form1();
			Application.Run(Form1);
		}
	}
}
