using System.Windows;

namespace Octgn.Windows
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Interaction logic for GameLog.xaml
    /// </summary>
    public partial class GameLog : Window
    {
        private bool realClose = false;
        public GameLog()
        {
            InitializeComponent();
            this.Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (!realClose)
            {
                this.Visibility = Visibility.Hidden;
                cancelEventArgs.Cancel = true;
            }
        }

        public void RealClose()
        {
            realClose = true;
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ChatControl.Save();
        }
    }
}
