using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Octgn.Networking;
using Octgn.Play;

namespace Octgn.Launcher
{
    public partial class StartGame
    {
        private bool _startingGame;
        private readonly bool _isLocal;
        public StartGame(bool isLocal = false)
        {
            InitializeComponent();
            _isLocal = isLocal;
            if(!isLocal)
            {
                this.HorizontalAlignment = HorizontalAlignment.Stretch;
                this.VerticalAlignment = VerticalAlignment.Stretch;
                this.Width = Double.NaN;
                this.Height = Double.NaN;
            }
            if (Program.IsHost)
            {
                descriptionLabel.Text =
                    "The following players have joined your game.\nClick 'Start' when everyone has joined. No one will be able to join once the game has started.";
            }
            else
            {
                descriptionLabel.Text =
                    "The following players have joined the game.\nPlease wait until the game starts, or click 'Cancel' to leave this game.";
                startBtn.Visibility = Visibility.Collapsed;
                options.IsEnabled = playersList.IsEnabled = false;
            }

            Loaded += delegate
                          {
                              Program.GameSettings.UseTwoSidedTable = Prefs.TwoSidedTable;
                              Program.Dispatcher = Dispatcher;
                              Program.ServerError += HandshakeError;
                              Program.GameSettings.PropertyChanged += SettingsChanged;
                              // Fix: defer the call to Program.Game.Begin(), so that the trace has 
                              // time to connect to the ChatControl (done inside ChatControl.Loaded).
                              // Otherwise, messages notifying a disconnection may be lost
                              try
                              {
                                  if(Program.Game != null)
                                    Dispatcher.BeginInvoke(new Action(Program.Game.Begin));
                              }
                              catch (Exception)
                              {
                                  if (Debugger.IsAttached) Debugger.Break();
                              }
                          };
            Unloaded += delegate
                            {
                                if (_startingGame == false)
                                    Program.StopGame();
                                Program.GameSettings.PropertyChanged -= SettingsChanged;
                                Program.ServerError -= HandshakeError;
                            };
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            if (Program.IsHost)
                Program.Client.Rpc.Settings(Program.GameSettings.UseTwoSidedTable);
            cbTwoSided.IsChecked = Program.GameSettings.UseTwoSidedTable;
        }

        internal void Start()
        {
            _startingGame = true;
            // Reset the InvertedTable flags if they were set and they are not used
            if (!Program.GameSettings.UseTwoSidedTable)
                foreach (Player player in Player.AllExceptGlobal)
                    player.InvertedTable = false;

            // At start the global items belong to the player with the lowest id
            if (Player.GlobalPlayer != null)
            {
                Player host = Player.AllExceptGlobal.OrderBy(p => p.Id).First();
                foreach (Group group in Player.GlobalPlayer.Groups)
                    group.Controller = host;
            }

            if (Program.PlayWindow != null) return;
            Program.Client.Rpc.Start();
            Program.PlayWindow = new PlayWindow(_isLocal);
            Program.PlayWindow.Show();
            if (!_isLocal)
                Program.MainWindow.HostJoinTab();
            else
            {
                Program.LauncherWindow.Navigate(new Login());
                Program.LauncherWindow.Hide();
            }
        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {
            if (_startingGame) return;
            _startingGame = true;
            Program.LobbyClient.HostedGameStarted();
            e.Handled = true;
            Start();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Back();
        }

        private void Back()
        {
            if (!_isLocal)
                Program.MainWindow.HostJoinTab();
            else
            {
                Program.LauncherWindow.NavigationService.Navigate(new Login());
            }
        }

        private void HandshakeError(object sender, ServerErrorEventArgs e)
        {
            MessageBox.Show("The server returned an error:\n" + e.Message, "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
            e.Handled = true;
            Back();
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (cbTwoSided.IsChecked != null) Program.GameSettings.UseTwoSidedTable = cbTwoSided.IsChecked.Value;
        }
    }
}