using System.Globalization;
using System.Windows;

namespace Octgn.Script
{
    // TODO : refactor this class
    public static class OCTGN
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
                MessageBox.Show(Program.PlayWindow, prompt, "Confirmation", MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public static void Message(string msg)
        {
            //fix MAINWINDOW bug
            MessageBox.Show(Program.PlayWindow, msg, "Message", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}