namespace Octgn.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;

    using Octgn.Definitions;
    using Octgn.ViewModels;

    using Skylabs.Lobby;

    using Client = Octgn.Networking.Client;
    using UserControl = System.Windows.Controls.UserControl;

    public partial class HostGameSettings : UserControl
    {
        public event Action<object, DialogResult> OnClose;
        protected virtual void FireOnClose(object sender, DialogResult result)
        {
            var handler = this.OnClose;
            if (handler != null)
            {
                handler(sender, result);
            }
        }

        public static DependencyProperty ErrorProperty = DependencyProperty.Register(
            "Error", typeof(String), typeof(HostGameSettings));

        public bool HasErrors { get; private set; }
        public string Error{
            get{return this.GetValue(ErrorProperty) as String;}
            private set { this.SetValue(ErrorProperty, value); }
        }

        public bool IsLocalGame { get; private set; }
        public string Gamename { get; private set; }
        public string Password { get; private set; }
        public Data.Game Game { get; private set; }
        public bool SuccessfulHost { get; private set; }

        private Decorator Placeholder;

        public ObservableCollection<DataGameViewModel> Games { get; private set; }

        public HostGameSettings()
        {
            InitializeComponent();
            Games = new ObservableCollection<DataGameViewModel>();
            Program.LobbyClient.OnDataReceived += LobbyClientOnDataReceviedCaller;
            TextBoxGameName.Text = Prefs.LastRoomName;
        }

        void RefreshInstalledGameList()
        {
            if (Games == null)
                Games = new ObservableCollection<DataGameViewModel>();
            var list = Program.GamesRepository.Games.Select(x => new DataGameViewModel(x)).ToList();
            Games.Clear();
            foreach (var l in list)
                Games.Add(l);
        }

        void ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(TextBoxGameName.Text))
                this.SetError("You must enter a game name");
            else if (ComboBoxGame.SelectedIndex == -1)
                this.SetError("You must select a game");
            else
                this.SetError();
        }

        void SetError(string error = "")
        {
            this.HasErrors = !string.IsNullOrWhiteSpace(error);
            Error = error;
        }

        #region LobbyEvents
        private void LobbyClientOnDataReceviedCaller(object sender, DataRecType type, object data)
        {
            Dispatcher.Invoke(new Action(() => this.LobbyClientOnOnDataReceived(sender, type, data)));
        }
        private void LobbyClientOnOnDataReceived(object sender, DataRecType type, object data)
        {
            try
            {
                if (type == DataRecType.HostedGameReady)
                {
                    var port = data as Int32?;
                    if (port == null)
                        throw new Exception("Could not start game.");
                    var game = this.ComboBoxGame.SelectedItem as DataGameViewModel;
                    Program.LobbyClient.CurrentHostedGamePort = (int)port;
                    Program.GameSettings.UseTwoSidedTable = true;
                    Program.Game = new Game(GameDef.FromO8G(game.FullPath));
                    Program.IsHost = true;

                    var hostAddress = Dns.GetHostAddresses(Program.GameServerPath).First();

                    Program.Client = new Client(hostAddress, (int)port);
                    Program.Client.Connect();
                    Prefs.LastRoomName = TextBoxGameName.Name;
                    SuccessfulHost = true;
                }

            }
            catch (Exception e)
            {
                this.SetError("Error: {0}" + e.Message);
            }
            this.EndWait();
            if(SuccessfulHost)
                this.Close(DialogResult.OK);
        }
        #endregion

        #region Dialog
        public void Show(Decorator placeholder)
        {
            Placeholder = placeholder;
            this.RefreshInstalledGameList();

            placeholder.Child = this;
        }

        public void Close()
        {
            Close(DialogResult.Abort);
        }

        private void Close(DialogResult result)
        {
            Program.LobbyClient.OnDataReceived -= LobbyClientOnDataReceviedCaller;
            IsLocalGame = CheckBoxIsLocalGame.IsChecked ?? false;
            Gamename = TextBoxGameName.Text;
            Password = PasswordGame.Password;
            if(ComboBoxGame.SelectedIndex != -1)
                Game = (ComboBoxGame.SelectedItem as DataGameViewModel).GetGame(Program.GamesRepository);
            Placeholder.Child = null;
            this.FireOnClose(this, result);
        }

        void StartWait()
        {
            BorderHostGame.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
        }

        void EndWait()
        {
            BorderHostGame.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Hidden;
        }

        #endregion

        #region UI Events
        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close(DialogResult.Cancel);
        }

        private void ButtonHostGameStartClick(object sender, RoutedEventArgs e)
        {
            this.ValidateFields();
            if (this.HasErrors) return;
            this.StartWait();
            Program.CurrentOnlineGameName = TextBoxGameName.Text;
            Program.LobbyClient.BeginHostGame((ComboBoxGame.SelectedItem as DataGameViewModel).GetGame(Program.GamesRepository),TextBoxGameName.Text);
        }

        private void ButtonRandomizeGameNameClick(object sender, RoutedEventArgs e)
        {
            TextBoxGameName.Text = Skylabs.Lobby.Randomness.GrabRandomJargonWord() + " " + Randomness.GrabRandomNounWord();
        }
        #endregion
    }
}
