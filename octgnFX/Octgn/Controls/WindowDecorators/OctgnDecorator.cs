/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Octgn.Core;
using Octgn.Extentions;

using Binding = System.Windows.Data.Binding;
using Cursors = System.Windows.Input.Cursors;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Orientation = System.Windows.Controls.Orientation;
using WinInterop = System.Windows.Interop;

namespace Octgn.Controls.WindowDecorators
{

    /// <summary>
    /// Default decorator for the OCTGN client windows
    /// <remarks>This class is basically the port of the OctgnChrome class as a WindowDecorator</remarks>
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. This rule fucks up using regions usefully."), SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Reviewed. Suppression is OK here.")]
    public class OctgnDecorator : WindowDecorator, IDisposable
    {
        #region Interop Junk

        /// <summary>
        /// Some value that means something.
        /// </summary>
        private const int WmSyscommand = 0x112;

        [DllImportAttribute("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr windowHandle, uint msg, IntPtr windowParam, IntPtr lparam);

        /// <summary>
        /// The resize direction for resizing the window.
        /// </summary>
        private enum ResizeDirection
        {
            /// <summary>
            /// The left.
            /// </summary>
            Left = 1,

            /// <summary>
            /// The right.
            /// </summary>
            Right = 2,

            /// <summary>
            /// The top.
            /// </summary>
            Top = 3,

            /// <summary>
            /// The top left.
            /// </summary>
            TopLeft = 4,

            /// <summary>
            /// The top right.
            /// </summary>
            TopRight = 5,

            /// <summary>
            /// The bottom.
            /// </summary>
            Bottom = 6,

            /// <summary>
            /// The bottom left.
            /// </summary>
            BottomLeft = 7,

            /// <summary>
            /// The bottom right.
            /// </summary>
            BottomRight = 8
        }

        /// <summary>
        /// Resizes the window in a given direction.
        /// </summary>
        /// <param name="direction">
        /// The direction.
        /// </param>
        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(new WindowInteropHelper(Decorated).Handle, WmSyscommand, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        #endregion

        #region private accessors

        /// <summary>
        /// Gets or sets the main border.
        /// </summary>
        private Border MainBorder { get; set; }

        /// <summary>
        /// UI element holding the content of the window
        /// </summary>
        private AdornerDecorator ContentArea { get; set; }

        /// <summary>
        /// Gets or sets the drag grid.
        /// </summary>
        private Grid DragGrid { get; set; }

        /// <summary>
        /// Gets or sets the main grid.
        /// </summary>
        private Grid MainGrid { get; set; }

        /// <summary>
        /// Gets or sets the label title.
        /// </summary>
        private TextBlock LabelTitle { get; set; }

        /// <summary>
        /// Gets or sets the Window Controls grid.
        /// </summary>
        private StackPanel WcGrid { get; set; }

        /// <summary>
        /// Gets or sets the icon image.
        /// </summary>
        private Image IconImage { get; set; }

        /// <summary>
        /// Gets or sets the window minimize button.
        /// </summary>
        private Border WindowMinimizeButton { get; set; }

        /// <summary>
        /// Gets or sets the window resize button.
        /// </summary>
        private Border WindowResizeButton { get; set; }

        /// <summary>
        /// Gets or sets the window close button.
        /// </summary>
        private Border WindowCloseButton { get; set; }

        /// <summary>
        /// The title bar row
        /// </summary>
        private RowDefinition TitleRow { get; set; }

        private Border IconBorder { get; set; }

        /// <summary>
        /// The window control hover brush.
        /// </summary>
        private readonly Brush windowControlHoverBrush = new SolidColorBrush(Colors.DodgerBlue);

        /// <summary>
        /// The window control off brush.
        /// </summary>
        private readonly Brush windowControlOffBrush = new SolidColorBrush(Colors.Transparent);

        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="OctgnDecorator"/> class.
        /// </summary>
        public OctgnDecorator(DecorableWindow decoratedWindow) : base(decoratedWindow)
        {
            this.MainBorder = GetContainer();
            this.ContentArea = GetContentArea();
        }
        public override void Apply()
        {
            
            this.MainBorder.Background = Brushes.Transparent;
            Decorated.WindowStyle = WindowStyle.None;
            Decorated.SourceInitialized += new EventHandler(win_SourceInitialized);
            if (!Decorated.IsInDesignMode())
            {
                if (Prefs.UseWindowTransparency)
                {
                    Decorated.AllowsTransparency = true;
                    Decorated.WindowBackground = Brushes.Transparent;
                    //decorated.MainBorder.SetResourceReference(Border.BackgroundProperty, "ControlBackgroundBrush");
                    this.MainBorder.BorderThickness = new Thickness(2);
                    this.MainBorder.CornerRadius = new CornerRadius(5);
                    this.MainBorder.BorderBrush = new LinearGradientBrush(
                        Color.FromArgb(40, 30, 30, 30), Color.FromArgb(150, 200, 200, 200), 45);
                }
                else
                {
                    if (Decorated.AllowsTransparency)
                    {
                        Decorated.AllowsTransparency = false;
                    }
                    Decorated.WindowBackground = new SolidColorBrush(Color.FromRgb(64, 64, 64));
                }

                var bimage = new BitmapImage(new Uri("pack://application:,,,/Resources/background.png"));

                var ib = new ImageBrush(bimage);
                ib.Stretch = Stretch.Fill;
                this.MainBorder.Background = ib;

            }


            this.MakeDrag();


            this.MainGrid = new Grid();
            this.TitleRow = new RowDefinition { Height = new GridLength(35) };
            this.MainGrid.RowDefinitions.Add(this.TitleRow);
            this.MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100, GridUnitType.Star) });
            this.MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            this.MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Star) });
            this.MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            this.DragGrid.Children.Add(this.MainGrid);
            Grid.SetColumn(this.MainGrid, 1);
            Grid.SetRow(this.MainGrid, 1);


            IconBorder = new Border();
            IconBorder.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
            IconBorder.CornerRadius = new CornerRadius(5, 0, 0, 0);
            IconBorder.Padding = new Thickness(5, 2, 5, 2);
            IconBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.IconBorder.MouseDown += this.BorderMouseDown1;
            this.MainGrid.Children.Add(IconBorder);
            Grid.SetColumnSpan(IconBorder, 2);

            var iconsp = new StackPanel();
            iconsp.Orientation = Orientation.Horizontal;
            iconsp.HorizontalAlignment = HorizontalAlignment.Stretch;
            IconBorder.Child = iconsp;



            // IconImage = new Image{Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/Icon.ico")) };
            this.IconImage = new Image();
            this.IconImage.Stretch = Stretch.Uniform;
            this.IconImage.Source = Decorated.Icon;
            this.IconImage.VerticalAlignment = VerticalAlignment.Center;
            this.IconImage.HorizontalAlignment = HorizontalAlignment.Center;
            //decorated.MainGrid.Children.Add(decorated.IconImage);
            iconsp.Children.Add(this.IconImage);
            iconsp.Children.Add(new Border { Width = 20 });


            // Add label
            this.LabelTitle = new TextBlock();
            //decorated.LabelTitle.FontFamily = new FontFamily("Euphemia");
            this.LabelTitle.FontSize = 20;
            this.LabelTitle.VerticalAlignment = VerticalAlignment.Center;
            this.LabelTitle.Foreground = new SolidColorBrush(Color.FromRgb(248, 248, 248));
            this.LabelTitle.FontWeight = FontWeights.Bold;
            this.LabelTitle.Effect = new DropShadowEffect()
            {
                BlurRadius = 5,
                Color = Color.FromRgb(64, 64, 64),
                //Color = Colors.DodgerBlue,
                Direction = 0,
                Opacity = .9,
                ShadowDepth = 0,
                RenderingBias = RenderingBias.Performance
            };
            //decorated.LabelTitle.FontStyle = FontStyles.Italic;
            this.LabelTitle.DataContext = Decorated;
            this.LabelTitle.SetBinding(TextBlock.TextProperty, new Binding("Title") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            //this.MainGrid.Children.Add(this.LabelTitle);
            //Grid.SetColumn(this.LabelTitle, 1);
            iconsp.Children.Add(this.LabelTitle);

            // Setup content area

            this.MainGrid.Children.Add(this.ContentArea);
            Grid.SetRow(this.ContentArea, 1);
            Grid.SetColumnSpan(this.ContentArea, 3);

            // Add window controls
            var wcborder = new Border();
            wcborder.Background = new SolidColorBrush(Color.FromArgb(200, 64, 64, 64));
            wcborder.CornerRadius = new CornerRadius(0, 5, 0, 0);
            //wcborder.Padding = new Thickness(5, 2, 5, 2);
            wcborder.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.MainGrid.Children.Add(wcborder);
            Grid.SetColumn(wcborder, 2);

            this.WcGrid = new StackPanel();
            this.WcGrid.Orientation = Orientation.Horizontal;
            //this.WcGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0,GridUnitType.Auto) });
            //this.WcGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0,GridUnitType.Auto) });
            //this.WcGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(le.NaN,GridUnitType.Auto) });
            //this.MainGrid.Children.Add(this.WcGrid);
            //Grid.SetColumn(this.WcGrid, 2);
            wcborder.Child = this.WcGrid;

            this.WindowMinimizeButton = new Border();
            this.WindowMinimizeButton.MouseEnter += this.WindowControlMouseEnter;
            this.WindowMinimizeButton.MouseLeave += this.WindowControlMouseLeave;
            this.WindowMinimizeButton.Focusable = true;
            this.WindowMinimizeButton.Cursor = Cursors.Hand;
            this.WindowMinimizeButton.Width = 40;
            this.WindowMinimizeButton.PreviewMouseLeftButtonUp += (sender, args) =>
                                                {
                                                    Decorated.WindowState = WindowState.Minimized;
                                                    args.Handled = true;
                                                };
            this.WindowMinimizeButton.Child = new Image() { Stretch = Stretch.None, Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/minimize.png")) };
            this.WcGrid.Children.Add(this.WindowMinimizeButton);
            Grid.SetColumn(this.WindowMinimizeButton, 0);

            this.WindowResizeButton = new Border();
            this.WindowResizeButton.MouseEnter += this.WindowControlMouseEnter;
            this.WindowResizeButton.MouseLeave += this.WindowControlMouseLeave;
            this.WindowResizeButton.Focusable = true;
            this.WindowResizeButton.Cursor = Cursors.Hand;
            this.WindowResizeButton.Width = 40;
            this.WindowResizeButton.PreviewMouseLeftButtonUp += (sender, args) =>
                                                               {
                                                                   Decorated.WindowState = (Decorated.WindowState == WindowState.Maximized)
                                                                                     ? WindowState.Normal
                                                                                     : WindowState.Maximized;
                                                                   args.Handled = true;
                                                               };
            this.WindowResizeButton.Child = new Image() { Stretch = Stretch.None, Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/minmax.png")) };
            this.WcGrid.Children.Add(this.WindowResizeButton);
            Grid.SetColumn(this.WindowResizeButton, 1);

            this.WindowCloseButton = new Border();
            this.WindowCloseButton.CornerRadius = new CornerRadius(0, 5, 0, 0);
            this.WindowCloseButton.MouseEnter += this.WindowControlMouseEnter;
            this.WindowCloseButton.MouseLeave += this.WindowControlMouseLeave;
            this.WindowCloseButton.Focusable = true;
            this.WindowCloseButton.Cursor = Cursors.Hand;
            this.WindowCloseButton.Width = 40;
            this.WindowCloseButton.PreviewMouseLeftButtonUp += (sender, args) =>
                                                              {
                                                                  Decorated.Close();
                                                                  args.Handled = true;
                                                              };
            this.WindowCloseButton.Child = new Image() { Stretch = Stretch.None, Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/closewindow.png")) };
            this.WcGrid.Children.Add(this.WindowCloseButton);
            Grid.SetColumn(this.WindowCloseButton, 2);


            Decorated.PropertyChanged += DecoratedOnPropertyChanged;
            DecoratedOnPropertyChanged(Decorated, new PropertyChangedEventArgs("TitleBarVisibility"));

        }

        public override bool Undo()
        {
            try
            {
                Decorated.WindowStyle = WindowStyle.SingleBorderWindow;
            }
            catch
            {
                //Window transparency is probably enabled and won't let us change the window style
                return false;
            }
            MainGrid.Children.Remove(ContentArea);
            MainBorder.Child = ContentArea;
            Decorated.Show();
            return true;
        }

        private void DecoratedOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case "WindowIcon":
                    this.IconImage.Source = Decorated.Icon;
                    break;
                case "MinimizeButtonVisibility":
                    this.WindowMinimizeButton.Visibility = Decorated.MinimizeButtonVisibility;
                    break;
                case "MinMaxButtonVisibility":
                    this.WindowResizeButton.Visibility = Decorated.MinMaxButtonVisibility;
                    break;
                case "CloseButtonVisibility":
                    this.WindowCloseButton.Visibility = Decorated.CloseButtonVisibility;
                    break;
                case "TitleBarVisibility":
                    switch (Decorated.TitleBarVisibility)
                    {
                        case Visibility.Visible:
                            this.TitleRow.Height = new GridLength(35);
                            break;
                        case Visibility.Hidden:
                            this.TitleRow.Height = new GridLength(0);
                            break;
                        case Visibility.Collapsed:
                            this.TitleRow.Height = new GridLength(0);
                            break;
                    }
                    break;
            }
        }

        private void win_SourceInitialized(object sender, EventArgs e)
        {
            System.IntPtr handle = (new WinInterop.WindowInteropHelper(Decorated)).Handle;
            WinInterop.HwndSource.FromHwnd(handle).AddHook(WindowProc);
        }

        /// <summary>
        /// The window control mouse leave.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="mouseEventArgs">
        /// The mouse event arguments.
        /// </param>
        private void WindowControlMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            var s = sender as Border;
            s.Background = this.windowControlOffBrush;
        }

        /// <summary>
        /// The window control mouse enter.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="mouseEventArgs">
        /// The mouse event arguments.
        /// </param>
        private void WindowControlMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            var s = sender as Border;
            if (s != null)
            {
                s.Background = this.windowControlHoverBrush;
            }
        }

        private Rectangle recTopLeft;
        private Rectangle recTop;
        private Rectangle recTopRight;
        private Rectangle recLeft;
        private Rectangle recRight;
        private Rectangle recBottomLeft;
        private Rectangle recBottom;
        private Rectangle recBottomRight;

        /// <summary>
        /// Makes the resize area around the window.
        /// </summary>
        private void MakeDrag()
        {
            this.DragGrid = new Grid();
            this.DragGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(8) });
            this.DragGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100, GridUnitType.Star) });
            this.DragGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(8) });
            this.DragGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8) });
            this.DragGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Star) });
            this.DragGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8) });

            recTopLeft = new Rectangle { Cursor = Cursors.SizeNWSE, Name = "dTopLeft", Fill = Brushes.Transparent };
            recTopLeft.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recTopLeft);

            recTop = new Rectangle { Cursor = Cursors.SizeNS, Name = "dTop", Fill = Brushes.Transparent };
            recTop.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recTop);
            Grid.SetColumn(recTop, 1);

            recTopRight = new Rectangle { Cursor = Cursors.SizeNESW, Name = "dTopRight", Fill = Brushes.Transparent };
            recTopRight.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recTopRight);
            Grid.SetColumn(recTopRight, 2);

            recLeft = new Rectangle { Cursor = Cursors.SizeWE, Name = "dLeft", Fill = Brushes.Transparent };
            recLeft.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recLeft);
            Grid.SetRow(recLeft, 1);

            recRight = new Rectangle { Cursor = Cursors.SizeWE, Name = "dRight", Fill = Brushes.Transparent };
            recRight.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recRight);
            Grid.SetRow(recRight, 1);
            Grid.SetColumn(recRight, 2);

            recBottomLeft = new Rectangle { Cursor = Cursors.SizeNESW, Name = "dBottomLeft", Fill = Brushes.Transparent };
            recBottomLeft.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recBottomLeft);
            Grid.SetRow(recBottomLeft, 2);

            recBottom = new Rectangle { Cursor = Cursors.SizeNS, Name = "dBottom", Fill = Brushes.Transparent };
            recBottom.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recBottom);
            Grid.SetColumn(recBottom, 1);
            Grid.SetRow(recBottom, 2);

            recBottomRight = new Rectangle { Cursor = Cursors.SizeNWSE, Name = "dBottomRight", Fill = Brushes.Transparent };
            recBottomRight.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recBottomRight);
            Grid.SetColumn(recBottomRight, 2);
            Grid.SetRow(recBottomRight, 2);

            this.MainBorder.Child = this.DragGrid;
        }

        /// <summary>
        /// When the mouse goes down on the resize area.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="mouseButtonEventArgs">
        /// The mouse button event args.
        /// </param>
        private void DragMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (!Decorated.CanResize || Decorated.WindowState == WindowState.Maximized)
            {
                return;
            }

            if (mouseButtonEventArgs.LeftButton != MouseButtonState.Pressed) return;
            if (mouseButtonEventArgs.MiddleButton == MouseButtonState.Pressed || mouseButtonEventArgs.RightButton == MouseButtonState.Pressed) return;

            var s = sender as Rectangle;
            if (s == null)
            {
                return;
            }
            switch (s.Name)
            {
                case "dTopLeft":
                    this.ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "dTop":
                    this.ResizeWindow(ResizeDirection.Top);
                    break;
                case "dTopRight":
                    this.ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "dLeft":
                    this.ResizeWindow(ResizeDirection.Left);
                    break;
                case "dRight":
                    this.ResizeWindow(ResizeDirection.Right);
                    break;
                case "dBottomLeft":
                    this.ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                case "dBottom":
                    this.ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "dBottomRight":
                    this.ResizeWindow(ResizeDirection.BottomRight);
                    break;
            }
        }

        /// <summary>
        /// Mouse down event on the main border.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private void BorderMouseDown1(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (e.MiddleButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed) return;
            Decorated.DragMove();
        }

        private System.IntPtr WindowProc(System.IntPtr hwnd, int msg, System.IntPtr wParam, System.IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:/* WM_GETMINMAXINFO */
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (System.IntPtr)0;
        }

        private void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {

            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            System.IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != System.IntPtr.Zero)
            {

                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                mmi.ptMinTrackSize.x = (int)Decorated.MinWidth;
                mmi.ptMinTrackSize.y = (int)Decorated.MinHeight;
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        /// <summary>
        /// POINT aka POINTAPI
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x;
            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y;

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };
        /// <summary>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            /// <summary>
            /// </summary>            
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            /// <summary>
            /// </summary>            
            public RECT rcMonitor = new RECT();

            /// <summary>
            /// </summary>            
            public RECT rcWork = new RECT();

            /// <summary>
            /// </summary>            
            public int dwFlags = 0;
        }


        /// <summary> Win32 </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            /// <summary> Win32 </summary>
            public int left;
            /// <summary> Win32 </summary>
            public int top;
            /// <summary> Win32 </summary>
            public int right;
            /// <summary> Win32 </summary>
            public int bottom;

            /// <summary> Win32 </summary>
            public static readonly RECT Empty = new RECT();

            /// <summary> Win32 </summary>
            public int Width
            {
                get { return Math.Abs(right - left); }  // Abs needed for BIDI OS
            }
            /// <summary> Win32 </summary>
            public int Height
            {
                get { return bottom - top; }
            }

            /// <summary> Win32 </summary>
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }


            /// <summary> Win32 </summary>
            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }

            /// <summary> Win32 </summary>
            public bool IsEmpty
            {
                get
                {
                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return left >= right || top >= bottom;
                }
            }
            /// <summary> Return a user friendly representation of this struct </summary>
            public override string ToString()
            {
                if (this == RECT.Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }

            /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }

            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }


            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
            }

            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }


        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        /// <summary>
        /// 
        /// </summary>
        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Decorated.SourceInitialized -= win_SourceInitialized;
            this.LabelTitle.MouseDown -= this.BorderMouseDown1;
            this.WindowMinimizeButton.MouseEnter -= this.WindowControlMouseEnter;
            this.WindowMinimizeButton.MouseLeave -= this.WindowControlMouseLeave;
            this.WindowResizeButton.MouseEnter -= this.WindowControlMouseEnter;
            this.WindowResizeButton.MouseLeave -= this.WindowControlMouseLeave;
            this.WindowCloseButton.MouseEnter -= this.WindowControlMouseEnter;
            this.WindowCloseButton.MouseLeave -= this.WindowControlMouseLeave;

            recTopLeft.MouseDown -= this.DragMouseDown;
            recTop.MouseDown -= this.DragMouseDown;
            recTopRight.MouseDown -= this.DragMouseDown;
            recLeft.MouseDown -= this.DragMouseDown;
            recRight.MouseDown -= this.DragMouseDown;
            recBottomLeft.MouseDown -= this.DragMouseDown;
            recBottom.MouseDown -= this.DragMouseDown;
            recBottomRight.MouseDown -= this.DragMouseDown;

        }

        #endregion

    }
}
