// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using log4net;
using Octgn.Extentions;
using Octgn.Utils;

namespace Octgn.Controls
{
    public class OctgnChrome2 : Window, IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Content Property

        public static new readonly DependencyProperty ContentProperty;

        public new object Content
        {
            get { return this.GetValue(ContentProperty); }
            set { this.SetValue(ContentProperty, value); }
        }

        private static void ContentChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var window = (OctgnChrome2)property;
            window._frame.Content = (UIElement)args.NewValue;
        }
        #endregion

        /// <summary>
        /// Gets or sets the minimize button visibility.
        /// </summary>
        public Visibility MinimizeButtonVisibility
        {
            get { return Visibility.Hidden; }
            set {  }
        }

        /// <summary>
        /// Gets or sets the min max button visibility.
        /// </summary>
        public Visibility MinMaxButtonVisibility
        {
            get { return Visibility.Hidden; }
            set {  }
        }

        /// <summary>
        /// Gets or sets the close button visibility.
        /// </summary>
        public Visibility CloseButtonVisibility
        {
            get { return Visibility.Hidden; }
            set {  }
        }

        private WindowFrame _frame { get; set; }

        static OctgnChrome2()
        {
            // this checks whether application runs in design mode or not; if not the DependencyProperties are initialized
            if (!ControlExtensions.IsInDesignMode())
            {
                ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(OctgnChrome2), new UIPropertyMetadata(null, ContentChangedCallback));
                //BackgroundProperty = DependencyProperty.Register("Background", typeof(object), typeof(OctgnChrome), new UIPropertyMetadata(Brushes.Transparent, BackgroundChangedCallback));
            }
        }

        public OctgnChrome2()
        {
            // Used to show diagnostics window
            //this.PreviewKeyUp += OnPreviewKeyUp;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.BorderThickness = new Thickness(0);
            this.AllowsTransparency = false;
            this.SourceInitialized += new EventHandler(win_SourceInitialized);
            if (!this.IsInDesignMode())
            {
                var bimage = new BitmapImage(new Uri("pack://application:,,,/Resources/background.png"));

                var ib = new ImageBrush(bimage);
                ib.Stretch = Stretch.Fill;
                this.Background = ib;
                //this.MainBorder.Background = ib;

                //Used to change transparency && background image
                //Program.OnOptionsChanged += ProgramOnOnOptionsChanged;
                //SubscriptionModule.Get().IsSubbedChanged += OnIsSubbedChanged;
            }

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            try
            {
                var curApp = System.Windows.Application.Current;
                var curMainWindow = curApp.MainWindow;
                var mainWindow = WindowManager.Main ?? curMainWindow;
                if (mainWindow != null && mainWindow.Owner == null && !Equals(mainWindow, this) && mainWindow.IsVisible)
                {
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                    this.Left = mainWindow.Left + 10;
                    this.Top = mainWindow.Top + 10;
                }

            }
            catch (Exception e)
            {
                Log.Warn("Error setting window position", e);
            }

            _frame = new WindowFrame();
            base.Content = _frame;

            this.Loaded += OnLoaded;
            this.LocationChanged += OnLocationChanged;
        }

        #region Used for size and location of window
        private void win_SourceInitialized(object sender, EventArgs e)
        {
            System.IntPtr handle = (new System.Windows.Interop.WindowInteropHelper(this)).Handle;
            System.Windows.Interop.HwndSource.FromHwnd(handle).AddHook(WindowProc);
        }

        private void OnLocationChanged(object sender, EventArgs eventArgs)
        {
            var myRec = new System.Drawing.Rectangle((int)this.Left + 50, (int)this.Top + 50, (int)this.Width - 50, (int)this.Height - 50);
            var screens = Screen.AllScreens;
            if (!screens.Any(x => x.Bounds.IntersectsWith(myRec)))
            {
                var bounds = Screen.PrimaryScreen.Bounds;
                this.Left = ((bounds.Right - bounds.Left) / 2) + (Width / 2);
                this.Top = ((bounds.Bottom - bounds.Top) / 2) + (Height / 2);
            }
            System.IntPtr handle = (new WindowInteropHelper(this)).Handle;
            //WinInterop.HwndSource.FromHwnd(handle).AddHook(new WinInterop.HwndSourceHook(WindowProc));
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var myRec = new System.Drawing.Rectangle((int)this.Left + 50, (int)this.Top + 50, (int)this.Width - 50, (int)this.Height - 50);
            var screens = Screen.AllScreens;
            if (!screens.Any(x => x.Bounds.IntersectsWith(myRec)))
            {
                var bounds = Screen.PrimaryScreen.Bounds;
                this.Left = ((bounds.Right - bounds.Left) / 2) + (Width / 2);
                this.Top = ((bounds.Bottom - bounds.Top) / 2) + (Height / 2);
            }
            //ProgramOnOnOptionsChanged();
        }

        private System.IntPtr WindowProc(System.IntPtr hwnd, int msg, System.IntPtr wParam, System.IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:/* WM_GETMINMAXINFO */
                    Win32.WmGetMinMaxInfo(this, hwnd, lParam);
                    handled = true;
                    break;
            }

            return (System.IntPtr)0;
        }
        #endregion


        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Dispose();
        }

        public void Dispose()
        {
            this.SourceInitialized -= win_SourceInitialized;
            //this.PreviewKeyUp -= OnPreviewKeyUp;
            this.Loaded -= OnLoaded;
            this.LocationChanged -= OnLocationChanged;
            try
            {
                var handle = (new System.Windows.Interop.WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).RemoveHook(WindowProc);

            }
            catch (Exception e)
            {
                Log.Error("Dispose Unhook Error", e);
            }
            //this.LabelTitle.MouseDown -= this.BorderMouseDown1;
            //this.WindowMinimizeButton.MouseEnter -= this.WindowControlMouseEnter;
            //this.WindowMinimizeButton.MouseLeave -= this.WindowControlMouseLeave;
            //this.WindowResizeButton.MouseEnter -= this.WindowControlMouseEnter;
            //this.WindowResizeButton.MouseLeave -= this.WindowControlMouseLeave;
            //this.WindowCloseButton.MouseEnter -= this.WindowControlMouseEnter;
            //this.WindowCloseButton.MouseLeave -= this.WindowControlMouseLeave;
            //recTopLeft.MouseDown -= this.DragMouseDown;
            //recTop.MouseDown -= this.DragMouseDown;
            //recTopRight.MouseDown -= this.DragMouseDown;
            //recLeft.MouseDown -= this.DragMouseDown;
            //recRight.MouseDown -= this.DragMouseDown;
            //recBottomLeft.MouseDown -= this.DragMouseDown;
            //recBottom.MouseDown -= this.DragMouseDown;
            //recBottomRight.MouseDown -= this.DragMouseDown;
        }
    }
}