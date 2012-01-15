using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using CassiniDev;

namespace SelfHostingCassiniDev
{
    public class Form1 : Form
    {
        private readonly CassiniDevServer _server;
        private readonly IContainer _components;
        private WebBrowser _webBrowser1;

        public Form1()
        {
            InitializeComponent();

            _server = new CassiniDevServer();

            // our content is Copy Always into bin
            _server.StartServer(Path.Combine(Environment.CurrentDirectory, "WebContent"));

            _webBrowser1.Navigate(_server.NormalizeUrl("Default.aspx"));
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _server.StopServer();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && (_components != null))
            {
                _components.Dispose();
            }
            base.Dispose(disposing);
        }


        private void InitializeComponent()
        {
            this._webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // _webBrowser1
            // 
            this._webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._webBrowser1.Location = new System.Drawing.Point(0, 0);
            this._webBrowser1.Name = "_webBrowser1";
            this._webBrowser1.Size = new System.Drawing.Size(355, 269);
            this._webBrowser1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(355, 269);
            this.Controls.Add(this._webBrowser1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);

        }
    }
}