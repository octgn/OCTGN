/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Octgn.Annotations;
using Octgn.Core;
using Octgn.Core.DataManagers;
using Octgn.Core.Util;
using Octgn.DeckBuilder;
using Octgn.Extentions;
using Octgn.Controls;
using Octgn.Library;
using Octgn.Library.Exceptions;
using log4net;
using Octgn.Communication;
using Octgn.Communication.Modules;
using System.Linq;

namespace Octgn.Windows
{
    public partial class Main : INotifyPropertyChanged
    {
        internal new static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ConnectionStatus ConnectionStatus => Program.LobbyClient.Status;

        /// <summary>
        /// Initializes a new instance of the <see cref="Main"/> class.
        /// </summary>
        public Main()
        {
            this.InitializeComponent();
            this.TabControlMain.SelectionChanged += TabControlMainOnSelectionChanged;
            if (Program.IsReleaseTest) {
                this.Title = "OCTGN " + "[Test v" + Const.OctgnVersion + "]";
            }
            Program.LobbyClient.Disconnected += LobbyClient_Disconnected;
            Program.LobbyClient.Connected += LobbyClient_Connected;
            Program.LobbyClient.Connecting += LobbyClient_Connecting;
            Program.LobbyClient.Stats().StatsModuleUpdate += LobbyClient_StatsModuleUpdate;
            this.PreviewKeyUp += this.OnPreviewKeyUp;
            this.Closing += this.OnClosing;
            this.Loaded += OnLoaded;
            this.Activated += OnActivated;
        }

        private void LobbyClient_StatsModuleUpdate(object sender, StatsModuleUpdateEventArgs e) {
            OnPropertyChanged(nameof(ConnectionStatus));
        }

        private async void LobbyClient_Connected(object sender, ConnectedEventArgs args)
        {
            try {
                OnPropertyChanged(nameof(ConnectionStatus));

                await Dispatcher.InvokeAsync(async ()=> {
                    ProfileTab.IsEnabled = true;
                    await ProfileTabContent.Load(Program.LobbyClient.User);
                });
            } catch (Exception ex) {
                Log.Error($"{nameof(LobbyClient_Connected)}", ex);
            }
        }

        private async void LobbyClient_Disconnected(object sender, DisconnectedEventArgs args)
        {
            try {
                OnPropertyChanged(nameof(ConnectionStatus));

                await Dispatcher.InvokeAsync(() => {
                    ProfileTab.IsEnabled = false;
                });
            } catch (Exception ex) {
                Log.Error($"{nameof(LobbyClient_Connected)}", ex);
            }
        }

        private async void LobbyClient_Connecting(object sender, ConnectingEventArgs e) {
            try {
                OnPropertyChanged(nameof(ConnectionStatus));

                await Dispatcher.InvokeAsync(() => {
                    ProfileTab.IsEnabled = false;
                });
            } catch (Exception ex) {
                Log.Error($"{nameof(LobbyClient_Connecting)}", ex);
            }
        }

        private void TabControlMainOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            TabCustomGamesList.VisibleChanged(TabCustomGames.IsSelected);
            TabHistory.VisibleChanged(TabItemHistory.IsSelected);
        }

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
            using (var s = new StreamReader(info.Stream)) {
                resource = s.ReadToEnd();
            }

            var hash = resource.Sha1().ToLowerInvariant();

            if (!hash.Equals(Prefs.CustomDataAgreementHash, StringComparison.InvariantCultureIgnoreCase)) {
                Prefs.AcceptedCustomDataAgreement = false;
            }

