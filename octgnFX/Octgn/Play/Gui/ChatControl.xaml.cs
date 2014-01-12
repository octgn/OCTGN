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
    using System.Linq;
    using System.Reflection;
    using System.Timers;
    using System.Windows.Controls;

    using Microsoft.Win32;

    using Octgn.Annotations;
    using Octgn.Core.DataExtensionMethods;

    using log4net;

    using Octgn.Core.Play;
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
                chatTimer2.Elapsed += ChatTimer2OnElapsed;
                //chatTimer.Start();
                //Program.ChatLog.ActionLock(
                //    x =>
                //    {
                //        foreach (var e in x)
                //        {
                //            listener.TraceEvent(e.Cache, e.Source, e.Type, e.Id, e.Format, e.Args);
                //        }
                //        Program.Trace.Listeners.Add(listener);
                //    });
            };
            Unloaded += delegate
            {
                chatTimer2.Enabled = false;
                chatTimer2.Elapsed -= ChatTimer2OnElapsed;
                chatTimer2.Dispose();

                //Program.Trace.Listeners.Remove(listener);
            };
        }

        private void ChatTimer2OnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
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
                        if (m is PlayerEventMessage)
                        {
                            if (m.IsMuted) continue;
                            var b = new Paragraph();
                            var prun = new Run(m.From + " ");
                            prun.Foreground = m.From.Color.CacheToBrush();
                            prun.FontWeight = FontWeights.Bold;
                            b.Inlines.Add(prun);

                            var chatRun = MergeArgs(m.Message, m.Arguments);
                            chatRun.Foreground = new SolidColorBrush(m.From.Color);
                            //chatRun.FontWeight = FontWeights.Bold;
                            b.Inlines.Add(chatRun);
                            this.output.Document.Blocks.Add(b);
                        }
                        else if (m is ChatMessage)
                        {
                            if (m.IsMuted) continue;

                            var b = new System.Windows.Documents.Paragraph();
                            b.Foreground = m.From.Color.CacheToBrush();
                            var chatRun = new Run("<" + m.From + "> ");
                            chatRun.Foreground = m.From.Color.CacheToBrush();
                            chatRun.FontWeight = FontWeights.Bold;
                            b.Inlines.Add(chatRun);

                            b.Inlines.Add(MergeArgs(m.Message, m.Arguments));

                            this.output.Document.Blocks.Add(b);
                        }
                        else if (m is WarningMessage)
                        {
                            if (m.IsMuted) continue;

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

                            this.output.Document.Blocks.Add(block);
                        }
                        else if (m is SystemMessage)
                        {
                            if (m.IsMuted) continue;

                            var b = new Paragraph();
                            var chatRun = MergeArgs(m.Message, m.Arguments);
                            chatRun.Foreground = m.From.Color.CacheToBrush();
                            b.Inlines.Add(chatRun);
                            this.output.Document.Blocks.Add(b);
                        }
                        else if (m is NotifyMessage)
                        {
                            if (m.IsMuted) continue;

                            var b = new Paragraph();
                            var chatRun = MergeArgs(m.Message, m.Arguments);
                            chatRun.Foreground = m.From.Color.CacheToBrush();
                            b.Inlines.Add(chatRun);
                            this.output.Document.Blocks.Add(b);
                        }
                        else if (m is TurnMessage)
                        {
                            if (m.IsMuted) continue;

                            var brush = m.From.Color.CacheToBrush();

                            var b = new Paragraph();
                            b.TextAlignment = TextAlignment.Center;
                            b.Margin = new Thickness(2);

                            b.Inlines.Add(
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
                            b.Inlines.Add(chatRun);

                            var prun = new Run(" " + (m as TurnMessage).TurnPlayer + " ");
                            prun.Foreground = (m as TurnMessage).TurnPlayer.Color.CacheToBrush();
                            prun.FontWeight = FontWeights.Bold;
                            b.Inlines.Add(prun);

                            b.Inlines.Add(
                                new Line
                                {
                                    X1 = 0,
                                    X2 = 40,
                                    Y1 = -4,
                                    Y2 = -4,
                                    StrokeThickness = 2,
                                    Stroke = brush
                                });

                            if (((Paragraph)output.Document.Blocks.LastBlock).Inlines.Count == 0) output.Document.Blocks.Remove(output.Document.Blocks.LastBlock);

                            this.output.Document.Blocks.Add(b);

                            output.Document.Blocks.Add(new Paragraph { Margin = new Thickness() });
                        }
                        else if (m is DebugMessage)
                        {
                            if (m.IsMuted) continue;
                            var b = new Paragraph();
                            var chatRun = MergeArgs(m.Message, m.Arguments);
                            chatRun.Foreground = m.From.Color.CacheToBrush();
                            b.Inlines.Add(chatRun);
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
                chatTimer2.Enabled = true;
            }
        }

        private long lastId = -1;

        private bool autoScroll;

        private void TOnTick(object sender, EventArgs eventArgs)
        {

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
}