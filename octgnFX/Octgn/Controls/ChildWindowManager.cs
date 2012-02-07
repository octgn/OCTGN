using System.Windows.Controls;

namespace Octgn.Controls
{
    public class ChildWindowManager : Canvas
    {
        private ChildWindow topWindow;

        public void Show(ChildWindow wnd)
        {
            double x = (ActualWidth - wnd.Width)/2;
            double y = (ActualHeight - wnd.Height)/2;
            SetLeft(wnd, x);
            SetTop(wnd, y);
            Children.Add(wnd);
            Activate(wnd);
        }

        internal void Hide(ChildWindow wnd)
        {
            if (topWindow == wnd) topWindow = null;
            Children.Remove(wnd);
        }

        internal void Activate(ChildWindow wnd)
        {
            if (topWindow == wnd) return;
            if (topWindow != null) topWindow.SetValue(ZIndexProperty, 0);
            topWindow = wnd;
            wnd.SetValue(ZIndexProperty, 1);
        }
    }
}