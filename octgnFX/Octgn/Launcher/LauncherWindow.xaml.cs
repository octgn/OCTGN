using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace Octgn.Launcher
{
    public partial class LauncherWindow
    {
        public RoutedCommand DebugWindowCommand = new RoutedCommand();

        #region Animation

        private static readonly Duration TransitionDuration = new Duration(TimeSpan.FromMilliseconds(300));
        private static readonly object BackTarget = new object();

        private readonly AnimationTimeline InAnimation = new DoubleAnimation(0, 1, TransitionDuration)
                                                             {BeginTime = TimeSpan.FromMilliseconds(200)};

        private readonly AnimationTimeline OutAnimation = new DoubleAnimation(0, TransitionDuration);

        private readonly DoubleAnimation ResizeAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300))
                                                               {
                                                                   EasingFunction =
                                                                       new ExponentialEase
                                                                           {EasingMode = EasingMode.EaseOut}
                                                               };

        private double bordersHeight;
        private double clientWidth;
        private bool isFirstLoad = true;
        private bool isInTransition;
        private object transitionTarget;

        private void ConstructAnim()
        {
            NavigationCommands.BrowseBack.InputGestures.Clear();

            OutAnimation.Completed += delegate
                                          {
                                              isInTransition = false;
                                              if (transitionTarget == BackTarget)
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
                                  if (isInTransition)
                                  {
                                      e.Cancel = true;
                                      return;
                                  }

                                  if (transitionTarget != null)
                                  {
                                      transitionTarget = null;
                                      return;
                                  }

                                  var page = Content as Page;
                                  if (page == null) return;

                                  if (Math.Abs(clientWidth - 0) < double.Epsilon)
                                  {
                                      clientWidth = page.ActualWidth;
                                      bordersHeight = ActualHeight - page.ActualHeight;
                                  }
                                  SizeToContent = SizeToContent.Manual;

                                  e.Cancel = true;
                                  isInTransition = true;
                                  transitionTarget = e.NavigationMode == NavigationMode.Back ? BackTarget : e.Content;
                                  page.BeginAnimation(OpacityProperty, OutAnimation, HandoffBehavior.SnapshotAndReplace);
                              };

            Navigated += delegate
                             {
                                 var page = Content as Page;
                                 if (page == null) return;

                                 if (isFirstLoad)
                                 {
                                     isFirstLoad = false;
                                     return;
                                 }

                                 page.Opacity = 0;
                                 page.Measure(new Size(clientWidth, double.PositiveInfinity));

                                 ResizeAnimation.To = page.DesiredSize.Height + bordersHeight;
                                 BeginAnimation(HeightProperty, ResizeAnimation);
                                 page.BeginAnimation(OpacityProperty, InAnimation);
                             };
        }

        #endregion Animation

        #region Constructors

        public LauncherWindow()
        {
            Initialized += Launcher_Initialized;
            InitializeComponent();
            DebugWindowCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));

            var cb = new CommandBinding(DebugWindowCommand,
                                        MyCommandExecute, MyCommandCanExecute);
            CommandBindings.Add(cb);

            var kg = new KeyGesture(Key.M, ModifierKeys.Control);
            var ib = new InputBinding(DebugWindowCommand, kg);
            InputBindings.Add(ib);

            ConstructAnim();
        }

        public void Launcher_Initialized(object sender, EventArgs e)
        {
            Top = double.Parse(SimpleConfig.ReadValue("LoginTopLoc", "100"));
            Left = double.Parse(SimpleConfig.ReadValue("LoginLeftLoc", "100"));
        }

        #endregion Constructors

        private static void MyCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static void MyCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            //System.Diagnostics.XmlWriterTraceListener tr = new System.Diagnostics.XmlWriterTraceListener()
            if (Program.DebugWindow == null)
            {
                Program.DebugWindow = new DWindow();
            }
            Program.DebugWindow.Visibility = Program.DebugWindow.Visibility == Visibility.Visible
                                                 ? Visibility.Hidden
                                                 : Visibility.Visible;
        }
    }
}