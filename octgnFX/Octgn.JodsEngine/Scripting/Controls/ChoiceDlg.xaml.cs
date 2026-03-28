using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace Octgn.Scripting.Controls
{
    public partial class ChoiceDlg
    {
        private int _intResult;

        public ChoiceDlg(string title, string prompt, List<string> choices, List<string> colors, List<string> customButtons)
        {
            InitializeComponent();
            //fix MAINWINDOW bug
            this.Owner = WindowManager.PlayWindow;
            this.SizeChanged += (sender, args) => CenterWindowInsideOwner();

            Title = title;
            promptLbl.Text = prompt;
            promptLbl.Margin = new Thickness(10, 5, 10, 5);
            
            int count = 0;
            foreach (string button in customButtons)
            {
                count -= 1;
                string buttonName = count.ToString();
                TextBlock buttonText = new TextBlock();
                buttonText.TextWrapping = TextWrapping.Wrap;
                buttonText.Margin = new Thickness(8, 3, 8, 3);
                buttonText.Text = button;
                Button customButton = new Button();
                customButton.HorizontalAlignment = HorizontalAlignment.Center;
                customButton.Margin = new Thickness(5);
                customButton.MinWidth = 40;
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
                buttonText.Margin = new Thickness(8, 3, 8, 3);
                buttonText.TextWrapping = TextWrapping.Wrap;
                buttonText.Text = choice;
                Button button = new Button();
                button.Margin = new Thickness(3, 1, 3, 1);
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

        private void CenterWindowInsideOwner()
        {
            // Position dialog at mouse cursor instead of centering in owner
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            var mousePosition = System.Windows.Forms.Control.MousePosition;
            
            this.Left = mousePosition.X - (this.ActualWidth / 2);
            this.Top = mousePosition.Y - (this.ActualHeight / 2);
            
            // Ensure dialog stays within screen bounds
            var screen = System.Windows.Forms.Screen.FromPoint(mousePosition);
            var workingArea = screen.WorkingArea;
            
            if (this.Left < workingArea.Left)
                this.Left = workingArea.Left;
            if (this.Top < workingArea.Top)
                this.Top = workingArea.Top;
            if (this.Right > workingArea.Right)
                this.Left = workingArea.Right - this.ActualWidth;
            if (this.Bottom > workingArea.Bottom)
                this.Top = workingArea.Bottom - this.ActualHeight;
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
