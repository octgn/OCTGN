using System.Reflection;
using log4net;

namespace Octgn.Utils
{
    using System;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Threading;

    public class MouseClickHandler
    {
        internal static ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly DispatcherTimer clickWaitTimer;
        private readonly Dispatcher dispatcher;

        private Action<MouseButtonEventArgs> click;
        private readonly Action<MouseButtonEventArgs> doubleClick;

        private int invalidateNextUpEvent;
        private int autoFireCount;

        private MouseButtonEventArgs lastArgs;

        public MouseClickHandler(Dispatcher dispatcher, Action<MouseButtonEventArgs> onClick, Action<MouseButtonEventArgs> onDoubleClick)
        {
            this.dispatcher = dispatcher;
            this.clickWaitTimer = 
                new DispatcherTimer(TimeSpan.FromMilliseconds(150)
                , DispatcherPriority.Input, TimerTick, this.dispatcher);
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

            while (autoFireCount > 0)
            {
                TimerTick(null, null);
                autoFireCount--;
                if(autoFireCount == 0)
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

        public void AutoFireNext()
        {
            invalidateNextUpEvent = 0;
            autoFireCount++;
        }

        private void TimerTick(object sender, EventArgs eventArgs)
        {
            try
            {
                clickWaitTimer.Stop();
                this.dispatcher.Invoke(new Action(() => this.click.Invoke(this.lastArgs)));

            }
            catch (Exception e)
            {
                Log.Warn("TimerTick",e);
            }
        }
    }
}