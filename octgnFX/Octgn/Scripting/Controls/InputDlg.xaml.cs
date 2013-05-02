using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Octgn.Scripting.Controls
{
    public partial class InputDlg
    {
        private int _intResult;
        private Kind _kind;

        public InputDlg(string title, string prompt, string defaultValue)
        {
            InitializeComponent();
            //fix MAINWINDOW bug
            Owner = WindowManager.PlayWindow;
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
            _kind = Kind.Integer;
            ShowDialog();
            return _intResult;
        }

        public int GetPositiveInt()
        {
            _kind = Kind.Positive;
            ShowDialog();
            return _intResult;
        }

        public string GetString()
        {
            _kind = Kind.Text;
            ShowDialog();
            return inputBox.Text;
        }

        protected void OkClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            switch (_kind)
            {
                case Kind.Text:
                    break;
                case Kind.Positive:
                case Kind.Integer:
                    if (!int.TryParse(inputBox.Text, out _intResult) ||
                        (_kind == Kind.Positive && _intResult < 0))
                    {
                        e.Cancel = true;
                        var anim = new ColorAnimation(Colors.White, Colors.Red,
                                                      new Duration(TimeSpan.FromMilliseconds(750))) {AutoReverse = true};
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