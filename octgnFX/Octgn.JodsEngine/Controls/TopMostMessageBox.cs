using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Octgn.Controls
{
    public class TopMostMessageBox
    {
        public static MessageBoxResult Show(string messageBoxText)
        {
            MessageBoxResult result = MessageBoxResult.None;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                        () =>
                      {
                        Window newWindow = new Window() { Topmost = true };
                        result = MessageBox.Show(newWindow, messageBoxText);
                        newWindow.Close();
                      }));
            return (result);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            MessageBoxResult result = MessageBoxResult.None;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                        () =>
                      {
                        Window newWindow = new Window() { Topmost = true };
                        result = MessageBox.Show(newWindow, messageBoxText, caption);
                        newWindow.Close();
                      }));
            return (result);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            MessageBoxResult result = MessageBoxResult.None;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                        () =>
                      {
                        Window newWindow = new Window() { Topmost = true };
                        result = MessageBox.Show(newWindow, messageBoxText, caption, button);
                        newWindow.Close();
                      }));
            return (result);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            MessageBoxResult result = MessageBoxResult.None;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                        () =>
                      {
                        Window newWindow = new Window() { Topmost = true };
                        result = MessageBox.Show(newWindow, messageBoxText, caption, button, icon);
                        newWindow.Close();
                      }));
            return (result);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            MessageBoxResult result = defaultResult;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                            () =>
                            {
                                Window newWindow = new Window() { Topmost = true };
                                result = MessageBox.Show(newWindow, messageBoxText, caption, button, icon, defaultResult);
                                newWindow.Close();
                            }));
            return (result);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            MessageBoxResult result = defaultResult;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                        () =>
                      {
                        Window newWindow = new Window() { Topmost = true };
                        result = MessageBox.Show(newWindow, messageBoxText, caption, button, icon, defaultResult, options);
                        newWindow.Close();
                      }));
            return (result);
        }
    }
}
