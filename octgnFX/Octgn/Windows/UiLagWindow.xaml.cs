using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using log4net;
using Octgn.Play;

namespace Octgn.Windows
{
    public partial class UiLagWindow
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _isClosing;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        public UiLagWindow()
        {
            InitializeComponent();
            this.Topmost = true;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public void ShowLagWindow(PlayWindow parent)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => { ShowLagWindow(parent); }));
                return;
            }
            Log.Info("Showing Lag Window");
            //double pwidth = 0;
            //double pheight = 0;
            //double pleft = 0;
            //double ptop = 0;
            //parent.Dispatcher.Invoke(new Action(() =>
            //{
            //    pwidth = parent.Width;
            //    pheight = parent.Height;
            //    pleft = parent.Left;
            //    ptop = parent.Top;
            //}));
            WindowInteropHelper helper = new WindowInteropHelper(this);
            helper.Owner = parent.Handle;

            RECT size;
            GetWindowRect(new HandleRef(this, parent.Handle), out size);

            //this.Owner = parent;
            this.Show();
            this.Left = ((size.Left + (size.Right - size.Left) / 2d)) - (this.ActualWidth / 2);
            this.Top = ((size.Top + (size.Bottom - size.Top) / 2d)) - (this.ActualHeight / 2);
        }

        public void HideLagWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(HideLagWindow));
                return;
            }
            Log.Info("Hiding Lag Window");
            this.Hide();
        }

        public void RealClose()
        {
            this._isClosing = true;
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_isClosing == false)
            {
                e.Cancel = true;
                return;
            }
            base.OnClosing(e);
        }
    }
}
