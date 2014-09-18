/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

using Octgn.Core;
using Octgn.Core.DataManagers;
using Octgn.DeckBuilder;
using Octgn.Extentions;

using agsXMPP;

using Octgn.Controls;
using Octgn.Library;
using Octgn.Library.Exceptions;

using Skylabs.Lobby;

using log4net;

namespace Octgn.Windows
{
    public partial class Main
    {
        internal new static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string ConnectMessage
        {
            get { return connectMessage; }
            set
            {
                if (value == connectMessage) return;
                connectMessage = value;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ConnectBox.Visibility = string.IsNullOrWhiteSpace(connectMessage) ? Visibility.Hidden : Visibility.Visible;
                    ConnectBoxProgressBar.IsIndeterminate = string.IsNullOrWhiteSpace(connectMessage);
                }));
                OnPropertyChanged("ConnectMessage");
            }
        }

        private string connectMessage;
        private bool showedSubscriptionMessageOnce = false;

        public Main()
        {
            this.InitializeComponent();
            if (X.Instance.ReleaseTest)
            {
                this.Title = "OCTGN " + "[Test v" + Const.OctgnVersion + "]";
            }
            this.MatchmakingTab.IsEnabled = false;
            ConnectBox.Visibility = Visibility.Hidden;
            ConnectBoxProgressBar.IsIndeterminate = false;
            if (this.IsInDesignMode())
                return;
            Program.LobbyClient.OnStateChanged += this.LobbyClientOnOnStateChanged;
            Program.LobbyClient.OnLoginComplete += this.LobbyClientOnOnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClientOnOnDisconnect;
            Program.LobbyClient.OnDataReceived += LobbyClient_OnDataReceived;
            this.PreviewKeyUp += this.OnPreviewKeyUp;
            this.Closing += this.OnClosing;
            //GameUpdater.Get().Start();
            this.Loaded += OnLoaded;
            ChatManager.Get().Start(ChatBar);
            this.Activated += OnActivated;
            //new GameFeedManager().CheckForUpdates();
        }

        #region UIEvents

        private void OnActivated(object sender, EventArgs eventArgs)
        {
            this.StopFlashingWindow();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Loaded -= OnLoaded;
            //SubscriptionModule.Get().IsSubbedChanged += Main_IsSubbedChanged;
            UpdateManager.Instance.Start();

            var uri = new System.Uri("/Resources/CustomDataAgreement.txt", UriKind.Relative);
            var info = Application.GetResourceStream(uri);
            var resource = "";
            using (var s = new StreamReader(info.Stream))
            {
                resource = s.ReadToEnd();
            }

            var hash = resource.Sha1().ToLowerInvariant();

            if (!hash.Equals(Prefs.CustomDataAgreementHash, StringComparison.InvariantCultureIgnoreCase))
            {
                Prefs.AcceptedCustomDataAgreement = false;
            }

            if (!Prefs.AcceptedCustomDataAgreement)
            {
                var result = TopMostMessageBox.Show(
                    resource + Environment.NewLine + Environment.NewLine + "By pressing 'Yes' you agree to the terms above. You must agree to the terms to use OCTGN.",
                    "OCTGN Custom Data Agreement",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    Prefs.AcceptedCustomDataAgreement = true;
                    Prefs.CustomDataAgreementHash = hash.Clone() as string;
                    return;
                }
                else
                {
                    Program.Exit();
                }
            }
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (WindowManager.PlayWindow != null)
            {
                if (!WindowManager.PlayWindow.TryClose())
                {
                    cancelEventArgs.Cancel = true;
                    return;
                }
            }
			LobbyChat.Dispose();
            //SubscriptionModule.Get().IsSubbedChanged -= this.Main_IsSubbedChanged;
            Program.LobbyClient.OnDisconnect -= LobbyClientOnOnDisconnect;
            Task.Factory.StartNew(Program.Exit);
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            switch (keyEventArgs.Key)
            {
                case Key.Escape:
                    if(this.menuExpanded)
                        HideMenu();
                    else
                        ChatBar.HideChat();
                    break;
                case Key.F7:
                    if (X.Instance.Debug || X.Instance.ReleaseTest)
                        Program.LobbyClient.Disconnect();
                    break;
                case Key.F8:
                    {
                        if (X.Instance.Debug || X.Instance.ReleaseTest)
                        {
                            WindowManager.GrowlWindow.AddNotification(new GameInviteNotification(new InviteToGame { From = new User(new Jid("jim@of.octgn.net")) }, new HostedGameData { Name = "Chicken" }, GameManager.Get().Games.First()));
                        }
                        break;
                    }
            }
        }

        #endregion UIEvents

        #region LobbyEvents

        private void LobbyClientOnOnStateChanged(object sender, XmppConnectionState state)
        {
            this.ConnectMessage = state == XmppConnectionState.Disconnected ? string.Empty : state.ToString();
            if (state == XmppConnectionState.Disconnected)
            {
                this.SetStateOffline();
            }
        }

        private void LobbyClientOnOnLoginComplete(object sender, LoginResults results)
        {
            this.Dispatcher.BeginInvoke(new Action(
                () =>
                {
                    ConnectBox.Visibility = Visibility.Hidden;
                    ConnectBoxProgressBar.IsIndeterminate = false;
                }));
            switch (results)
            {
                case LoginResults.Success:
                    this.SetStateOnline();
                    this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (GameManager.Get().GameCount == 0)
                                TabCustomGames.Focus();
                            else
                                TabCommunityChat.Focus();

                        })).Completed += (o, args) => Task.Factory.StartNew(() =>
                        {
                            Thread.Sleep(15000);
                            this.Dispatcher.Invoke(new Action(()
                                                              =>
                                {
                                    var s =
                                        SubscriptionModule.Get
                                            ().IsSubscribed;
                                    if (s != null && s == false)
                                    {
                                        if (showedSubscriptionMessageOnce == false)
                                        {
                                            ShowSubMessage();
                                            showedSubscriptionMessageOnce = true;
                                        }
                                    }
                                }));
                        });
                    break;
                default:
                    this.SetStateOffline();
                    break;
            }
        }

        void LobbyClient_OnDataReceived(object sender, DataRecType type, object data)
        {
            if (type == DataRecType.GameInvite)
            {
                if (Program.IsGameRunning) return;
                var idata = data as InviteToGame;
                Task.Factory.StartNew(() =>
                {
                    var hostedgame = Program.LobbyClient.GetHostedGames().FirstOrDefault(x => x.Id == idata.SessionId);
                    var endTime = DateTime.Now.AddSeconds(15);
                    while (hostedgame == null && DateTime.Now < endTime)
                    {
                        hostedgame = Program.LobbyClient.GetHostedGames().FirstOrDefault(x => x.Id == idata.SessionId);
                    }
                    if (hostedgame == null)
                    {
                        Log.WarnFormat(
                            "Tried to read game invite from {0}, but there was no matching running game", idata.From.UserName);
                        return;
                    }
                    var game = GameManager.Get().GetById(hostedgame.GameGuid);
                    if (game == null)
                    {
                        throw new UserMessageException("Game is not installed.");
                    }
                    WindowManager.GrowlWindow.AddNotification(new GameInviteNotification(idata, hostedgame, game));
                });
            }
        }

        private void LobbyClientOnOnDisconnect(object sender, EventArgs eventArgs)
        {
            if (Program.LobbyClient.DisconnectedBecauseConnectionReplaced)
            {
                // TODO Ask the user if they want to reconnect, that way the can override the other bullshit connection.
                Program.DoCrazyException(new Exception("Disconnected because connection replaced"), "You have been disconnected because you logged in somewhere else. You'll have to exit and reopen OCTGN to reconnect.");
                return;
            }
            Program.LobbyClient.Stop();
            Program.LobbyClient.BeginReconnect();
            //TopMostMessageBox.Show(
            //    "You have been disconnected", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Main States

        /// <summary>
        /// The set state offline.
        /// </summary>
        private void SetStateOffline()
        {
            this.Dispatcher.BeginInvoke(
                new Action(
                    () =>
                    {
                        this.MatchmakingTab.IsEnabled = false;
                        //TabCommunityChat.IsEnabled = false;
                        ProfileTab.IsEnabled = false;
                        //TabMain.Focus();
                        //menuSub.Visibility = Visibility.Collapsed;
                    }));
        }

        /// <summary>
        /// The set state online.
        /// </summary>
        private void SetStateOnline()
        {
            Dispatcher.BeginInvoke(
                new Action(
                    () =>
                    {
                        this.MatchmakingTab.IsEnabled = true;
                        TabCommunityChat.IsEnabled = true;
                        ProfileTab.IsEnabled = true;
                        ProfileTabContent.Load(Program.LobbyClient.Me);
                        var subbed = SubscriptionModule.Get().IsSubscribed;
                        //if (subbed == null || subbed == false)
                        //    menuSub.Visibility = Visibility.Visible;
                        //else
                        //    menuSub.Visibility = Visibility.Collapsed;
                        if (Program.LobbyClient.Me.UserName.Contains(" "))
                            TopMostMessageBox.Show(
                                "WARNING: You have a space in your username. This will cause a host of problems on here. If you don't have a subscription, it would be best to make yourself a new account.",
                                "WARNING",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                    }));
        }

        #endregion

        public void ShowSubMessage()
        {
            //TODO this can just be a SubMessageVisible property
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(this.ShowSubMessage));
                return;
            }
            this.SubMessage.Visibility = Visibility.Visible;
        }

        private bool menuExpanded = false;

        private void LeftMenuButtonClick(object sender, RoutedEventArgs e)
        {
            switch ((sender as System.Windows.Controls.Button).Name.ToLower())
            {
                case "menubutton":
                {
                    ToggleMenu();
                    break;
                }
                case "communitychatbutton":
                {
                    this.TabControlMain.SelectedIndex = 1;
                    break;
                }
                case "matchmakingbutton":
                {
                    this.TabControlMain.SelectedIndex = 2;
                    break;
                }
                case "playbutton":
                {
                    this.TabControlMain.SelectedIndex = 3;
                    break;
                }
                case "twitchbutton":
                {
                    this.TabControlMain.SelectedIndex = 4;
                    break;
                }
                case "gamemanagerbutton":
                {
                    this.TabControlMain.SelectedIndex = 5;
                    break;
                }
                case "profilebutton":
                {
                    this.TabControlMain.SelectedIndex = 6;
                    break;
                }

            }
        }

        private void MainMenuClick(object sender, RoutedEventArgs e)
        {
            HideMenu();
            var sen = sender as System.Windows.Controls.Button;
            switch ((sen.Content as string).ToLower())
            {
                case "deck editor":
                {
                    if (GameManager.Get().GameCount == 0)
                    {
                        TopMostMessageBox.Show(
                            "You need to install a game before you can use the deck editor.",
                            "OCTGN",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                    if (WindowManager.DeckEditor == null)
                    {
                        WindowManager.DeckEditor = new DeckBuilderWindow();
                        WindowManager.DeckEditor.Show();
                    }
                    break;
                }
                case "options":
                {
                    var options = new Options();
                    options.ShowDialog();
                    break;
                }
                case "get help online":
                {
                    Program.LaunchUrl(AppConfig.WebsitePath);
                    break;
                }
                case "diagnostics":
                {
                    Diagnostics.Instance.Show();
                    break;
                }
                case "about":
                {
                    var w = new AboutWindow();
                    w.ShowDialog();
                    break;
                }
                case "feature funding":
                {
                    var win = new OctgnChrome();
                    win.SizeToContent = SizeToContent.WidthAndHeight;
                    win.CanResize = false;
                    win.MinMaxButtonVisibility = Visibility.Hidden;
                    win.MinimizeButtonVisibility = Visibility.Hidden;
                    win.Content = new FeatureFundingMessage();
                    win.Title = "Feature Funding";
                    win.ShowDialog();
                    break;
                }
                case "source code":
                {
                    Program.LaunchUrl("http://repo.octgn.net");
                    break;
                }
                case "subscribe":
                {
                    Log.InfoFormat("Show sub site {0}", subtype);
                    var url = SubscriptionModule.Get().GetSubscribeUrl(subtype);
                    if (url != null)
                    {
                        Program.LaunchUrl(url);
                    }
                    break;
                }
                case "exit":
                {
                    this.Close();
                    break;
                }
                default:
                {
                    throw new Exception("There is no menu named " + (sen.Content as string));
                }
            }
        }

        private void ToggleMenu()
        {
            if (menuExpanded)
                HideMenu();
            else
                ShowMenu();
        }

        private void HideMenu()
        {
            var rn = "HideLeftDropDownMenuStory";
            menuExpanded = false;
            var sb = this.FindResource(rn) as Storyboard;
            Storyboard.SetTarget(sb, this.LeftDropDownMenu);
            sb.Begin();
        }

        private void ShowMenu()
        {
            var rn = "ShowLeftDropDownMenuStory";
            menuExpanded = true;
            var sb = this.FindResource(rn) as Storyboard;
            Storyboard.SetTarget(sb, this.LeftDropDownMenu);
            sb.Begin();
        }
    }
}