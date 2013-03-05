using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Octgn.Controls
{
    using System.Windows.Controls.Primitives;

    /// <summary>
    /// Interaction logic for InstallGame.xaml
    /// </summary>
    public partial class InstallGame : UserControl
    {
        public InstallGame()
        {
            InitializeComponent();
        }

        private void ListViewGameList_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (e == null || e.OriginalSource == null) return;
            var senderAsThumb = e.OriginalSource as Thumb;
            if (senderAsThumb == null || senderAsThumb.TemplatedParent == null) return;
            var header = senderAsThumb.TemplatedParent as GridViewColumnHeader;
            if (header == null) return;
            if (header.Column.ActualWidth < 20)
                header.Column.Width = 20;
            //if (header.Column.ActualWidth > 100)
            //    header.Column.Width = 100;
        }
    }
}
