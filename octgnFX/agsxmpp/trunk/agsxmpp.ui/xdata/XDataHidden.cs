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
using System.Windows.Forms;

namespace agsXMPP.ui.xdata
{
	/// <summary>
	/// This is a hidden field, so we dont have to show it in teh UI.
	/// This makes this field different than all the others.
	/// </summary>
	public class XDataHidden : agsXMPP.ui.xdata.Base.XDataFieldControl
	{
		private System.ComponentModel.IContainer components = null;

		public XDataHidden()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// XDataHidden
			// 
			this.Name = "XDataHidden";
			this.Resize += new System.EventHandler(this.XDataHidden_Resize);

		}
		#endregion
		
		#region << Properties >>
		private string m_Value = null;

		public string Value
		{
			get {  return m_Value; }			
			set { m_Value = value; }
		}
		#endregion

		public override bool Validate()
		{
			// a hidden fields needs no validation because it cant be wrong
			return true;
		}

		private void XDataHidden_Resize(object sender, System.EventArgs e)
		{
			this.Height	= 0;
		}
	}
}