            if (!Prefs.AcceptedCustomDataAgreement) {
                var result = TopMostMessageBox.Show(
                    resource + Environment.NewLine + Environment.NewLine + "By pressing 'Yes' you agree to the terms above. You must agree to the terms to use OCTGN.",
                    "OCTGN Custom Data Agreement",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes) {
                    Prefs.AcceptedCustomDataAgreement = true;
                    Prefs.CustomDataAgreementHash = hash.Clone() as string;
                    return;
                } else {
                    Program.Exit();
                }
            }
        }

        void Main_IsSubbedChanged(bool obj)
        {
            //Dispatcher.Invoke(
            //    new Action(() =>
            //              { this.menuSub.Visibility = obj == false ? Visibility.Visible : Visibility.Collapsed; }));
        }

        /// <summary>
        /// Happens when the window is closing
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="cancelEventArgs">
        /// The cancel event args.
        /// </param>
        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (WindowManager.PlayWindow != null) {
                WindowManager.PlayWindow.Activate();
                if (!WindowManager.PlayWindow.TryClose()) {
                    cancelEventArgs.Cancel = true;
                    return;
                }
            }
            if (WindowManager.DeckEditor != null) {
                WindowManager.DeckEditor.Activate();
                if (!WindowManager.DeckEditor.TryClose()) {
                    cancelEventArgs.Cancel = true;
                    return;
                }
            }
            Program.LobbyClient.Disconnected -= LobbyClient_Disconnected;
            Program.LobbyClient.Connected -= LobbyClient_Connected;
            Program.LobbyClient.Connecting -= LobbyClient_Connecting;
            Program.LobbyClient.Stats().StatsModuleUpdate -= LobbyClient_StatsModuleUpdate;
            Program.LobbyClient.Stop();
            GameUpdater.Get().Dispose();
            Task.Factory.StartNew(Program.Exit);
        }

        /// <summary>
        /// The on preview key up.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="keyEventArgs">
        /// The key event arguments.
        /// </param>
        private void OnPreviewKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            switch (keyEventArgs.Key) {
                case Key.F7:
                    if (X.Instance.Debug || Program.IsReleaseTest)
                        Program.LobbyClient.Stop();
                    break;
                case Key.F8: {
                        if (X.Instance.Debug) {
                            //WindowManager.GrowlWindow.AddNotification(new GameInviteNotification(new InviteToGame { From = new User("jim") }, new HostedGameData { Name = "Chicken" }, GameManager.Get().Games.First()));
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// The menu about clicked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MenuAboutClick(object sender, RoutedEventArgs e)
        {
            var w = new AboutWindow();
            w.ShowDialog();
        }

        /// <summary>
        /// Exits the program. Fired from the menu item MenuExit
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuDeckEditorClick(object sender, RoutedEventArgs e)
        {
            if (GameManager.Get().GameCount == 0) {
                TopMostMessageBox.Show(
                    "You need to install a game before you can use the deck editor.",
                    "OCTGN",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            if (WindowManager.DeckEditor == null) {
                WindowManager.DeckEditor = new DeckBuilderWindow();
                WindowManager.DeckEditor.Show();
            }
        }

        private void MenuOptionsClick(object sender, RoutedEventArgs e)
        {
            var options = new Options();
            options.ShowDialog();
        }

        private void MenuSubClick(object sender, RoutedEventArgs e)
        {
            this.ShowSubscribeSite(new SubType() { Description = "", Name = "" });
        }

        private void ShowSubscribeSite(SubType subtype)
        {
            Log.InfoFormat("Show sub site {0}", subtype);
            var url = SubscriptionModule.Get().GetSubscribeUrl(subtype);
            if (url != null) {
                Program.LaunchUrl(url);
            }
        }

        private void MenuHelpClick(object sender, RoutedEventArgs e)
        {
            Program.LaunchUrl(AppConfig.WebsitePath);
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MenuDiagClick(object sender, RoutedEventArgs e)
        {
            Octgn.Windows.Diagnostics.Instance.Show();
        }


        private void MenuSourceCodeClick(object sender, RoutedEventArgs e)
        {
            Program.LaunchUrl("http://repo.octgn.net");
        }

        private void MenuPullRequestClick(object sender, RoutedEventArgs e)
        {
            Program.LaunchUrl("https://github.com/octgn/OCTGN/pulls?q=is%3Apr+is%3Aclosed");
        }

        private void MenuSubscribeClick(object sender, RoutedEventArgs e)
        {
            var win = new DecorableWindow();
            win.SizeToContent = SizeToContent.WidthAndHeight;
            win.CanResize = false;
            win.MinMaxButtonVisibility = Visibility.Hidden;
            win.MinimizeButtonVisibility = Visibility.Hidden;
            win.Content = new SubscribeMessageLarge();
            win.Title = "Subscribe";
            win.ShowDialog();
        }

        private void MenuAndroidAppClick(object sender, RoutedEventArgs e)
        {
            Program.LaunchUrl("https://play.google.com/store/apps/details?id=com.octgn.app");
        }

        private void MenuDonateClick(object sender, RoutedEventArgs e) {
            Program.LaunchUrl("http://octgn.net/Home/Donate");
        }
    }
}