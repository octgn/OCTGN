using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace Octgn.Launcher
{
    public partial class LauncherWindow : NavigationWindow
    {
        public RoutedCommand DebugWindowCommand = new RoutedCommand();

        #region Animation

        private static readonly Duration TransitionDuration = new Duration(TimeSpan.FromMilliseconds(300));
        private readonly AnimationTimeline OutAnimation = new DoubleAnimation(0, TransitionDuration);
        private readonly AnimationTimeline InAnimation = new DoubleAnimation(0, 1, TransitionDuration) { BeginTime = TimeSpan.FromMilliseconds(200) };
        private readonly DoubleAnimation ResizeAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut } };

        private static readonly object BackTarget = new object();
        private bool isInTransition = false;
        private bool isFirstLoad = true;
        private double clientWidth = 0, bordersHeight = 0;
        private object transitionTarget;

        private void ConstructAnim()
        {
            NavigationCommands.BrowseBack.InputGestures.Clear();

            OutAnimation.Completed += delegate
            {
                isInTransition = false;
                if(transitionTarget == BackTarget)
                    GoBack();
                else
                    Navigate(transitionTarget);
            };
            OutAnimation.Freeze();

            ResizeAnimation.Completed += (s, e) => SizeToContent = SizeToContent.WidthAndHeight;

            Navigating += delegate(object sender, NavigatingCancelEventArgs e)
            {
                // FIX (jods): prevent further navigation when a navigation is already in progress
                //						 (e.g. double-click a button in the main menu). This would break the transitions.
                if(isInTransition)
                { e.Cancel = true; return; }

                if(transitionTarget != null)
                {
                    transitionTarget = null;
                    return;
                }

                var page = Content as Page;
                if(page == null) return;

                if(clientWidth == 0)
                {
                    clientWidth = page.ActualWidth;
                    bordersHeight = ActualHeight - page.ActualHeight;
                }
                SizeToContent = System.Windows.SizeToContent.Manual;

                e.Cancel = true;
                isInTransition = true;
                if(e.NavigationMode == NavigationMode.Back)
                    transitionTarget = BackTarget;
                else
                    transitionTarget = e.Content;
                page.BeginAnimation(UIElement.OpacityProperty, OutAnimation, HandoffBehavior.SnapshotAndReplace);
            };

            Navigated += delegate
            {
                var page = Content as Page;
                if(page == null) return;

                if(isFirstLoad)
                {
                    isFirstLoad = false;
                    return;
                }

                page.Opacity = 0;
                page.Measure(new Size(clientWidth, double.PositiveInfinity));

                ResizeAnimation.To = page.DesiredSize.Height + bordersHeight;
                this.BeginAnimation(Window.HeightProperty, ResizeAnimation);
                page.BeginAnimation(UIElement.OpacityProperty, InAnimation);
            };
        }

        #endregion Animation

        #region Constructors

        public LauncherWindow()
        {
            this.Initialized += Launcher_Initialized;
            InitializeComponent();
            DebugWindowCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));

            CommandBinding cb = new CommandBinding(DebugWindowCommand,
                MyCommandExecute, MyCommandCanExecute);
            this.CommandBindings.Add(cb);

            KeyGesture kg = new KeyGesture(Key.M, ModifierKeys.Control);
            InputBinding ib = new InputBinding(DebugWindowCommand, kg);
            this.InputBindings.Add(ib);

            ConstructAnim();
        }

        public void Launcher_Initialized(object sender, EventArgs e)
        {
            this.Top = Properties.Settings.Default.LoginTopLoc;
            this.Left = Properties.Settings.Default.LoginLeftLoc;
        }

        #endregion Constructors

        private void MyCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MyCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            //System.Diagnostics.XmlWriterTraceListener tr = new System.Diagnostics.XmlWriterTraceListener()
            if(Program.DebugWindow == null)
            {
                Program.DebugWindow = new DWindow();
            }
            if(Program.DebugWindow.Visibility == System.Windows.Visibility.Visible)
                Program.DebugWindow.Visibility = System.Windows.Visibility.Hidden;
            else
                Program.DebugWindow.Visibility = System.Windows.Visibility.Visible;
        }
    }
}