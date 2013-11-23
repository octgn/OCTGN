namespace Octide.Views
{
    using System;
    using System.Windows;

    using GalaSoft.MvvmLight.Messaging;

    using Octide.Messages;
    using Octide.ViewModel;

    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class SplashWindow
    {
        public SplashWindow()
        {
            this.InitializeComponent();
            Messenger.Default.Register<WindowActionMessage<SplashViewModel>>(this,HandleWindowMessage);
        }

        internal void HandleWindowMessage(WindowActionMessage<SplashViewModel> message)
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
