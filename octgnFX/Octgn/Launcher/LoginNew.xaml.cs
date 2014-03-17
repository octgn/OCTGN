/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace Octgn.Launcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Navigation;

    using Octgn.Core;
    using Octgn.Extentions;
    using Octgn.Site.Api;

    using Skylabs.Lobby;

    using log4net;

    using HorizontalAlignment = System.Windows.HorizontalAlignment;
    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using Uri = System.Uri;
using Octgn.Controls;

    using LoginResult = Skylabs.Lobby.LoginResult;

    /// <summary>
    ///   Interaction logic for Login.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here."),
    SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Reviewed. Suppression is OK here.")]
    public partial class LoginNew
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private bool _isLoggingIn;
        private System.Threading.Timer _loginTimer;
        private bool _inLoginDone = false;
        public LoginNew()
        {
            InitializeComponent();

            string password = Prefs.Password;
            if (password != null)
            {
                passwordBox1.Password = password.Decrypt();
                cbSavePassword.IsChecked = true;
            }
            textBox1.Text = Prefs.Username;
            //TODO ToString for state may be wrong.
            Program.LobbyClient.OnLoginComplete += LobbyClientOnLoginComplete;
	        Program.LobbyClient.OnDisconnect += LobbyClientOnDisconnect;

            this.labelRegister.MouseLeftButtonUp += (sender, args) => Program.LaunchUrl(AppConfig.WebsitePath );
            this.labelForgot.MouseLeftButtonUp +=
                (sender, args) => Program.LaunchUrl(AppConfig.WebsitePath );
            this.labelSubscribe.MouseLeftButtonUp += delegate
                {
                    var url = SubscriptionModule.Get().GetSubscribeUrl(new SubType() { Description = "", Name = "" });
                    if (url != null)
                    {
                        Program.LaunchUrl(url);
                    }
                };
            Task.Factory.StartNew(GetTwitterStuff);
        }

        #region News Feed
            private void GetTwitterStuff()
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        var str = wc.DownloadString(AppConfig.WebsitePath + "news.xml");
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
			void LobbyClientOnDisconnect(object o, EventArgs args)
			{
                Log.Info("Login Window Client Disconnected");
                //Dispatcher.BeginInvoke(new Action(() => spleft.IsEnabled = true));
			}
            void LobbyClientOnLoginComplete(object sender, LoginResults results)
            {
                Log.InfoFormat("Lobby Login Complete {0}",results);
                if (_isLoggingIn)
                {
                    switch (results)
                    {
                        case LoginResults.ConnectionError:

                            _isLoggingIn = false;
                            DoErrorMessage("Could not connect to the server.");
                            break;
                        case LoginResults.Success:
                            LoginFinished(LoginResult.Success, DateTime.Now, "");
                            break;
                        case LoginResults.Failure:
                            LoginFinished(LoginResult.Failure, DateTime.Now, "Username/Password Incorrect.");
                            break;
                    }
                    _isLoggingIn = false;
                }
            }

            private void DoLogin()
            {
                if (_isLoggingIn)
                {
                    Log.Warn("Can't log in, already trying to log in");
                    return;
                }
                Log.Info("Do Login");
	            spleft.IsEnabled = false;

                _isLoggingIn = true;
                bError.Visibility = Visibility.Collapsed;
                var username = textBox1.Text;
                var password = passwordBox1.Password;
                new Action(
                    () =>
                        {

                            var client = new ApiClient();
                            var result = client.Login(username, password);

                            switch (result)
                            {
                                case Site.Api.LoginResult.UnknownError:
                                    break;
                                case Site.Api.LoginResult.Ok:
                                                this.DoLogin1(username,password);
                                    break;
                                case Site.Api.LoginResult.EmailUnverified:
                                                this.LoginFinished(LoginResult.Failure, DateTime.Now,"Your e-mail hasn't been verified. Please check your e-mail. If you haven't received one, you can contact us as support@octgn.net for help.");
                                    break;
                                case Site.Api.LoginResult.UnknownUsername:
                                                this.LoginFinished(LoginResult.Failure,DateTime.Now,"The username you entered doesn't exist.");
                                    break;
                                case Site.Api.LoginResult.PasswordWrong:
                                                this.LoginFinished(LoginResult.Failure,DateTime.Now,"The password you entered is incorrect.");
                                    break;
                                case Site.Api.LoginResult.NotSubscribed:
                                    this.LoginFinished(LoginResult.Failure, DateTime.Now,"You are required to subscribe on our site in order to play online.");
                                    break;
                                default:
                                    this.LoginFinished(LoginResult.Failure, DateTime.Now,"There was a problem. Please try again.");
                                    break;
                            }
                        }).BeginInvoke(null,null);
            }

            private void DoLogin1(string username, string password)
            {
                _loginTimer =
                    new System.Threading.Timer(
                        o =>
                        {
                            Log.Info("Login attempt timed out");
                            Program.LobbyClient.Stop();
                            LoginFinished(LoginResult.Failure , DateTime.Now ,
                                          "Please try again later.");
                        } ,
                        null , Prefs.LoginTimeout , System.Threading.Timeout.Infinite);
                Log.Info("Starting Lobby login");
                Program.LobbyClient.BeginLogin(username, password);
            }


            private void LoginFinished(LoginResult success, DateTime banEnd, string message, bool showEmail =false)
            {
                if (_inLoginDone)
                {
                    Log.Info("Already done logging in?");
                    return;
                }
                Log.Info("Login Finished");
                _inLoginDone = true;
                if (_loginTimer != null)
                {
                    _loginTimer.Dispose();
                    _loginTimer = null;
                }
                Dispatcher.Invoke((Action) (() =>
                                                {
                                                    Log.InfoFormat("Updating UI for Log result {0}",success);
                                                    _isLoggingIn = false;
                                                    switch (success)
                                                    {
                                                        case Skylabs.Lobby.LoginResult.Success:
                                                            Prefs.Username = textBox1.Text;
                                                            Prefs.Nickname = textBox1.Text;
                                                            Prefs.Password = cbSavePassword.IsChecked == true
                                                                                 ? passwordBox1.Password.Encrypt()
                                                                                 : "";
                                                            break;
                                                        case Skylabs.Lobby.LoginResult.Banned:
		                                                    spleft.IsEnabled = true;
                                                            DoErrorMessage("You have been banned until " +
                                                                           banEnd.ToShortTimeString() + " on " +
                                                                           banEnd.ToShortDateString());
                                                            break;
                                                        case Skylabs.Lobby.LoginResult.Failure:
															spleft.IsEnabled = true;
                                                            DoErrorMessage("Login Failed: " + message);
                                                            break;
                                                    }
                                                    if (showEmail)
                                                    {
                                                        Log.Info("No email found, must enter e-mail");
                                                        textBoxEmail.Text = "";
                                                        textBoxEmail.Visibility = Visibility.Visible;
                                                        labelEmail.Visibility = Visibility.Visible;
                                                    }
                                                    else
                                                    {
                                                        Log.Info("Email stored");
                                                        textBoxEmail.Visibility = Visibility.Collapsed;
                                                        labelEmail.Visibility = Visibility.Collapsed;
                                                    }
                                                    Log.Info("Full Login done");
                                                    _inLoginDone = false;
                                                }), new object[] {});
            }

            private void DoErrorMessage(string message)
            {
                TopMostMessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        #endregion

        #region UI Events
            private void Button1Click(object sender, RoutedEventArgs e) { DoLogin(); }
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
                //TODO [NEW UI] Check for server here
            }
        #endregion            
        internal struct NewsFeedItem
        {
            public DateTime Time { get; set; }
            public string Message { get; set; }
        }

        private void ServerStatusLinkClick(object sender, MouseButtonEventArgs e)
        {
            Program.LaunchUrl("http://stats.pingdom.com/qrzmgw5y7owr");
        }

        private void TwitterLinkClick(object sender, MouseButtonEventArgs e)
        {
            Program.LaunchUrl("https://twitter.com/octgn_official");
        }
    }
}
