using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
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
using Octgn.Launcher;
using Skylabs.Lobby;
using agsXMPP;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Client = Octgn.Networking.Client;
using ContextMenu = System.Windows.Forms.ContextMenu;
using Cursors = System.Windows.Input.Cursors;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;
using Octgn.Data;

namespace Octgn.Windows
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
        public string IsHideJoinsChecked
        {
            get { return (string) GetValue(IsHideLoginNotificationsCheckedProperty); }
            set { SetValue(IsHideLoginNotificationsCheckedProperty, value); }
        }
        public RoutedCommand DebugWindowCommand = new RoutedCommand();
        public NotifyIcon SystemTrayIcon;
        private bool _allowDirectNavigation;
        private SetList _currentSetList;
        private bool _isLegitClosing;
        private NavigatingCancelEventArgs _navArgs;
        private Brush _originalBorderBrush;
        private bool _joiningGame = false;

        public Main()
        {
            Initialized += MainInitialized;
            InitializeComponent();
            frame1.NavigationService.LoadCompleted += delegate(object sender, NavigationEventArgs args)
                                                      { this.frame1.NavigationService.RemoveBackEntry(); };
            //Set title with version info.
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Title = "Octgn  version " + version;

            frame1.Navigate(new ContactList());
            DebugWindowCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));

            var cb = new CommandBinding(DebugWindowCommand,
                                        MyCommandExecute, MyCommandCanExecute);
            CommandBindings.Add(cb);

            var kg = new KeyGesture(Key.M, ModifierKeys.Control);
            var ib = new InputBinding(DebugWindowCommand, kg);
            InputBindings.Add(ib);
            //Program.LobbyClient.OnFriendRequest += lobbyClient_OnFriendRequest;
            Program.LobbyClient.OnFriendRequest += LobbyClientOnOnFriendRequest;
            Program.LobbyClient.OnDataRecieved += LobbyClientOnOnDataRecieved;
            Program.LobbyClient.OnDisconnect += LobbyClientOnOnDisconnect;
            tbUsername.Text = Program.LobbyClient.Username;
            tbStatus.Text = Program.LobbyClient.CustomStatus;
            _originalBorderBrush = NotificationTab.Background;
            var cm = new ContextMenu();
            cm.MenuItems.Add("Show", CmShowClick).DefaultItem = true;
            cm.MenuItems.Add("Log Off", CmLogOffClick);
            cm.MenuItems.Add("-");
            cm.MenuItems.Add("Quit", CmQuitClick);
            SystemTrayIcon = new NotifyIcon
                                 {
                                     Icon = Properties.Resources.Icon,
                                     Visible = false,
                                     ContextMenu = cm,
                                     Text = Properties.Resources.Main_Main_Octgn
                                 };
            SystemTrayIcon.DoubleClick += SystemTrayIconDoubleClick;
            // Insert code required on object creation below this point.
            RefreshGameFilter(true);
            tbUsername.Cursor = Cursors.Arrow;
            tbUsername.ForceCursor = true;
            tbStatus.Cursor = Cursors.Pen;
            tbStatus.ForceCursor = true;
        }

        private void LobbyClientOnOnDisconnect(object sender , EventArgs eventArgs)
        {

            Program.LobbyClient.OnDisconnect -= LobbyClientOnOnDisconnect;
            if(!Program.LobbyClient.DisconnectedBecauseConnectionReplaced)
            {
                Dispatcher.Invoke(new Action(() =>
                {

                    var win = ReconnectingWindow.Reconnect();
                    if(win.Canceled)
                    {
                        CloseDownShop(false);
                        return;
                    }
                    Program.LobbyClient.OnDisconnect += LobbyClientOnOnDisconnect;
                }));
            }
            else
            {
                Dispatcher.Invoke(new Action(() =>
                {

                    CloseDownShop(false);
                    MessageBox.Show("You have been logged out because you signed in somewhere else.");
                    Program.LobbyClient.OnDisconnect += LobbyClientOnOnDisconnect;
                }));                
            }
        }

        private void LobbyClientOnOnDataRecieved(object sender, Skylabs.Lobby.Client.DataRecType type, object data)
        {
            Dispatcher.Invoke(new Action(()=>
            {
                switch(type)
                {
                    case Skylabs.Lobby.Client.DataRecType.FriendList:
                        break;
                    case Skylabs.Lobby.Client.DataRecType.MyInfo:
                        break;
                    case Skylabs.Lobby.Client.DataRecType.GameList:
                        break;
                    case Skylabs.Lobby.Client.DataRecType.HostedGameReady:
                        break;
                    case Skylabs.Lobby.Client.DataRecType.GamesNeedRefresh:
                        break;
                    case Skylabs.Lobby.Client.DataRecType.Announcement:
                        var d = data as Dictionary<String , String>;
                        if(d != null)
                            MessageBox.Show(d["Message"] , d["Subject"] , MessageBoxButton.OK , MessageBoxImage.Exclamation);
                        break;
                }
                tbStatus.Text = Program.LobbyClient.CustomStatus;
                switch(Program.LobbyClient.Status)
                {
                    case UserStatus.Unknown:
                        rgStatus.LargeImageSource = bOfflineStatus.LargeImageSource;
                        break;
                    case UserStatus.Offline:
                        rgStatus.LargeImageSource = bOfflineStatus.LargeImageSource;
                        break;
                    case UserStatus.Online:
                        rgStatus.LargeImageSource = bOnlineStatus.LargeImageSource;
                        break;
                    case UserStatus.Away:
                        rgStatus.LargeImageSource = bAwayStatus.LargeImageSource;
                        break;
                    case UserStatus.DoNotDisturb:
                        rgStatus.LargeImageSource = bBusyStatus.LargeImageSource;
                        break;
                    case UserStatus.Invisible:
                        rgStatus.LargeImageSource = bOfflineStatus.LargeImageSource;
                        break;
                }
                Prefs.Nickname = Program.LobbyClient.Me.User.User;
            }
            ));
        }

        private void LobbyClientOnOnFriendRequest(object sender, Jid user)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (frame1.Content as NotificationList != null) return;
                NotificationTab.HeaderStyle =
                    Resources["AlertHeaderColor"] as Style;
                NotificationTab.InvalidateVisual();
            }));
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
                h.IsChecked = Prefs.getFilterGame(g.Name);
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
            Prefs.setFilterGame(sender.Label,show);
            if (frame1.Content.GetType() != typeof (HostedGameList)) return;
            HostedGameList hostedGameList = frame1.Content as HostedGameList;
            if (hostedGameList != null)
                hostedGameList.FilterGames(sender.GameId, show);
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
        
        private void MainInitialized(object sender, EventArgs e)
        {
            Left = Prefs.MainLocation.X;
            Top = Prefs.MainLocation.Y;
        }

        private void SaveLocation()
        {
            Prefs.MainLocation = new System.Windows.Point(Left,Top);
        }

        private void SystemTrayIconDoubleClick(object sender, EventArgs e)
        {
            CmShowClick(sender, e);
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
                case "Alerts":
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
                Program.LauncherWindow = new Windows.LauncherWindow();
                Program.LauncherWindow.Show();
                if(Application.Current != null)
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
                Application.Current.MainWindow = Program.MainWindow;                
            }
            Program.MainWindow.Close();
            Program.LobbyClient.OnDataRecieved -= LobbyClientOnOnDataRecieved;
            Program.LobbyClient.OnFriendRequest -= LobbyClientOnOnFriendRequest;
            Program.LobbyClient.OnDisconnect -= LobbyClientOnOnDisconnect;
            Program.LobbyClient.Stop();
            if (exiting) Program.Exit();
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
                Program.DebugWindow = new Windows.DWindow();
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
            var hg = sender as HostedGameData;
            Data.Game theGame =
                Program.GamesRepository.AllGames.FirstOrDefault(g => hg != null && g.Id == hg.GameGuid);
            if (theGame == null) return;
            if (_joiningGame) return;
            _joiningGame = true;
            Program.IsHost = false;
            Program.Game = new Game(GameDef.FromO8G(theGame.FullPath));
