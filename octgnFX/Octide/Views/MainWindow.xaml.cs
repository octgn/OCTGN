// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.ComponentModel;
using System.Windows.Input;

namespace Octide.Views
{
    using GalaSoft.MvvmLight.Messaging;
    using Octide.Messages;
    using Octide.ViewModel;
    using System;
    using System.Windows;

    public partial class MainWindow
    {
        private bool _realClose;

        public MainWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<WindowActionMessage<MainViewModel>>(this, HandleWindowMessage);
        }

        internal void HandleWindowMessage(WindowActionMessage<MainViewModel> message)
        {
            if (this.CheckAccess() == false)
            {
                Dispatcher.Invoke(new Action(() => HandleWindowMessage(message)));
                return;
            }
            switch (message.Action)
            {
                case WindowActionType.Close:
                    this.Close();
                    break;
                case WindowActionType.Hide:
                    this.Hide();
                    break;
                case WindowActionType.Show:
                    this.Show();
                    break;
                case WindowActionType.SetMain:
                    Application.Current.MainWindow = this;
                    break;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_realClose)
            {
                e.Cancel = true;
                Messenger.Default.Unregister<WindowActionMessage<MainViewModel>>(this, HandleWindowMessage);
                Messenger.Default.Send(new CleanupMessage());
                CloseCommand(null, default);
            }
        }

        private void CloseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;
            if (WindowLocator.MainViewModel.CleanupCurrentGame())
            {
                _realClose = true;

                Dispatcher.BeginInvoke(new Action(Close));
            }
        }

        private void MetroWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }
    }
}
