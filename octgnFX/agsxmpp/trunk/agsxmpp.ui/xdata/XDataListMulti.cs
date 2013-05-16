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
	/// Summary description for XDataListMulti.
	/// </summary>
	public class XDataListMulti : XDataFieldControl
	{
		internal System.Windows.Forms.Label label;
		private System.Windows.Forms.CheckedListBox checkedListBox;
		
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
	

		public XDataListMulti()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			

			this.label = new System.Windows.Forms.Label();            
			this.checkedListBox = new System.Windows.Forms.CheckedListBox();            
			this.SuspendLayout();
			// 
			// label
			// 			
			this.label.TextAlign = ContentAlignment.TopLeft;
            this.label.BackColor = Color.Transparent;
			// 
			// checkedListBox
			// 
			this.checkedListBox.Height = MULTILINE_HEIGHT;
			// 
			// XDataListMulti
			// 
			this.panelLeft.Controls.Add(this.checkedListBox);
			this.panelLeft.Controls.Add(this.label);			
			this.Resize += new System.EventHandler(this.XDataListMulti_Resize);
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
		
		public string[] Values
		{
			get
			{
				//checkedListBox
				int count = checkedListBox.CheckedItems.Count;
				string[] result = new string[count];
				for (int i = 0; i < count; i++)
				{					
					XDataListItem li = checkedListBox.CheckedItems[i] as XDataListItem;
					result[i] = li.Value;
				}
				return result;
			}
		}

		public void AddValue(string val, string label)
		{						
			AddValue(val, label, CheckState.Unchecked);
		}

		public void AddValue(string val, string label, CheckState check)
		{						
			XDataListItem item = new XDataListItem(label, val);				
			checkedListBox.Items.Add(item, check);		
		}

		public override bool Validate()
		{
			return true;
		}

		private void XDataListMulti_Resize(object sender, System.EventArgs e)
		{
			label.Location		= new Point(0, 0);
			label.Width			= (int) (label.Parent.Width / 3);
			int lblHeight		= GetTextHeight(label);
			if (lblHeight > checkedListBox.Height)
				label.Height = lblHeight;
			else
				label.Height = checkedListBox.Height;

			checkedListBox.Location	= new Point(label.Width, 0);
			checkedListBox.Width	= checkedListBox.Parent.Width - label.Width;

			this.Height = Math.Max(label.Height, checkedListBox.Height) + H_SPACE;
		}

	}
}
