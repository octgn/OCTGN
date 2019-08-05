namespace Octide.Views
{
    using System;
    using System.Windows;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Messaging;

    using Octide.Messages;
    using Octide.ViewModel;

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
    }
}
