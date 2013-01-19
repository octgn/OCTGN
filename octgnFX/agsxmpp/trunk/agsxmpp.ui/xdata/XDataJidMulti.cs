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

namespace agsXMPP.ui.xdata
{
	/// <summary>
	/// Summary description for XDataJidMulti.
	/// </summary>
	public class XDataJidMulti : XDataTextBox
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public XDataJidMulti()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			base.Multiline = true;
			
//			
//			this.panelRight.Controls.Add(cmdAdd);
			//this.Resize += new System.EventHandler(this.XDataJidMulti_Resize);			
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

		public override bool Validate()
		{
			// TODO, validae Jids
			if (IsRequired && Value==null)
			{
				errorProv.SetError(this.textBox, "field is required.");
				return false;
			}
			else
			{
				errorProv.SetError(this.textBox, "");
				return true;
			}
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

	}
}
