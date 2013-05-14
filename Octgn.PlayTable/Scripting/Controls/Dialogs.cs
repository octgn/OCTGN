using System.Globalization;
using System.Windows;

namespace Octgn.Scripting.Controls
{
    public static class Dialog
    {
        public static int InputInt(string title, string prompt, int n)
        {
            var dlg = new InputDlg(title, prompt, n.ToString(CultureInfo.InvariantCulture));
            return dlg.GetInteger();
        }

        public static int InputPositiveInt(string title, string prompt, int n)
        {
            var dlg = new InputDlg(title, prompt, n.ToString(CultureInfo.InvariantCulture));
            return dlg.GetPositiveInt();
        }

        public static bool Confirm(string prompt)
        {
            //fix MAINWINDOW bug
            return
                MessageBox.Show(WindowManager.PlayWindow, prompt, "Confirmation", MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public static void Message(string msg)
        {
            //fix MAINWINDOW bug
            MessageBox.Show(WindowManager.PlayWindow, msg, "Message", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}