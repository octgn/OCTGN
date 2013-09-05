using System.ComponentModel;
using System.Windows.Controls;

namespace Octgn.Controls
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;

    using Skylabs.Lobby.Threading;

    /// <summary>
    /// Interaction logic for UpdateBar.xaml
    /// </summary>
    public partial class UpdateBar : INotifyPropertyChanged
    {
        public string Message
        {
            get
            {
                return String.Format("There is a new version of OCTGN available, {0}.",UpdateManager.Instance.LatestVersion.Version);
            }
        }
        public UpdateBar()
        {
            InitializeComponent();
            UpdateManager.Instance.UpdateAvailable += InstanceOnUpdateAvailable;
            this.Visibility = Visibility.Collapsed;
        }

        private void InstanceOnUpdateAvailable(object sender, EventArgs eventArgs)
        {
            Dispatcher.Invoke(new Action(
                ()=>
                    this.Visibility = Visibility.Visible)
                );
            OnPropertyChanged("Message");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RestartClick(object sender, MouseButtonEventArgs e)
        {
            UpdateManager.Instance.UpdateAndRestart();
        }
    }
}
