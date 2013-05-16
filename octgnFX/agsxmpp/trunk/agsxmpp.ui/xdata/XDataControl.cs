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

using agsXMPP;
using agsXMPP.protocol;
using agsXMPP.protocol.x.data;

using agsXMPP.ui.xdata.Base;

namespace agsXMPP.ui.xdata
{
	/// <summary>
	/// Summary for XDataControl
	/// </summary>
	public class XDataControl : System.Windows.Forms.UserControl
	{
        
		/// <summary>
		/// horinzontal space between title and instructions
		/// </summary>
		private const int H_SPACE			= 6;
		/// <summary>
		/// Space between instructions and the beginning of the fields.
		/// </summary>
		private const int H_SPACE_FIELDS	= 6;



		private System.Windows.Forms.Panel panelButtons;

		/// <summary>
		/// This button is public for changing the text in translations
		/// </summary>		
		public System.Windows.Forms.Button cmdCancel;
		/// <summary>
		/// This button is public for changing the text in translations
		/// </summary>
		public System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Panel panelTop;
		public System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblInstructions;
		private System.Windows.Forms.Panel panelFields;
		public System.Windows.Forms.Panel panelLegend;
		private System.Windows.Forms.PictureBox pictureBox1;
		public System.Windows.Forms.Label lblLegendRequired;
	
		/// <summary> 
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        #region << Properties >>
        private Jid m_From;
        
        /// <summary>
        /// Property which could be used to store the sender of this form
        /// </summary>
        public Jid From
        {
            get { return m_From; }
            set { m_From = value; }
        }
        #endregion

        #region << Events >>
        public event ObjectHandler	OnCancel;
		public event ObjectHandler	OnOk;
		#endregion

        

		public XDataControl()
		{
			// Dieser Aufruf ist für den Windows Form-Designer erforderlich.
			InitializeComponent();

			// TODO: Initialisierungen nach dem Aufruf von InitializeComponent hinzufügen

		}

		/// <summary> 
		/// Die verwendeten Ressourcen bereinigen.
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

