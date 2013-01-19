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
	/// Summary description for XDataListSingle.
	/// </summary>
	public class XDataListSingle : XDataFieldControl
	{
		private System.Windows.Forms.Label label;
		private System.Windows.Forms.ComboBox comboBox;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		
		public XDataListSingle()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			this.label = new System.Windows.Forms.Label();            
			this.comboBox = new System.Windows.Forms.ComboBox();            
			this.SuspendLayout();
                        
			// 
			// label
			// 			
			this.label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label.BackColor = Color.Transparent;
			// 
			// comboBox
			// 
			this.comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;						
			// 
			// XDataListSingle
			// 
			this.panelLeft.Controls.Add(this.comboBox);
			this.panelLeft.Controls.Add(this.label);			
			this.Resize += new System.EventHandler(this.XDataListSingle_Resize);			
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			

		}
		#endregion


		public override string Text
		{
			get { return label.Text; }
			set { label.Text = value; }			
		}

		public string Value
		{
			get
			{
				XDataListItem li = (XDataListItem) comboBox.SelectedItem;
				return li.Value;
			}
			set
			{
				for (int i=0; i < comboBox.Items.Count; i++)
				{
					XDataListItem li = comboBox.Items[i] as XDataListItem;
					if (li.Value == value)
					{
						comboBox.SelectedIndex = i;
						break;
					}
				}				
			}
		}

		public void AddValue(string val, string label)
		{			
			XDataListItem item = new XDataListItem(label, val);
			comboBox.Items.Add(item);			
		}

		public override bool Validate()
		{
			return true;
		}

		private void XDataListSingle_Resize(object sender, System.EventArgs e)
		{
			label.Location		= new Point(0, 0);
			label.Width			= (int) (label.Parent.Width / 3);
			int lblHeight		= GetTextHeight(label);
			if (lblHeight > comboBox.Height)
				label.Height = lblHeight;
			else
				label.Height = comboBox.Height;

			comboBox.Location	= new Point(label.Width, 0);
			comboBox.Width		= comboBox.Parent.Width - label.Width;

			this.Height = Math.Max(label.Height, comboBox.Height) + H_SPACE;
		}
	}
}
