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

using agsXMPP.protocol.x;
using agsXMPP.protocol.x.data;

namespace agsXMPP.ui.xdata.Base
{
	public class XDataFieldControl : UserControl
	{
		internal System.Windows.Forms.Panel panelRight;
		internal System.Windows.Forms.Panel panelLeft;

		internal const int H_SPACE			= 6;		
		internal const int MULTILINE_HEIGHT = 72;

		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.PictureBox picRequired;
		internal System.Windows.Forms.ErrorProvider errorProv;
		
		//private Field	m_Field;		

		public XDataFieldControl()
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

		#region << Properties >>
		internal string m_Var = null;

		public string Var
		{
			get { return m_Var; }
			set { m_Var = value; }
		}
//		public Field Field
//		{
//			get { return m_Field; }
//			set { m_Field = value; }
//		}
		
		/// <summary>
		/// Is this a required field?
		/// Show an icon nexr to all required fields
		/// </summary>
		public bool IsRequired
		{
			get { return picRequired.Visible; }
			set { picRequired.Visible = value; }
		}
		#endregion

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(XDataFieldControl));
			this.panelRight = new System.Windows.Forms.Panel();
			this.picRequired = new System.Windows.Forms.PictureBox();
			this.panelLeft = new System.Windows.Forms.Panel();
			this.errorProv = new System.Windows.Forms.ErrorProvider();
			this.panelRight.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelRight
			// 
			this.panelRight.Controls.Add(this.picRequired);
			this.panelRight.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelRight.Location = new System.Drawing.Point(120, 0);
			this.panelRight.Name = "panelRight";
			this.panelRight.Size = new System.Drawing.Size(32, 64);
			this.panelRight.TabIndex = 0;
			// 
			// picRequired
			// 
			this.picRequired.Image = ((System.Drawing.Image)(resources.GetObject("picRequired.Image")));
			this.picRequired.Location = new System.Drawing.Point(2, 2);
			this.picRequired.Name = "picRequired";
			this.picRequired.Size = new System.Drawing.Size(16, 16);
			this.picRequired.TabIndex = 0;
			this.picRequired.TabStop = false;
			this.picRequired.Visible = false;
			// 
			// panelLeft
			// 
			this.panelLeft.BackColor = System.Drawing.SystemColors.Control;
			this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelLeft.Location = new System.Drawing.Point(0, 0);
			this.panelLeft.Name = "panelLeft";
			this.panelLeft.Size = new System.Drawing.Size(120, 64);
			this.panelLeft.TabIndex = 1;
			// 
			// errorProv
			// 
			this.errorProv.BlinkRate = 1000;
			this.errorProv.ContainerControl = this;
			// 
			// XDataFieldControl
			// 
			this.Controls.Add(this.panelLeft);
			this.Controls.Add(this.panelRight);
			this.Name = "XDataFieldControl";
			this.Size = new System.Drawing.Size(152, 64);
			this.panelRight.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
		
		internal int GetTextHeight(Control ctl)
		{
			SizeF textSize = ctl.CreateGraphics().MeasureString(ctl.Text, ctl.Font, ctl.Width, StringFormat.GenericDefault);
			return (int)(textSize.Height);
		}
		
		public virtual new bool Validate()
		{
			return false;
		}
		
	}
}

