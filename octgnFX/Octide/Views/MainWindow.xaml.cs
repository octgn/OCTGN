namespace Octide.Views
{
    using System;
    using System.Windows;

    using GalaSoft.MvvmLight.Messaging;

    using Octide.Messages;
    using Octide.ViewModel;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<WindowActionMessage<MainViewModel>>(this,HandleWindowMessage);
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
