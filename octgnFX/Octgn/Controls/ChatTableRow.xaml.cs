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
    using System.Net;
    using System.Net.Cache;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;

    using Octgn.Library.Utils;

    using WpfAnimatedGif;

    using Xceed.Wpf.DataGrid.Utils;

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

        private bool? useLightChat;

        private bool enableGifs;

        private bool enableImages;

        public event MouseEventHandler OnMouseUsernameEnter;
        public event MouseEventHandler OnMouseUsernameLeave;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatTableRow"/> class.
        /// </summary>
        public ChatTableRow()
            : this(new User(new Jid("NoUser", "server.octgn.info", "agsxmpp")), "TestMessage", DateTime.Now, LobbyMessageType.Standard)
        {

        }

        public ChatTableRow(User user, string message, DateTime messageDate, LobbyMessageType messageType)
        {
            this.InitializeComponent();
            this.User = user;
            this.Message = message;
            this.MessageDate = messageDate;
            this.MessageType = messageType;
            this.Unloaded += OnUnloaded;
            this.Loaded += OnLoaded;
            this.UsernameParagraph.Inlines.Add(new Run());
            enableGifs = Prefs.EnableChatGifs;
            enableImages = Prefs.EnableChatImages;
        }

        private void UsernameParagraphOnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            if (OnMouseUsernameLeave != null) OnMouseUsernameLeave(this, mouseEventArgs);
        }

        private void UsernameParagraphOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            if (OnMouseUsernameEnter != null) OnMouseUsernameEnter(this, mouseEventArgs);
        }

        private void OnLoaded(object sender, EventArgs eventArgs)
        {
            //this.Loaded -= this.OnLoaded;
            //this.User = user;
            //this.Message = message;
            //this.MessageDate = messageDate;
            this.UsernameParagraph.MouseEnter += UsernameParagraphOnMouseEnter;
            this.UsernameParagraph.MouseLeave += UsernameParagraphOnMouseLeave;
            Program.OnOptionsChanged += ProgramOnOnOptionsChanged;
            this.ProgramOnOnOptionsChanged();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Program.OnOptionsChanged -= this.ProgramOnOnOptionsChanged;
            this.UsernameParagraph.MouseEnter -= this.UsernameParagraphOnMouseEnter;
            this.UsernameParagraph.MouseLeave -= this.UsernameParagraphOnMouseLeave;
        }

        private void ProgramOnOnOptionsChanged()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(this.ProgramOnOnOptionsChanged));
                return;
            }
            var cur = Prefs.UseLightChat;
            if (useLightChat != null && (bool)useLightChat == cur) return;
            useLightChat = cur;
            if (cur)
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
            enableGifs = Prefs.EnableChatGifs;
            enableImages = Prefs.EnableChatImages;
            foreach (var hl in MessageParagraph.Inlines.OfType<Hyperlink>())
            {
                hl.Foreground = UrlBrush;
            }
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
            return f.Width + 10;
        }

        private static Brush almostBlack = new SolidColorBrush(Color.FromRgb(2, 2, 2));
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
                    System.Uri uri;
                    bool gotIt;
                    if (!System.Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out uri))
                    {
                        gotIt = System.Uri.TryCreate("http://" + s, UriKind.RelativeOrAbsolute, out uri);
                    }
                    else gotIt = true;

                    if (gotIt && uri != null)
                    {
                        if (!IsImageUrl(uri))
                        {
                            var hl = new Hyperlink(new Run(s)) { NavigateUri = uri };
                            hl.Foreground = UrlBrush;
                            hl.RequestNavigate += this.RequestNavigate;
                            MessageParagraph.Inlines.Add(hl);
                            continue;
                        }
                        else
                        {
                            try
                            {
                                var bi = BitmapFrame.Create(uri);
                                var image = new Image
                                {
                                    Source = bi,
                                    Stretch = Stretch.Uniform,
                                    VerticalAlignment = VerticalAlignment.Top,
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    Width = double.NaN,
                                    Height = double.NaN,
                                    StretchDirection = StretchDirection.DownOnly
                                };
                                var border = new Border
                                {
                                    MaxWidth = 300,
                                    MaxHeight = 500,
                                    Child = image,
                                    VerticalAlignment = VerticalAlignment.Top,
                                    HorizontalAlignment = HorizontalAlignment.Left
                                };
                                if (enableGifs)
                                {
                                    ImageBehavior.SetAnimatedSource(image, bi);
                                    ImageBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);
                                }
                                var container = new InlineUIContainer(border);
                                var hl = new Hyperlink(container){TextDecorations = null};


                                hl.Unloaded += LinkUnloaded;
                                hl.RequestNavigate += this.RequestNavigate;

                                MouseButtonEventHandler imageMouseUpHandler = (a, b) => RequestNavigate(hl, new RequestNavigateEventArgs(hl.NavigateUri, null));
                                RoutedEventHandler imageUnloadHandler = (a, b) =>
                                                                            {
                                                                                image.PreviewMouseUp -= imageMouseUpHandler;
                                                                            };
                                image.PreviewMouseUp += imageMouseUpHandler;
                                image.Unloaded += imageUnloadHandler;
                                MessageParagraph.Inlines.Add(hl);
                                continue;
                            }
                            catch
                            {}
                        }
                    }
                }
                var sb = new StringBuilder();
                var thingcount = 0;
                foreach (var c in s)
                {
                    if (c == '`')
                    {
                        if (thingcount == 0)
                        {
                            MessageParagraph.Inlines.Add(sb.ToString());
                            sb.Clear();
                        }
                        thingcount++;
                    }
                    if (thingcount == 2)
                    {
                        var str = sb.ToString().TrimStart('`');
                        var textBorder = new Border()
                                             {
                                                 BorderBrush = almostBlack,
                                                 Background = Brushes.LightGray,
                                                 CornerRadius = new CornerRadius(5),
                                                 //Padding = new Thickness(10,1,10,1),
                                                 VerticalAlignment = VerticalAlignment.Top,
                                                 HorizontalAlignment = HorizontalAlignment.Left
                                             };
                        var textBlock = new TextBlock()
                                            {
                                                Text = str,
                                                FontWeight = FontWeights.Bold,
                                                Foreground = almostBlack,
                                                Background = Brushes.Transparent,
                                                Margin = new Thickness(10,1,10,1)
                                            };
                        textBorder.Child = textBlock;
                        var block = new InlineUIContainer(textBorder);
                        MessageParagraph.Inlines.Add(block);
                        thingcount = 0;
                        sb.Clear();
                    }
                    else
                        sb.Append(c);
                }
                MessageParagraph.Inlines.Add(sb.ToString());
                //MessageParagraph.Inlines.Add(s);
            }
            
        }

        private void LinkUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var o = sender as Hyperlink;
            if (o == null) return;
            o.Unloaded -= LinkUnloaded;
            o.RequestNavigate -= this.RequestNavigate;
        }

        bool IsImageUrl(System.Uri url)
        {
            if (!enableImages) return false;
            if ((SubscriptionModule.Get().IsSubscribed ?? false) == false) return false;
            var req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "HEAD";
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    using (var resp = req.GetResponse())
                    {
                        return resp.ContentType.ToLower(CultureInfo.InvariantCulture).StartsWith("image/");
                    }
                }
                catch
                {
                }
            }
            return false;
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
                Program.LaunchUrl(navigateUri);
                //Process.Start(new ProcessStartInfo(navigateUri));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            e.Handled = true;
        }
    }
}
