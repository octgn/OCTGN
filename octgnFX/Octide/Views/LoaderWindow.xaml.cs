// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octide.Views
{
    using GalaSoft.MvvmLight.Messaging;
    using Octide.Messages;
    using Octide.ViewModel;
    using System;
    using System.ComponentModel;
    using System.Windows;

    /// <summary>
    /// Interaction logic for LoaderWindow.xaml
    /// </summary>
    public partial class LoaderWindow
    {
        public LoaderWindow()
        {
            this.InitializeComponent();
            Messenger.Default.Register<WindowActionMessage<LoaderViewModel>>(this, HandleWindowMessage);
        }

        internal void HandleWindowMessage(WindowActionMessage<LoaderViewModel> message)
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
            Messenger.Default.Unregister<WindowActionMessage<LoaderViewModel>>(this, HandleWindowMessage);
        }

    }
}
