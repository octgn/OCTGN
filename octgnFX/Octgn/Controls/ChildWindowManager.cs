using System;
using System.Windows.Controls;

namespace Octgn.Controls
{
    public class ChildWindowManager : Canvas
    {
        private ChildWindow _topWindow;

        public void Show(ChildWindow wnd)
        {
            double x = Math.Max((ActualWidth - wnd.Width)/2, 0);
            double y = Math.Max((ActualHeight - wnd.Height)/2, 0);
            SetLeft(wnd, x);
            SetTop(wnd, y);
            Children.Add(wnd);
            Activate(wnd);
        }

        internal void Hide(ChildWindow wnd)
        {
            if (_topWindow == wnd) _topWindow = null;
            Children.Remove(wnd);
        }

        internal void Activate(ChildWindow wnd)
        {
            if (_topWindow == wnd) return;
            if (_topWindow != null) _topWindow.SetValue(ZIndexProperty, 0);
            _topWindow = wnd;
            wnd.SetValue(ZIndexProperty, 1);
        }
    }
}