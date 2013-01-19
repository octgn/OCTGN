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
	/// Summary description for XDataCheckBox.
	/// </summary>
	public class XDataCheckBox : XDataFieldControl
	{
		private System.Windows.Forms.CheckBox checkBox;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public XDataCheckBox()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			
			this.checkBox = new System.Windows.Forms.CheckBox();
            
			this.SuspendLayout();
			// 
			// checkBox
			// 			
			this.checkBox.FlatStyle = FlatStyle.System;
            //this.checkBox.BackColor = Color.Transparent;
			
			// 
			// XDataCheckBox
			// 
			this.panelLeft.Controls.Add(this.checkBox);			
			this.Resize += new System.EventHandler(this.XDataCheckBox_Resize);
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

		public bool Value
		{
			get { return checkBox.Checked; }
			set { checkBox.Checked = value; }
		}

		public override string Text
		{
			get { return checkBox.Text; }
			set { checkBox.Text = value; }
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

		public override bool Validate()
		{
			// a checkbox needs no validation because it cant be wrong
			return true;
		}

		private void XDataCheckBox_Resize(object sender, System.EventArgs e)
		{
			checkBox.Location	= new Point(0,0);
			checkBox.Width		= this.checkBox.Parent.Width;
			checkBox.Height		= GetTextHeight(checkBox);
			
			this.Height =  checkBox.Height + H_SPACE;
		}
	}
}
