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
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            var of = new System.Windows.Forms.FolderBrowserDialog();
            var result = of.ShowDialog();
            if (result != DialogResult.OK) return;
            textBoxLocation.Text = of.SelectedPath;
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            try
            {
                GameEditorState.CreateGame(textBoxLocation.Text, textBoxGameName.Text);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (ValidationException ex)
            {
                labelError.Text = ex.Message;
            }
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
