using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Octgn.Utils;

namespace Octgn.Windows
{
    /// <summary>
    ///   Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DWindow
    {
        private readonly Brush _turnBrush;
        public RoutedCommand DebugWindowCommand = new RoutedCommand();

        public DWindow()
        {
            InitializeComponent();
            Color color = Color.FromRgb(0x00, 0x00, 0x00);
            _turnBrush = new SolidColorBrush(color);
            _turnBrush.Freeze();

            DebugWindowCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));

            var cb = new CommandBinding(DebugWindowCommand,
                                        MyCommandExecute, MyCommandCanExecute);
            CommandBindings.Add(cb);

            var kg = new KeyGesture(Key.M, ModifierKeys.Control);
            var ib = new InputBinding(DebugWindowCommand, kg);
            InputBindings.Add(ib);
        }

        private static void MyCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MyCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void WindowIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var v = (bool) e.NewValue;
            if (v)
            {
                Program.DebugListener.OnEventAdd += AddEvent;
                output.Document.Blocks.Clear();
                var events = Program.DebugListener.Events.ToArray();
                foreach (TraceEvent te in events)
                {
                    AddEvent(te);
                }
            }
            else
                Program.DebugListener.OnEventAdd -= AddEvent;
        }

        private void AddEvent(TraceEvent te)
        {
            Dispatcher.Invoke(new Action<TraceEvent>(IAddEvent), new object[] {te});
        }

        private void IAddEvent(TraceEvent te)
        {
            var p = new Paragraph
                        {
                            TextAlignment = TextAlignment.Left,
                            Margin = new Thickness(0),
                            Inlines =
                                {
                                    //new Line() { X1 = 0, X2 = 40, Y1 = -4, Y2 = -4, StrokeThickness = 2, Stroke = TurnBrush },
                                    new Run(te.ToString()) {Foreground = _turnBrush, FontWeight = FontWeights.Bold}
                                    //new Line() { X1 = 0, X2 = 40, Y1 = -4, Y2 = -4, StrokeThickness = 2, Stroke = TurnBrush }
                                }
                        };
            if (output.Document.Blocks.LastBlock != null)
                if (((Paragraph) output.Document.Blocks.LastBlock).Inlines.Count == 0)
                    output.Document.Blocks.Remove(output.Document.Blocks.LastBlock);
            output.Document.Blocks.Add(p);
            output.Document.Blocks.Add(new Paragraph {Margin = new Thickness()}); // Restore left alignment
            output.ScrollToEnd();
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            //Hide Window
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                                       (DispatcherOperationCallback) delegate
                                                                                         {
                                                                                             Hide();
                                                                                             return null;
                                                                                         }, null);
            //Do not close application
            e.Cancel = true;
        }
    }
}