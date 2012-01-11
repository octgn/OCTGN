using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace Octgn.Script
{
    public partial class InputDlg : Window
    {
        private enum Kind { Text, Integer, Positive };
        private Kind kind;
        private int intResult;

        public InputDlg(string title, string prompt, string defaultValue)
        {
            InitializeComponent();
            //fix MAINWINDOW bug
            this.Owner = Program.PlayWindow;
            this.Title = title;
            promptLbl.Text  = prompt;
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
            this.ShowDialog();
            return intResult;
        }

        public int GetPositiveInt()
        {
            kind = Kind.Positive;
            this.ShowDialog();
            return intResult;
        }

        protected void OkClicked(object sender, RoutedEventArgs e)
        { DialogResult = true; }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
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
                        ColorAnimation anim = new ColorAnimation(Colors.White, Colors.Red, new Duration(TimeSpan.FromMilliseconds(750)));
                        anim.AutoReverse = true;
                        SolidColorBrush brush = (SolidColorBrush)inputBox.Background.Clone();
                        inputBox.Background = brush;
                        brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
                    }
                    break;
                default:
                    throw new NotImplementedException("Unknown kind in InputDlg");
            }
        }
    }
}