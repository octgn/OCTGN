/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using log4net;
using Octgn.Extentions;

namespace Octgn.Tabs.Login
{
    /// <summary>
    ///   Interaction logic for Login.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here."),
    SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Reviewed. Suppression is OK here.")]
    public partial class LoginTab : UserControl
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public LoginTabViewModel LoginVM { get; set; }

        public LoginTab()
        {
            if (!this.IsInDesignMode())
            {
                LoginVM = new LoginTabViewModel();
            }
            InitializeComponent();

            if (this.IsInDesignMode()) return;

            this.labelRegister.MouseLeftButtonUp += (sender, args) => Program.LaunchUrl(AppConfig.WebsitePath);
            this.labelForgot.MouseLeftButtonUp +=
                (sender, args) => Program.LaunchUrl(AppConfig.WebsitePath);
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
                    var builder = new UriBuilder(AppConfig.StaticWebsitePath);
                    builder.Path = "news.xml";

                    var downloadUrl = builder.ToStringValue();
                    var newsXml = wc.DownloadString(downloadUrl);
                    if (string.IsNullOrWhiteSpace(newsXml))
                    {
                        throw new Exception("Null news feed.");
                    }

                    var doc = System.Xml.Linq.XDocument.Parse(newsXml);
                    var nitems = doc.Root.Elements("item");
                    var feeditems = new List<NewsFeedItem>();
                    foreach (var f in nitems)
                    {
                        var nf = new NewsFeedItem { Message = (string)f };
                        var dt = f.Attribute("date");
                        if (dt == null) continue;
                        DateTimeOffset dto;
                        if (!DateTimeOffset.TryParse(dt.Value, out dto))
                            continue;
                        nf.Time = dto.LocalDateTime;
                        feeditems.Add(nf);
                    }

                    Dispatcher.BeginInvoke(
                        new Action(
                            () => this.ShowTwitterStuff(feeditems.OrderByDescending(x => x.Time).Take(5).ToList())));
                }
            }
            catch (Exception e)
            {
                Log.Warn("GetTwitterStuff", e);
                Dispatcher.Invoke(new Action(() => textBlock5.Text = "Could not retrieve news feed."));
            }
        }
        private void ShowTwitterStuff(List<NewsFeedItem> tweets)
        {
            textBlock5.HorizontalAlignment = HorizontalAlignment.Stretch;
            textBlock5.VerticalAlignment = VerticalAlignment.Top;
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
            //Dispatcher.BeginInvoke(new Action(StartTwitterAnim) , DispatcherPriority.Background);
        }
        private Paragraph AddTweetText(string text)
        {
            var ret = new Paragraph();
            var words = text.Split(' ');
            var b = new SolidColorBrush(Colors.White);
            foreach (var inn in words.Select(word => StringToRun(word, b)))
            {
                if (inn != null)
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
            if (reg.IsMatch(s))
            {
                b = Brushes.LightBlue;
                var h = new Hyperlink(r);
                h.Foreground = new SolidColorBrush(Colors.LawnGreen);
                h.RequestNavigate += HOnRequestNavigate;
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
                        //var ul = new Underline(r);
                    }
                }
                ret = h;
            }
            else
                ret = new Run(s) { Foreground = b };
            return ret;
        }

        private void HOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {

            var hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            try
            {
                Process.Start(new ProcessStartInfo(navigateUri));
            }
            catch (Exception ex)
            {
                Log.Warn("HOnRequestNavigate", ex);
            }
            e.Handled = true;
        }
        #endregion

        #region UI Events
        private void Button1Click(object sender, RoutedEventArgs e) {
            LoginVM.LoginAsync();
        }
        private void PasswordBox1KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginVM.LoginAsync();
            }
        }
        #endregion

        internal struct NewsFeedItem
        {
            public DateTime Time { get; set; }
            public string Message { get; set; }
        }

        private void TwitterLinkClick(object sender, MouseButtonEventArgs e)
        {
            Program.LaunchUrl("https://twitter.com/octgn_official");
        }

        private void DiscordLinkClick(object sender, MouseButtonEventArgs e)
        {
            Program.LaunchUrl("https://discord.gg/Yn3Jrpj");
        }
    }
}
