/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 *																					 *
 * Copyright (c) 2005-2009 by AG-Software												 *
 * All Rights Reserved.																 *
 *																					 *
 * You should have received a copy of the AG-Software Shared Source License			 *
 * along with this library; if not, email gnauck@ag-software.de to request a copy.   *
 *																					 *
 * For general enquiries, email gnauck@ag-software.de or visit our website at:		 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using agsXMPP.ui.xdata.Base;

namespace agsXMPP.ui.xdata
{
	/// <summary>
	/// Summary description for XDataLabel.
	/// </summary>
	public class XDataLabel : XDataFieldControl
	{
		private System.Windows.Forms.Label label;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public XDataLabel()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
            
			this.label = new System.Windows.Forms.Label();            
			this.SuspendLayout();			
			this.label.UseMnemonic = false;
            this.label.BackColor = Color.Transparent;
			this.panelLeft.Controls.Add(this.label);			
			this.Resize += new System.EventHandler(this.XDataLabel_Resize);			
			this.ResumeLayout(false);

			
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

		public string Value
		{
			get { return label.Text; }
			set { label.Text = value; }
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			
		}
		#endregion

		private void XDataLabel_Resize(object sender, System.EventArgs e)
		{
			label.Location	= new Point(0,0);
			label.Width		= label.Parent.Width;
			label.Height	= GetTextHeight(label);
			
			this.Height =  label.Height + H_SPACE;
		}

		public override bool Validate()
		{
			return true;
		}
	}
}
