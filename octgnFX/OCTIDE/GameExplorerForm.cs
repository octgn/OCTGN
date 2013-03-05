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

    public partial class GameExplorerForm : DockContent
    {
        public GameExplorerForm()
        {
            InitializeComponent();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
    }
}
