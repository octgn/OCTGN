using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Octgn.Updater.Runners;

namespace Octgn.Updater
{
    public partial class frmLog : Form
    {
        public frmLog()
        {
            this.Visible = false;
            InitializeComponent();
#if(DEBUG)
            this.Visible = true;
#endif
        }

        private void frmLog_Load(object sender, EventArgs e)
        {
            
            Updater.Runner = new UpdateRunner();
            var context = new UpdaterContext();
            context.Log += Log;
            Updater.Runner.TaskRunnerCompleted += TaskRunnerCompleted ;
            Updater.Runner.Start(context);
        }

        private void TaskRunnerCompleted(UpdaterContext updaterContext)
        {
            Invoke(new Action(Close));
        }

        private void Log(UpdaterContext updaterContext, string s)
        {
            Updater.LogList.Add(s);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
        }

        private void frmLog_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void frmLog_VisibleChanged(object sender, EventArgs e)
        {
            if(this.Visible)
            {
                rtbLog.Text = "";
                foreach(var s in Updater.LogList)
                    rtbLog.Text += s + Environment.NewLine;
            }
        }

        private void frmLog_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon1.Visible = false;
            notifyIcon1.Dispose();
            if (Updater.Runner.FailedUpdate)
            {
                throw new Exception("Failed Update");
            }
        }
    }
}
