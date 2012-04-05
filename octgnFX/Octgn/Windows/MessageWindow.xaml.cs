using System;

namespace Octgn.Windows
{
    public partial class MessageWindow
    {
        public MessageWindow(string text)
        {
            InitializeComponent();
            contentBlock.Text = text;
        }

        protected void OkClicked(object sender, EventArgs e)
        {
            DialogResult = true;
        }
    }
}