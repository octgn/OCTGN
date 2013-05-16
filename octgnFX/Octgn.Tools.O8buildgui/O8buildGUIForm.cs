using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Octgn.Tools.O8buildgui
{
    public partial class O8buildGUIForm : Form
    {
        private Process _process;
        public O8buildGUIForm()
        {
            InitializeComponent();
        }

        private void RunCommand(string arguments)
        {
            RunButton.Enabled = false;
            string o8build = GetO8BuildPath();

            if (o8build.Length == 0)
            {
                return;
            }

            _process = new Process();
            _process.StartInfo.FileName = o8build;
            _process.StartInfo.Arguments = arguments;
            _process.StartInfo.UseShellExecute = false;
            _process.EnableRaisingEvents = true;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.OutputDataReceived += new DataReceivedEventHandler(OnOutputDataReceived);
            _process.Exited += new EventHandler(OnProcessExited);
            _process.Start();
            _process.BeginOutputReadLine();
        }

        void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            AddToListbox(e.Data);
        }

        void AddToListbox(string data)
        {
            if (ConsoleOutput.InvokeRequired && !String.IsNullOrEmpty(data))
            {
                ConsoleOutput.Invoke(new MethodInvoker(delegate() { ConsoleOutput.Items.Add(data); }));
            }
        }

        void OnProcessExited(object sender, EventArgs e)
        {
            if (RunButton.InvokeRequired)
            {
                RunButton.Invoke(new MethodInvoker(delegate() { RunButton.Enabled = true; }));
            }
            AddToListbox("******Done******");
            AddToListbox("      ");
        }

        private string GetO8BuildPath()
        {
            string mydocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string o8build = "";
            if (!useTesto8buildCheckbox.Checked)
            {
                o8build = Path.Combine(mydocs, "OCTGN", "OCTGN", "o8build.exe");
            }
            else
            {
                o8build = Path.Combine(mydocs, "OCTGN", "OCTGN-Test", "o8build.exe");
            }
            
            if (!File.Exists(o8build))
            {
                AddToListbox(string.Format("Could not find o8build.exe at location: {0}", o8build));
                o8build = "";
            }

            return (o8build);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadActionList();
        }

        private void LoadActionList()
        {
            Dictionary<string, string> test = new Dictionary<string, string>();
            test.Add("-?", "Help");
            test.Add("-v", "Verify");
            test.Add("", "Build");
            test.Add("-c", "Convert o8s");
            test.Add("-i", "install package");
            ActionComboBox.DataSource = new BindingSource(test, null);
            ActionComboBox.DisplayMember = "Value";
            ActionComboBox.ValueMember = "Key";

        }

        private void LoadDirButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                PathTextBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            string value = ((KeyValuePair<string, string>)ActionComboBox.SelectedItem).Key;
            string arg = value;
            if (arg != "-?")
            {
                arg = arg + string.Format(" -d=\"{0}\"", PathTextBox.Text);
            }

            RunCommand(arg);
        }

        private void copySelectedLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ConsoleOutput.SelectedItems.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string line in ConsoleOutput.SelectedItems)
                {
                    sb.AppendLine(line);
                }
                Clipboard.SetText(sb.ToString());
            }
        }
    }
}
