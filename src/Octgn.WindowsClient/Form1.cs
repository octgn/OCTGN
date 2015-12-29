using System;
using System.Windows.Forms;
using Octgn.Client;
using System.Diagnostics;

namespace Octgn.WindowsClient
{
    public partial class Form1 : Form
    {
        public UIBackend UIBackend { get; set; }

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UIBackend = new UIBackend();
            UIBackend.Start(@"/../../../Octgn.WebApp");
            linkLabel1.Text = "http://localhost:" + UIBackend.Port + "/";
            Process.Start("http://localhost:" + UIBackend.Port + "/");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://localhost:" + UIBackend.Port + "/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UIBackend.PingClients();
        }
    }
}
