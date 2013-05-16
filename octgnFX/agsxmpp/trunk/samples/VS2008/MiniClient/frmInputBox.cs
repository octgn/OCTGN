using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MiniClient
{
    public partial class frmInputBox : Form
    {
        public frmInputBox()
        {
            InitializeComponent();
        }

        public frmInputBox(string Prompt, string Title, string Default) : this()
        {
            this.lblMessage.Text = Prompt;
            this.Text = Title;
            this.txtInput.Text = Default;
        }
    
        private void cmdOK_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {            
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string Result
        {
            get { return txtInput.Text; }
        }
       
    }
}