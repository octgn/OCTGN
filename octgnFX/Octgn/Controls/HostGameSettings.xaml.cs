namespace Octgn.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;

    using Octgn.Core;
    using Octgn.Core.DataManagers;
    using Octgn.Library.Exceptions;
    using Octgn.Networking;
    using Octgn.ViewModels;

    using Skylabs.Lobby;

    using log4net;

    using UserControl = System.Windows.Controls.UserControl;

    public partial class HostGameSettings : UserControl,IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
        public string Username { get; set; }
        public DataNew.Entities.Game Game { get; private set; }
        public bool SuccessfulHost { get; private set; }

        private Decorator Placeholder;
        private Guid lastHostedGameType;

        public ObservableCollection<DataGameViewModel> Games { get; private set; }

        public HostGameSettings()
        {
            InitializeComponent();
            Program.IsHost = true;
            Games = new ObservableCollection<DataGameViewModel>();
            Program.LobbyClient.OnDataReceived += LobbyClientOnDataReceviedCaller;
            Program.LobbyClient.OnLoginComplete += LobbyClientOnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClientOnDisconnect;
            TextBoxGameName.Text = Prefs.LastRoomName ?? Skylabs.Lobby.Randomness.RandomRoomName();
            CheckBoxIsLocalGame.IsChecked = !Program.LobbyClient.IsConnected;
            CheckBoxIsLocalGame.IsEnabled = Program.LobbyClient.IsConnected;
            LabelIsLocalGame.IsEnabled = Program.LobbyClient.IsConnected;
            lastHostedGameType = Prefs.LastHostedGameType;
            TextBoxUserName.Text = (Program.LobbyClient.IsConnected == false 
                || Program.LobbyClient.Me == null 
                || Program.LobbyClient.Me.UserName == null) ? Prefs.Nickname : Program.LobbyClient.Me.UserName;
			Program.OnOptionsChanged += ProgramOnOptionsChanged;
            TextBoxUserName.IsReadOnly = Program.LobbyClient.IsConnected;
            if(Program.LobbyClient.IsConnected)
                PasswordGame.IsEnabled = SubscriptionModule.Get().IsSubscribed ?? false;
            else
            {
                PasswordGame.IsEnabled = true;
            }
            StackPanelIsLocalGame.Visibility = Prefs.EnableAdvancedOptions ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ProgramOnOptionsChanged()
        {
            StackPanelIsLocalGame.Visibility = Prefs.EnableAdvancedOptions ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LobbyClientOnDisconnect(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
                { 
                    CheckBoxIsLocalGame.IsChecked = true;
                    CheckBoxIsLocalGame.IsEnabled = false;
                    LabelIsLocalGame.IsEnabled = false;
                    TextBoxUserName.IsReadOnly = false;
                }));
        }

        private void LobbyClientOnLoginComplete(object sender, LoginResults results)
        {
            if (results != LoginResults.Success) return;
            Dispatcher.Invoke(new Action(() =>
                { 
                    CheckBoxIsLocalGame.IsChecked = false;
                    CheckBoxIsLocalGame.IsEnabled = true;
                    LabelIsLocalGame.IsEnabled = true;
                    TextBoxUserName.IsReadOnly = true;
                    TextBoxUserName.Text = Program.LobbyClient.Me.UserName;
                }));
            
        }

        void RefreshInstalledGameList()
        {
            if (Games == null)
                Games = new ObservableCollection<DataGameViewModel>();
            var list = GameManager.Get().Games.Select(x => new DataGameViewModel(x)).ToList();
            Games.Clear();
            foreach (var l in list)
                Games.Add(l);
        }

        void ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(TextBoxGameName.Text))
                this.SetError("You must enter a game name");
            else if (ComboBoxGame.SelectedIndex == -1) this.SetError("You must select a game");
            else
            {
                if(String.IsNullOrWhiteSpace(PasswordGame.Password))
                    this.SetError();
                else
                {
                    if(PasswordGame.Password.Contains(":,:") || PasswordGame.Password.Contains("=") || PasswordGame.Password.Contains("-") || PasswordGame.Password.Contains(" "))
                        this.SetError("The password has invalid characters");
                    else
                        this.SetError();
                }
            }
        }

        void SetError(string error = "")
        {
            this.HasErrors = !string.IsNullOrWhiteSpace(error);
            Error = error;
        }

        #region LobbyEvents
        private void LobbyClientOnDataReceviedCaller(object sender, DataRecType type, object data)
        {
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
                    Program.GameEngine = new GameEngine(game,Program.LobbyClient.Me.UserName,this.Password);
                    Program.IsHost = true;

                    var hostAddress = Dns.GetHostAddresses(AppConfig.GameServerPath).First();

                    Program.Client = new ClientSocket(hostAddress, (int)port);
                    Program.Client.Connect();
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
            
            if (lastHostedGameType != Guid.Empty)
            {
                var game = GameManager.Get().Games.FirstOrDefault(x => x.Id == lastHostedGameType);
                if (game != null)
                {
                    var model = Games.FirstOrDefault(x => x.Id == game.Id);
                    if (model != null) this.ComboBoxGame.SelectedItem = model;
                }
            }

            placeholder.Child = this;
        }

        public void Close()
        {
            Close(DialogResult.Abort);
        }

        private void Close(DialogResult result)
        {
            Program.OnOptionsChanged -= ProgramOnOptionsChanged;
            Program.LobbyClient.OnDataReceived -= LobbyClientOnDataReceviedCaller;
            IsLocalGame = CheckBoxIsLocalGame.IsChecked ?? false;
            Gamename = TextBoxGameName.Text;
            Password = PasswordGame.Password;
            if (ComboBoxGame.SelectedIndex != -1)
                Game = (ComboBoxGame.SelectedItem as DataGameViewModel).GetGame();
            Placeholder.Child = null;
            this.FireOnClose(this, result);
        }

        void StartWait()
        {
            BorderHostGame.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
        }

        void EndWait()
        {
            BorderHostGame.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Hidden;
            ProgressBar.IsIndeterminate = false;
        }

        void StartLocalGame(DataNew.Entities.Game game, string name, string password)
        {
            var hostport = new Random().Next(5000,6000);
            while (!Networking.IsPortAvailable(hostport)) hostport++;
            var hs = new HostedGame(hostport, game.Id, game.Version, game.Name, name, Password, new User(Username + "@" + AppConfig.ChatServerPath), true);
            if (!hs.StartProcess())
            {
                throw new UserMessageException("Cannot start local game. You may be missing a file.");
            }
            Prefs.Nickname = Username;
            Program.LobbyClient.CurrentHostedGamePort = hostport;
            Program.GameSettings.UseTwoSidedTable = true;
            Program.GameEngine = new GameEngine(game, Username, password,true);
            Program.IsHost = true;

            var ip = IPAddress.Parse("127.0.0.1");

            for (var i = 0; i < 5; i++)
            {
                try
                {
                    Program.Client = new ClientSocket(ip, hostport);
                    Program.Client.Connect();
                    SuccessfulHost = true;
                    return;
                }
                catch (Exception e)
                {
                    Log.Warn("Start local game error",e);
                    if (i == 4) throw;
                }
                Thread.Sleep(2000);
            }
            
        }

        void StartOnlineGame(DataNew.Entities.Game game, string name, string password)
        {
            var client = new Octgn.Site.Api.ApiClient();
            if (!client.IsGameServerRunning(Program.LobbyClient.Username, Program.LobbyClient.Password))
            {
                throw new UserMessageException("The game server is currently down. Please try again later.");
            }
            Program.CurrentOnlineGameName = name;
            // TODO: Replace this with a server-side check
            password = SubscriptionModule.Get().IsSubscribed == true ? password : String.Empty;
            Program.LobbyClient.BeginHostGame(game, name, password, game.Name);
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
            this.Game = (ComboBoxGame.SelectedItem as DataGameViewModel).GetGame();
            this.Gamename = TextBoxGameName.Text;
            this.Password = PasswordGame.Password;
            this.Username = TextBoxUserName.Text;
            var isLocalGame = (CheckBoxIsLocalGame.IsChecked == null || CheckBoxIsLocalGame.IsChecked == false) == false;
            Task task = null;
            task = isLocalGame ? new Task(() => this.StartLocalGame(Game, Gamename, Password)) : new Task(() => this.StartOnlineGame(Game, Gamename, Password));

            Prefs.LastRoomName = this.Gamename;
            Prefs.LastHostedGameType = this.Game.Id;
            task.ContinueWith((continueTask) =>
                {
                    var error = "";
                    if (continueTask.IsFaulted)
                    {
                        if (continueTask.Exception != null &&  continueTask.Exception.InnerExceptions.OfType<UserMessageException>().Any())
                        {
                            error =
                                continueTask.Exception.InnerExceptions.OfType<UserMessageException>().First().Message;
                        }
                        else
                            error = "There was a problem, please try again.";
                        Log.Warn("Start Game Error",continueTask.Exception);
                        SuccessfulHost = false;
                    }
                    else
                    {
                        var startTime = DateTime.Now;
                        while (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMinutes <=1)
                        {
                            if (SuccessfulHost) break;
                            Thread.Sleep(1000);
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

        private void ButtonRandomizeUserNameClick(object sender, RoutedEventArgs e)
        {
            if (Program.LobbyClient.IsConnected == false)
                TextBoxUserName.Text = Randomness.GrabRandomJargonWord() + "-" + Randomness.GrabRandomNounWord();
        }
        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (OnClose != null)
            {
                foreach (var d in OnClose.GetInvocationList())
                {
                    OnClose -= (Action<object, DialogResult>)d;
                }
            }
            Program.LobbyClient.OnDataReceived -= LobbyClientOnDataReceviedCaller;
            Program.LobbyClient.OnLoginComplete -= LobbyClientOnLoginComplete;
            Program.LobbyClient.OnDisconnect -= LobbyClientOnDisconnect;
        }

        #endregion

        private void CheckBoxIsLocalGame_OnChecked(object sender, RoutedEventArgs e)
        {
            PasswordGame.IsEnabled = true;
        }

        private void CheckBoxIsLocalGame_OnUnchecked(object sender, RoutedEventArgs e)
        {
            PasswordGame.IsEnabled = SubscriptionModule.Get().IsSubscribed ?? false;
        }
    }
}
