using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
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
            this.Owner = WindowManager.PlayWindow;
            this.SizeChanged += (sender, args) => CenterWindowInsideOwner();
            Title = title;
            promptLbl.Text = prompt;
            inputBox.Text = defaultValue;

            Loaded += delegate
                          {
                              inputBox.Focus();
                              inputBox.SelectAll();
                          };
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
                        var brush = (SolidColorBrush) inputBox.Background.Clone();
                        var anim = new ColorAnimation(brush.Color, Colors.Red,
                                                      new Duration(TimeSpan.FromMilliseconds(150))) {AutoReverse = true};
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