#if(DEBUG)
            var ad = new IPAddress[1];
            IPAddress ip = IPAddress.Parse("127.0.0.1");

#else
            var ad = Dns.GetHostAddresses("www.skylabsonline.com");
            IPAddress ip = ad[0];
#endif

            if (ad.Length <= 0) return;
            try
            {
                if (hg != null) Program.Client = new Client(ip, hg.Port);
                Program.Client.Connect();
                Dispatcher.Invoke(new Action(() => frame1.Navigate(new StartGame())));
                _joiningGame = false;
            }
            catch (Exception ex)
            {
                _joiningGame = false;
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

        private void TbUsernameMouseUp(object sender, MouseButtonEventArgs e)
        {
            //tbUsername.Style = (Style) TryFindResource(typeof (TextBox));
            //tbUsername.Focus();
            //tbUsername.SelectAll();
        }

        private void TbUsernameLostFocus(object sender, RoutedEventArgs e)
        {
            tbUsername.Style = (Style) TryFindResource("LabelBoxUnSelected");
            tbUsername.Text = Program.LobbyClient.Username;
            //tbUsername.Text = Program.LobbyClient.Me.DisplayName;
        }

        private void TbUsernameLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbUsername.Style = (Style) TryFindResource("LabelBoxUnSelected");
            tbUsername.Text = Program.LobbyClient.Username;
            //tbUsername.Text = Program.LobbyClient.Me.DisplayName;
        }

        private void TbUsernameKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            //Program.LobbyClient.SetDisplayName(tbUsername.Text);
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
            tbStatus.Text = Program.LobbyClient.CustomStatus;
            if (String.IsNullOrWhiteSpace(tbStatus.Text) && !tbStatus.IsKeyboardFocused)
                tbStatus.Text = "Set a custom status here";
        }

        private void TbStatusLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbStatus.Style = (Style) TryFindResource("LabelBoxUnSelected");
            tbStatus.Text = Program.LobbyClient.CustomStatus;
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
            SystemTrayIcon.ShowBalloonTip(5000, "Octgn",
                                          "Octgn has minimized to your system tray and is still running. Double click the icon to open it again.",
                                          ToolTipIcon.Info);
            e.Cancel = true;
        }

        private void bHideBoard_Checked(object sender, RoutedEventArgs e)
        {
            Program.GameSettings.HideBoard = true;
        }

        private void bHideBoard_Unchecked(object sender, RoutedEventArgs e)
        {
            Program.GameSettings.HideBoard = false;
        }

    }
}