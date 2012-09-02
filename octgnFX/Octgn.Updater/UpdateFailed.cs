using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Octgn.Updater
{
    public partial class UpdateFailed : Form
    {
        private Exception _exception;
        public UpdateFailed():this(null)
        {
        }
        public UpdateFailed(Exception e)
        {
            InitializeComponent();
            _exception = e;
            if (_exception != null)
                textBox1.Text += _exception.Message + Environment.NewLine +
                                 Environment.NewLine;
            foreach (var a in Updater.LogList)
                textBox1.Text += a + Environment.NewLine;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {


            var postURI = new Uri("http://www.octgn.info/errorreport.php");
            string postDataTemplate = @"name=updater&assembly=%%assembly%%&version=%%version%%&error=%%error%%";

            string errorData = textBox1.Text;

            var postDataString = postDataTemplate.Replace("%%error%%", errorData)
                .Replace("%%assembly%%", Assembly.GetExecutingAssembly().GetName().Name)
                .Replace("%%version%%", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            tbResult.Text += postDataString + Environment.NewLine + Environment.NewLine;

            SendPost(postURI.ToString(), postDataString);
            Application.Exit();            

        }

        public string SendPost(string url, string postData)
        {
            string webpageContent = string.Empty;

            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = byteArray.Length;

                using (Stream webpageStream = webRequest.GetRequestStream())
                {
                    webpageStream.Write(byteArray, 0, byteArray.Length);
                }

                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    tbResult.Text += ((HttpWebResponse) webResponse).StatusDescription + Environment.NewLine;
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        webpageContent = reader.ReadToEnd();
                        tbResult.Text += webpageContent + Environment.NewLine;
                    }
                }
            }
            catch (Exception ex)
            {
                var res = MessageBox.Show("There was an error uploading this bug request. Do you want to chat with someone for help?", "Submit Error", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (res == DialogResult.Yes)
                    Process.Start("http://www.octgn.info/");
            }

            return webpageContent;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
