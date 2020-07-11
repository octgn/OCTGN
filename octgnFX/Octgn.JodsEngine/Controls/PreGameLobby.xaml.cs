﻿using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;

using log4net;

using Octgn.Core;
using Octgn.Extentions;
using Octgn.Networking;
using Octgn.Play;

namespace Octgn.Controls
{
    public partial class PreGameLobby : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event Action<object> OnClose;
        public bool CanChangeSettings { get; }

        protected virtual void FireOnClose(object obj) => this.OnClose?.Invoke(obj);

        public bool StartingGame { get; private set; }
        public bool IsOnline { get; private set; }
        private readonly bool _isLocal;

        public PreGameLobby()
        {
            CanChangeSettings = Program.IsHost && !Program.GameEngine.IsReplay;
            IsOnline = Program.GameEngine.IsLocal == false;
            var isLocal = Program.GameEngine.IsLocal;
            InitializeComponent();
            if (this.IsInDesignMode()) return;
            Player.OnLocalPlayerWelcomed += PlayerOnOnLocalPlayerWelcomed;
            _isLocal = isLocal;
            if (!isLocal)
            {
                this.HorizontalAlignment = HorizontalAlignment.Stretch;
                this.VerticalAlignment = VerticalAlignment.Stretch;
                this.Width = Double.NaN;
                this.Height = Double.NaN;
            }

            if (CanChangeSettings)
            {
                skipBtn.Visibility = Visibility.Collapsed;
                descriptionLabel.Text =
                    "The following players have joined your game.\n\nClick 'Start' when everyone has joined. No one will be able to join once the game has started.";
                if (isLocal)
                {
                    if (Program.Client is ClientSocket clientSocket) {
                        descriptionLabel.Text += "\n\nHosting on port: " + clientSocket.EndPoint.Port;
                        GetIps();

                        // save game/port so a new client can start up and connect
                        Prefs.LastLocalHostedGamePort = clientSocket.EndPoint.Port;
                        Prefs.LastHostedGameType = Program.GameEngine.Definition.Id;
                    }
                }
            }
            else
            {
                descriptionLabel.Text =
                    "The following players have joined the game.\nPlease wait until the game starts, or click 'Cancel' to leave this game.";
                startBtn.Visibility = Visibility.Collapsed;
                if (Program.GameEngine.IsReplay) {
                    skipBtn.Visibility = Visibility.Visible;
                } else {
                    skipBtn.Visibility = Visibility.Collapsed;
                }
            }
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.StartingGame == false)
                Program.StopGame();
            Program.GameSettings.PropertyChanged -= SettingsChanged;
            Program.ServerError -= HandshakeError;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            Program.GameSettings.UseTwoSidedTable = Program.GameEngine.Definition.UseTwoSidedTable;
            Program.GameSettings.ChangeTwoSidedTable = Program.GameEngine.Definition.ChangeTwoSidedTable;

