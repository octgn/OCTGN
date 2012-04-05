using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Octgn.Windows
{
    public partial class PatchProgressDialog
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
                                                          var p = (Paragraph) output.Document.Blocks.LastBlock;
                                                          if (p.Inlines.Count > 0) p.Inlines.Add(new LineBreak());
                                                          var run = new Run(message);
                                                          if (isError) run.Foreground = Brushes.Red;
                                                          p.Inlines.Add(run);
                                                      }

                                                      if (current == max) okBtn.Visibility = Visibility.Visible;
                                                  }));
        }

        private void OkClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}