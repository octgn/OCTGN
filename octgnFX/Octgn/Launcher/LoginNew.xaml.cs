// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Login.xaml.cs" company="OCTGN">
//   GNU Stuff
// </copyright>
// <summary>
//   Interaction logic for Login.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Octgn.Launcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Navigation;

    using agsXMPP;

    using Octgn.Extentions;

    using Skylabs.Lobby;
    using Skylabs.Lobby.Threading;

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
        /// <summary>
        /// Is logging in.
        /// </summary>
        private bool isLoggingIn;

        /// <summary>
        /// Is login done.
        /// </summary>
        private bool inLoginDone;

        /// <summary>
        /// Initializes a new instance of the <see cref="Login"/> class.
        /// </summary>
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
            Program.LobbyClient.OnLoginComplete += this.LobbyClientOnLoginComplete;
            Program.LobbyClient.OnStateChanged += this.LobbyClient_OnStateChanged;
            LazyAsync.Invoke(this.GetTwitterStuff);
        }

        #region News Feed
        /// <summary>
        /// Get twitter feed stuff.
        /// </summary>
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
                        if (dt == null) continue;
                        DateTime dto;
                        if (!DateTime.TryParse(dt.Value, out dto))
                            continue;
                        nf.Time = dto;
                        feeditems.Add(nf);
                    }

                    Dispatcher.BeginInvoke(
                        new Action(
                            () => this.ShowTwitterStuff(feeditems.OrderByDescending(x => x.Time).Take(5).ToList())));
                }
            }
            catch (Exception)
            {
                Dispatcher.Invoke(new Action(() => textBlock5.Text = "Could not retrieve news feed."));
            }
        }

        /// <summary>
        /// Show twitter stuff. Must be on the UI Thread.
        /// </summary>
        /// <param name="tweets">
        /// The tweets.
        /// </param>
        private void ShowTwitterStuff(List<NewsFeedItem> tweets)
        {
            textBlock5.HorizontalAlignment = HorizontalAlignment.Stretch;
            textBlock5.Inlines.Clear();
            textBlock5.Text = "";
            foreach (var tweet in tweets)
            {
                Inline dtime =
                    new Run(tweet.Time.ToShortDateString() + "  "
                            + tweet.Time.ToShortTimeString());
                dtime.Foreground =
                    new SolidColorBrush(Colors.Khaki);
                textBlock5.Inlines.Add(dtime);
                textBlock5.Inlines.Add("\n");
                var inlines = AddTweetText(tweet.Message).Inlines.ToArray();
                foreach (var i in inlines)
                    textBlock5.Inlines.Add(i);
                textBlock5.Inlines.Add("\n\n");
            }
        }

        /// <summary>
        /// Converts a tweet's text into a paragraph.
        /// </summary>
        /// <param name="text">
        /// The tweet text.
        /// </param>
        /// <returns>
        /// The <see cref="Paragraph"/>.
        /// </returns>
        private Paragraph AddTweetText(string text)
        {
            var ret = new Paragraph();
            var words = text.Split(' ');
            var b = new SolidColorBrush(Colors.White);
            foreach (var inn in words.Select(word => this.StringToRun(word, b)))
            {
                if (inn != null)
                {
                    ret.Inlines.Add(inn);
                }

                ret.Inlines.Add(" ");
            }

            return ret;
        }

        /// <summary>
        /// Converts a String to a Run
        /// </summary>
        /// <param name="s">
        /// The string
        /// </param>
        /// <param name="b">
        /// The default brush.
        /// </param>
        /// <returns>
        /// The <see cref="Inline"/>.
        /// </returns>
        public Inline StringToRun(string s, Brush b)
        {
            Inline ret = null;
            const string StrUrlRegex =
                "(?i)\\b((?:[a-z][\\w-]+:(?:/{1,3}|[a-z0-9%])|www\\d{0,3}[.]|[a-z0-9.\\-]+[.][a-z]{2,4}/)(?:[^\\s()<>]+|\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\))+(?:\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\)|[^\\s`!()\\[\\]{};:'\".,<>?«»“”‘’]))";
            var reg = new Regex(StrUrlRegex);
            s = s.Trim();
            var r = new Run(s);
            if (reg.IsMatch(s))
            {
                b = Brushes.LightBlue;
                var h = new Hyperlink(r);
                h.Foreground = new SolidColorBrush(Colors.LawnGreen);
                h.RequestNavigate += this.HOnRequestNavigate;
                try
                {
                    h.NavigateUri = new Uri(s);
                }
                catch (UriFormatException)
                {
                    s = "http://" + s;
                    try
                    {
                        h.NavigateUri = new Uri(s);
                    }
                    catch (Exception)
                    {
                        r.Foreground = b;
                    }
                }

                ret = h;
            }
            else
            {
                ret = new Run(s) { Foreground = b };
            }
            return ret;
        }

        /// <summary>
        /// Fires when a hyperlink is clicked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The arguments.
        /// </param>
        private void HOnRequestNavigate(object sender , RequestNavigateEventArgs e) 
        {
            var hl = (Hyperlink)sender;
            var navigateUri = hl.NavigateUri.ToString();
            try
            {
                Process.Start(new ProcessStartInfo(navigateUri));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            e.Handled = true;
        }
        #endregion

        #region LoginStuff

        /// <summary>
        /// lobby client on state changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="state">
        /// The state.
        /// </param>
        private void LobbyClient_OnStateChanged(object sender, agsXMPP.XmppConnectionState state)
        {
            Dispatcher.BeginInvoke(new Action(() =>
                                                  {
                                                      if (state == XmppConnectionState.Disconnected)
                                                      {
                                                          spLogin.Visibility = Visibility.Visible;
                                                          spLogin.IsEnabled = true;
                                                          lineSplit.Visibility = Visibility.Visible;
                                                      }
                                                  }));
        }

        /// <summary>
        /// Fires when the lobby login request is complete.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="results">
        /// The results.
        /// </param>
        private void LobbyClientOnLoginComplete(object sender, LoginResults results)
        {
            var mess = string.Empty;
            switch (results)
            {
                case LoginResults.ConnectionError:
                    mess = "Could not connect to the server.";
                    break;
                case LoginResults.Success:
                    mess = string.Empty;
                    break;
                case LoginResults.Failure:
                    mess = "Unknown failure.";
                    break;
                case LoginResults.FirewallError:
                    mess = "This program is being blocked by a firewall on your pc.";
                    break;
                case LoginResults.AuthError:
                    mess = "Your username or password was incorrect.";
                    break;
            }

            this.isLoggingIn = false;
            this.LoginFinished(results, mess);
        }

        /// <summary>
        /// Try to log in.
        /// </summary>
        private void DoLogin()
        {
            if (this.isLoggingIn)
            {
                return;
            }

            spLogin.IsEnabled = false;
            this.isLoggingIn = true;
            lError.Visibility = Visibility.Hidden;
            Program.LobbyClient.BeginLogin(textBox1.Text, passwordBox1.Password);
        }

        /// <summary>
        /// The login finished.
        /// </summary>
        /// <param name="success">
        /// The login results
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        private void LoginFinished(LoginResults success, string message)
        {
            if (this.inLoginDone)
            {
                return;
            }

            this.inLoginDone = true;
            Trace.TraceInformation("Login finished.");
            Dispatcher.BeginInvoke(
                (Action)(() =>
                    {
                        spLogin.IsEnabled = true;
                                                this.isLoggingIn = false;
                                                switch (success)
                                                {
                                                    case LoginResults.Success:
                                                        Prefs.Password = cbSavePassword.IsChecked == true
                                                                             ? passwordBox1.Password.Encrypt()
                                                                             : string.Empty;
                                                        Prefs.Username = textBox1.Text;
                                                        Prefs.Nickname = textBox1.Text;
                                                        spLogin.Visibility = Visibility.Collapsed;
                                                        lineSplit.Visibility = Visibility.Collapsed;
                                                        break;
                                                    default:
                                                        DoErrorMessage(message);
                                                        Program.LobbyClient.Stop();
                                                        spLogin.Visibility = Visibility.Visible;
                                                        break;
                                                }

                        this.inLoginDone = false;
                    }),
                new object[] { });
        }

        /// <summary>
        /// Display an error message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private void DoErrorMessage(string message)
        {
            Dispatcher.Invoke(
                (Action)(() =>
                    {
                        lError.Text = message;
                        lError.Visibility = Visibility.Visible;
                    }),
                new object[] { });
        }

        #endregion

        #region Offline Gaming
        #endregion

        #region UI Events
        /// <summary>
        /// Login button clicked.
        /// </summary>
        /// <param name="sender">The Sender</param>
        /// <param name="e">The Arguments</param>
        private void Button1Click(object sender, RoutedEventArgs e)
        {
            DoLogin();
        }

        /// <summary>
        /// Username textbox text changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TextBox1TextChanged(object sender, TextChangedEventArgs e)
        {
            lError.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// The password box password changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void PasswordBox1PasswordChanged(object sender, RoutedEventArgs e)
        {
            lError.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// The register button clicked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnRegisterClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.Navigate(new Register());
            }
        }

        /// <summary>
        /// Username box key up.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TextBox1KeyUp(object sender, KeyEventArgs e)
        {
            cbSavePassword.IsChecked = false;
        }

        /// <summary>
        /// Password box key up.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void PasswordBox1KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.DoLogin();
            }
            else if (cbSavePassword.IsChecked == true)
            {
                cbSavePassword.IsChecked = false;
            }
        }
        #endregion

        internal struct NewsFeedItem
        {
            public DateTime Time { get; set; }
            public string Message { get; set; }
        }
    }
}