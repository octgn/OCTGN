// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using log4net;
using Octgn.Extentions;
using Octgn.Utils;
using Octgn.Annotations;

namespace Octgn.Controls
{
    public class OctgnChrome2 : Window, IDisposable, INotifyPropertyChanged
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

        public bool MinimizeButtonVisible { get { return _frame.MinimizeButtonVisible; } set { _frame.MinimizeButtonVisible = value; } }
        public bool ResizeButtonVisible { get { return _frame.ResizeButtonVisible; } set { _frame.ResizeButtonVisible = value; } }
        public bool CloseButtonVisible { get { return _frame.CloseButtonVisible; } set { _frame.CloseButtonVisible = value; } }
        public bool CanResize { get { return _frame.CanResize; } set { _frame.CanResize = value; } }

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
            base.ResizeMode = ResizeMode.CanMinimize;
            this.BorderThickness = new Thickness(0);
            this.AllowsTransparency = false;
            this.SourceInitialized += win_SourceInitialized;
            if (!this.IsInDesignMode())
            {
                //var bimage = new BitmapImage(new Uri("pack://application:,,,/Resources/background.png"));
                //this.Background = ib;

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
            var titleBinding = new System.Windows.Data.Binding();
            titleBinding.Source = this;
            titleBinding.Path = new PropertyPath("Title");
            titleBinding.Mode = BindingMode.OneWay;
            titleBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            _frame.SetBinding(WindowFrame.TitleProperty, titleBinding);

            var iconBinding = new System.Windows.Data.Binding();
            iconBinding.Source = this;
            iconBinding.Path = new PropertyPath("Icon");
            iconBinding.Mode = BindingMode.OneWay;
            iconBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            _frame.SetBinding(WindowFrame.IconProperty, iconBinding);

            //TODO These should be replaced with custom event handlers in WindowFrame
            // Shouldn't be calling the elements directly like this
            _frame.MinimizeButton.Click += MinimizeButtonOnClick;
            _frame.ResizeButton.Click += ResizeButtonOnClick;
            _frame.CloseButton.Click += CloseButtonOnClick;
            _frame.DragBorder.MouseDown += FrameOnMouseDown;
            _frame.Resize += FrameOnResize;

            if (this.IsInDesignMode() == false)
            {
                var bimage =
                    new BitmapImage(new Uri("pack://application:,,,/Resources/backtile.png"));

                var ib = new ImageBrush(bimage);
                ib.Stretch = Stretch.None;
                ib.TileMode = TileMode.Tile;
                ib.ViewportUnits = BrushMappingMode.Absolute;
                ib.Viewport = new Rect(0, 0, bimage.PixelWidth, bimage.PixelHeight);
                this._frame.Background = ib;
            this.Loaded += OnLoaded;
            this.LocationChanged += OnLocationChanged;
        }
        }

        private void FrameOnResize(Win32.ResizeDirection resizeDirection)
        {
            Win32.ResizeWindow(this, resizeDirection);
        }

        private void FrameOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.LeftButton != MouseButtonState.Pressed) return;
            if (mouseButtonEventArgs.MiddleButton == MouseButtonState.Pressed || mouseButtonEventArgs.RightButton == MouseButtonState.Pressed) return;
            this.DragMove();
        }

        private void CloseButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Close();
        }

        private void ResizeButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.WindowState = this.WindowState != WindowState.Maximized ? WindowState.Maximized : WindowState.Normal;
        }

        private void MinimizeButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.WindowState = WindowState.Minimized;
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


        protected override void OnClosing( CancelEventArgs e ) {
            base.OnClosing( e );
            Dispose();
        }

        public void Dispose() {
            SourceInitialized -= win_SourceInitialized;
            //this.PreviewKeyUp -= OnPreviewKeyUp;
            Loaded -= OnLoaded;
            LocationChanged -= OnLocationChanged;
            if( _frame != null ) {
                _frame.MinimizeButton.Click -= MinimizeButtonOnClick;
                _frame.ResizeButton.Click -= ResizeButtonOnClick;
                _frame.CloseButton.Click -= CloseButtonOnClick;
                _frame.Resize -= FrameOnResize;
                _frame.DragBorder.MouseDown -= FrameOnMouseDown;
            }
            var handle = new WindowInteropHelper( this ).Handle;
            HwndSource.FromHwnd( handle ).RemoveHook( WindowProc );
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}