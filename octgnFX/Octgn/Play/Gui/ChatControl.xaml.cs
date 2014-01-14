using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Octgn.Play.Gui
{
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Timers;
    using System.Windows.Controls;
    using System.Windows.Data;

    using Microsoft.Win32;

    using Octgn.Annotations;
    using Octgn.Core.DataExtensionMethods;

    using log4net;

    using Octgn.Core.Play;
    using Octgn.Extentions;
    using Octgn.Utils;

    partial class ChatControl : INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool showInput = true;

        private bool hideErrors;

        public bool IgnoreMute { get; set; }

        public bool ShowInput
        {
            get
            {
                return this.showInput;
            }
            set
            {
                if (value.Equals(this.showInput))
                {
                    return;
                }
                this.showInput = value;
                this.OnPropertyChanged("ShowInput");
            }
        }

        public bool HideErrors
        {
            get
            {
                return this.hideErrors;
            }
            set
            {
                if (value.Equals(this.hideErrors))
                {
                    return;
                }
                this.hideErrors = value;
                this.OnPropertyChanged("HideErrors");
            }
        }

        public bool AutoScroll
        {
            get
            {
                return this.autoScroll;
            }
            set
            {
                if (value == this.autoScroll) return;
                this.autoScroll = value;
                OnPropertyChanged("AutoScroll");
            }
        }

        public Action<IGameMessage> NewMessage;

        private System.Timers.Timer chatTimer2;

        public ChatControl()
        {
            AutoScroll = true;
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            (output.Document.Blocks.FirstBlock).Margin = new Thickness();

            //var listener = new ChatTraceListener("ChatListener", this);

            Loaded += delegate
            {
                chatTimer2 = new System.Timers.Timer(100);
                chatTimer2.Enabled = true;
                chatTimer2.Elapsed += this.TickMessage;
            };
            Unloaded += delegate
            {
                chatTimer2.Enabled = false;
                chatTimer2.Elapsed -= this.TickMessage;
                chatTimer2.Dispose();
            };
        }

        public static Block GameMessageToBlock(IGameMessage m)
        {
            if (m == null)
                return null;

            if (m is PlayerEventMessage)
            {
                if (m.IsMuted) return null;
                var b = new GameMessageBlock(m);
                var p = new Paragraph();
                var prun = new Run(m.From + " ");
                prun.Foreground = m.From.Color.CacheToBrush();
                prun.FontWeight = FontWeights.Bold;
                p.Inlines.Add(prun);

                var chatRun = MergeArgs(m.Message, m.Arguments);
                chatRun.Foreground = new SolidColorBrush(m.From.Color);
                //chatRun.FontWeight = FontWeights.Bold;
                p.Inlines.Add(chatRun);

                b.Blocks.Add(p);

                return b;
            }
            else if (m is ChatMessage)
            {
                if (m.IsMuted) return null;

                var p = new Paragraph();
                var b = new GameMessageBlock(m);

                var inline = new Span();

                inline.Foreground = m.From.Color.CacheToBrush();
                var chatRun = new Run("<" + m.From + "> ");
                chatRun.Foreground = m.From.Color.CacheToBrush();
                chatRun.FontWeight = FontWeights.Bold;
                inline.Inlines.Add(chatRun);

                inline.Inlines.Add(MergeArgs(m.Message, m.Arguments));

                p.Inlines.Add(inline);

                b.Blocks.Add(p);

                return b;
            }
            else if (m is WarningMessage)
            {
                if (m.IsMuted) return null;

                var b = new GameMessageBlock(m);
                var block = new BlockUIContainer();
                var border = new Border()
                {
                    CornerRadius = new CornerRadius(4),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(5),
                    Background = Brushes.LightGray,
                };
                var tb = new TextBlock(MergeArgs(m.Message, m.Arguments));
                tb.Foreground = m.From.Color.CacheToBrush();
                tb.TextWrapping = TextWrapping.Wrap;

                border.Child = tb;
                block.Child = border;

				b.Blocks.Add(block);

                return b;
            }
            else if (m is SystemMessage)
            {
                if (m.IsMuted) return null;

                var p = new Paragraph();
                var b = new GameMessageBlock(m);
                var chatRun = MergeArgs(m.Message, m.Arguments);
                chatRun.Foreground = m.From.Color.CacheToBrush();
                p.Inlines.Add(chatRun);
                b.Blocks.Add(p);
                return b;
            }
            else if (m is NotifyMessage)
            {
                if (m.IsMuted) return null;

                var p = new Paragraph();
                var b = new GameMessageBlock(m);
                var chatRun = MergeArgsv2(m.Message, m.Arguments);
                chatRun.Foreground = m.From.Color.CacheToBrush();
				b.Blocks.Add(p);
                p.Inlines.Add(chatRun);
                return b;
            }
            else if (m is TurnMessage)
            {
                if (m.IsMuted) return null;

                var brush = m.From.Color.CacheToBrush();

                var p = new Paragraph();
                var b = new GameMessageBlock(m);
                b.TextAlignment = TextAlignment.Center;
                b.Margin = new Thickness(2);

                p.Inlines.Add(
                    new Line
                    {
                        X1 = 0,
                        X2 = 40,
                        Y1 = -4,
                        Y2 = -4,
                        StrokeThickness = 2,
                        Stroke = brush
                    });

                var chatRun = new Run(string.Format(m.Message, m.Arguments));
                chatRun.Foreground = brush;
                chatRun.FontWeight = FontWeights.Bold;
                p.Inlines.Add(chatRun);

                var prun = new Run(" " + (m as TurnMessage).TurnPlayer + " ");
                prun.Foreground = (m as TurnMessage).TurnPlayer.Color.CacheToBrush();
                prun.FontWeight = FontWeights.Bold;
                p.Inlines.Add(prun);

                p.Inlines.Add(
                    new Line
                    {
                        X1 = 0,
                        X2 = 40,
                        Y1 = -4,
                        Y2 = -4,
                        StrokeThickness = 2,
                        Stroke = brush
                    });

                b.Blocks.Add(p);

                //if (((Paragraph)output.Document.Blocks.LastBlock).Inlines.Count == 0) 
                //    output.Document.Blocks.Remove(output.Document.Blocks.LastBlock);

                return b;

                //output.Document.Blocks.Add(new Paragraph { Margin = new Thickness() });
            }
            else if (m is DebugMessage)
            {
                if (m.IsMuted) return null;
                var p = new Paragraph();
                var b = new GameMessageBlock(m);
                var chatRun = MergeArgs(m.Message, m.Arguments);
                chatRun.Foreground = m.From.Color.CacheToBrush();
                p.Inlines.Add(chatRun);
                b.Blocks.Add(p);
                return b;
            }
			else if (m is NotifyBarMessage)
			{
                if (m.IsMuted) return null;
                var p = new Paragraph();
                var b = new GameMessageBlock(m);
                var chatRun = MergeArgs(m.Message, m.Arguments);
                chatRun.Foreground = (m as NotifyBarMessage).MessageColor.CacheToBrush();
                p.Inlines.Add(chatRun);
                b.Blocks.Add(p);
                return b;
			}
            return null;
        }

        private void TickMessage(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (this)
            {
                if (chatTimer2.Enabled == false) return;
                chatTimer2.Enabled = false;
            }
            try
            {
                var newMessages = Program.GameMess.Messages.OrderBy(x => x.Id).Where(x => x.Id > lastId).ToArray();
                if (newMessages.Length == 0)
                {
                    return;
                }

                lastId = newMessages.Last().Id;

                Dispatcher.Invoke(new Action(() =>
                {
                    foreach (var m in newMessages)
                    {

                        if (NewMessage != null)
                            NewMessage(m);

                        var b = GameMessageToBlock(m);
                        if (b != null)
                        {
                            this.output.Document.Blocks.Add(b);
                        }
                    }
                }));
            }
            finally
            {
                if (AutoScroll)
                {
                    Dispatcher.Invoke(new Action(() => this.output.ScrollToEnd()));
                }
                try
                {
                    chatTimer2.Enabled = true;
                }
                catch
                {
                }
            }
        }

        private long lastId = -1;

        private bool autoScroll;

        //Like a boss
        public static Inline MergeArgsv2(string format, object[] arguments)
        {
            var args = arguments.ToList();
            var ret = new Span();
            bool foundLeft = false;
            int tStart = 0;
            var sb = new StringBuilder();
            var numString = "";
            var i = 0;

            // Replace any instances of any players name with the goods.

            foreach (var p in Player.AllExceptGlobal)
            {
                if (format.Contains(p.Name))
                {
                    var ind = -1;
                    for (var a = 0; a < args.Count; a++)
                    {
                        if (args[a] == p)
                        {
                            ind = a;
                            break;
                        }
                    }
                    if (ind == -1)
                    {
                        ind = args.Count;
                        args.Add(p);
                    }
                    format = format.Replace(p.Name, "{" + ind + "}");
                }
            }

            // Now we replace the format shit with objects like a boss.
            foreach (var c in format)
            {
                sb.Append(c);
                if (c == '{')
                {
                    numString = "";
                    if (foundLeft)
                    {
                        foundLeft = false;
                    }
                    else
                    {
                        foundLeft = true;
                        tStart = 0;
                    }
                }
                else if (c.IsANumber() && foundLeft)
                {
                    numString += c;
                    tStart++;
                }
                else if (c == '}')
                {
                    if (foundLeft && numString.IsANumber())
                    {
                        // Add our current string to the ret inline
                        if (sb.Length > 0)
                        {
                            var str = sb.ToString();
							str = str.Substring(0, str.Length - (tStart + 2));
                            if (str.Length > 0)
                            {
                                var il = new Run(str);
                                ret.Inlines.Add(il);
                            }
                            sb.Clear();
                            i = -1;
                        }
                        int num = int.Parse(numString);

                        var arg = args[num];

                        var cardModel = arg as DataNew.Entities.Card;
                        var cardId = arg as CardIdentity;
                        var card = arg as Card;
                        if (card != null && (card.FaceUp || card.MayBeConsideredFaceUp))
                            cardId = card.Type;

                        if (cardId != null || cardModel != null || arg is IPlayPlayer)
                        {

                            if (arg is IPlayPlayer)
                            {
                                ret.Inlines.Add(new PlayerRun((arg as IPlayPlayer)));
                            }
                            else
                            {
                                ret.Inlines.Add(cardId != null ? new CardRun(cardId) : new CardRun(cardModel));
                            }
                        }
                        else
                            sb.Append(arg == null ? "[?]" : arg.ToString());
                    }
                    foundLeft = false;
                }
                else
                {
                    foundLeft = false;
                }
                i++;
            }
            if (sb.Length > 0)
            {
                var str = sb.ToString();
				//if(tStart > 0)
				//	str = str.Substring(0, tStart);
                var il = new Run(str);
                ret.Inlines.Add(il);
                sb.Clear();
            }
            return ret;
        }

        private static Inline MergeArgs(string format, object[] args, int startAt = 0)
        {
            if (args == null) args = new object[0];
            for (int i = startAt; i < args.Length; i++)
            {
                object arg = args[i];
                string placeholder = "{" + i + "}";

                var cardModel = arg as DataNew.Entities.Card;
                var cardId = arg as CardIdentity;
                var card = arg as Card;
                if (card != null && (card.FaceUp || card.MayBeConsideredFaceUp))
                    cardId = card.Type;

                if (cardId != null || cardModel != null || arg is IPlayPlayer)
                {
                    string[] parts = format.Split(new[] { placeholder }, StringSplitOptions.None);
                    var result = new Span();
                    for (int j = 0; j < parts.Length; j++)
                    {
                        result.Inlines.Add(MergeArgs(parts[j], args, i + 1));
                        if (j + 1 < parts.Length)
                        {
                            if (arg is IPlayPlayer)
                            {
                                result.Inlines.Add(new PlayerRun((arg as IPlayPlayer)));
                            }
                            else
                            {
                                result.Inlines.Add(cardId != null ? new CardRun(cardId) : new CardRun(cardModel));
                            }
                        }
                    }
                    return result;
                }
                format = format.Replace(placeholder, arg == null ? "[?]" : arg.ToString());
            }
            return new Run(format);
        }

        public bool DisplayKeyboardShortcut
        {
            set { if (value) watermark.Text += "  (Ctrl+T)"; }
        }

        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    {
                        e.Handled = true;

                        string msg = input.Text;
                        input.Clear();
                        if (string.IsNullOrEmpty(msg)) return;

                        Program.Client.Rpc.ChatReq(msg);
                    }
                    break;
                case Key.Escape:
                    {
                        e.Handled = true;
                        input.Clear();
                        Window window = Window.GetWindow(this);
                        if (window != null)
                            ((UIElement)window.Content).MoveFocus(
                                new TraversalRequest(FocusNavigationDirection.First));
                    }
                    break;
            }
        }

        private void InputGotFocus(object sender, RoutedEventArgs e)
        {
            watermark.Visibility = Visibility.Hidden;
        }

        private void InputLostFocus(object sender, RoutedEventArgs e)
        {
            if (input.Text == "") watermark.Visibility = Visibility.Visible;
        }

        public void FocusInput()
        {
            input.Focus();
        }

        public void Save()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(this.Save));
                return;
            }
            try
            {
                var sfd = new SaveFileDialog { Filter = "Octgn Game Log (*.txt) | *.txt" };
                if (sfd.ShowDialog().GetValueOrDefault(false))
                {
                    var tr = new TextRange(output.Document.ContentStart, output.Document.ContentEnd);
                    using (var stream = sfd.OpenFile())
                    {
                        tr.Save(stream, DataFormats.Text);
                        stream.Flush();
                    }
                }

            }
            catch (Exception e)
            {
                Log.Warn("Save log error", e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    internal class CardModelEventArgs : RoutedEventArgs
    {
        public readonly DataNew.Entities.Card CardModel;

        public CardModelEventArgs(DataNew.Entities.Card model, RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
            CardModel = model;
        }
    }

    internal class CardRun : Underline
    {
        public static readonly RoutedEvent ViewCardModelEvent = EventManager.RegisterRoutedEvent("ViewCardIdentity",
                                                                                                 RoutingStrategy.Bubble,
                                                                                                 typeof(
                                                                                                     EventHandler
                                                                                                     <CardModelEventArgs
                                                                                                     >),
                                                                                                 typeof(CardRun));

        private DataNew.Entities.Card _card;

        public CardRun(CardIdentity id)
            : base(new Run(id.ToString()))
        {
            this.FontWeight = FontWeights.Bold;
            this.Foreground = Brushes.DarkSlateGray;
            this.Cursor = Cursors.Hand;
            _card = id.Model;
            if (id.Model == null)
                id.Revealed += new CardIdentityNamer { Target = this }.Rename;
        }

        public CardRun(DataNew.Entities.Card model)
            : base(new Run(model.PropertyName()))
        {
            _card = model;
        }

        public void SetCardModel(DataNew.Entities.Card model)
        {
            Debug.Assert(_card == null, "Cannot set the CardModel of a CardRun if it is already defined");
            _card = model;
            (this.Inlines.FirstInline as Run).Text = model.PropertyName();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (_card != null)
                RaiseEvent(new CardModelEventArgs(_card, ViewCardModelEvent, this));
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (_card != null)
                RaiseEvent(new CardModelEventArgs(null, ViewCardModelEvent, this));
        }
    }

    internal class PlayerRun : Run
    {
        private IPlayPlayer _player;

        public PlayerRun(IPlayPlayer player)
            : base(player.Name)
        {
            _player = player;
            Foreground = _player.Color.CacheToBrush();
            FontWeight = FontWeights.Bold;
        }
    }

    public class GameMessageBlock : Section
    {
        public IGameMessage Message { get; private set; }
		
        public GameMessageBlock(IGameMessage mess)
        {
            Message = mess;
        }
    }

    public class GameMessageToBlockConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mess = value as IGameMessage;
            if (mess == null) return null;
            return ChatControl.GameMessageToBlock(mess);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is GameMessageBlock) == false) return null;
            return (value as GameMessageBlock).Message;
        }
    }
}