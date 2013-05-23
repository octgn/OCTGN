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

        public ChoiceDlg(string title, string prompt, List<string> choices, string custom, bool cancel)
        {
            InitializeComponent();
            //fix MAINWINDOW bug
            Owner = WindowManager.PlayWindow;
            Title = title;
            promptLbl.Text = prompt;
            int count = 0;
            if (custom != null)
            {
                Button customButton = new Button();
                customButton.Content = custom;
                customButton.Click += Button_Click;
                customButton.Uid = "0";
                choiceWindow.Children.Add(customButton);
            };

            if (cancel)
            {
                Button cancelButton = new Button();
                cancelButton.IsCancel = true;
                cancelButton.Content = "Cancel";
                choiceWindow.Children.Add(cancelButton);
            };

            foreach (string choice in choices)
            {
                count += 1;
                string buttonName = count.ToString();
                Button button = new Button();
                button.Content = choice;
                button.Uid = buttonName;
                button.Click += Button_Click;
                buttonField.Children.Add(button);
            }
        }

        public int? GetChoice()
        {
            ShowDialog();
            return _intResult;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string intresult = ((Button)sender).Uid;
            _intResult = (int)Convert.ToInt32(intresult);
            DialogResult = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            }
        }
    }
