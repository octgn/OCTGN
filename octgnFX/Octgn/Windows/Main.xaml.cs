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

namespace Octgn.Windows
{
    public partial class Main : INotifyPropertyChanged
    {
        internal new static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            this.PreviewKeyUp += this.OnPreviewKeyUp;
            this.Closing += this.OnClosing;
            this.Loaded += OnLoaded;
            this.Activated += OnActivated;
        }

        private async void LobbyClient_Connected(object sender, ConnectedEventArgs args)
        {
            try {
                await Dispatcher.InvokeAsync(SetStateOnline);
            } catch (Exception ex) {
                Log.Error($"{nameof(LobbyClient_Connected)}", ex);
            }
        }

        private void LobbyClient_Disconnected(object sender, DisconnectedEventArgs args)
        {
            this.SetStateOffline();
        }

        private void TabControlMainOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            TabCustomGamesList.VisibleChanged(TabCustomGames.IsSelected);
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
                if (!WindowManager.PlayWindow.TryClose()) {
                    cancelEventArgs.Cancel = true;
                    return;
                }
            }
            Program.LobbyClient.Disconnected -= LobbyClient_Disconnected;
            Program.LobbyClient.Connected -= LobbyClient_Connected;
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

        #region Main States

        /// <summary>
        /// The set state offline.
        /// </summary>
        private void SetStateOffline()
        {
            this.Dispatcher.BeginInvoke(
                new Action(
                    () => {
                        ProfileTab.IsEnabled = false;
                    }));
        }

        private async Task SetStateOnline()
        {
            Dispatcher.VerifyAccess();

            ProfileTab.IsEnabled = true;
            await ProfileTabContent.Load(Program.LobbyClient.User);

            if (Program.LobbyClient.User.DisplayName.Contains(" "))
                TopMostMessageBox.Show(
                    "WARNING: You have a space in your username. This will cause a host of problems on here. If you don't have a subscription, it would be best to make yourself a new account.",
                    "WARNING",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
        }

        #endregion

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

        private void MenuShareCurrentLogClick(object sender, RoutedEventArgs e)
        {
            try {
                if (!File.Exists(Config.Instance.Paths.CurrentLogPath)) {
                    TopMostMessageBox.Show(
                        "Log file doesn't exist at " + Config.Instance.Paths.CurrentLogPath,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var res = TextUploader.Instance.UploadText(File.ReadAllText(Config.Instance.Paths.CurrentLogPath));

                Clipboard.SetText(res);

                TopMostMessageBox.Show(
                    "Your log file has been shared. The URL to your log file has been copied to your clipboard. You can press ctrl+v to paste it.");
            } catch (UserMessageException) {
                throw;
            } catch (Exception ex) {
                TopMostMessageBox.Show(
                    "Error " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void MenuOpenCurrentLogClick(object sender, RoutedEventArgs e)
        {
            try {
                if (!File.Exists(Config.Instance.Paths.CurrentLogPath)) {
                    TopMostMessageBox.Show(
                        "Log file doesn't exist at " + Config.Instance.Paths.CurrentLogPath,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                var process = new Process();
                process.StartInfo = new ProcessStartInfo() {
                    UseShellExecute = true,
                    FileName = Config.Instance.Paths.CurrentLogPath
                };

                process.Start();
            } catch (Exception ex) {
                Log.Warn("MenuOpenCurrentLogClick Error", ex);
            }
        }

        private void MenuSaveAsCurrentLogClick(object sender, RoutedEventArgs e)
        {
            try {
                if (!File.Exists(Config.Instance.Paths.CurrentLogPath)) {
                    TopMostMessageBox.Show(
                        "Log file doesn't exist at " + Config.Instance.Paths.CurrentLogPath,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                var sfd = new SaveFileDialog();
                sfd.Title = "Save Log File To...";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                sfd.FileName = "currentlog.txt";
                sfd.OverwritePrompt = true;
                if ((sfd.ShowDialog() ?? false)) {
                    File.Copy(Config.Instance.Paths.CurrentLogPath, sfd.FileName, true);
                    //var str = File.ReadAllText(Config.Instance.Paths.CurrentLogPath);
                    //File.WriteAllText(sfd.FileName, str);
                }

            } catch (Exception ex) {
                TopMostMessageBox.Show(
                    "Error " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void MenuSharePreviousLogClick(object sender, RoutedEventArgs e)
        {
            try {
                if (!File.Exists(Config.Instance.Paths.PreviousLogPath)) {
                    TopMostMessageBox.Show(
                        "Log file doesn't exist at " + Config.Instance.Paths.PreviousLogPath,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var res = TextUploader.Instance.UploadText(File.ReadAllText(Config.Instance.Paths.PreviousLogPath));

                Clipboard.SetText(res);

                TopMostMessageBox.Show(
                    "Your log file has been shared. The URL to your log file has been copied to your clipboard. You can press ctrl+v to paste it.");
            } catch (UserMessageException) {
                throw;
            } catch (Exception ex) {
                TopMostMessageBox.Show(
                    "Error " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void MenuOpenPreviousLogClick(object sender, RoutedEventArgs e)
        {
            try {
                if (!File.Exists(Config.Instance.Paths.PreviousLogPath)) {
                    TopMostMessageBox.Show(
                        "Log file doesn't exist at " + Config.Instance.Paths.PreviousLogPath,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                var process = new Process();
                process.StartInfo = new ProcessStartInfo() {
                    UseShellExecute = true,
                    FileName = Config.Instance.Paths.PreviousLogPath
                };

                process.Start();
            } catch (Exception ex) {
                Log.Warn("MenuOpenPreviousLogClick Error", ex);
            }
        }

        private void MenuSaveAsPreviousLogClick(object sender, RoutedEventArgs e)
        {
            try {
                if (!File.Exists(Config.Instance.Paths.CurrentLogPath)) {
                    TopMostMessageBox.Show(
                        "Log file doesn't exist at " + Config.Instance.Paths.PreviousLogPath,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                var sfd = new SaveFileDialog();
                sfd.Title = "Save Log File To...";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                sfd.FileName = "prevlog.txt";
                sfd.OverwritePrompt = true;
                if ((sfd.ShowDialog() ?? false)) {
                    var str = File.ReadAllText(Config.Instance.Paths.PreviousLogPath);
                    File.WriteAllText(sfd.FileName, str);
                }

            } catch (Exception ex) {
                TopMostMessageBox.Show(
                    "Error " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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
    }
}