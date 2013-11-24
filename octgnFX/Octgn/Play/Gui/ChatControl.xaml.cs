using System;
using System.Collections.Generic;
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
    using System.Reflection;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Media3D;
    using System.Windows.Threading;

    using Microsoft.Win32;

    using Octgn.Annotations;
    using Octgn.Core.DataExtensionMethods;

    using log4net;

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
                            ((UIElement) window.Content).MoveFocus(
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
                Log.Warn("Save log error",e);
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

    internal sealed class ChatTraceListener : TraceListener
    {
        private static readonly Brush TurnBrush;
        private readonly ChatControl _ctrl;
        private readonly Dispatcher Dispatcher;

        static ChatTraceListener()
        {
            Color color = Color.FromRgb(0x5A, 0x9A, 0xCF);
            TurnBrush = new SolidColorBrush(color);
            TurnBrush.Freeze();
        }

        public ChatTraceListener(string name, ChatControl ctrl)
            : base(name)
        {
            Dispatcher = ctrl.Dispatcher;
            _ctrl = ctrl;
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
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(()=>this.TraceEvent(eventCache, source, eventType, id, format, args)));
                return;
            }
            Program.LastChatTrace = null;

            if (!_ctrl.IgnoreMute)
            {
                if (eventType > TraceEventType.Warning && IsMuted() && ((id & EventIds.Explicit) == 0)) return;
            }
            if (_ctrl.HideErrors)
            {
                if (eventType == TraceEventType.Critical || eventType == TraceEventType.Error
                    || eventType == TraceEventType.Warning)
                {
                    return;
                }
            }
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
                if (((Paragraph) _ctrl.output.Document.Blocks.LastBlock).Inlines.Count == 0)
                    _ctrl.output.Document.Blocks.Remove(_ctrl.output.Document.Blocks.LastBlock);
                _ctrl.output.Document.Blocks.Add(p);
                _ctrl.output.Document.Blocks.Add(new Paragraph {Margin = new Thickness()}); // Restore left alignment
                _ctrl.output.ScrollToEnd();
            }
            else
                InsertLine(FormatInline(_ctrl,MergeArgs(format, args), eventType, id, args));
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
                                        string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => this.TraceEvent(eventCache, source, eventType, id, message)));
                return;
            }
            Program.LastChatTrace = null;

            if (!_ctrl.IgnoreMute)
            {
                if (eventType > TraceEventType.Warning && IsMuted() && ((id & EventIds.Explicit) == 0)) return;
            }
            InsertLine(FormatMsg(_ctrl,message, eventType, id));
        }

        private static bool IsMuted()
        {
            return Program.Client.Muted != 0;
        }

        private void InsertLine(Inline message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => this.InsertLine(message)));
                return;
            }
            //TextIndent="-60" Margin="60,20,0,0"
            var p = new Paragraph();
            p.TextIndent = -15;
            p.Margin = new Thickness(15, 0, 0, 0);
            p.Inlines.Add(message);
            Program.LastChatTrace = message;
            _ctrl.output.Document.Blocks.Add(p);
            _ctrl.output.ScrollToEnd();

            //var p = (Paragraph)this._ctrl.output.Document.Blocks.LastBlock;
            //if (p.Inlines.Count > 0) p.Inlines.Add(new LineBreak());
            //p.Inlines.Add(message);
            //Program.LastChatTrace = message;
            //_ctrl.output.ScrollToEnd();
        }

        /// <summary>
        /// Format an inline. MUST BE CALLED ON THE UI THREAD
        /// </summary>
        /// <param name="control"></param>
        /// <param name="inline"></param>
        /// <param name="eventType"></param>
        /// <param name="id"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Inline FormatInline(ChatControl control, Inline inline, TraceEventType eventType, int id, Object[] args = null)
        {
            switch (eventType)
            {
                case TraceEventType.Error:
                case TraceEventType.Warning:
                    inline.Foreground = Brushes.Red;
                    inline.FontWeight = FontWeights.Bold;
                    break;
                case TraceEventType.Information:
                    if ((id & EventIds.Chat) != 0)
                        inline.FontWeight = FontWeights.Bold;
                    if (args == null || args.GetUpperBound(0) == -1)
                    {
                        if ((id & EventIds.OtherPlayer) == 0)
                            inline.Foreground = Brushes.Black;
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
                        inline.Foreground = p != null ? new SolidColorBrush(p.Color) : Brushes.Red;

                        if (p != null && Player.LocalPlayer.Id != p.Id)
                        {
                            var theinline = inline;
                            theinline.Initialized += (sender, eventArgs) =>
                                {
                                    try
                                    {
                                        var curcolor = (theinline.Foreground as SolidColorBrush).Color;
                                        var dbAscending = new ColorAnimation(curcolor, Colors.Crimson, new Duration(TimeSpan.FromMilliseconds(500)))
                                            { RepeatBehavior = new RepeatBehavior(2), AutoReverse = true };
                                        var storyboard = new Storyboard();
                                        Storyboard.SetTarget(dbAscending, theinline);
                                        Storyboard.SetTargetProperty(dbAscending, new PropertyPath("Foreground.Color"));
                                        storyboard.Children.Add(dbAscending);
                                        storyboard.Begin(control);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                };
                        }
                    }
                    break;
            }
            return inline;
        }

        /// <summary>
        /// Formate a message. MUST BE CALLED ON THE UI THREAD.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="text"></param>
        /// <param name="eventType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private static Inline FormatMsg(ChatControl control,string text, TraceEventType eventType, int id)
        {
            var result = new Run(text);
            return FormatInline(control,result, eventType, id);
        }

        /// <summary>
        /// Merge arguments...MUST BE CALLED ON THE UI THREAD.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <param name="startAt"></param>
        /// <returns></returns>
        private static Inline MergeArgs(string format, IList<object> args, int startAt = 0)
        {
            for (int i = startAt; i < args.Count; i++)
            {
                object arg = args[i];
                string placeholder = "{" + i + "}";

                var cardModel = arg as DataNew.Entities.Card;
                var cardId = arg as CardIdentity;
                var card = arg as Card;
                if (card != null && (card.FaceUp || card.MayBeConsideredFaceUp))
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
        public readonly DataNew.Entities.Card CardModel;

        public CardModelEventArgs(DataNew.Entities.Card model, RoutedEvent routedEvent, object source)
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

        private DataNew.Entities.Card _card;

        public CardRun(CardIdentity id)
            : base(id.ToString())
        {
            _card = id.Model;
            if (id.Model == null)
                id.Revealed += new CardIdentityNamer {Target = this}.Rename;
        }

        public CardRun(DataNew.Entities.Card model)
            : base(model.PropertyName())
        {
            _card = model;
        }

        public void SetCardModel(DataNew.Entities.Card model)
        {
            Debug.Assert(_card == null, "Cannot set the CardModel of a CardRun if it is already defined");
            _card = model;
            Text = model.PropertyName();
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
}