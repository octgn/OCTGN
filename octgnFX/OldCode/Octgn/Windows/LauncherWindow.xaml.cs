using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Octgn.Data;

namespace Octgn.Windows
{
    public partial class LauncherWindow
    {
        public RoutedCommand DebugWindowCommand = new RoutedCommand();

        #region Animation

        private static readonly Duration TransitionDuration = new Duration(TimeSpan.FromMilliseconds(300));
        private static readonly object BackTarget = new object();

        private readonly AnimationTimeline _inAnimation = new DoubleAnimation(0, 1, TransitionDuration)
                                                              {BeginTime = TimeSpan.FromMilliseconds(200)};

        private readonly AnimationTimeline _outAnimation = new DoubleAnimation(0, TransitionDuration);

        private readonly DoubleAnimation _resizeAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300))
                                                                {
                                                                    EasingFunction =
                                                                        new ExponentialEase
                                                                            {EasingMode = EasingMode.EaseOut}
                                                                };

        private double _bordersHeight;
        private double _clientWidth;
        private bool _isFirstLoad = true;
        private bool _isInTransition;
        private object _transitionTarget;

        private void ConstructAnim()
        {
            NavigationCommands.BrowseBack.InputGestures.Clear();

            _outAnimation.Completed += delegate
                                           {
                                               _isInTransition = false;
                                               if (_transitionTarget == BackTarget)
                                                   GoBack();
                                               else
                                                   Navigate(_transitionTarget);
                                           };
            _outAnimation.Freeze();

            _resizeAnimation.Completed += (s, e) => SizeToContent = SizeToContent.WidthAndHeight;

            Navigating += delegate(object sender, NavigatingCancelEventArgs e)
                              {
                                  // FIX (jods): prevent further navigation when a navigation is already in progress
                                  //						 (e.g. double-click a button in the main menu). This would break the transitions.
                                  if (_isInTransition)
                                  {
                                      e.Cancel = true;
                                      return;
                                  }

                                  if (_transitionTarget != null)
                                  {
                                      _transitionTarget = null;
                                      return;
                                  }

                                  var page = Content as Page;
                                  if (page == null) return;

                                  if (Math.Abs(_clientWidth - 0) < double.Epsilon)
                                  {
                                      _clientWidth = page.ActualWidth;
                                      _bordersHeight = ActualHeight - page.ActualHeight;
                                  }
                                  SizeToContent = SizeToContent.Manual;

                                  e.Cancel = true;
                                  _isInTransition = true;
                                  _transitionTarget = e.NavigationMode == NavigationMode.Back ? BackTarget : e.Content;
                                  page.BeginAnimation(OpacityProperty, _outAnimation, HandoffBehavior.SnapshotAndReplace);
                              };

            Navigated += delegate
                             {
                                 var page = Content as Page;
                                 if (page == null) return;

                                 if (_isFirstLoad)
                                 {
                                     _isFirstLoad = false;
                                     return;
                                 }

                                 page.Opacity = 0;
                                 page.Measure(new Size(_clientWidth, double.PositiveInfinity));

                                 _resizeAnimation.To = page.DesiredSize.Height + _bordersHeight;
                                 BeginAnimation(HeightProperty, _resizeAnimation);
                                 page.BeginAnimation(OpacityProperty, _inAnimation);
                             };
        }

        #endregion Animation

        #region Constructors

        public LauncherWindow()
        {
            Initialized += LauncherInitialized;
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

        public void LauncherInitialized(object sender, EventArgs e)
        {
            Top = Prefs.LoginLocation.Y;
            Left = Prefs.LoginLocation.X;
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
                Program.DebugWindow = new Windows.DWindow();
            }
            Program.DebugWindow.Visibility = Program.DebugWindow.Visibility == Visibility.Visible
                                                 ? Visibility.Hidden
                                                 : Visibility.Visible;
        }
    }
}