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

        private void CenterWindowInsideOwner()
        {
            if (Owner != null)
            {
                double windowWidth = this.ActualWidth;
                double windowHeight = this.ActualHeight;
                var ownerPosition = new System.Drawing.Point((int)Owner.Left, (int)Owner.Top);
                var ownerScreen = System.Windows.Forms.Screen.FromPoint(ownerPosition);
                var screenBounds = ownerScreen.Bounds;

                if (Owner.WindowState == WindowState.Maximized)
                {
                    this.Left = screenBounds.X + (screenBounds.Width - windowWidth) / 2;
                    this.Top = screenBounds.Y + (screenBounds.Height - windowHeight) / 2;
                }
                else
                {
                    double ownerLeft = Owner.Left;
                    double ownerTop = Owner.Top;
                    double ownerWidth = Owner.ActualWidth;
                    double ownerHeight = Owner.ActualHeight;

                    this.Left = ownerLeft + (ownerWidth - windowWidth) / 2;
                    this.Top = ownerTop + (ownerHeight - windowHeight) / 2;
                }
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
