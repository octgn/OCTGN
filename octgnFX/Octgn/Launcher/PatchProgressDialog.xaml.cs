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

namespace Octgn.Launcher
{
  public partial class PatchProgressDialog : Window
  {
    public PatchProgressDialog()
    {
      InitializeComponent();
    }

    public void UpdateProgress(int current, int max, string message, bool isError)
    {
      Dispatcher.BeginInvoke(new Action(delegate
        {
          progress.Maximum = max;
          progress.Value = current;

          if (message != null)
          {
            var p = (Paragraph)output.Document.Blocks.LastBlock;
            if (p.Inlines.Count > 0) p.Inlines.Add(new LineBreak());
            var run = new Run(message);
            if (isError) run.Foreground = Brushes.Red;
            p.Inlines.Add(run);
          }

          if (current == max) okBtn.Visibility = Visibility.Visible;
        }));
    }

    private void OKClicked(object sender, RoutedEventArgs e)
    {
      Close();
    }
  }
}
