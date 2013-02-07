// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatTableRow.xaml.cs" company="OCTGN">
//   GNU Stuff
// </copyright>
// <summary>
//   Interaction logic for ChatTableRow
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Octgn.Controls
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Navigation;

    using Octgn.Library.Utils;

    using agsXMPP;

    using Skylabs.Lobby;

    /// <summary>
    /// Interaction logic for ChatTableRow
    /// </summary>
    public partial class ChatTableRow : TableRow
    {
        /// <summary>
        /// The user.
        /// </summary>
        private User user;

        /// <summary>
        /// The message.
        /// </summary>
        private string message;

        /// <summary>
        /// The message date.
        /// </summary>
        private DateTime messageDate;

        private Brush UrlBrush { get; set; }



        /// <summary>
        /// Initializes a new instance of the <see cref="ChatTableRow"/> class.
        /// </summary>
        public ChatTableRow()
        {
            this.InitializeComponent();
            this.User = new User(new Jid("NoUser", "server.octgn.info", "agsxmpp"));
            this.MessageDate = DateTime.Now;
            this.Message = "TestMessage";
            this.Unloaded += OnUnloaded;
            this.Loaded += (sender, args) =>
                {
                    Program.OnOptionsChanged += ProgramOnOnOptionsChanged;
                    this.ProgramOnOnOptionsChanged();
                };
            this.UsernameParagraph.Inlines.Add(new Run());
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Program.OnOptionsChanged -= this.ProgramOnOnOptionsChanged;
            this.Unloaded -= this.OnUnloaded;
        }

        private void ProgramOnOnOptionsChanged()
        {
            Dispatcher.Invoke(new Action(() => {
                if (Prefs.UseLightChat)
                {
                    var res = this.Resources["LightUserColor"] as SolidColorBrush;
                    UrlBrush = Brushes.Green;
                    UsernameParagraph.Foreground = res;
                }
                else
                {
                    var res = this.Resources["DarkUserColor"] as SolidColorBrush;
                    UrlBrush = Brushes.LightGreen;
                    UsernameParagraph.Foreground = res;
                }
                foreach (var hl in MessageParagraph.Inlines.OfType<Hyperlink>())
                {
                    hl.Foreground = UrlBrush;
                }
            }));
        }

        /// <summary>
        /// Gets or sets the user
        /// </summary>
        public User User
        {
            get
            {
                return this.user;
            }

            set
            {
                this.user = value;
                Dispatcher.BeginInvoke(new Action(() =>
                                                      {
                                                          (UsernameParagraph.Inlines.FirstInline as Run).Text = this.user.UserName;
                                                          UsernameColumn.Width = new GridLength(GetUsernameWidth());
                                                      }));
            }
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                this.message = value;
                Dispatcher.BeginInvoke(new Action(this.ConstructMessage));
            }
        }

        /// <summary>
        /// Gets or sets the message date.
        /// </summary>
        public DateTime MessageDate
        {
            get
            {
                return this.messageDate;
            }

            set
            {
                this.messageDate = value;
                Dispatcher.BeginInvoke(new Action(() =>
                                                      {
                                                          TimeParagraph.Inlines.Clear();
                                                          TimeParagraph.Inlines.Add(new Run(this.messageDate.ToShortTimeString()));
                                                      }));
            }
        }

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        public LobbyMessageType MessageType { get; set; }

        /// <summary>
        /// The get username width.
        /// </summary>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        private double GetUsernameWidth()
        {
            var f = new FormattedText(
                this.User.UserName,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                Brushes.Black);
            f.SetFontWeight(FontWeights.Bold);
            return f.Width + 5;
        }

        /// <summary>
        /// The construct message and takes care of hyperlinks and other visual fluff.
        /// </summary>
        private void ConstructMessage()
        {
            const string Urlsplit = "===OMGURL===";
            MessageParagraph.Inlines.Clear();
            var urledmess = Regex.Replace(this.message, RegexPatterns.Urlrx, Urlsplit + "$0" + Urlsplit);
            var urlSplits = urledmess.Split(new string[1] { Urlsplit }, StringSplitOptions.None);
            foreach (var s in urlSplits)
            {
                if (Regex.IsMatch(s, RegexPatterns.Urlrx))
                {
                    try
                    {
                        var uri = new System.Uri(s);
                        var hl = new Hyperlink(new Run(s)) { NavigateUri = uri };
                        hl.Foreground = UrlBrush;
                        hl.RequestNavigate += this.RequestNavigate;
                        MessageParagraph.Inlines.Add(hl);
                    }
                    catch (UriFormatException)
                    {
                        try
                        {
                            var uri = new System.Uri("http://" + s);
                            var hl = new Hyperlink(new Run(s)) { NavigateUri = uri };
                            hl.Foreground = UrlBrush;
                            hl.RequestNavigate += this.RequestNavigate;                        
                            MessageParagraph.Inlines.Add(hl);
                        }
                        catch (Exception)
                        {
                            MessageParagraph.Inlines.Add(s);
                        }
                    }
                }
                else
                {
                    MessageParagraph.Inlines.Add(s);
                }
            }
        }

        /// <summary>
        /// Happens on hyperlink click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The Request Navigate Event Arguments.
        /// </param>
        private void RequestNavigate(object sender, RequestNavigateEventArgs e)
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
    }
}
