using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Octgn
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DWindow : Window
    {
        Brush TurnBrush;
        public RoutedCommand DebugWindowCommand = new RoutedCommand();

        public DWindow()
        {
            InitializeComponent();
            var color = Color.FromRgb(0xFF, 0x00, 0x00);
            TurnBrush = new SolidColorBrush(color);
            TurnBrush.Freeze();

            DebugWindowCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));

            CommandBinding cb = new CommandBinding(DebugWindowCommand,
                MyCommandExecute, MyCommandCanExecute);
            this.CommandBindings.Add(cb);

            KeyGesture kg = new KeyGesture(Key.M, ModifierKeys.Control);
            InputBinding ib = new InputBinding(DebugWindowCommand, kg);
            this.InputBindings.Add(ib);
        }

        private void MyCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MyCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool v = (bool)e.NewValue;
            if(v)
            {
                Program.DebugListener.OnEventAdd += new CacheTraceListener.EventAdded(Add_Event);
                output.Document.Blocks.Clear();
                foreach(TraceEvent te in Program.DebugListener.Events)
                {
                    Add_Event(te);
                }
            }
            else
                Program.DebugListener.OnEventAdd -= Add_Event;
        }

        private void Add_Event(TraceEvent te)
        {
            this.Dispatcher.Invoke(new Action<TraceEvent>(i_Add_Event), new object[1] { te });
        }

        private void i_Add_Event(TraceEvent te)
        {
            var p = new Paragraph()
            {
                TextAlignment = TextAlignment.Left,
                Margin = new Thickness(2),
                Inlines =
                        {
                            new Line() { X1 = 0, X2 = 40, Y1 = -4, Y2 = -4, StrokeThickness = 2, Stroke = TurnBrush },
                            new Run(System.Environment.NewLine + te.ToString() + System.Environment.NewLine) { Foreground = TurnBrush, FontWeight = FontWeights.Bold  },
                            new Line() { X1 = 0, X2 = 40, Y1 = -4, Y2 = -4, StrokeThickness = 2, Stroke = TurnBrush }
                        }
            };
            if(output.Document.Blocks.LastBlock != null)
                if(((Paragraph)output.Document.Blocks.LastBlock).Inlines.Count == 0)
                    output.Document.Blocks.Remove(output.Document.Blocks.LastBlock);
            output.Document.Blocks.Add(p);
            output.Document.Blocks.Add(new Paragraph() { Margin = new Thickness() });    // Restore left alignment
            output.ScrollToEnd();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Hide Window
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
            {
                Hide();
                return null;
            }, null);
            //Do not close application
            e.Cancel = true;
        }
    }
}