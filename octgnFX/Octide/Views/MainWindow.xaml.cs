using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Win32;

namespace Octide.Views
{
    using System;
    using System.Windows;

    using GalaSoft.MvvmLight.Messaging;

    using Octide.Messages;
    using Octide.ViewModel;

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
                CloseCommand(null, default(ExecutedRoutedEventArgs));
            }
        }

        private void CloseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;
            if (CleanupCurrentGame())
            {
                _realClose = true;

                Dispatcher.BeginInvoke(new Action(Close));
            }
        }

        private void SaveCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModelLocator.GameLoader.SaveGame();
        }

        private void OpenCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (CleanupCurrentGame())
            {

                var fo = new OpenFileDialog();
                fo.Filter = "Definition File (definition.xml)|definition.xml";
                if ((bool)fo.ShowDialog() == false)
                {
                    return;
                }

                ViewModelLocator.GameLoader.LoadGame(fo.FileName);
            }
        }

        private void NewCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (CleanupCurrentGame())
                ViewModelLocator.GameLoader.New();
        }

        private bool CleanupCurrentGame()
        {
            if (ViewModelLocator.GameLoader.NeedsSave && ViewModelLocator.GameLoader.DidManualSave)
            {
                var res = MessageBox.Show("Do you want to save your changes?", "Save Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    ViewModelLocator.GameLoader.SaveGame();
                }
                else if (res == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }
            if (ViewModelLocator.GameLoader.DidManualSave == false)
            {
                ViewModelLocator.GameLoader.DeleteGame();
            }
            return true;
        }
    }
}
