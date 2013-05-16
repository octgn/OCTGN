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
	/// Summary description for XDataTextBox.
	/// </summary>
	public class XDataTextBox : XDataFieldControl // System.Windows.Forms.UserControl
	{
		

		internal TextBox textBox;
		private System.Windows.Forms.Label label;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public XDataTextBox()
		{			
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			
			// TODO: Add any initialization after the InitializeComponent call
			this.textBox = new System.Windows.Forms.TextBox();
			this.label = new System.Windows.Forms.Label();
			this.SuspendLayout();
						
			this.label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label.BackColor = Color.Transparent;
			
			this.panelLeft.Controls.Add(this.textBox);
			this.panelLeft.Controls.Add(this.label);			
			this.Resize += new System.EventHandler(this.XDataTextBox_Resize);			
			this.ResumeLayout(false);

			errorProv.SetIconAlignment (this.textBox, ErrorIconAlignment.MiddleLeft);			
			errorProv.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.AlwaysBlink;
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// XDataTextBox
			// 
			this.Name = "XDataTextBox";

		}
		#endregion


		public bool Multiline
		{
			get { return textBox.Multiline; }
			set 
			{ 
				if (value == true)
				{					
					textBox.Multiline = true;
					textBox.Height		= MULTILINE_HEIGHT;
					textBox.ScrollBars	= ScrollBars.Vertical;
					label.TextAlign		= ContentAlignment.TopLeft;
				}
				else
				{
					textBox.Multiline	= false;
					label.TextAlign		= ContentAlignment.MiddleLeft;
				}
			
			}
		}

		public override string Text
		{
			get { return label.Text; }
			set { label.Text = value; }			
		}
		
		public string Value
		{
			get 
			{ 
				if (textBox.Text.Length > 0)
					return textBox.Text;
				else
					return null;
			}
			set { textBox.Text = value; }
		}

		public string[] Values
		{
			get { return textBox.Lines; }
			set { textBox.Lines = value; }
		}

		/// <summary>
		/// is this a password char textbox? (private)
		/// </summary>
		public bool IsPrivate
		{
			get { return textBox.PasswordChar == 0 ? false : true; }
			set 
			{ 
				if (value)
					textBox.PasswordChar = '*';
				else
					textBox.PasswordChar = (char) 0;
			}
		}

		public string[] Lines
		{
			get { return textBox.Lines; }
			set { textBox.Lines = value; }
		}

		private void XDataTextBox_Resize(object sender, System.EventArgs e)
		{
			label.Location		= new Point(0, 0);
			label.Width			= (int) (this.label.Parent.Width / 3);
			int lblHeight		= GetTextHeight(label);
			if (lblHeight > textBox.Height)
				label.Height = lblHeight;
			else
				label.Height = textBox.Height;

			textBox.Location	= new Point(label.Width, 0);
			textBox.Width		= textBox.Parent.Width - label.Width;

			this.Height = Math.Max(label.Height, textBox.Height) + H_SPACE;			
		}

		public override bool Validate()
		{
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

	}
}