            Program.Dispatcher = Dispatcher;
            Program.ServerError += HandshakeError;
            Program.GameSettings.PropertyChanged += SettingsChanged;
            // Fix: defer the call to Program.Game.Begin(), so that the trace has
            // time to connect to the ChatControl (done inside ChatControl.Loaded).
            // Otherwise, messages notifying a disconnection may be lost
            try
            {
                if (Program.GameEngine != null)
                    Dispatcher.BeginInvoke(new Action(() => Program.GameEngine.Begin()));
            }
            catch (Exception)
            {
                if (Debugger.IsAttached) Debugger.Break();
            }
        }

        private void GetIps()
        {
            var task = new Task(GetLocalIps);
            task.ContinueWith(GetExternalIp);
            task.Start();
        }

        private void GetLocalIps()
        {
            try
            {
                var addr = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList;
                this.Dispatcher.Invoke(
                    new Action(
                        () =>
                        {
                            var paragraph = new Paragraph(new Run("--Local Ip's--")) { Foreground = Brushes.Brown };
                            foreach (var a in addr)
                            {
                                paragraph.Inlines.Add(new LineBreak());
                                paragraph.Inlines.Add(new Run(a.ToString()));
                            }
                            this.chatControl.output.Document.Blocks.Add(paragraph);
                            this.chatControl.output.ScrollToEnd();
                        }));
            }
            catch (Exception)
            {

            }
        }

        private void GetExternalIp(Task task)
        {
            try
            {
                const string Dyndns = "http://checkip.dyndns.org";
                var wc = new System.Net.WebClient();
                var utf8 = new System.Text.UTF8Encoding();
                var requestHtml = "";
                var ipAddress = "";
                requestHtml = utf8.GetString(wc.DownloadData(Dyndns));
                var fullStr = requestHtml.Split(':');
                ipAddress = fullStr[1].Remove(fullStr[1].IndexOf('<')).Trim();
                var externalIp = System.Net.IPAddress.Parse(ipAddress);
                this.Dispatcher.Invoke(
                    new Action(
                        () =>
                        {
                            var paragraph = new Paragraph(new Run("--Remote Ip--")) { Foreground = Brushes.Brown };
                            paragraph.Inlines.Add(new LineBreak());
                            paragraph.Inlines.Add(new Run(externalIp.ToString()));
                            this.chatControl.output.Document.Blocks.Add(paragraph);
                            this.chatControl.output.ScrollToEnd();
                        }));

            }
            catch (Exception)
            {

            }
        }

        private void PlayerOnOnLocalPlayerWelcomed()
        {
            if (Player.LocalPlayer.Id == 255) return;
            if (Player.LocalPlayer.Id == 1 && !Program.GameEngine.IsReplay)
            {
                Dispatcher.BeginInvoke(new Action(() => { startBtn.Visibility = Visibility.Visible; }));
                Program.Client.Rpc.Settings(Program.GameSettings.UseTwoSidedTable, Program.GameSettings.AllowSpectators, Program.GameSettings.MuteSpectators);
            }
			Player.LocalPlayer.SetPlayerColor(Player.LocalPlayer.Id);
            this.StartingGame = true;
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            if (Program.IsHost)
                Program.Client.Rpc.Settings(Program.GameSettings.UseTwoSidedTable, Program.GameSettings.AllowSpectators, Program.GameSettings.MuteSpectators);
        }

        private bool calledStart = false;
        internal void Start(bool callStartGame = true)
        {
            lock (this)
            {
                if (calledStart) return;
                calledStart = true;
            }
            // Reset the InvertedTable flags if they were set and they are not used
            if (!Program.GameSettings.UseTwoSidedTable)
                foreach (Player player in Player.AllExceptGlobal)
                    player.InvertedTable = false;
            foreach (Player player in Player.Spectators)
            {
                if (player.InvertedTable)
                    player.InvertedTable = false;
            }

            // At start the global items belong to the player with the lowest id
            if (Player.GlobalPlayer != null)
            {
                Player host = Player.AllExceptGlobal.OrderBy(p => p.Id).First();
                foreach (Octgn.Play.Group group in Player.GlobalPlayer.Groups)
                    group.Controller = host;
            }
            if (callStartGame)
            {
                Program.Client.Rpc.Start(); // I believe this is for table only mode - Kelly
            }
            this.StartingGame = true;
            Back();
        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            e.Handled = true;
            Start();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.StartingGame = false;
            e.Handled = true;
            Back();
        }

        private void Back()
        {
            this.FireOnClose(this);
        }

        private void HandshakeError(object sender, ServerErrorEventArgs e)
        {
            TopMostMessageBox.Show("The server returned an error:\n" + e.Message, "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
            e.Handled = true;
            Back();
        }

        //private void CheckBoxClick(object sender, RoutedEventArgs e)
        //{
        //    if (cbTwoSided.IsChecked != null) Program.GameSettings.UseTwoSidedTable = cbTwoSided.IsChecked.Value;
        //}

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Player.OnLocalPlayerWelcomed -= PlayerOnOnLocalPlayerWelcomed;
            if (OnClose != null)
            {
                foreach (var d in OnClose.GetInvocationList())
                {
                    OnClose -= (Action<object>)d;
                }
            }
        }

        #endregion

        private void KickPlayer_OnClick(object sender, RoutedEventArgs e)
        {
            var sen = sender as Button;
            if (sen == null) return;
            var play = sen.DataContext as Player;
            if (play == null) return;
            if (Program.IsHost == false) return;

            Program.Client.Rpc.Boot(play, "The host has booted them from the game.");
        }

        private async void ProfileMouseUp(object sender, MouseButtonEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var play = fe.DataContext as Octgn.Play.Player;
            if (play == null) return;
            throw new NotImplementedException();
        }

        private void SkipClicked(object sender, RoutedEventArgs e) {
            Program.GameEngine.ReplayEngine.FastForwardToStart();
        }
    }
}
