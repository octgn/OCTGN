using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

using Octgn.DeckBuilder;
using Octgn.Extentions;

using Skylabs.Lobby;
using Octgn.Definitions;
using Skylabs.Lobby.Threading;
using Client = Octgn.Networking.Client;
using Octgn.Data;
using Application = System.Windows.Application;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Threading.Timer;

namespace Octgn.Launcher
{
    using System.Web;
    using System.Xml;

    using Client = Skylabs.Lobby.Client;
    using Game = Octgn.Data.Game;

    /// <summary>
    ///   Interaction logic for Login.xaml
    /// </summary>
    public partial class Login
    {
        private readonly DispatcherTimer _animationTimer;
        private bool _bSpin;
        private bool _isLoggingIn;
        private Timer _loginTimer;
        private bool _inLoginDone = false;
        public Login()
        {
            InitializeComponent();

            SpinnerRotate.CenterX = image2.Width/2;
            SpinnerRotate.CenterY = image2.Height/2;
            _animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher)
                                  {Interval = new TimeSpan(0, 0, 0, 0, 100)};
            versionText.Text = string.Format("Version {0}", OctgnApp.OctgnVersion.ToString(4));
            _animationTimer.Tick += HandleAnimationTick;
            string password = Prefs.Password;
            if (password != null)
            {
                passwordBox1.Password = password.Decrypt();
                cbSavePassword.IsChecked = true;
            }
            textBox1.Text = Prefs.Username;
            //TODO ToString for state may be wrong.
            Program.LobbyClient.OnStateChanged += (sender , state) => UpdateLoginStatus(state.ToString());
            Program.LobbyClient.OnLoginComplete += LobbyClientOnLoginComplete;

            this.labelRegister.MouseLeftButtonUp += (sender, args) => Process.Start(Program.WebsitePath + "register.php");
            this.labelForgot.MouseLeftButtonUp +=
                (sender, args) => Process.Start(Program.WebsitePath + "passwordreset.php");
            this.labelResend.MouseLeftButtonUp += (sender, args) =>
                {
                    var url = Program.WebsitePath + "api/user/resendemailverify.php?username="
                              + HttpUtility.UrlEncode(textBox1.Text);
                    using (var wc = new WebClient())
                    {
                        try
                        {
                            wc.DownloadStringAsync(new Uri(url));
                        }
                        catch (Exception)
                        {
                        }
                    }
                };

            LazyAsync.Invoke(GetTwitterStuff);
        }

        #region News Feed
            private void GetTwitterStuff()
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        var str = wc.DownloadString(Program.WebsitePath + "news.xml");
                        if (string.IsNullOrWhiteSpace(str))
                        {
                            throw new Exception("Null news feed.");
                        }

                        var doc = System.Xml.Linq.XDocument.Parse(str);
                        var nitems = doc.Root.Elements("item");
                        var feeditems = new List<NewsFeedItem>();
                        foreach (var f in nitems)
                        {
                            var nf = new NewsFeedItem { Message = (string)f };
                            var dt = f.Attribute("date");
                            if(dt == null) continue;
                            DateTime dto;
                            if(!DateTime.TryParse(dt.Value,out dto)) 
                                continue;
                            nf.Time = dto;
                            feeditems.Add(nf);
                        }

