using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using Octgn.Play;

namespace Octgn.Launcher
{
    public partial class StartGame : Page
    {
        private bool StartingGame = false;
        public StartGame()
        {


            InitializeComponent();

            if (Program.IsHost)
            {
                descriptionLabel.Text = "The following players have joined your game.\nClick 'Start' when everyone has joined. No one will be able to join once the game has started.";
            }
            else
            {
                descriptionLabel.Text = "The following players have joined the game.\nPlease wait until the game starts, or click 'Cancel' to leave this game.";
                startBtn.Visibility = Visibility.Collapsed;
                options.IsEnabled = playersList.IsEnabled = false;
            }

            Loaded += delegate
            {
                Program.GameSettings.UseTwoSidedTable = true;
                Program.Dispatcher = this.Dispatcher;
                Program.ServerError += HandshakeError;
                Program.GameSettings.PropertyChanged += SettingsChanged;
                // Fix: defer the call to Program.Game.Begin(), so that the trace has 
                // time to connect to the ChatControl (done inside ChatControl.Loaded).
                // Otherwise, messages notifying a disconnection may be lost
                try { Dispatcher.BeginInvoke(new Action(Program.Game.Begin)); }
                catch (Exception)
                {
                    if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                }

            };
            Unloaded += delegate
            {
                if (StartingGame == false)
                    Program.StopGame();
                Program.GameSettings.PropertyChanged -= SettingsChanged;
                Program.ServerError -= HandshakeError;
            };
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (Program.IsHost)
                    Program.Client.Rpc.Settings(Program.GameSettings.UseTwoSidedTable);
                cbTwoSided.IsChecked = Program.GameSettings.UseTwoSidedTable;
            }
        }

        internal void Start()
        {
            StartingGame = true;
            // Reset the InvertedTable flags if they were set and they are not used
            if (!Program.GameSettings.UseTwoSidedTable)
                foreach (var player in Play.Player.AllExceptGlobal)
                    player.InvertedTable = false;

            // At start the global items belong to the player with the lowest id
            if (Player.GlobalPlayer != null)
            {
                var host = Player.AllExceptGlobal.OrderBy(p => p.Id).First();
                foreach (var group in Player.GlobalPlayer.Groups)
                    group.Controller = host;
            }

            if (Program.PlayWindow == null)
            {
                Program.Client.Rpc.Start();
                Program.PlayWindow = new Octgn.Play.PlayWindow();
                Program.PlayWindow.Show();
                Program.ClientWindow.HostJoinTab();
            }
        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {
            if (!StartingGame)
            {
                StartingGame = true;
                Program.lobbyClient.HostedGameStarted();
                e.Handled = true;
                Start();
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Back();
        }

        private void Back()
        {
            Program.ClientWindow.HostJoinTab();

        }

        private void HandshakeError(object sender, ServerErrorEventArgs e)
        {
            MessageBox.Show("The server returned an error:\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
            Back();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Program.GameSettings.UseTwoSidedTable = (bool)cbTwoSided.IsChecked.Value;
        }
    }
}