using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Octgn.Scripting.Controls
{
    public partial class ChoiceDlg
    {
        private int _intResult;

        public ChoiceDlg(string title, string prompt, List<string> choices)
        {
            InitializeComponent();
            //fix MAINWINDOW bug
            Owner = WindowManager.PlayWindow;
            Title = title;
            promptLbl.Text = prompt;
            int count = 0;
            foreach (string choice in choices)
            {
                count += 1;
                string buttonName = count.ToString();
                RadioButton button = new RadioButton();
                button.Content = choice;
                button.Uid = buttonName;
                button.Click += radioButton_Click;
                radioField.Children.Add(button);
            }
        }

        public int GetChoice()
        {
            ShowDialog();
            return _intResult;
        }

        protected void OkClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void radioButton_Click(object sender, RoutedEventArgs e)
        {
            string intresult = ((RadioButton)sender).Uid;
            _intResult = (int)Convert.ToInt32(intresult);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            }
        }
    }
