// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Login.xaml.cs" company="OCTGN">
//   GNU Stuff
// </copyright>
// <summary>
//   Interaction logic for Login.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.ComponentModel;
using System.Web;
using System.Windows;
using Octgn.Data;

namespace Octgn.Launcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Navigation;

    using Octgn.Extentions;

    using Skylabs.Lobby;
    using Skylabs.Lobby.Threading;

	using System.Windows.Threading;
	using Octgn.DeckBuilder;
	using Octgn.Definitions;

    using HorizontalAlignment = System.Windows.HorizontalAlignment;
    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using Uri = System.Uri;

    /// <summary>
    ///   Interaction logic for Login.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here."),
    SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Reviewed. Suppression is OK here.")]
    public partial class LoginNew
    {
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
            Program.LobbyClient.OnStateChanged += (sender , state) => UpdateLoginStatus(state.ToString());
            Program.LobbyClient.OnLoginComplete += LobbyClientOnLoginComplete;
	        Program.LobbyClient.OnDisconnect += LobbyClientOnDisconnect;

            this.labelRegister.MouseLeftButtonUp += (sender, args) => Process.Start(Program.WebsitePath + "register.php");
            this.labelForgot.MouseLeftButtonUp +=
				(sender, args) => Process.Start(Program.WebsitePath + "passwordresetrequest.php");
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
			void LobbyClientOnDisconnect(object o, EventArgs args)
			{
				Dispatcher.BeginInvoke(new Action(() => spleft.IsEnabled = true));
			}
            void LobbyClientOnLoginComplete(object sender, LoginResults results)
            {
                switch (results)
                {
                    case LoginResults.ConnectionError:
                        UpdateLoginStatus("");
                        _isLoggingIn = false;
                        DoErrorMessage("Could not connect to the server.");
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
	            spleft.IsEnabled = false;

                _isLoggingIn = true;
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
                    new System.Threading.Timer(
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
                                                    _isLoggingIn = false;
                                                    switch (success)
                                                    {
                                                        case Skylabs.Lobby.LoginResult.Success:
                                                            Prefs.Password = cbSavePassword.IsChecked == true
                                                                                 ? passwordBox1.Password.Encrypt()
                                                                                 : "";
                                                            Prefs.Username = textBox1.Text;
                                                            Prefs.Nickname = textBox1.Text;
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
                //TODO Check for server here
            }
        #endregion            
        internal struct NewsFeedItem
        {
            public DateTime Time { get; set; }
            public string Message { get; set; }
        }
    }
}