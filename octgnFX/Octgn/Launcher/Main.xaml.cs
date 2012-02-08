﻿using System;
using System.ComponentModel;
using System.Drawing;
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
using Octgn.Properties;
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
    public partial class Main : RibbonWindow
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
            Initialized += Main_Initialized;
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
            Program.lobbyClient.OnFriendRequest += lobbyClient_OnFriendRequest;
            Program.lobbyClient.OnDisconnect += lobbyClient_OnDisconnectEvent;
            Program.lobbyClient.OnUserStatusChanged += lobbyClient_OnUserStatusChanged;
            Program.lobbyClient.Chatting.eChatEvent += Chatting_eChatEvent;
            Program.lobbyClient.OnDataRecieved += lobbyClient_OnDataRecieved;
            tbUsername.Text = Program.lobbyClient.Me.DisplayName;
            tbStatus.Text = Program.lobbyClient.Me.CustomStatus;
            _originalBorderBrush = NotificationTab.Background;
            var cm = new ContextMenu();
            cm.MenuItems.Add("Show", cmShow_Click).DefaultItem = true;
            cm.MenuItems.Add("Log Off", cmLogOff_Click);
            cm.MenuItems.Add("-");
            cm.MenuItems.Add("Quit", cmQuit_Click);
            SystemTrayIcon = new NotifyIcon();
            SystemTrayIcon.Icon = new Icon("Resources/Icon.ico");
            SystemTrayIcon.Visible = false;
            SystemTrayIcon.ContextMenu = cm;
            SystemTrayIcon.Text = Properties.Resources.Main_Main_Octgn;
            SystemTrayIcon.DoubleClick += SystemTrayIcon_DoubleClick;
            // Insert code required on object creation below this point.
        }

        public string IsHideJoinsChecked
        {
            get { return (string) GetValue(IsHideLoginNotificationsCheckedProperty); }
            set { SetValue(IsHideLoginNotificationsCheckedProperty, value); }
        }

        private void frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (Content != null && !_allowDirectNavigation)
            {
                e.Cancel = true;

                _navArgs = e;
                Dispatcher.BeginInvoke(new Action(() =>
                                                      {
                                                          var animation0 = new DoubleAnimation();
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
                                   (ThreadStart) delegate
                                                     {
                                                         frame1.UpdateLayout();
                                                         UpdateLayout();
                                                         var animation0 = new DoubleAnimation();
                                                         animation0.From = 0;
                                                         animation0.To = 1;
                                                         animation0.Duration = _duration;
                                                         frame1.BeginAnimation(OpacityProperty, animation0);
                                                     });
        }

        private void lobbyClient_OnDataRecieved(DataRecType type, object e)
        {
            if (type == DataRecType.ServerMessage)
            {
                var m = e as string;
                if (m != null && !String.IsNullOrWhiteSpace(m))
                {
                    MessageBox.Show(m, "Server Message", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void Main_Initialized(object sender, EventArgs e)
        {
            Left = double.Parse(SimpleConfig.ReadValue("MainLeftLoc", "100"));
            Top = double.Parse(SimpleConfig.ReadValue("MainTopLoc", "100"));
        }

        private void SaveLocation()
        {
            SimpleConfig.WriteValue("MainLeftLoc", Left.ToString());
            SimpleConfig.WriteValue("MainTopLoc", Top.ToString());
        }

        private void SystemTrayIcon_DoubleClick(object Sender, EventArgs e)
        {
            cmShow_Click(Sender, e);
        }

        private void Chatting_eChatEvent(ChatRoom cr, Chatting.ChatEvent e, User user, object data)
        {
            Dispatcher.Invoke(new Action(() =>
                                             {
                                                 ChatWindow cw =
                                                     Program.ChatWindows.FirstOrDefault(cww => cww.ID == cr.ID);
                                                 if (cw == null)
                                                 {
                                                     var c = new ChatWindow(cr.ID);
                                                     c.Loaded += delegate { c.ChatEvent(cr, e, user, data); };
                                                     Program.ChatWindows.Add(c);
                                                     if (cr.ID != 0)
                                                         c.Show();
                                                 }
                                                 else
                                                 {
                                                     if (cw.ID != 0)
                                                     {
                                                         cw.Show();
                                                     }
                                                 }
                                             }));
        }

        private void cmQuit_Click(object sender, EventArgs e)
        {
            CloseDownShop(true);
        }

        private void cmLogOff_Click(object sender, EventArgs e)
        {
            CloseDownShop(false);
        }

        private void cmShow_Click(object sender, EventArgs e)
        {
            Visibility = Visibility.Visible;
            SystemTrayIcon.Visible = false;
        }

        private void lobbyClient_OnFriendRequest(User u)
        {
            Dispatcher.Invoke(new Action(() =>
                                             {
                                                 if (frame1.Content as NotificationList == null)
                                                 {
                                                     NotificationTab.HeaderStyle =
                                                         Resources["AlertHeaderColor"] as Style;
                                                     NotificationTab.InvalidateVisual();
                                                 }
                                             }));
        }

        private void Ribbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tab = Ribbon.SelectedItem as RibbonTab;
            if (tab != null)
            {
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
        }

        public void LobbyTab()
        {
            frame1.Navigate(new ContactList());
        }

        public void HostJoinTab()
        {
            var hgl = new HostedGameList();
            hgl.OnGameClick += hgl_OnGameClick;
            frame1.Navigate(hgl);
        }

        private void gl_OnGameDoubleClick(object sender, EventArgs e)
        {
            var g = sender as Data.Game;
            _currentSetList = new SetList(g);
            frame1.Navigate(_currentSetList);
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            CloseDownShop(true);
            Program.Exit();
        }

        private void lobbyClient_OnDisconnectEvent(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action<bool>(CloseDownShop), false);
        }

        private void LogOff_Click(object sender, RoutedEventArgs e)
        {
            CloseDownShop(false);
        }

        private void CloseDownShop(bool Exiting)
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
            if (!Exiting)
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
            Program.lobbyClient.OnFriendRequest -= lobbyClient_OnFriendRequest;
            Program.lobbyClient.OnDisconnect -= lobbyClient_OnDisconnectEvent;
            Program.lobbyClient.OnUserStatusChanged -= lobbyClient_OnUserStatusChanged;
            Program.lobbyClient.OnDataRecieved -= lobbyClient_OnDataRecieved;
            Program.lobbyClient.Stop();
            //Program.lobbyClient.Close(DisconnectReason.CleanDisconnect);
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            var gl = frame1.Content as GameList;
            if (gl == null)
            {
                gl = new GameList(GameList.LoadEvent.InstallGame);
                gl.OnGameClick += gl_OnGameDoubleClick;
                frame1.Navigate(gl);
            }
            else
                gl.Install_Game();
        }

        private void RibbonButton_Click_1(object sender, RoutedEventArgs e)
        {
            _currentSetList = null;
            var gl = new GameList();
            gl.OnGameClick += gl_OnGameDoubleClick;
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
            var gl = new GameList();
            gl.OnGameClick += gl_HostGameClick;
            frame1.Navigate(gl);
        }

        private void gl_HostGameClick(object sender, EventArgs e)
        {
            if (Program.PlayWindow == null)
            {
                var g = sender as Data.Game;
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
            if (Program.DebugWindow.Visibility == Visibility.Visible)
                Program.DebugWindow.Visibility = Visibility.Hidden;
            else
                Program.DebugWindow.Visibility = Visibility.Visible;
        }

        private void bJoin_Click(object sender, RoutedEventArgs e)
        {
            var hgl = new HostedGameList();
            hgl.OnGameClick += hgl_OnGameClick;
            frame1.Navigate(hgl);
        }

        public void StartGame()
        {
            var sg = frame1.Content as StartGame;
            if (sg != null)
            {
                sg.Start();
                frame1.Navigate(new HostedGameList());
            }
        }

        private void hgl_OnGameClick(object sender, EventArgs e)
        {
            if (Program.PlayWindow == null)
            {
                var hg = sender as HostedGame;
                Program.IsHost = false;
                Data.Game theGame =
                    Program.GamesRepository.AllGames.FirstOrDefault(g => hg != null && g.Id == hg.GameGuid);
                if (theGame != null)
                {
                    Program.Game = new Game(GameDef.FromO8G(theGame.Filename));
#if(DEBUG)
                    var ad = new IPAddress[1];
                    IPAddress ip = IPAddress.Parse("127.0.0.1");

#else
                ad = Dns.GetHostAddresses(Program.LobbySettings.Server);
                IPAddress ip = ad[0];
#endif

                    if (ad.Length > 0)
                    {
                        try
                        {
                            if (hg != null) Program.Client = new Client(ip, hg.Port);
                            Program.Client.Connect();
                            Dispatcher.Invoke(new Action(() => frame1.Navigate(new StartGame())));
                        }
                        catch (Exception)
                        {
                        }
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

        private void lobbyClient_OnUserStatusChanged(UserStatus eve, User u)
        {
            Dispatcher.Invoke(new Action(() =>
                                             {
                                                 if (u.Equals(Program.lobbyClient.Me))
                                                 {
                                                     tbUsername.Text = Program.lobbyClient.Me.DisplayName;
                                                     tbStatus.Text = Program.lobbyClient.Me.CustomStatus;
                                                     SimpleConfig.WriteValue("Nickname", Program.lobbyClient.Me.DisplayName);
                                                 }
                                             }));
        }

        private void tbUsername_MouseUp(object sender, MouseButtonEventArgs e)
        {
            tbUsername.Style = (Style) TryFindResource(typeof (TextBox));
            tbUsername.Focus();
            tbUsername.SelectAll();
        }

        private void tbUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            tbUsername.Style = (Style) TryFindResource("LabelBoxUnSelected");
            tbUsername.Text = Program.lobbyClient.Me.DisplayName;
        }

        private void tbUsername_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbUsername.Style = (Style) TryFindResource("LabelBoxUnSelected");
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
            tbStatus.Style = (Style) TryFindResource(typeof (TextBox));
            tbStatus.Focus();
            tbStatus.SelectAll();
        }

        private void tbStatus_LostFocus(object sender, RoutedEventArgs e)
        {
            tbStatus.Style = (Style) TryFindResource("LabelBoxUnSelected");
            tbStatus.Text = Program.lobbyClient.Me.CustomStatus;
            if (String.IsNullOrWhiteSpace(tbStatus.Text) && !tbStatus.IsKeyboardFocused)
                tbStatus.Text = "Set a custom status here";
        }

        private void tbStatus_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbStatus.Style = (Style) TryFindResource("LabelBoxUnSelected");
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

        private void bHideLoginNotifications_Unchecked(object sender, RoutedEventArgs e)
        {
            Prefs.HideLoginNotifications = "false";
        }

        private void bHideLoginNotifications_Checked(object sender, RoutedEventArgs e)
        {
            Prefs.HideLoginNotifications = "true";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_isLegitClosing)
            {
                SystemTrayIcon.Visible = true;
                Visibility = Visibility.Hidden;
                SystemTrayIcon.ShowBalloonTip(5000, "OCTGN",
                                              "OCTGN has minimized to your system tray and is still running. Double click the icon to open it again.",
                                              ToolTipIcon.Info);
                e.Cancel = true;
            }
        }
    }
}