namespace Octgn.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;

    using Octgn.Definitions;
    using Octgn.Library.Exceptions;
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
        public string Error
        {
            get { return this.GetValue(ErrorProperty) as String; }
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
            Program.LobbyClient.OnLoginComplete += LobbyClientOnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClientOnDisconnect;
            TextBoxGameName.Text = Prefs.LastRoomName;
            CheckBoxIsLocalGame.IsChecked = !Program.LobbyClient.IsConnected;
            CheckBoxIsLocalGame.IsEnabled = Program.LobbyClient.IsConnected;
        }

        private void LobbyClientOnDisconnect(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
                { 
                    CheckBoxIsLocalGame.IsChecked = true;
                    CheckBoxIsLocalGame.IsEnabled = false;
                }));
        }

        private void LobbyClientOnLoginComplete(object sender, LoginResults results)
        {
            Dispatcher.Invoke(new Action(() =>
                { 
                    CheckBoxIsLocalGame.IsChecked = false;
                    CheckBoxIsLocalGame.IsEnabled = true;
                }));
            
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
        //    Dispatcher.Invoke(new Action(() => this.LobbyClientOnOnDataReceived(sender, type, data)));
            
        //}
        //private void LobbyClientOnOnDataReceived(object sender, DataRecType type, object data)
        //{
            try
            {
                if (type == DataRecType.HostedGameReady)
                {
                    var port = data as Int32?;
                    if (port == null)
                        throw new Exception("Could not start game.");
                    var game = this.Game;
                    Program.LobbyClient.CurrentHostedGamePort = (int)port;
                    Program.GameSettings.UseTwoSidedTable = true;
                    Program.Game = new Game(GameDef.FromO8G(game.FullPath));
                    Program.IsHost = true;

                    var hostAddress = Dns.GetHostAddresses(Program.GameServerPath).First();

                    Program.Client = new Client(hostAddress, (int)port);
                    Program.Client.Connect();
                    Prefs.LastRoomName = this.Gamename;
                    SuccessfulHost = true;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }

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
            if (ComboBoxGame.SelectedIndex != -1)
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

        void StartLocalGame(Data.Game game, string name, string password)
        {
            var hostport = 5000;
            while (!Networking.IsPortAvailable(hostport)) hostport++;
            var hs = new HostedGame(hostport, game.Id, game.Version, name, "", null, true);
            if (!hs.StartProcess())
            {
                throw new UserMessageException("Cannot start local game. You may be missing a file.");
            }

            Program.LobbyClient.CurrentHostedGamePort = hostport;
            Program.GameSettings.UseTwoSidedTable = true;
            Program.Game = new Game(GameDef.FromO8G(game.FullPath), true);
            Program.IsHost = true;

            var ip = IPAddress.Parse("127.0.0.1");

            Program.Client = new Client(ip, hostport);
            Program.Client.Connect();
            SuccessfulHost = true;
        }

        void StartOnlineGame(Data.Game game, string name, string password)
        {
            Program.CurrentOnlineGameName = name;
            Program.LobbyClient.BeginHostGame(game, name);
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
            Program.Dispatcher = this.Dispatcher;
            if (this.HasErrors) return;
            this.StartWait();
            this.Game = (ComboBoxGame.SelectedItem as DataGameViewModel).GetGame(Program.GamesRepository);
            this.Gamename = TextBoxGameName.Text;
            this.Password = PasswordGame.Password;
            var isLocalGame = !(CheckBoxIsLocalGame.IsChecked == null || CheckBoxIsLocalGame.IsChecked == false);
            Task task = null;
            task = isLocalGame ? new Task(() => this.StartOnlineGame(Game, Gamename, Password)) : new Task(() => this.StartLocalGame(Game, Gamename, Password));

            task.ContinueWith((continueTask) =>
                {
                    var error = "";
                    if (continueTask.IsFaulted)
                    {
                        error = "There was a problem, please try again.";
                        SuccessfulHost = false;
                    }
                    else
                    {
                        var i = 0;
                        while (!SuccessfulHost || i < 10)
                        {
                            Thread.Sleep(1000);
                            i++;
                        }
                    }
                    Dispatcher.Invoke(new Action(() =>
                        {
                            if(!string.IsNullOrWhiteSpace(error))
                                this.SetError(error);
                            this.EndWait();
                            if(SuccessfulHost)
                                this.Close(DialogResult.OK);
                        }));
                });
            task.Start();
        }

        private void ButtonRandomizeGameNameClick(object sender, RoutedEventArgs e)
        {
            TextBoxGameName.Text = Skylabs.Lobby.Randomness.GrabRandomJargonWord() + " " + Randomness.GrabRandomNounWord();
        }
        #endregion
    }
}
