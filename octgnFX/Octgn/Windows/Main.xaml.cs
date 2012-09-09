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
using System.Windows.Shapes;
using CustomWindow;
using Octgn.Controls;
using WPF.Themes;

namespace Octgn.Windows
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : OctgnChrome
    {
        public Main()
        {
            InitializeComponent();
        }

        private void ThemeOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var mi = routedEventArgs.OriginalSource as MenuItem;
            this.ApplyTheme(mi.Header.ToString());
        }

        private void borderHeader_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void Thumb_DragDelta_1(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.Width + e.HorizontalChange > 10)
                this.Width += e.HorizontalChange;
            if (this.Height + e.VerticalChange > 10)
                this.Height += e.VerticalChange;
        }

        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {
            var w = new AboutWindow();
            w.ShowDialog();
        }
    }
}
