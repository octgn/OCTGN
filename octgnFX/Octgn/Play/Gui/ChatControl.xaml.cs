using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Octgn.Data;

namespace Octgn.Play.Gui
{
    partial class ChatControl
    {
        public ChatControl()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            (output.Document.Blocks.FirstBlock).Margin = new Thickness();

            Loaded += delegate { Program.Trace.Listeners.Add(new ChatTraceListener("ChatListener", this)); };
            Unloaded += delegate { Program.Trace.Listeners.Remove("ChatListener"); };
        }

        public bool DisplayKeyboardShortcut
        {
            set { if (value) watermark.Text += "  (Ctrl+T)"; }
        }

        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                string msg = input.Text;
                input.Clear();
                if (string.IsNullOrEmpty(msg)) return;

                Program.Client.Rpc.ChatReq(msg);
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                input.Clear();
                var window = Window.GetWindow(this);
                if (window != null)
                    ((UIElement) window.Content).MoveFocus(
                        new TraversalRequest(FocusNavigationDirection.First));
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
    }

    internal sealed class ChatTraceListener : TraceListener
    {
        private static readonly Brush TurnBrush;
        private readonly ChatControl ctrl;

        static ChatTraceListener()
        {
            Color color = Color.FromRgb(0x5A, 0x9A, 0xCF);
            TurnBrush = new SolidColorBrush(color);
            TurnBrush.Freeze();
        }

        public ChatTraceListener(string name, ChatControl ctrl)
            : base(name)
        {
            this.ctrl = ctrl;
        }

        public override void Write(string message)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void WriteLine(string message)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
                                        string format, params object[] args)
        {
            Program.LastChatTrace = null;

            if (eventType > TraceEventType.Warning &&
                IsMuted() &&
                ((id & EventIds.Explicit) == 0))
                return;

            if (id == EventIds.Turn)
            {
                var p = new Paragraph
                            {
                                TextAlignment = TextAlignment.Center,
                                Margin = new Thickness(2),
                                Inlines =
                                    {
                                        new Line
                                            {X1 = 0, X2 = 40, Y1 = -4, Y2 = -4, StrokeThickness = 2, Stroke = TurnBrush},
                                        new Run(" " + string.Format(format, args) + " ")
                                            {Foreground = TurnBrush, FontWeight = FontWeights.Bold},
                                        new Line
                                            {X1 = 0, X2 = 40, Y1 = -4, Y2 = -4, StrokeThickness = 2, Stroke = TurnBrush}
                                    }
                            };
                if (((Paragraph) ctrl.output.Document.Blocks.LastBlock).Inlines.Count == 0)
                    ctrl.output.Document.Blocks.Remove(ctrl.output.Document.Blocks.LastBlock);
                ctrl.output.Document.Blocks.Add(p);
                ctrl.output.Document.Blocks.Add(new Paragraph {Margin = new Thickness()}); // Restore left alignment
                ctrl.output.ScrollToEnd();
            }
            else
                InsertLine(FormatInline(MergeArgs(format, args), eventType, id, args));
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
                                        string message)
        {
            Program.LastChatTrace = null;

            if (eventType > TraceEventType.Warning &&
                IsMuted() &&
                ((id & EventIds.Explicit) == 0))
                return;

            InsertLine(FormatMsg(message, eventType, id));
        }

        private bool IsMuted()
        {
            if (Program.Client.Muted != 0)
                return true;
            return false;
        }

        private void InsertLine(Inline message)
        {
            var p = (Paragraph) ctrl.output.Document.Blocks.LastBlock;
            if (p.Inlines.Count > 0) p.Inlines.Add(new LineBreak());
            p.Inlines.Add(message);
            Program.LastChatTrace = message;
            ctrl.output.ScrollToEnd();
        }

        private Inline FormatInline(Inline inline, TraceEventType eventType, int id, Object[] args = null)
        {
            if (eventType == TraceEventType.Warning || eventType == TraceEventType.Error)
            {
                inline.Foreground = Brushes.Red;
                inline.FontWeight = FontWeights.Bold;
            }
            else if (eventType == TraceEventType.Information)
            {
                if ((id & EventIds.Chat) != 0)
                    inline.FontWeight = FontWeights.Bold;
                if (args == null || args.GetUpperBound(0) == -1)
                {
                    if ((id & EventIds.OtherPlayer) == 0)
                        inline.Foreground = Brushes.DarkGray;
                }
                else
                {
                    int i = 0;
                    var p = args[i] as Player;
                    while (p == null && i < args.Length - 1)
                    {
                        i++;
                        p = args[i] as Player;
                    }
                    if (p != null)
                        inline.Foreground = new SolidColorBrush(p.Color);
                    else
                        inline.Foreground = Brushes.Red;
                }
            }
            return inline;
        }

        private Inline FormatMsg(string text, TraceEventType eventType, int id)
        {
            var result = new Run(text);
            return FormatInline(result, eventType, id);
        }

        private Inline MergeArgs(string format, object[] args)
        {
            return MergeArgs(format, args, 0);
        }

        private Inline MergeArgs(string format, object[] args, int startAt)
        {
            for (int i = startAt; i < args.Length; i++)
            {
                object arg = args[i];
                string placeholder = "{" + i + "}";

                var cardModel = arg as CardModel;
                var cardId = arg as CardIdentity;
                var card = arg as Card;
                if (card != null && (card.FaceUp || card.mayBeConsideredFaceUp))
                    cardId = card.Type;

                if (cardId != null || cardModel != null)
                {
                    string[] parts = format.Split(new[] {placeholder}, StringSplitOptions.None);
                    var result = new Span();
                    for (int j = 0; j < parts.Length; j++)
                    {
                        result.Inlines.Add(MergeArgs(parts[j], args, i + 1));
                        if (j + 1 < parts.Length)
                            result.Inlines.Add(cardId != null ? new CardRun(cardId) : new CardRun(cardModel));
                    }
                    return result;
                }
                format = format.Replace(placeholder, arg == null ? "[?]" : arg.ToString());
            }
            return new Run(format);
        }
    }

    internal class CardModelEventArgs : RoutedEventArgs
    {
        public readonly CardModel CardModel;

        public CardModelEventArgs(CardModel model, RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
            CardModel = model;
        }
    }

    internal class CardRun : Run
    {
        public static readonly RoutedEvent ViewCardModelEvent = EventManager.RegisterRoutedEvent("ViewCardIdentity",
                                                                                                 RoutingStrategy.Bubble,
                                                                                                 typeof (
                                                                                                     EventHandler
                                                                                                     <CardModelEventArgs
                                                                                                     >),
                                                                                                 typeof (CardRun));

        private CardModel card;

        public CardRun(CardIdentity id)
            : base(id.ToString())
        {
            card = id.model;
            if (id.model == null)
                id.Revealed += new CardIdentityNamer {Target = this}.Rename;
        }

        public CardRun(CardModel model)
            : base(model.Name)
        {
            card = model;
        }

        public void SetCardModel(CardModel model)
        {
            Debug.Assert(card == null, "Cannot set the CardModel of a CardRun if it is already defined");
            card = model;
            Text = model.Name;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (card != null)
                RaiseEvent(new CardModelEventArgs(card, ViewCardModelEvent, this));
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (card != null)
                RaiseEvent(new CardModelEventArgs(null, ViewCardModelEvent, this));
        }
    }
}