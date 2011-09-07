using System;
using System.Windows;

namespace Octgn.Script
{
    // TODO : refactor this class
    public static class OCTGN
    {
        public static int InputInt(string title, string prompt, int n)
        {
            InputDlg dlg = new InputDlg(title, prompt, n.ToString());
            return dlg.GetInteger();
        }

        public static int InputPositiveInt(string title, string prompt, int n)
        {
            InputDlg dlg = new InputDlg(title, prompt, n.ToString());
            return dlg.GetPositiveInt();
        }

        public static bool Confirm(string prompt)
        {
            return MessageBox.Show(Application.Current.MainWindow, prompt, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public static void Message(string msg)
        {
            MessageBox.Show(Application.Current.MainWindow, msg, "Message", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
