using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Octgn.Launcher
{
    public partial class ChangeSetsProgressDialog
    {
        public ChangeSetsProgressDialog(string header = "Installing Sets...")
        {
            InitializeComponent();
            AAText.Text = header;
            Title = header;
        }

        public void UpdateProgress(int current, int max, string message, bool isError)
        {

            if (message != null)
                ShowMessage(message, isError ? Brushes.Red : null);
            Dispatcher.BeginInvoke(new Action(delegate
                                                  {
                                                      progress.Maximum = max;
                                                      progress.Value = current;

                                                      if (current == max) okBtn.Visibility = Visibility.Visible;
                                                  }));
        }

        public void ShowMessage(string message, Brush b = null)
        {
            Dispatcher.BeginInvoke(new Action(delegate
                                                  {
                                                      var p = (Paragraph)output.Document.Blocks.LastBlock;
                                                      if (p.Inlines.Count > 0) p.Inlines.Add(new LineBreak());
                                                      var run = new Run(message);
                                                      if (b != null) run.Foreground = b;
                                                      p.Inlines.Add(run);
                                                  }));
        }

        private void OkClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}