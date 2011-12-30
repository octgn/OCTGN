using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Windows.Controls.Ribbon;
using Octgn.DeckBuilder;
using Octgn.Definitions;
using Skylabs.Lobby;
using Skylabs.Net;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : RibbonWindow
    {
        public RoutedCommand DebugWindowCommand = new RoutedCommand();
        private bool _allowDirectNavigation = false;
        private NavigatingCancelEventArgs _navArgs = null;
        private Duration _duration = new Duration(TimeSpan.FromSeconds(.5));
        private SetList _currentSetList;
        private Brush _originalBorderBrush;
        private void frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (Content != null && !_allowDirectNavigation)
            {
                e.Cancel = true;

                _navArgs = e;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DoubleAnimation animation0 = new DoubleAnimation();
                    animation0.From = 1;
                    animation0.To = 0;
                    animation0.Duration = _duration;
                    animation0.Completed += SlideCompleted;
                    frame1.BeginAnimation(OpacityProperty, animation0);
                }));
            }
            _allowDirectNavigation = false;
        }

        private void SlideCompleted(object sender, EventArgs e)
        {
            _allowDirectNavigation = true;
            switch (_navArgs.NavigationMode)
            {
                case NavigationMode.New:
                    if (_navArgs.Uri == null)
                        frame1.Navigate(_navArgs.Content);
                    else
                        frame1.Navigate(_navArgs.Uri);
                    break;
                case NavigationMode.Back:
                    frame1.GoBack();
                    break;
                case NavigationMode.Forward:
                    frame1.GoForward();
                    break;
                case NavigationMode.Refresh:
                    frame1.Refresh();
                    break;
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                (ThreadStart)delegate()
                {
                    frame1.UpdateLayout();
                    UpdateLayout();
                    DoubleAnimation animation0 = new DoubleAnimation();
                    animation0.From = 0;
                    animation0.To = 1;
                    animation0.Duration = _duration;
                    frame1.BeginAnimation(OpacityProperty, animation0);
                });
        }

        public Main()
        {
            InitializeComponent();
            frame1.Navigate(new ContactList());
            DebugWindowCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));

            CommandBinding cb = new CommandBinding(DebugWindowCommand,
                MyCommandExecute, MyCommandCanExecute);
            this.CommandBindings.Add(cb);

            KeyGesture kg = new KeyGesture(Key.M, ModifierKeys.Control);
            InputBinding ib = new InputBinding(DebugWindowCommand, kg);
            this.InputBindings.Add(ib);
            Program.lobbyClient.OnFriendRequest += new LobbyClient.FriendRequest(lobbyClient_OnFriendRequest);
            Program.lobbyClient.OnDisconnectEvent += new EventHandler(lobbyClient_OnDisconnectEvent);
            Program.lobbyClient.OnUserStatusChanged += new LobbyClient.UserStatusChanged(lobbyClient_OnUserStatusChanged);
            tbUsername.Text = Program.lobbyClient.Me.DisplayName;
            tbStatus.Text = Program.lobbyClient.Me.CustomStatus;
            _originalBorderBrush = NotificationTab.Background;
            // Insert code required on object creation below this point.
        }

        void lobbyClient_OnFriendRequest(User u)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (frame1.Content as NotificationList == null)
                {
                    NotificationTab.HeaderStyle = this.Resources["AlertHeaderColor"] as Style;
                    NotificationTab.InvalidateVisual();
                }

            }));

        }

        private void Ribbon_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RibbonTab tab = Ribbon.SelectedItem as RibbonTab;
            if (tab != null)
            {
                switch ((String)tab.Header)
                {
                    case "Lobby":
                        frame1.Navigate(new ContactList());
                        break;
                    case "Host/Join":
                        HostedGameList hgl = new HostedGameList();
                        hgl.OnGameClick += new EventHandler(hgl_OnGameClick);
                        frame1.Navigate(hgl);
                        break;
                    case "Games":
                        GameList gl = new GameList();
                        gl.OnGameClick += new EventHandler(gl_OnGameDoubleClick);
                        frame1.Navigate(gl);
                        break;
                    case "!":
                        frame1.Navigate(new NotificationList());
                        NotificationTab.HeaderStyle = this.Resources["NormalHeaderColor"] as Style;
                        NotificationTab.InvalidateVisual();
                        break;
                }
            }
        }

        void gl_OnGameDoubleClick(object sender, EventArgs e)
        {
            Data.Game g = sender as Data.Game;
            _currentSetList = new SetList(g);
            frame1.Navigate(_currentSetList);
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Program.lobbyClient.Close(DisconnectReason.CleanDisconnect);
            Program.Exit();
        }
        void lobbyClient_OnDisconnectEvent(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(CloseDownShop));
        }
        private void LogOff_Click(object sender, RoutedEventArgs e)
        {
            CloseDownShop();
        }
        private void CloseDownShop()
        {
            if (Program.DeckEditor != null)
                Program.DeckEditor.Close();
            Program.LauncherWindow = new LauncherWindow();
            Program.LauncherWindow.Show();
            Program.ClientWindow.Close();
            Program.lobbyClient.OnFriendRequest -= lobbyClient_OnFriendRequest;
            Program.lobbyClient.OnDisconnectEvent -= lobbyClient_OnDisconnectEvent;
            Program.lobbyClient.OnUserStatusChanged -= lobbyClient_OnUserStatusChanged;
            Program.lobbyClient.Close(DisconnectReason.CleanDisconnect);
        }
        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            GameList gl = frame1.Content as GameList;
            if (gl == null)
            {
                gl = new GameList(GameList.LoadEvent.InstallGame);
                gl.OnGameClick += new EventHandler(gl_OnGameDoubleClick);
                frame1.Navigate(gl);
            }
            else
                gl.Install_Game();

        }

        private void RibbonButton_Click_1(object sender, RoutedEventArgs e)
        {
            _currentSetList = null;
            GameList gl = new GameList();
            gl.OnGameClick += new EventHandler(gl_OnGameDoubleClick);
            frame1.Navigate(gl);
        }

        private void RibbonButton_Click_2(object sender, RoutedEventArgs e)
        {
            if (_currentSetList != null)
                _currentSetList.Deleted_Selected();
        }

        private void bInstallSets_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSetList != null)
                _currentSetList.Install_Sets();
        }

        private void bPatchSets_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSetList != null)
                _currentSetList.Patch_Selected();
        }

        private void bDeckEditor_Click(object sender, RoutedEventArgs e)
        {
            if (Program.GamesRepository.Games.Count == 0)
            {
                MessageBox.Show("You have no game installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Program.DeckEditor == null)
            {
                Program.DeckEditor = new DeckBuilderWindow();
                Program.DeckEditor.Show();
            }
            else if (Program.DeckEditor.IsVisible == false)
            {
                Program.DeckEditor = new DeckBuilderWindow();
                Program.DeckEditor.Show();
            }
        }

        private void bAddFriend_Click(object sender, RoutedEventArgs e)
        {
            frame1.Navigate(new AddFriendPage());
        }

        private void bHost_Click(object sender, RoutedEventArgs e)
        {
            GameList gl = new GameList();
            gl.OnGameClick += new EventHandler(gl_HostGameClick);
            frame1.Navigate(gl);
        }

        void gl_HostGameClick(object sender, EventArgs e)
        {
            if (Program.PlayWindow == null)
            {
                Data.Game g = sender as Data.Game;
                frame1.Navigate(new HostGameSettings(g));
            }
        }
        private void MyCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MyCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            //System.Diagnostics.XmlWriterTraceListener tr = new System.Diagnostics.XmlWriterTraceListener()
            if (Program.DebugWindow == null)
            {
                Program.DebugWindow = new DWindow();
            }
            if (Program.DebugWindow.Visibility == System.Windows.Visibility.Visible)
                Program.DebugWindow.Visibility = System.Windows.Visibility.Hidden;
            else
                Program.DebugWindow.Visibility = System.Windows.Visibility.Visible;
        }

        private void bJoin_Click(object sender, RoutedEventArgs e)
        {
            HostedGameList hgl = new HostedGameList();
            hgl.OnGameClick += new EventHandler(hgl_OnGameClick);
            frame1.Navigate(hgl);
        }
        public void StartGame()
        {
            StartGame sg = frame1.Content as StartGame;
            if (sg != null)
            {
                sg.Start();
                frame1.Navigate(new HostedGameList());
            }
        }
        void hgl_OnGameClick(object sender, EventArgs e)
        {
            if (Program.PlayWindow == null)
            {
                HostedGame hg = sender as HostedGame;
                Program.IsHost = false;
                Octgn.Data.Game theGame = Program.GamesRepository.AllGames.FirstOrDefault(g => g.Id == hg.GameGuid);
                if (theGame != null)
                {
                    Program.Game = new Game(GameDef.FromO8G(theGame.Filename));
                    IPAddress[] ad = new IPAddress[0];
#if(DEBUG)
                    ad = new IPAddress[1];
                    IPAddress ip = IPAddress.Parse("127.0.0.1");

#else
                ad = Dns.GetHostAddresses(Program.LobbySettings.Server);
                IPAddress ip = ad[0];
#endif

                    if (ad.Length > 0)
                    {
                        try
                        {
                            Program.Client = new Networking.Client(ip, hg.Port);
                            Program.Client.Connect();
                            this.Dispatcher.Invoke(new Action(() => frame1.Navigate(new StartGame())));
                        }
                        catch (Exception) { }

                    }
                }
            }
        }

        private void bOnlineStatus_Click(object sender, RoutedEventArgs e)
        {
            rgStatus.LargeImageSource = bOnlineStatus.LargeImageSource;
            Program.lobbyClient.SetStatus(UserStatus.Online);
        }

        private void bBusyStatus_Click(object sender, RoutedEventArgs e)
        {
            rgStatus.LargeImageSource = bBusyStatus.LargeImageSource;
            Program.lobbyClient.SetStatus(UserStatus.DoNotDisturb);
        }

        private void bOfflineStatus_Click(object sender, RoutedEventArgs e)
        {
            rgStatus.LargeImageSource = bOfflineStatus.LargeImageSource;
            Program.lobbyClient.SetStatus(UserStatus.Invisible);
        }

        private void bAwayStatus_Click(object sender, RoutedEventArgs e)
        {
            rgStatus.LargeImageSource = bAwayStatus.LargeImageSource;
            Program.lobbyClient.SetStatus(UserStatus.Away);
        }
        void lobbyClient_OnUserStatusChanged(UserStatus eve, User u)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (u.Equals(Program.lobbyClient.Me))
                {
                    tbUsername.Text = Program.lobbyClient.Me.DisplayName;
                    tbStatus.Text = Program.lobbyClient.Me.CustomStatus;
                }
            }));

        }
        private void tbUsername_MouseUp(object sender, MouseButtonEventArgs e)
        {
            tbUsername.Style = (Style)TryFindResource(typeof(TextBox));
            tbUsername.Focus();
            tbUsername.SelectAll();
        }

        private void tbUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            tbUsername.Style = (Style)TryFindResource("LabelBoxUnSelected");
            tbUsername.Text = Program.lobbyClient.Me.DisplayName;
        }
        private void tbUsername_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbUsername.Style = (Style)TryFindResource("LabelBoxUnSelected");
            tbUsername.Text = Program.lobbyClient.Me.DisplayName;
        }
        private void tbUsername_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Program.lobbyClient.SetDisplayName(tbUsername.Text);
                tbUsername.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
            }
        }

        private void tbStatus_MouseUp(object sender, MouseButtonEventArgs e)
        {
            tbStatus.Style = (Style)TryFindResource(typeof(TextBox));
            tbStatus.Focus();
            tbStatus.SelectAll();
        }

        private void tbStatus_LostFocus(object sender, RoutedEventArgs e)
        {
            tbStatus.Style = (Style)TryFindResource("LabelBoxUnSelected");
            tbStatus.Text = Program.lobbyClient.Me.CustomStatus;
            if (String.IsNullOrWhiteSpace(tbStatus.Text) && !tbStatus.IsKeyboardFocused)
                tbStatus.Text = "Set a custom status here";
        }
        private void tbStatus_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbStatus.Style = (Style)TryFindResource("LabelBoxUnSelected");
            tbStatus.Text = Program.lobbyClient.Me.CustomStatus;
            if (String.IsNullOrWhiteSpace(tbStatus.Text) && !tbStatus.IsKeyboardFocused)
                tbStatus.Text = "Set a custom status here";
        }

        private void tbStatus_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Program.lobbyClient.SetCustomStatus(tbStatus.Text);
                tbStatus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
            }
        }

        private void tbStatus_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(tbStatus.Text) && !tbStatus.IsKeyboardFocused)
                tbStatus.Text = "Set a custom status here";
        }

    }
}