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
            this.Closed += (sender, args) => WindowManager.PreGameLobbyWindow = null;
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
            lobby = new PreGameLobby(isLocalGame);
            lobby.OnClose += PreGameLobbyOnOnClose;
            this.Content = lobby;
        }

        private void PreGameLobbyOnOnClose(object o)
        {
            this.Close();
        }
    }
}
