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
    using WeifenLuo.WinFormsUI.Docking;

    public partial class MainWindow : Form
    {
        private GameExplorerForm GameExplorer { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            GameEditorState.GameOpened += GameEditorStateOnGameOpened;
            GameEditorState.GameClosed += GameEditorStateOnGameClosed;
            GameEditorState.GameCreated += GameEditorStateOnGameCreated;
        }

        private void GameEditorStateOnGameCreated(object sender, EventArgs eventArgs)
        {
            try
            {
                GameEditorState.Get().Save();
            }
            catch (UserMessageException e)
            {
                MessageBox.Show(this, e.Message, "OCTIDE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Invoke(new Action(() => { this.Text = "OCTIDE ♣ " + GameEditorState.Get().Name; this.Invalidate(); }));
        }

        private void GameEditorStateOnGameClosed(object sender, EventArgs eventArgs)
        {
            if (GameExplorer != null)
            {
                GameExplorer.Close();
                GameExplorer = null;
            }
        }

        private void GameEditorStateOnGameOpened(object sender, EventArgs eventArgs)
        {
            GameExplorer = new GameExplorerForm();
            GameExplorer.Show(dockPanel1,DockState.DockLeft);
            this.Invoke(new Action(() => { this.Text = "OCTIDE ♣ " + GameEditorState.Get().Name; this.Invalidate();}));
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new CreateGameForm().ShowDialog(this);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var of = new OpenFileDialog();
                //Text files (*.txt)|*.txt|
                of.Filter = "OCTIDE Project File (*.o8proj)|*.o8proj";
                if (of.ShowDialog(this) != DialogResult.OK) return;

                if (GameEditorState.Get() != null)
                {
                    var res = MessageBox.Show(
                        this,
                        "Do you want to save changes to your current project?",
                        "OCTIDE",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                    switch (res)
                    {
                        case DialogResult.Yes:
                            GameEditorState.Get().Save();
                            GameEditorState.Close();
                            GameEditorState.Open(of.FileName);
                            return;
                        case DialogResult.No:
                            GameEditorState.Close();
                            GameEditorState.Open(of.FileName);
                            return;
                        default:
                            return;
                    }
                }
                GameEditorState.Open(of.FileName);
            }
            catch (UserMessageException ex)
            {
                MessageBox.Show(this, ex.Message, "OCTIDE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