		#region Vom Komponenten-Designer generierter Code
		/// <summary> 
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XDataControl));
            this.panelButtons = new System.Windows.Forms.Panel();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblInstructions = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelLegend = new System.Windows.Forms.Panel();
            this.lblLegendRequired = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panelFields = new System.Windows.Forms.Panel();
            this.panelButtons.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.panelLegend.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.cmdOK);
            this.panelButtons.Controls.Add(this.cmdCancel);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 173);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(372, 40);
            this.panelButtons.TabIndex = 1;
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdOK.Location = new System.Drawing.Point(212, 8);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(72, 24);
            this.cmdOK.TabIndex = 1;
            this.cmdOK.Text = "OK";
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdCancel.Location = new System.Drawing.Point(292, 8);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(72, 24);
            this.cmdCancel.TabIndex = 0;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.lblInstructions);
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(372, 56);
            this.panelTop.TabIndex = 3;
            // 
            // lblInstructions
            // 
            this.lblInstructions.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInstructions.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstructions.Location = new System.Drawing.Point(0, 24);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Size = new System.Drawing.Size(372, 16);
            this.lblInstructions.TabIndex = 1;
            this.lblInstructions.UseMnemonic = false;
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.SystemColors.Control;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(372, 24);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTitle.UseMnemonic = false;
            // 
            // panelLegend
            // 
            this.panelLegend.BackColor = System.Drawing.SystemColors.Info;
            this.panelLegend.Controls.Add(this.lblLegendRequired);
            this.panelLegend.Controls.Add(this.pictureBox1);
            this.panelLegend.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelLegend.Location = new System.Drawing.Point(0, 141);
            this.panelLegend.Name = "panelLegend";
            this.panelLegend.Size = new System.Drawing.Size(372, 32);
            this.panelLegend.TabIndex = 4;
            // 
            // lblLegendRequired
            // 
            this.lblLegendRequired.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLegendRequired.Location = new System.Drawing.Point(32, 8);
            this.lblLegendRequired.Name = "lblLegendRequired";
            this.lblLegendRequired.Size = new System.Drawing.Size(332, 16);
            this.lblLegendRequired.TabIndex = 1;
            this.lblLegendRequired.Text = "required fields";
            this.lblLegendRequired.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 16);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // panelFields
            // 
            this.panelFields.AutoScroll = true;
            this.panelFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelFields.Location = new System.Drawing.Point(0, 56);
            this.panelFields.Name = "panelFields";
            this.panelFields.Size = new System.Drawing.Size(372, 85);
            this.panelFields.TabIndex = 5;
            // 
            // XDataControl
            // 
            this.Controls.Add(this.panelFields);
            this.Controls.Add(this.panelLegend);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelButtons);
            this.Name = "XDataControl";
            this.Size = new System.Drawing.Size(372, 213);
            this.Resize += new System.EventHandler(this.XDataControl_Resize);
            this.panelButtons.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.panelLegend.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		public bool ShowLegend
		{
			get { return panelLegend.Visible; }
			set { panelLegend.Visible = value; }
		}

        public bool ShowButtons
        {
            get { return panelButtons.Visible; }
            set { panelButtons.Visible = value; }
        }

        /// <summary>
        /// Creates the Xdata from from the given XData Element
        /// </summary>
        /// <param name="xdata"></param>
		public void CreateForm(Data xdata)
		{
			this.SuspendLayout();
			
			this.panelFields.SuspendLayout();
			this.panelFields.Controls.Clear();			
			
			lblTitle.Text			= xdata.Title;
			lblInstructions.Text	= xdata.Instructions;
			
			XDataControl_Resize(this, null);

			Field[] fields = xdata.GetFields();
			for (int i = fields.Length -1; i >= 0; i--)
			{				
				CreateField(fields[i], i);
			}
			
			this.panelFields.ResumeLayout();
			
			this.ResumeLayout();
		}

        public void CreateForm(agsXMPP.protocol.iq.search.Search search)
        {
            /*
             * Send: <iq xmlns="jabber:client" id="agsXMPP_9" type="get" to="users.jabber.org"><query xmlns="jabber:iq:search" />
             * </iq> 
                Recv: <iq xmlns="jabber:client" to="gnauck@amessage.de/SharpIM" type="result" id="agsXMPP_9" from="users.jabber.org">
             * <query xmlns="jabber:iq:search">
             * <instructions>Find a contact by entering the search criteria in the given fields. Note: Each field supports wild card searches (%)</instructions>
             * <first />
             * <last />
             * <nick />
             * <email />
             * </query></iq> 
             */
            
            this.SuspendLayout();

            this.panelFields.SuspendLayout();
            this.panelFields.Controls.Clear();

            //lblTitle.Text = xdata.Title;
            lblInstructions.Text =  search.Instructions;

            XDataControl_Resize(this, null);

            agsXMPP.Xml.Dom.ElementList list = new agsXMPP.Xml.Dom.ElementList();
            foreach (agsXMPP.Xml.Dom.Node n in search.ChildNodes)
            {
                if (n.NodeType == agsXMPP.Xml.Dom.NodeType.Element)
                    list.Add(n as agsXMPP.Xml.Dom.Element);                
            }

            for (int i = list.Count -1; i >= 0; i--)
			{                 
                agsXMPP.Xml.Dom.Element el = list.Item(i);
                if (el.TagName == "key")
                {
                    CreateField(CreateDummyHiddenField(el.TagName, el.Value), i);
                }
                else if (el.TagName == "instructions")
                {

                }
                else
                {
                    CreateField(CreateDummyTextField(el.TagName, el.TagName), i);
                }
                
            }
           
            this.panelFields.ResumeLayout();

            this.ResumeLayout();
        }

        /// <summary>
        /// helper function that maps old jabber search and register fields to our XData fields
        /// </summary>
        /// <param name="var"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        private Field CreateDummyTextField(string var, string label)
        {
            return new Field(var, label, FieldType.Text_Single);
        }

        /// <summary>
        /// helper function that maps old jabber hidden search and register fields to our XData fields
        /// </summary>
        /// <param name="var"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private Field CreateDummyHiddenField(string var, string val)
        {
            Field field = new Field(var, var, FieldType.Hidden);
            field.SetValue(val);
            return field;
        }

        /// <summary>
        /// Create a form element from a given field
        /// </summary>
        /// <param name="field"></param>
        /// <param name="tabIdx"></param>
        /// <returns></returns>
		private XDataFieldControl CreateField(Field field, int tabIdx)
		{		
			XDataFieldControl m_DataControl;
//			panelFields.SuspendLayout();

			FieldType m_Type = field.Type;
			string lbl;
			switch (m_Type)
			{
				case FieldType.Hidden:
					m_DataControl = new XDataHidden();
					((XDataHidden) m_DataControl).Value		= field.GetValue();
					
					break;
				case FieldType.Fixed:
					m_DataControl = new XDataLabel();
					
					((XDataLabel) m_DataControl).Value		= field.GetValue();			
					
					break;
				case FieldType.Boolean:
					m_DataControl = new XDataCheckBox();
					
					lbl = field.Label;
					if (lbl == null)
						lbl = field.Var;
					
					((XDataCheckBox) m_DataControl).Text	= lbl;
					((XDataCheckBox) m_DataControl).Value	= field.GetValueBool();					
											
					break;				
				case FieldType.Text_Private:
					m_DataControl = new XDataTextBox();
					
					lbl = field.Label;
					if (lbl == null)
						lbl = field.Var;

					((XDataTextBox) m_DataControl).Text			= lbl;
					((XDataTextBox) m_DataControl).Value		= field.GetValue();
					((XDataTextBox) m_DataControl).IsPrivate	= true;
					
					break;
				case FieldType.Text_Single:
					m_DataControl = new XDataTextBox();
					
					lbl = field.Label;
					if (lbl == null)
						lbl = field.Var;

					((XDataTextBox) m_DataControl).Text			= lbl;
					((XDataTextBox) m_DataControl).Value		= field.GetValue();
					
					break;
				case FieldType.Text_Multi:
					m_DataControl = new XDataTextBox();
					
					lbl = field.Label;
					if (lbl == null)
						lbl = field.Var;

					((XDataTextBox) m_DataControl).Text			= lbl;
					((XDataTextBox) m_DataControl).Multiline	= true;
					((XDataTextBox) m_DataControl).Lines		= field.GetValues();										
					
					break;				
				case FieldType.List_Single:
					m_DataControl = new XDataListSingle();

					lbl = field.Label;
					if (lbl == null)
						lbl = field.Var;

					((XDataListSingle) m_DataControl).Text		= lbl;
					
					foreach(Option o in field.GetOptions())
					{
						((XDataListSingle) m_DataControl).AddValue(o.GetValue(), o.Label);
					}					
					((XDataListSingle) m_DataControl).Value		= field.GetValue();
					
					break;
				case FieldType.List_Multi:
					m_DataControl = new XDataListMulti();
					
					lbl = field.Label;
					if (lbl == null)
						lbl = field.Var;
					
					((XDataListMulti) m_DataControl).Text		= lbl;
					
					foreach(Option o in field.GetOptions())
					{
						string optionValue = o.GetValue();
						CheckState chk;
						if (field.HasValue(optionValue))
							chk = CheckState.Checked;
						else
							chk = CheckState.Unchecked;								
						
						((XDataListMulti) m_DataControl).AddValue(optionValue, o.Label, chk);
					}

					break;
				case FieldType.Jid_Single:
					m_DataControl = new XDataJidSingle();

					lbl = field.Label;
					if (lbl == null)
						lbl = field.Var;

					((XDataJidSingle) m_DataControl).Text	= lbl;
					((XDataJidSingle) m_DataControl).Value	= field.Value;
					break;
				case FieldType.Jid_Multi:
					m_DataControl = new XDataJidMulti();
					
					lbl = field.Label;
					if (lbl == null)
						lbl = field.Var;

					((XDataJidMulti) m_DataControl).Text	= lbl;
					((XDataJidMulti) m_DataControl).Lines	= field.GetValues();
					
					break;
				default:
					m_DataControl = new XDataLabel();

					break;
			}
			m_DataControl.IsRequired	= field.IsRequired;
			m_DataControl.m_Var			= field.Var;
			m_DataControl.Parent		= panelFields;
			m_DataControl.Location		= new Point(0, 0);
			m_DataControl.Width			= panelFields.Width;			
			m_DataControl.Dock			= DockStyle.Top;
			
			if (m_Type != FieldType.Hidden && 
				m_Type != FieldType.Fixed)
			{
				m_DataControl.TabIndex	= tabIdx;
				m_DataControl.Focus();
			}


			panelFields.Controls.Add(m_DataControl);

//			panelFields.ResumeLayout();
			lbl = null;
			return m_DataControl;
		}

		internal int GetTextHeight(Control ctl)
		{
			SizeF textSize = ctl.CreateGraphics().MeasureString(ctl.Text, ctl.Font, ctl.Width, StringFormat.GenericDefault);
			return (int)(textSize.Height);
		}
		
		private void XDataControl_Resize(object sender, System.EventArgs e)
		{
			lblTitle.Height = GetTextHeight(lblTitle) + H_SPACE;
			lblInstructions.Height = GetTextHeight(lblInstructions);
			panelTop.Height = lblTitle.Height + lblInstructions.Height  + H_SPACE_FIELDS;
		}

		private void cmdOK_Click(object sender, System.EventArgs e)
		{
			if( Validate() )
			{
				//Console.WriteLine(CreateResponse().ToString());
				if (OnOk!=null)
					OnOk(this);
			}
		}

        /// <summary>
        /// Validates the xData from. You should call this function for validation 
        /// before submitting a response, and only submit if the result is true.
        /// </summary>
        /// <returns></returns>
		public new bool Validate()
		{
			int errCount = 0;
			foreach(XDataFieldControl ctl in panelFields.Controls)
			{
				if (!ctl.Validate())
					errCount++;
			}
			return errCount == 0 ? true : false;
		}

        /// <summary>
        /// Creates the XData Response Element from the entered Data in the GUI Form
        /// </summary>
        /// <returns></returns>
		public Data CreateResponse()
		{
			Data data = new Data(XDataFormType.submit);
			foreach(XDataFieldControl ctl in panelFields.Controls)
			{
				Type type = ctl.GetType();

				if ( type == typeof(XDataTextBox) )
				{
					Field f;
					XDataTextBox txt = ctl as XDataTextBox;
					if (txt.Multiline == true)
					{
						f = new Field(FieldType.Text_Multi);						
					}	
					else
					{
						if (txt.IsPrivate)
							f = new Field(FieldType.Text_Private);
						else
							f = new Field(FieldType.Text_Single);
						
					}
					f.AddValues(txt.Values);
					f.Var = txt.Var;
					data.AddField(f);

				}
				else if ( type == typeof(XDataHidden) )
				{
					Field f;
					XDataHidden hidden = ctl as XDataHidden;
					
					f = new Field(FieldType.Hidden);
					
					f.AddValue(hidden.Value);
					f.Var = hidden.Var;
					
					data.AddField(f);
				}
				else if ( type == typeof(XDataJidSingle) )
				{
					XDataJidSingle jids = ctl as XDataJidSingle;
					Field f = new Field(FieldType.Jid_Single);
					f.SetValue(jids.Value);
					f.Var = jids.Var;
					data.AddField(f);
				}
				else if ( type == typeof(XDataJidMulti) )
				{
					XDataJidMulti jidm = ctl as XDataJidMulti;
					Field f = new Field(FieldType.Jid_Multi);
					f.AddValues(jidm.Values);
					f.Var = jidm.Var;
					data.AddField(f);
					
				}
				else if ( type == typeof(XDataCheckBox) )
				{
					XDataCheckBox chk = ctl as XDataCheckBox;
					Field f = new Field(FieldType.Boolean);
					f.SetValueBool(chk.Value);
					f.Var = chk.Var;
					data.AddField(f);
				}
				else if ( type == typeof(XDataListMulti) )
				{
					XDataListMulti listm = ctl as XDataListMulti;
					Field f = new Field(FieldType.List_Multi);					
					f.AddValues(listm.Values);
					f.Var = listm.Var;
					data.AddField(f);
				}
				else if ( type == typeof(XDataListSingle) )
				{
					XDataListSingle lists = ctl as XDataListSingle;
					Field f = new Field(FieldType.List_Single);					
					f.SetValue(lists.Value);
					f.Var = lists.Var;
					data.AddField(f);
				}				
			}

			return data;
		}


        /// <summary>
        /// Build the response from a old jabber search form
        /// </summary>
        /// <returns></returns>
        public agsXMPP.protocol.iq.search.Search CreateSearchResponse()
        {
            agsXMPP.protocol.iq.search.Search search = new agsXMPP.protocol.iq.search.Search();
            
            foreach (XDataFieldControl ctl in panelFields.Controls)
            {
                Type type = ctl.GetType();

                if (type == typeof(XDataTextBox))
                {                    
                    XDataTextBox txt = ctl as XDataTextBox;
                    if (txt.Value != null)
                    {
                        search.SetTag(txt.Var, txt.Value);                        
                    }
                }
                else if (type == typeof(XDataHidden))
                {
                    XDataHidden hidden = ctl as XDataHidden;
                    search.SetTag(hidden.Var, hidden.Value);
                }
            }

            return search;
        }

		private void cmdCancel_Click(object sender, System.EventArgs e)
		{
			if (OnCancel!=null)
				OnCancel(this);
		}		

	}
}
