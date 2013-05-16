using System;
using System.Windows;

namespace Octgn.Windows
{
    public partial class ErrorWindow
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
            try
            {
                try
                {
                    DialogResult = true;
                }
                catch
                {
                }
                this.Close();
            }
            catch
            {
            }
        }
    }
}