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
            this.CloseButtonVisibility = Visibility.Hidden;
            this.Closed += (sender, args) => Program.PreGameLobbyWindow = null;
        }

        public void Setup()
        {
            if(this.Visibility != Visibility.Visible)
                this.Show();
            this.Content = null;
            this.lobby = null;
            if (Program.Game != null)
            {
                lobby = new PreGameLobby(Program.Game.IsLocal);
                lobby.OnClose += PreGameLobbyOnOnClose;
                this.Content = lobby;
            }
        }

        private void PreGameLobbyOnOnClose(object o)
        {
            this.Close();
        }
    }
}
