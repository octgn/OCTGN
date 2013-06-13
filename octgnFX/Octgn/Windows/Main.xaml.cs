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
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    using Octgn.Core.DataManagers;
    using Octgn.DeckBuilder;

    using agsXMPP;

    using Octgn.Controls;

    using Skylabs.Lobby;

    using log4net;

    /// <summary>
    /// Logic for Main
    /// </summary>
    public partial class Main : OctgnChrome
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initializes a new instance of the <see cref="Main"/> class.
        /// </summary>
        public Main()
        {
            this.InitializeComponent();
#if(Release_Test)
            this.Title = "OCTGN " + "[Test v" + Const.OctgnVersion + "]";
#endif
            ConnectBox.Visibility = Visibility.Hidden;
            Program.LobbyClient.OnStateChanged += this.LobbyClientOnOnStateChanged;
            Program.LobbyClient.OnLoginComplete += this.LobbyClientOnOnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClientOnOnDisconnect;
            this.PreviewKeyUp += this.OnPreviewKeyUp;
            this.Closing += this.OnClosing;
            GameUpdater.Get().Start();
            this.Loaded += OnLoaded;
            //new GameFeedManager().CheckForUpdates();
        }

        private void LobbyClientOnOnDisconnect(object sender, EventArgs eventArgs)
        {
            TopMostMessageBox.Show(
                "You have been disconnected", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Loaded -= OnLoaded;
            SubscriptionModule.Get().IsSubbedChanged += Main_IsSubbedChanged;
        }

        void Main_IsSubbedChanged(bool obj)
        {
            Dispatcher.Invoke(
                new Action(() =>
                          { this.menuSub.Visibility = obj == false ? Visibility.Visible : Visibility.Collapsed; }));
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
            SubscriptionModule.Get().IsSubbedChanged -= this.Main_IsSubbedChanged;
            Program.LobbyClient.OnDisconnect -= LobbyClientOnOnDisconnect;
            Program.LobbyClient.Stop();
            GameUpdater.Get().Stop();
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
                    this.Dispatcher.BeginInvoke(new Action(() =>
                        { 
                            TabCommunityChat.Focus();

                        })).Completed += (o, args) => Task.Factory.StartNew(() => { 
                                                                                      Thread.Sleep(15000);
                                                                                      this.Dispatcher.Invoke(new Action(()
                                                                                                                        =>
                                                                                          {
                                                                                              var s =
                                                                                                  SubscriptionModule.Get
                                                                                                      ().IsSubscribed;
                                                                                              if(s != null && s == false)
                                                                                                this.SubMessage.Visibility = Visibility.Visible;
                                                                                          }));
                        });
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
                        ProfileTab.IsEnabled = false;
                        TabMain.Focus();
                        menuSub.Visibility = Visibility.Collapsed;
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
                        ProfileTab.IsEnabled = true;
                        ProfileTabContent.Load(Program.LobbyClient.Me);
                        var subbed = SubscriptionModule.Get().IsSubscribed;
                        if(subbed == null || subbed == false)
                            menuSub.Visibility = Visibility.Visible;
                        else
                            menuSub.Visibility = Visibility.Collapsed;
                        if (Program.LobbyClient.Me.UserName.Contains(" "))
                            TopMostMessageBox.Show(
                                "WARNING: You have a space in your username. This will cause a host of problems on here. If you don't have a subscription, it would be best to make yourself a new account.",
                                "WARNING",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

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

        private void MenuSubClick(object sender, RoutedEventArgs e)
        {
            this.ShowSubscribeSite(new SubType(){Description = "",Name = ""});
        }

        private void ShowSubscribeSite(SubType subtype)
        {
            Log.InfoFormat("Show sub site {0}", subtype);
            var url = SubscriptionModule.Get().GetSubscribeUrl(subtype);
            if (url != null)
            {
                Program.LaunchUrl(url);
            }
        }

        private void MenuHelpClick(object sender, RoutedEventArgs e)
        {
            Program.LaunchUrl(AppConfig.WebsitePath);
        }
    }
}