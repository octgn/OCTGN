namespace Octgn.Utils
{
    using System;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Threading;

    public class MouseClickHandler
    {
        private readonly DispatcherTimer clickWaitTimer;
        private readonly Dispatcher dispatcher;

        private Action<MouseButtonEventArgs> click;
        private readonly Action<MouseButtonEventArgs> doubleClick;

        private int invalidateNextUpEvent;
        private bool dragging;

        private MouseButtonEventArgs lastArgs;

        public MouseClickHandler(Dispatcher dispatcher, Action<MouseButtonEventArgs> onClick, Action<MouseButtonEventArgs> onDoubleClick)
        {
            this.dispatcher = dispatcher;
            this.clickWaitTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(SystemInformation.DoubleClickTime), DispatcherPriority.Background, TimerTick
                , this.dispatcher);
            this.clickWaitTimer.Stop();
            click = onClick;
            doubleClick = onDoubleClick;
        }

        public void OnMouseUp(MouseButtonEventArgs args)
        {
            if (Interlocked.CompareExchange(ref invalidateNextUpEvent, 0, 1) == 1)
            {
                return;
            }
            lastArgs = args;
            if (dragging)
            {
                dragging = false;
                TimerTick(null, null);
                return;
            }
            clickWaitTimer.Start();
        }

        public void OnDoubleClick(MouseButtonEventArgs args)
        {
            invalidateNextUpEvent = 1;
            args.Handled = true;
            clickWaitTimer.Stop();
            this.dispatcher.Invoke(new Action(()=>this.doubleClick(args)));
        }

        public void StartDrag()
        {
            dragging = true;
        }

        private void TimerTick(object sender, EventArgs eventArgs)
        {
            clickWaitTimer.Stop();
            this.dispatcher.Invoke(new Action(() => this.click.Invoke(this.lastArgs)));
        }
    }
}