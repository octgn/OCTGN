using System;
using System.Windows;

namespace Octgn
{
    public partial class MessageWindow : Window
    {
        public MessageWindow(string text)
        {
            InitializeComponent();
            contentBlock.Text = text;
        }

        protected void OKClicked(object sender, EventArgs e)
        {
            DialogResult = true;
        }
    }
}