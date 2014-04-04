using System.Windows.Input;
using Microsoft.Win32;
using Octgn.Core.Annotations;

namespace Octide.Views
{
    using System;
    using System.Windows;

    using GalaSoft.MvvmLight.Messaging;

    using Octide.Messages;
    using Octide.ViewModel;

    public partial class MainWindow
    {
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

        private void CloseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (ViewModelLocator.GameLoader.NeedsSave)
            {
                var res = MessageBox.Show("Do you want to save your changes?", "Save Changes",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    ViewModelLocator.GameLoader.SaveGame();
                }
                else if (res == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            this.Close();
        }

        private void SaveCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModelLocator.GameLoader.SaveGame();
        }

        private void OpenCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModelLocator.GameLoader.NeedsSave)
            {
                var res = MessageBox.Show("Do you want to save your changes?", "Save Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    ViewModelLocator.GameLoader.SaveGame();
                }
                else if (res == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            var fo = new OpenFileDialog();
            fo.Filter = "Definition File (definition.xml)|definition.xml";
            if ((bool)fo.ShowDialog() == false)
            {
                return;
            }

            ViewModelLocator.GameLoader.LoadGame(fo.FileName);
        }

        private void NewCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModelLocator.GameLoader.NeedsSave)
            {
                var res = MessageBox.Show("Do you want to save your changes?", "Save Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    ViewModelLocator.GameLoader.SaveGame();
                }
                else if (res == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            ViewModelLocator.GameLoader.New();
        }
    }
}
