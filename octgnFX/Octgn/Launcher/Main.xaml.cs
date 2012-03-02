using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Windows.Controls.Ribbon;
using Octgn.DeckBuilder;
using Octgn.Definitions;
using Octgn.Networking;
using Skylabs.Lobby;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using ContextMenu = System.Windows.Forms.ContextMenu;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace Octgn.Launcher
{
    /// <summary>
    ///   Interaction logic for Main.xaml
    /// </summary>
    public partial class Main
    {
        public static readonly DependencyProperty IsHideLoginNotificationsCheckedProperty =
            DependencyProperty.Register("IsHideLoginNotificationsChecked", typeof (string), typeof (Window),
                                        new UIPropertyMetadata(Prefs.HideLoginNotifications));

        private readonly Duration _duration = new Duration(TimeSpan.FromSeconds(.5));

        public RoutedCommand DebugWindowCommand = new RoutedCommand();
        public NotifyIcon SystemTrayIcon;
        private bool _allowDirectNavigation;
        private SetList _currentSetList;
        private bool _isLegitClosing;
        private NavigatingCancelEventArgs _navArgs;
        private Brush _originalBorderBrush;

        public Main()
        {
            Initialized += MainInitialized;
            InitializeComponent();
            //Set title with version info.
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Title = "OCTGN  verson " + version;

            frame1.Navigate(new ContactList());
            DebugWindowCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));

            var cb = new CommandBinding(DebugWindowCommand,
                                        MyCommandExecute, MyCommandCanExecute);
            CommandBindings.Add(cb);

            var kg = new KeyGesture(Key.M, ModifierKeys.Control);
            var ib = new InputBinding(DebugWindowCommand, kg);
            InputBindings.Add(ib);
            Program.LobbyClient.OnFriendRequest += lobbyClient_OnFriendRequest;
            Program.LobbyClient.OnDisconnect += lobbyClient_OnDisconnectEvent;
            Program.LobbyClient.OnUserStatusChanged += lobbyClient_OnUserStatusChanged;
            Program.LobbyClient.Chatting.EChatEvent += ChattingEChatEvent;
            Program.LobbyClient.OnDataRecieved += lobbyClient_OnDataRecieved;
            tbUsername.Text = Program.LobbyClient.Me.DisplayName;
            tbStatus.Text = Program.LobbyClient.Me.CustomStatus;
            _originalBorderBrush = NotificationTab.Background;
            var cm = new ContextMenu();
            cm.MenuItems.Add("Show", CmShowClick).DefaultItem = true;
            cm.MenuItems.Add("Log Off", CmLogOffClick);
            cm.MenuItems.Add("-");
            cm.MenuItems.Add("Quit", CmQuitClick);
            SystemTrayIcon = new NotifyIcon
                                 {
                                     Icon = new Icon("Resources/Icon.ico"),
                                     Visible = false,
                                     ContextMenu = cm,
                                     Text = Properties.Resources.Main_Main_Octgn
                                 };
            SystemTrayIcon.DoubleClick += SystemTrayIconDoubleClick;
            // Insert code required on object creation below this point.
            RefreshGameFilter(true);
        }

        public void RefreshGameFilter(bool ForceRefresh = false)
        {
            if (!GameList.GamesChanged && !ForceRefresh) return;
            bFilterGames.Items.Clear();
            foreach (Data.Game g in Program.GamesRepository.AllGames)
            {
                Controls.HostedGameListFilterItem h = new Controls.HostedGameListFilterItem
                                                          {
                                                              GameId = g.Id,
                                                              Label = g.Name,
                                                              LargeImageSource =
                                                                  new System.Windows.Media.ImageSourceConverter().
                                                                      ConvertFrom(g.GetCardBackUri()) as
                                                                  System.Windows.Media.ImageSource
                                                          };
                string s = SimpleConfig.ReadValue("FilterGames_" + g.Name);
                h.IsChecked = s == null || Convert.ToBoolean(s);
                h.Checked += GameFilterItem_Checked;
                h.Unchecked += GameFilterItem_Unchecked;
                bFilterGames.Items.Add(h);
            }
            GameList.GamesChanged = false;
        }

        void GameFilterItem_Checked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            GameFiltered(sender as Controls.HostedGameListFilterItem, true);
        }

        void GameFilterItem_Unchecked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            GameFiltered(sender as Controls.HostedGameListFilterItem, false);
        }

        void GameFiltered(Controls.HostedGameListFilterItem sender, Boolean show)
        {
            SimpleConfig.WriteValue("FilterGames_" + sender.Label, show.ToString());
            if (frame1.Content.GetType() != typeof (HostedGameList)) return;
            HostedGameList hostedGameList = frame1.Content as HostedGameList;
            if (hostedGameList != null)
                hostedGameList.FilterGames(sender.GameId, show);
        }

        public string IsHideJoinsChecked
        {
            get { return (string) GetValue(IsHideLoginNotificationsCheckedProperty); }
            set { SetValue(IsHideLoginNotificationsCheckedProperty, value); }
        }

        private void FrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (Content != null && !_allowDirectNavigation)
            {
                e.Cancel = true;

                _navArgs = e;
                Dispatcher.BeginInvoke(new Action(() =>
                                                      {
                                                          var animation0 = new DoubleAnimation
                                                                               {From = 1, To = 0, Duration = _duration};
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
                                   (ThreadStart) delegate
                                                     {
                                                         frame1.UpdateLayout();
                                                         UpdateLayout();
                                                         var animation0 = new DoubleAnimation
                                                                              {From = 0, To = 1, Duration = _duration};
                                                         frame1.BeginAnimation(OpacityProperty, animation0);
                                                     });
        }

        private static void lobbyClient_OnDataRecieved(DataRecType type, object e)
        {
            if (type != DataRecType.ServerMessage) return;
            var m = e as string;
            if (m != null && !String.IsNullOrWhiteSpace(m))
            {
                MessageBox.Show(m, "Server Message", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MainInitialized(object sender, EventArgs e)
        {
            Left = double.Parse(SimpleConfig.ReadValue("MainLeftLoc", "100"));
            Top = double.Parse(SimpleConfig.ReadValue("MainTopLoc", "100"));
        }

        private void SaveLocation()
        {
            SimpleConfig.WriteValue("MainLeftLoc", Left.ToString(CultureInfo.InvariantCulture));
            SimpleConfig.WriteValue("MainTopLoc", Top.ToString(CultureInfo.InvariantCulture));
        }

        private void SystemTrayIconDoubleClick(object sender, EventArgs e)
        {
            CmShowClick(sender, e);
        }

        private void ChattingEChatEvent(ChatRoom cr, Chatting.ChatEvent e, User user, object data)
        {
            Dispatcher.Invoke(new Action(() =>
                                             {
                                                 ChatWindow cw =
                                                     Program.ChatWindows.FirstOrDefault(cww => cww.Id == cr.Id);
                                                 if (cw == null)
                                                 {
                                                     var c = new ChatWindow(cr.Id);
                                                     c.Loaded += delegate { c.ChatEvent(cr, e, user, data); };
                                                     Program.ChatWindows.Add(c);
                                                     if (cr.Id != 0)
                                                         c.Show();
                                                 }
                                                 else
                                                 {
                                                     if (cw.Id != 0)
                                                     {
                                                         cw.Show();
                                                     }
                                                 }
                                             }));
        }

        private void CmQuitClick(object sender, EventArgs e)
        {
            CloseDownShop(true);
        }

        private void CmLogOffClick(object sender, EventArgs e)
        {
            CloseDownShop(false);
        }

        private void CmShowClick(object sender, EventArgs e)
        {
            Visibility = Visibility.Visible;
            SystemTrayIcon.Visible = false;
        }

        private void lobbyClient_OnFriendRequest(User u)
        {
            Dispatcher.Invoke(new Action(() =>
                                             {
                                                 if (frame1.Content as NotificationList != null) return;
                                                 NotificationTab.HeaderStyle =
                                                     Resources["AlertHeaderColor"] as Style;
                                                 NotificationTab.InvalidateVisual();
                                             }));
        }

        private void RibbonSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tab = Ribbon.SelectedItem as RibbonTab;
            if (tab == null) return;
            switch ((String) tab.Header)
            {
                case "Lobby":
                    LobbyTab();
                    break;
                case "Host/Join":
                    HostJoinTab();
                    break;
                case "Games":
                    var gl = new GameList();
                    gl.OnGameClick += gl_OnGameDoubleClick;
                    frame1.Navigate(gl);
                    break;
                case "!":
                    frame1.Navigate(new NotificationList());
                    NotificationTab.HeaderStyle = Resources["NormalHeaderColor"] as Style;
                    NotificationTab.InvalidateVisual();
                    break;
            }
        }

        public void LobbyTab()
        {
            frame1.Navigate(new ContactList());
        }

        public void HostJoinTab()
        {
            var hgl = new HostedGameList();
            hgl.OnGameClick += hgl_OnGameClick;
            RefreshGameFilter();
            frame1.Navigate(hgl);
        }

        private void gl_OnGameDoubleClick(object sender, EventArgs e)
        {
            var g = sender as Data.Game;
            _currentSetList = new SetList(g);
            frame1.Navigate(_currentSetList);
        }

        private void QuitClick(object sender, RoutedEventArgs e)
        {
            CloseDownShop(true);
            Program.Exit();
        }

        private void lobbyClient_OnDisconnectEvent(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action<bool>(CloseDownShop), false);
        }

        private void LogOffClick(object sender, RoutedEventArgs e)
        {
            CloseDownShop(false);
        }

        private void CloseDownShop(bool exiting)
        {
            _isLegitClosing = true;
            SaveLocation();
            SystemTrayIcon.Visible = false;
            SystemTrayIcon.Dispose();
            if (Program.DeckEditor != null)
                Program.DeckEditor.Close();
            foreach (ChatWindow cw in Program.ChatWindows)
            {
                cw.CloseChatWindow();
            }
            Program.ChatWindows.Clear();
            if (!exiting)
            {
                if (Program.LauncherWindow != null)
                {
                    if (Program.LauncherWindow.IsLoaded)
                    {
                        Program.LauncherWindow.Close();
                    }
                    Program.LauncherWindow = null;
                }
                Program.LauncherWindow = new LauncherWindow();
                Program.LauncherWindow.Show();
                Application.Current.MainWindow = Program.LauncherWindow;
            }
            else
            {
                if (Program.LauncherWindow != null)
                {
                    if (Program.LauncherWindow.IsLoaded)
                    {
                        Program.LauncherWindow.Close();
                    }
                    Program.LauncherWindow = null;
                }
                Application.Current.MainWindow = Program.ClientWindow;
            }
            Program.ClientWindow.Close();
            Program.LobbyClient.OnFriendRequest -= lobbyClient_OnFriendRequest;
            Program.LobbyClient.OnDisconnect -= lobbyClient_OnDisconnectEvent;
            Program.LobbyClient.OnUserStatusChanged -= lobbyClient_OnUserStatusChanged;
            Program.LobbyClient.OnDataRecieved -= lobbyClient_OnDataRecieved;
            Program.LobbyClient.Stop();
            Program.Exit();
            //Program.lobbyClient.Close(DisconnectReason.CleanDisconnect);
        }

        private void RibbonButtonClick(object sender, RoutedEventArgs e)
        {
            var gl = frame1.Content as GameList;
            if (gl == null)
            {
                gl = new GameList(GameList.LoadEvent.InstallGame);
                gl.OnGameClick += gl_OnGameDoubleClick;
                frame1.Navigate(gl);
            }
            else
                gl.InstallGame();
        }

        private void RibbonButtonClick1(object sender, RoutedEventArgs e)
        {
            _currentSetList = null;
            var gl = new GameList();
            gl.OnGameClick += gl_OnGameDoubleClick;
            frame1.Navigate(gl);
        }

        private void RibbonButtonClick2(object sender, RoutedEventArgs e)
        {
            if (_currentSetList != null)
                _currentSetList.DeletedSelected();
        }

        private void BInstallSetsClick(object sender, RoutedEventArgs e)
        {
            if (_currentSetList != null)
                _currentSetList.InstallSets();
        }

        private void BPatchSetsClick(object sender, RoutedEventArgs e)
        {
            if (_currentSetList != null)
                _currentSetList.PatchSelected();
        }

        private void BDeckEditorClick(object sender, RoutedEventArgs e)
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

        private void BAddFriendClick(object sender, RoutedEventArgs e)
        {
            frame1.Navigate(new AddFriendPage());
        }

        private void BHostClick(object sender, RoutedEventArgs e)
        {
            var gl = new GameList();
            gl.OnGameClick += GlHostGameClick;
            frame1.Navigate(gl);
        }

        private void GlHostGameClick(object sender, EventArgs e)
        {
            if (Program.PlayWindow != null) return;
            var g = sender as Data.Game;
            frame1.Navigate(new HostGameSettings(g));
        }

        private static void MyCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static void MyCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            //System.Diagnostics.XmlWriterTraceListener tr = new System.Diagnostics.XmlWriterTraceListener()
            if (Program.DebugWindow == null)
            {
                Program.DebugWindow = new DWindow();
            }
            Program.DebugWindow.Visibility = Program.DebugWindow.Visibility == Visibility.Visible
                                                 ? Visibility.Hidden
                                                 : Visibility.Visible;
        }

        private void BJoinClick(object sender, RoutedEventArgs e)
        {
            var hgl = new HostedGameList();
            hgl.OnGameClick += hgl_OnGameClick;
            frame1.Navigate(hgl);
        }

        public void StartGame()
        {
            var sg = frame1.Content as StartGame;
            if (sg == null) return;
            sg.Start();
            frame1.Navigate(new HostedGameList());
        }

        private void hgl_OnGameClick(object sender, EventArgs e)
        {
            if (Program.PlayWindow != null) return;
            var hg = sender as HostedGame;
            Program.IsHost = false;
            Data.Game theGame =
                Program.GamesRepository.AllGames.FirstOrDefault(g => hg != null && g.Id == hg.GameGuid);
            if (theGame == null) return;
            Program.Game = new Game(GameDef.FromO8G(theGame.Filename));
#if(DEBUG)
            var ad = new IPAddress[1];
            IPAddress ip = IPAddress.Parse("127.0.0.1");

#else
            var ad = Dns.GetHostAddresses(Program.LobbySettings.Server);
            IPAddress ip = ad[0];
#endif

            if (ad.Length <= 0) return;
            try
            {
                if (hg != null) Program.Client = new Client(ip, hg.Port);
                Program.Client.Connect();
                Dispatcher.Invoke(new Action(() => frame1.Navigate(new StartGame())));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                if (Debugger.IsAttached) Debugger.Break();
            }
        }

        private void BOnlineStatusClick(object sender, RoutedEventArgs e)
        {
            rgStatus.LargeImageSource = bOnlineStatus.LargeImageSource;
            Program.LobbyClient.SetStatus(UserStatus.Online);
        }

        private void BBusyStatusClick(object sender, RoutedEventArgs e)
        {
            rgStatus.LargeImageSource = bBusyStatus.LargeImageSource;
            Program.LobbyClient.SetStatus(UserStatus.DoNotDisturb);
        }

        private void BOfflineStatusClick(object sender, RoutedEventArgs e)
        {
            rgStatus.LargeImageSource = bOfflineStatus.LargeImageSource;
            Program.LobbyClient.SetStatus(UserStatus.Invisible);
        }

        private void BAwayStatusClick(object sender, RoutedEventArgs e)
        {
            rgStatus.LargeImageSource = bAwayStatus.LargeImageSource;
            Program.LobbyClient.SetStatus(UserStatus.Away);
        }

        private void lobbyClient_OnUserStatusChanged(UserStatus eve, User u)
        {
            Dispatcher.Invoke(new Action(() =>
                                             {
                                                 if (!u.Equals(Program.LobbyClient.Me)) return;
                                                 tbUsername.Text = Program.LobbyClient.Me.DisplayName;
                                                 tbStatus.Text = Program.LobbyClient.Me.CustomStatus;
                                                 SimpleConfig.WriteValue("Nickname", Program.LobbyClient.Me.DisplayName);
                                             }));
        }

        private void TbUsernameMouseUp(object sender, MouseButtonEventArgs e)
        {
            tbUsername.Style = (Style) TryFindResource(typeof (TextBox));
            tbUsername.Focus();
            tbUsername.SelectAll();
        }

        private void TbUsernameLostFocus(object sender, RoutedEventArgs e)
        {
            tbUsername.Style = (Style) TryFindResource("LabelBoxUnSelected");
            tbUsername.Text = Program.LobbyClient.Me.DisplayName;
        }

        private void TbUsernameLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbUsername.Style = (Style) TryFindResource("LabelBoxUnSelected");
            tbUsername.Text = Program.LobbyClient.Me.DisplayName;
        }

        private void TbUsernameKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            Program.LobbyClient.SetDisplayName(tbUsername.Text);
            tbUsername.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
        }

        private void TbStatusMouseUp(object sender, MouseButtonEventArgs e)
        {
            tbStatus.Style = (Style) TryFindResource(typeof (TextBox));
            tbStatus.Focus();
            tbStatus.SelectAll();
        }

        private void TbStatusLostFocus(object sender, RoutedEventArgs e)
        {
            tbStatus.Style = (Style) TryFindResource("LabelBoxUnSelected");
            tbStatus.Text = Program.LobbyClient.Me.CustomStatus;
            if (String.IsNullOrWhiteSpace(tbStatus.Text) && !tbStatus.IsKeyboardFocused)
                tbStatus.Text = "Set a custom status here";
        }

        private void TbStatusLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbStatus.Style = (Style) TryFindResource("LabelBoxUnSelected");
            tbStatus.Text = Program.LobbyClient.Me.CustomStatus;
            if (String.IsNullOrWhiteSpace(tbStatus.Text) && !tbStatus.IsKeyboardFocused)
                tbStatus.Text = "Set a custom status here";
        }

        private void TbStatusKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            Program.LobbyClient.SetCustomStatus(tbStatus.Text);
            tbStatus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
        }

        private void TbStatusTextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(tbStatus.Text) && !tbStatus.IsKeyboardFocused)
                tbStatus.Text = "Set a custom status here";
        }

        private void BHideLoginNotificationsUnchecked(object sender, RoutedEventArgs e)
        {
            Prefs.HideLoginNotifications = "false";
        }

        private void BHideLoginNotificationsChecked(object sender, RoutedEventArgs e)
        {
            Prefs.HideLoginNotifications = "true";
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (_isLegitClosing) return;
            SystemTrayIcon.Visible = true;
            Visibility = Visibility.Hidden;
            SystemTrayIcon.ShowBalloonTip(5000, "OCTGN",
                                          "OCTGN has minimized to your system tray and is still running. Double click the icon to open it again.",
                                          ToolTipIcon.Info);
            e.Cancel = true;
        }
    }
}