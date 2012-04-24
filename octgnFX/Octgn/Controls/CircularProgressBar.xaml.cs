using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Octgn.Controls
{
    /// <summary>
    ///   A circular type progress bar, that is simliar to popular web based progress bars
    /// </summary>
    public partial class CircularProgressBar : INotifyPropertyChanged
    {
        #region Data

        private readonly DispatcherTimer _animationTimer;

        #endregion Data

        #region Constructor

        public CircularProgressBar()
        {
            InitializeComponent();
            C0.Fill = Foreground;
            C1.Fill = Foreground;
            C2.Fill = Foreground;
            C3.Fill = Foreground;
            C4.Fill = Foreground;
            C5.Fill = Foreground;
            C6.Fill = Foreground;
            C7.Fill = Foreground;
            C8.Fill = Foreground;
            _animationTimer = new DispatcherTimer(
                DispatcherPriority.ContextIdle, Dispatcher) {Interval = new TimeSpan(0, 0, 0, 0, 100)};
        }

        #endregion Constructor

        #region Private Methods

        private void Start()
        {
            _animationTimer.Tick += HandleAnimationTick;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _animationTimer.Start();
            }
        }

        private void Stop()
        {
            _animationTimer.Stop();
            _animationTimer.Tick -= HandleAnimationTick;
        }

        private void HandleAnimationTick(object sender, EventArgs e)
        {
            SpinnerRotate.Angle = (SpinnerRotate.Angle + 30)%360;
        }

        private void SetPositions()
        {
            const double offset = Math.PI;
            const double step = Math.PI*2/10.0;
            double ew = Width/6;
            double eh = Height/6;
            double w = Width/2;
            double h = Height/2;
            double m = Math.Min(w, h);
            double r = 4*m/5;

            SetPosition(C0, offset, 0.0, step, ew, eh, w, h, r);
            SetPosition(C1, offset, 1.0, step, ew, eh, w, h, r);
            SetPosition(C2, offset, 2.0, step, ew, eh, w, h, r);
            SetPosition(C3, offset, 3.0, step, ew, eh, w, h, r);
            SetPosition(C4, offset, 4.0, step, ew, eh, w, h, r);
            SetPosition(C5, offset, 5.0, step, ew, eh, w, h, r);
            SetPosition(C6, offset, 6.0, step, ew, eh, w, h, r);
            SetPosition(C7, offset, 7.0, step, ew, eh, w, h, r);
            SetPosition(C8, offset, 8.0, step, ew, eh, w, h, r);
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            SetPositions();
        }

        private static void SetPosition(FrameworkElement ellipse, double offset,
                                        double posOffSet, double step, double ew, double eh, double w, double h,
                                        double r)
        {
            double t = 2*Math.PI*posOffSet/10;
            ellipse.Width = ew;
            ellipse.Height = eh;

            ellipse.SetValue(Canvas.LeftProperty, w + r*Math.Cos(t));
            ellipse.SetValue(Canvas.TopProperty, h + r*Math.Sin(t));

            //ellipse.SetValue(Canvas.LeftProperty,w
            //    + Math.Sin(offset + posOffSet * step) * w);

            //ellipse.SetValue(Canvas.TopProperty, h
            //    + Math.Cos(offset + posOffSet * step) * h);
        }

        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void HandleVisibleChanged(object sender,
                                          DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool) e.NewValue;

            if (isVisible)
                Start();
            else
                Stop();
        }

        #endregion Private Methods

        private void UserControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetPositions();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}