using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OCTIDE
{
    public partial class CreateGameForm : Form
    {
        public CreateGameForm()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.None;
            ButtonBrowse.Click += this.ButtonBrowseClick;
            ButtonCreate.Click += this.ButtonCreateClick;
            ButtonCancel.Click += this.ButtonCancelClick;
        }

        private void ButtonBrowseClick(object sender, EventArgs e)
        {
            var of = new System.Windows.Forms.FolderBrowserDialog();
            var result = of.ShowDialog();
            if (result != DialogResult.OK) return;
            TextBoxLocation.Text = of.SelectedPath;
        }

        private void ButtonCreateClick(object sender, EventArgs e)
        {
            try
            {
                GameEditorState.CreateGame(TextBoxLocation.Text, TextBoxGameName.Text);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (ValidationException ex)
            {
                LabelError.Text = ex.Message;
            }
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
