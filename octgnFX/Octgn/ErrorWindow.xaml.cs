using System;
using System.Windows;

namespace Octgn
{
    public partial class ErrorWindow : Window
    {
        public ErrorWindow()
        {
            InitializeComponent();
        }

        public ErrorWindow(Exception ex)
            : this()
        {
            detailsBox.Text = ex.ToString();
        }

        public ErrorWindow(string ex)
            : this()
        {
            detailsBox.Text = ex;
        }

        private void CopyDetails(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Clipboard.SetText(detailsBox.Text);
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}