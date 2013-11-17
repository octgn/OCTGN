namespace Octgn.Windows
{
    using System;
    using System.Windows;

    using Octgn.Controls;

    public partial class PreGameLobbyWindow : OctgnChrome
    {
        private PreGameLobby lobby;
        public PreGameLobbyWindow()
        {
            InitializeComponent();
            WindowManager.PreGameLobbyWindow = this;
            this.CloseButtonVisibility = Visibility.Hidden;
            this.Closed += OnClosed;
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            WindowManager.PreGameLobbyWindow = null;
            this.Dispose();
        }

        public void Setup(bool isLocalGame, Window owner)
        {
            //this.Owner = owner;
            //this.Topmost = false;
            if(this.Visibility != Visibility.Visible)
                this.Show();
            this.Focus();
            this.Content = null;
            this.lobby = null;

            if (Program.GameEngine == null || Program.GameEngine.Definition == null)
            {
                TopMostMessageBox.Show(
                    "Something went wrong. Please tell someone!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Question);
                return;
                this.Close();
            }

            lobby = new PreGameLobby(isLocalGame);
            lobby.OnClose += PreGameLobbyOnOnClose;
            this.Content = lobby;
        }

        private void PreGameLobbyOnOnClose(object o)
        {
            lobby.OnClose -= this.PreGameLobbyOnOnClose;
            lobby.Dispose();
            this.Close();
        }
        public new void Dispose()
        {
            this.Closed -= OnClosed;
            base.Dispose();
        }
    }
}
