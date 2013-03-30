// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Main.xaml.cs" company="OCTGN">
//   GNU Stuff
// </copyright>
// <summary>
//   Interaction logic for Main.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Octgn.Windows
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;

    using Octgn.Core.DataManagers;
    using Octgn.DeckBuilder;
    using Octgn.GameManagement;

    using agsXMPP;

    using Octgn.Controls;

    using Skylabs.Lobby;

    /// <summary>
    /// Logic for Main
    /// </summary>
    public partial class Main : OctgnChrome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Main"/> class.
        /// </summary>
        public Main()
        {
            this.InitializeComponent();
            ConnectBox.Visibility = Visibility.Hidden;
            Program.LobbyClient.OnStateChanged += this.LobbyClientOnOnStateChanged;
            Program.LobbyClient.OnLoginComplete += this.LobbyClientOnOnLoginComplete;
            this.PreviewKeyUp += this.OnPreviewKeyUp;
            this.Closing += this.OnClosing;
            Core.GameFeedManager.Get().Start();
            //new GameFeedManager().CheckForUpdates();
        }

        /// <summary>
        /// Gets or sets the connect message.
        /// </summary>
        private string ConnectMessage
        {
            get
            {
                var textboxText = string.Empty;
                Dispatcher.Invoke(new Action(() =>
                                                 {
                                                     textboxText = tbConnect.Content as string;
                                                     ConnectBox.Visibility = string.IsNullOrWhiteSpace(textboxText) ? Visibility.Hidden : Visibility.Visible;
                                                 }));
                return textboxText;
            }

            set
            {
                Dispatcher.BeginInvoke(new Action(() =>
                                                      {
                                                          tbConnect.Content = value;
                                                          ConnectBox.Visibility = string.IsNullOrWhiteSpace(value) ? Visibility.Hidden : Visibility.Visible;
                                                      }));
            }
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
            Program.LobbyClient.Stop();
            Core.GameFeedManager.Get().Stop();
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
            switch (keyEventArgs.Key)
            {
                case Key.Escape:
                    ChatBar.HideChat();
                    break;
            }
        }

        #region LobbyEvents

        /// <summary>
        /// The lobby client on on state changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="state">
        /// The state.
        /// </param>
        private void LobbyClientOnOnStateChanged(object sender, XmppConnectionState state)
        {
            this.ConnectMessage = state == XmppConnectionState.Disconnected ? string.Empty : state.ToString();
            if (state == XmppConnectionState.Disconnected)
            {
                this.SetStateOffline();
            }
        }

        /// <summary>
        /// The lobby client on on login complete.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="results">
        /// The results.
        /// </param>
        private void LobbyClientOnOnLoginComplete(object sender, LoginResults results)
        {
            this.Dispatcher.BeginInvoke(new Action(() => ConnectBox.Visibility = Visibility.Hidden));
            switch (results)
            {
                case LoginResults.Success:
                    this.SetStateOnline();
                    this.Dispatcher.BeginInvoke(new Action(() => TabCommunityChat.Focus()));
                    break;
                default:
                    this.SetStateOffline();
                    break;
            }
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
                        TabCommunityChat.IsEnabled = false;
                        TabMain.Focus();
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
                        TabCommunityChat.IsEnabled = true;
                    }));
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
            if (GameManager.Get().GameCount == 0)
            {
                MessageBox.Show(
                    "You need to install a game before you can use the deck editor.",
                    "OCTGN",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            if (Program.DeckEditor == null)
            {
                Program.DeckEditor = new DeckBuilderWindow();
                Program.DeckEditor.Show();
            }
        }

        private void MenuOptionsClick(object sender, RoutedEventArgs e)
        {
            var options = new Options();
            options.ShowDialog();
        }

        private void MenuLogOffClick(object sender, RoutedEventArgs e)
        {
            Program.LobbyClient.LogOut();
        }
    }
}