                        Dispatcher.BeginInvoke(
                            new Action(
                                () => this.ShowTwitterStuff(feeditems.OrderByDescending(x => x.Time).Take(5).ToList())));
                    }
                }
                catch(Exception)
                {
                    Dispatcher.Invoke(new Action(() => textBlock5.Text = "Could not retrieve news feed."));
                }
            }
            private void ShowTwitterStuff(List<NewsFeedItem> tweets )
            {
                textBlock5.HorizontalAlignment = HorizontalAlignment.Stretch;
                textBlock5.Inlines.Clear();
                textBlock5.Text = "";
                foreach( var tweet in tweets)
                {
                    Inline dtime =
                        new Run(tweet.Time.ToShortDateString() + "  "
                                + tweet.Time.ToShortTimeString());
                    dtime.Foreground =
                        new SolidColorBrush(Colors.Khaki);
                    textBlock5.Inlines.Add(dtime);
                    textBlock5.Inlines.Add("\n");
                    var inlines = AddTweetText(tweet.Message).Inlines.ToArray();
                    foreach(var i in inlines)
                        textBlock5.Inlines.Add(i);     
                    textBlock5.Inlines.Add("\n\n");
                }
                //Dispatcher.BeginInvoke(new Action(StartTwitterAnim) , DispatcherPriority.Background);
            }
            private Paragraph AddTweetText(string text)
            {
                var ret = new Paragraph();
                var words = text.Split(' ');
                var b = new SolidColorBrush(Colors.White);
                foreach(var inn in words.Select(word=>StringToRun(word,b)))
                {
                    if(inn != null)
                        ret.Inlines.Add(inn);
                    ret.Inlines.Add(" ");
                }
                return ret;
            }
            public Inline StringToRun(string s, Brush b)
            {
                Inline ret = null;
                const string strUrlRegex =
                    "(?i)\\b((?:[a-z][\\w-]+:(?:/{1,3}|[a-z0-9%])|www\\d{0,3}[.]|[a-z0-9.\\-]+[.][a-z]{2,4}/)(?:[^\\s()<>]+|\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\))+(?:\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\)|[^\\s`!()\\[\\]{};:'\".,<>?«»“”‘’]))";
                var reg = new Regex(strUrlRegex);
                s = s.Trim();
                //b = Brushes.Black;
                Inline r = new Run(s);
                if(reg.IsMatch(s))
                {
                    b = Brushes.LightBlue;
                    var h = new Hyperlink(r);
                    h.Foreground = new SolidColorBrush(Colors.LawnGreen);
                    h.RequestNavigate += HOnRequestNavigate;
                    try
                    {
                        h.NavigateUri = new Uri(s);
                    }
                    catch(UriFormatException)
                    {
                        s = "http://" + s;
                        try
                        {
                            h.NavigateUri = new Uri(s);
                        }
                        catch(Exception)
                        {
                            r.Foreground = b;
                            //var ul = new Underline(r);
                        }
                    }
                    ret = h;
                }
                else
                    ret = new Run(s){Foreground = b};
                return ret;
            }

            private void HOnRequestNavigate(object sender , RequestNavigateEventArgs e) 
            {
 
                var hl = (Hyperlink) sender;
                string navigateUri = hl.NavigateUri.ToString();
                try
                {
                    Process.Start(new ProcessStartInfo(navigateUri));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    if (Debugger.IsAttached) Debugger.Break();
                }
                e.Handled = true;
            }
        #endregion

        #region LoginStuff
            private void StartSpinning()
            {
                if (_bSpin || _animationTimer.IsEnabled) return;
                _bSpin = true;
                _animationTimer.Start();
            }

            private void StopSpinning()
            {
                _bSpin = false;
            }

            private void HandleAnimationTick(object sender, EventArgs e)
            {
                SpinnerRotate.Angle = (SpinnerRotate.Angle + 10)%360;
                if (Math.Abs(SpinnerRotate.Angle - 0) < double.Epsilon && _bSpin == false)
                    _animationTimer.Stop();
            }

            void LobbyClientOnLoginComplete(object sender, LoginResults results)
            {
                
                switch (results)
                {
                    case LoginResults.ConnectionError:
                        UpdateLoginStatus("");
                        _isLoggingIn = false;
                        DoErrorMessage("Could not connect to the server.");
                        StopSpinning();    

                        break;
                    case LoginResults.Success:
                        LoginFinished(LoginResult.Success, DateTime.Now,"");
                        break;
                    case LoginResults.Failure:
                        LoginFinished(LoginResult.Failure, DateTime.Now,"Username/Password Incorrect.");
                        break;
                }
                _isLoggingIn = false;
            }

            private void DoLogin()
            {
                if (_isLoggingIn) return;
                _isLoggingIn = true;
                this.StartSpinning();
                bError.Visibility = Visibility.Collapsed;
                var username = textBox1.Text;
                var password = passwordBox1.Password;
                var email = textBoxEmail.Visibility == Visibility.Visible ? textBoxEmail.Text : null;
                new Action(
                    () =>
                        {
                            using (var wc = new WebClient())
                            {
                                try
                                {
                                    var ustring = Program.WebsitePath + "api/user/login.php?username=" + HttpUtility.UrlEncode(username)
                                                  + "&password=" + HttpUtility.UrlEncode(password);
                                    if (email != null) ustring += "&email=" + HttpUtility.UrlEncode(email);
                                    var res = wc.DownloadString(new Uri(ustring));
                                    res = res.Trim();
                                    switch (res)
                                    {
                                        case "ok":
                                            {
                                                this.DoLogin1(username,password);
                                                break;
                                            }
                                        case "EmailUnverifiedException":
                                            {
                                                //TODO Needs a way to resend e-mail and stuff
                                                this.LoginFinished(LoginResult.Failure, DateTime.Now,"Your e-mail hasn't been verified. Please check your e-mail.");
                                                break;
                                            }
                                        case "UnknownUsernameException":
                                            {
                                                this.LoginFinished(LoginResult.Failure,DateTime.Now,"The username you entered doesn't exist.");
                                                break;
                                            }
                                        case "PasswordDifferentException":
                                            {
                                                this.LoginFinished(LoginResult.Failure,DateTime.Now,"The password you entered is incorrect.");
                                                break;
                                            }
                                        case "NoEmailException":
                                            {
                                                this.LoginFinished(LoginResult.Failure,DateTime.Now,"Your account does not have an e-mail associated with it. Please enter one above.",true);
                                                break;
                                            }
                                        default:
                                            {
                                                throw new Exception();
                                            }
                                    }
                                }
                                catch (Exception)
                                {
                                    this.LoginFinished(LoginResult.Failure, DateTime.Now,"Please try again later.");
                                }

                            }
                        }).BeginInvoke(null,null);
            }

            private void DoLogin1(string username, string password)
            {
                _loginTimer =
                    new Timer(
                        o =>
                        {
                            Program.LobbyClient.Stop();
                            LoginFinished(LoginResult.Failure , DateTime.Now ,
                                          "Please try again later.");
                        } ,
                        null , Prefs.LoginTimeout , System.Threading.Timeout.Infinite);
                Program.LobbyClient.BeginLogin(username, password);
            }


            private void UpdateLoginStatus(string message)
            {
                Dispatcher.Invoke(new Action(() => lblLoginStatus.Content = message));
            }

            private void LoginFinished(LoginResult success, DateTime banEnd, string message, bool showEmail =false)
            {
                if (_inLoginDone) return;
                _inLoginDone = true;
                Trace.TraceInformation("Login finished.");
                if (_loginTimer != null)
                {
                    _loginTimer.Dispose();
                    _loginTimer = null;
                }
                Dispatcher.Invoke((Action) (() =>
                                                {
                                                    Program.LauncherWindow.Closing -= LauncherWindowClosing;
                                                    _isLoggingIn = false;
                                                    StopSpinning();
                                                    switch (success)
                                                    {
                                                        case Skylabs.Lobby.LoginResult.Success:
                                                            Prefs.Password = cbSavePassword.IsChecked == true
                                                                                 ? passwordBox1.Password.Encrypt()
                                                                                 : "";
                                                            Prefs.Username = textBox1.Text;
                                                            Prefs.Nickname = textBox1.Text;
                                                            Program.MainWindow = new Windows.Main();
                                                            Program.MainWindow.Show();
                                                            Application.Current.MainWindow = Program.MainWindow;
                                                            Program.LauncherWindow.Close();
                                                            break;
                                                        case Skylabs.Lobby.LoginResult.Banned:
                                                            DoErrorMessage("You have been banned until " +
                                                                           banEnd.ToShortTimeString() + " on " +
                                                                           banEnd.ToShortDateString());
                                                            break;
                                                        case Skylabs.Lobby.LoginResult.Failure:
                                                            DoErrorMessage("Login Failed: " + message);
                                                            break;
                                                    }
                                                    if (showEmail)
                                                    {
                                                        textBoxEmail.Text = "";
                                                        textBoxEmail.Visibility = Visibility.Visible;
                                                        labelEmail.Visibility = Visibility.Visible;
                                                    }
                                                    else
                                                    {
                                                        textBoxEmail.Visibility = Visibility.Collapsed;
                                                        labelEmail.Visibility = Visibility.Collapsed;
                                                    }
                                                    _inLoginDone = false;
                                                }), new object[] {});
            }

            private void DoErrorMessage(string message)
            {
                Dispatcher.Invoke((Action) (() =>
                                                {
                                                    lError.Text = message;
                                                    bError.Visibility = Visibility.Visible;
                                                }), new object[] {});
            }
        #endregion

        #region Offline Gaming
            private void MenuOfflineClick(object sender, RoutedEventArgs e)
            {
                var g = new GameList();
                var sg = new StartGame();
                g.Row2.Height = new GridLength(25);
                g.btnCancel.Click += delegate(object o, RoutedEventArgs args)
                                         {
                                             Program.LauncherWindow.NavigationService.GoBack();
                                         };
                g.OnGameClick += GOnOnGameClick;
                Program.LauncherWindow.Navigate(g);
            }

            private void GOnOnGameClick(object sender, EventArgs eventArgs)
            {
                var hg = sender as Octgn.Data.Game;
                if (hg == null || Program.PlayWindow != null)
                {
                    Program.LauncherWindow.Navigate(new Login());
                    return;
                }
                var hostport = 5000;
                while (!Skylabs.Lobby.Networking.IsPortAvailable(hostport))
                    hostport++;
                var hs = new HostedGame(hostport, hg.Id, hg.Version, "LocalGame", "", null,true);
                hs.HostedGameDone += hs_HostedGameDone;
                if (!hs.StartProcess())
                {
                    hs.HostedGameDone -= hs_HostedGameDone;
                    Program.LauncherWindow.Navigate(new Login());
                    return;
                }

                Program.IsHost = true;
                Game theGame =
                    Program.GamesRepository.Games.FirstOrDefault(g => g.Id == hg.Id);
                if (theGame == null) return;
                Program.Game = new Octgn.Game(GameDef.FromO8G(theGame.FullPath),true);

                var ad = new IPAddress[1];
                IPAddress ip = IPAddress.Parse("127.0.0.1");

                if (ad.Length <= 0) return;
                try
                {
                    Program.Client = new Networking.Client(ip, hostport);
                    Program.Client.Connect();
                    Dispatcher.Invoke(new Action(() => Program.LauncherWindow.NavigationService.Navigate(new StartGame(true){Width = 400})));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    if (Debugger.IsAttached) Debugger.Break();
                }
            }
            
            void hs_HostedGameDone(object sender, EventArgs e)
            {
                //throw new NotImplementedException();
            }
            private void GOoffConnOnGameClick(object sender, EventArgs eventArgs)
            {
                var hg = sender as Octgn.Data.Game;
                if (hg == null || Program.PlayWindow != null)
                {
                    Program.LauncherWindow.NavigationService.Navigate(new Login());
                    return;
                }
                Program.IsHost = false;
                Game theGame =
                    Program.GamesRepository.Games.FirstOrDefault(g => g.Id == hg.Id);
                if (theGame == null)
                {
                    Program.LauncherWindow.Navigate(new Login());
                    return;
                }
                Program.Game = new Octgn.Game(GameDef.FromO8G(theGame.FullPath),true);
                Program.LauncherWindow.Navigate(new ConnectLocalGame());
            }

            private void MenuOfflineConnectClick(object sender, RoutedEventArgs e)
            {
                var g = new GameList();
                g.Row2.Height = new GridLength(25);
                g.btnCancel.Click += delegate(object o, RoutedEventArgs args)
                {
                    Program.LauncherWindow.NavigationService.GoBack();
                };
                g.OnGameClick += GOoffConnOnGameClick;
                Program.LauncherWindow.Navigate(g);
                
            }
        #endregion

        #region UI Events
            private void Button1Click(object sender, RoutedEventArgs e) { DoLogin(); }
            private void MenuDeckEditorClick(object sender, RoutedEventArgs e)
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
            private void menuAboutUs_Click(object sender, RoutedEventArgs e)
            {
                new Windows.AboutWindow().ShowDialog();
            }
            private void menuHelp_Click(object sender, RoutedEventArgs e)
            {
                Process.Start("https://github.com/kellyelton/OCTGN/wiki");
            }
            private void menuBug_Click(object sender, RoutedEventArgs e)
            {
                Process.Start("https://github.com/kellyelton/OCTGN/issues");
            }
            private void TextBox1TextChanged(object sender, TextChangedEventArgs e){bError.Visibility = Visibility.Hidden;}
            private void PasswordBox1PasswordChanged(object sender, RoutedEventArgs e){bError.Visibility = Visibility.Hidden;}

            private void TextBox1KeyUp(object sender, KeyEventArgs e)
            {
                cbSavePassword.IsChecked = false;
            }
            private void PasswordBox1KeyUp(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                {
                    DoLogin();
                }
                else if (cbSavePassword.IsChecked == true)
                {
                    cbSavePassword.IsChecked = false;
                }
            }
        #endregion

        #region Window stuff
            private void PageUnloaded(object sender, RoutedEventArgs e)
            {
                Program.LobbyClient.OnLoginComplete -= LobbyClientOnLoginComplete;
            }

            private void PageLoaded(object sender, RoutedEventArgs e)
            {
                //TODO Check for server here
                menuInstallOnBoot.IsChecked = Prefs.InstallOnBoot;
            }
            private void LauncherWindowClosing(object sender, CancelEventArgs e){if (_isLoggingIn)e.Cancel = true;}
            private void MenuExitClick(object sender, RoutedEventArgs e){if (!_isLoggingIn)Program.Exit();}
        #endregion            

            private void menuCD_Click(object sender, RoutedEventArgs e)
            {
                FolderBrowserDialog pf = new FolderBrowserDialog();
                pf.SelectedPath = GamesRepository.BasePath;
                var dr = pf.ShowDialog();
                if(dr == DialogResult.OK)
                {
                    if(pf.SelectedPath.ToLower() != GamesRepository.BasePath.ToLower())
                    {
                        Prefs.DataDirectory = pf.SelectedPath;
                        var asm = System.Reflection.Assembly.GetExecutingAssembly();
                        var thispath = asm.Location;
                        Program.Exit();
                        Process.Start(thispath);
                        /*Application.Current.Exit += delegate(object o , ExitEventArgs args)
                                                    {
                                                        Process.Start(thispath);

                                                    };*/
                    }
                }
            }

            private void menuInstallOnBoot_Checked(object sender, RoutedEventArgs e) { Prefs.InstallOnBoot = menuInstallOnBoot.IsChecked; }

            private void menuInstallOnBoot_Unchecked(object sender, RoutedEventArgs e)
            {
                Prefs.InstallOnBoot = menuInstallOnBoot.IsChecked;
            }
        internal struct NewsFeedItem
        {
            public DateTime Time { get; set; }
            public string Message { get; set; }
        }
    }

}