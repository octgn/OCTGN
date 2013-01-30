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
            Program.PreGameLobbyWindow = this;
            this.CloseButtonVisibility = Visibility.Hidden;
            this.Closed += (sender, args) => Program.PreGameLobbyWindow = null;
        }

        public void Setup(bool isLocalGame, Window owner)
        {
            this.Owner = owner;
            if(this.Visibility != Visibility.Visible)
                this.Show();
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
