using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using agsXMPP.protocol.x.data;

namespace MiniClient
{
	/// <summary>
	/// Summary description for frmXData.
	/// </summary>
	public class frmXData : System.Windows.Forms.Form
	{
		private agsXMPP.ui.xdata.XDataControl xDataControl1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmXData(Data data)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.xDataControl1.CreateForm(data);
		}

		/// <summary>
		/// Clean up any resources being used.
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.xDataControl1 = new agsXMPP.ui.xdata.XDataControl();
			this.SuspendLayout();
			// 
			// xDataControl1
			// 
			this.xDataControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.xDataControl1.Location = new System.Drawing.Point(0, 0);
			this.xDataControl1.Name = "xDataControl1";
			this.xDataControl1.ShowLegend = true;
			this.xDataControl1.Size = new System.Drawing.Size(288, 222);
			this.xDataControl1.TabIndex = 0;
			// 
			// frmXData
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(288, 222);
			this.Controls.Add(this.xDataControl1);
			this.Name = "frmXData";
			this.Text = "frmXData";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
