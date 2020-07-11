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

        public ChoiceDlg(string title, string prompt, List<string> choices, List<string> colors, List<string> customButtons)
        {
            InitializeComponent();
            //fix MAINWINDOW bug
            Owner = WindowManager.PlayWindow;
            Title = title;
            promptLbl.Text = prompt;
            int count = 0;
            foreach (string button in customButtons)
            {
                count -= 1;
                string buttonName = count.ToString();
                TextBlock buttonText = new TextBlock();
                buttonText.TextWrapping = TextWrapping.Wrap;
                buttonText.Margin = new Thickness(10, 5, 10, 5);
                buttonText.Text = button;
                Button customButton = new Button();
                customButton.HorizontalAlignment = HorizontalAlignment.Center;
                customButton.Margin = new Thickness(8);
                customButton.MinWidth = 50;
                customButton.Content = buttonText;
                customButton.Click += Button_Click;
                customButton.Uid = buttonName;
                customField.Children.Add(customButton);
            };
            count = 0;
            foreach (string choice in choices)
            {
                count += 1;
                string buttonName = count.ToString();
                TextBlock buttonText = new TextBlock();
                buttonText.Margin = new Thickness(10, 5, 10, 5);
                buttonText.TextWrapping = TextWrapping.Wrap;
                buttonText.Text = choice;
                Button button = new Button();
                button.Margin = new Thickness(5, 1, 5, 1);
                button.Content = buttonText;
                if (colors[count - 1] != "None")
                {
                    var converter = new BrushConverter();
                    var brush = (Brush)converter.ConvertFromString(colors[count - 1]);
                    button.Background = brush;
                }
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
