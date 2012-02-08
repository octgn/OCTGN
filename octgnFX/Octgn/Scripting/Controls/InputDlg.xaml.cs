using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Octgn.Script
{
    public partial class InputDlg : Window
    {
        private int intResult;
        private Kind kind;

        public InputDlg(string title, string prompt, string defaultValue)
        {
            InitializeComponent();
            //fix MAINWINDOW bug
            Owner = Program.PlayWindow;
            Title = title;
            promptLbl.Text = prompt;
            inputBox.Text = defaultValue;

            Loaded += delegate
                          {
                              inputBox.Focus();
                              inputBox.SelectAll();
                          };
        }

        public int GetInteger()
        {
            kind = Kind.Integer;
            ShowDialog();
            return intResult;
        }

        public int GetPositiveInt()
        {
            kind = Kind.Positive;
            ShowDialog();
            return intResult;
        }

        protected void OkClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            switch (kind)
            {
                case Kind.Text:
                    break;
                case Kind.Positive:
                case Kind.Integer:
                    if (!int.TryParse(inputBox.Text, out intResult) ||
                        (kind == Kind.Positive && intResult < 0))
                    {
                        e.Cancel = true;
                        var anim = new ColorAnimation(Colors.White, Colors.Red,
                                                      new Duration(TimeSpan.FromMilliseconds(750)));
                        anim.AutoReverse = true;
                        var brush = (SolidColorBrush) inputBox.Background.Clone();
                        inputBox.Background = brush;
                        brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
                    }
                    break;
                default:
                    throw new NotImplementedException("Unknown kind in InputDlg");
            }
        }

        #region Nested type: Kind

        private enum Kind
        {
            Text,
            Integer,
            Positive
        };

        #endregion
    }
}