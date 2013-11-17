/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octgn.Controls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    using log4net;

    using Octgn.Core;
    using Octgn.Extentions;

    using Binding = System.Windows.Data.Binding;
    using Cursors = System.Windows.Input.Cursors;
    using HorizontalAlignment = System.Windows.HorizontalAlignment;
    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using MouseEventArgs = System.Windows.Input.MouseEventArgs;
    using Orientation = System.Windows.Controls.Orientation;
    using WinInterop = System.Windows.Interop;

    /// <summary>
    /// Interaction logic for OctgnChrome.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. This rule fucks up using regions usefully."), SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Reviewed. Suppression is OK here.")]
    public partial class OctgnChrome : Window, IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        
        #region Content Property

        /// <summary>
        /// The content property dependency property.
        /// </summary>
        public static new readonly DependencyProperty ContentProperty;

        /// <summary>
        /// Gets or sets Window content
        /// <remarks>Hides base window class 'Content' property</remarks>
        /// </summary>
        public new object Content
        {
            get { return this.GetValue(ContentProperty); }
            set { this.SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Called when 'Content' property changed
        /// </summary>
        /// <param name="property">Dependency object</param>
        /// <param name="args">Dependency Property changed arguments</param>
        private static void ContentChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var window = (OctgnChrome)property;
            window.ContentArea.Child = (UIElement)args.NewValue;
        }
        #endregion

        #region Background Property

        /// <summary>
        /// The background property dependency property.
        /// </summary>
        public static new readonly DependencyProperty BackgroundProperty;

        /// <summary>
        /// Gets or sets Window background
        /// <remarks>Hides base window class 'Background' property</remarks>
        /// </summary>
        public new Brush Background
        {
            get
            {
                return (Brush)GetValue(BackgroundProperty);
            }

            set
            {
                this.SetValue(BackgroundProperty, value);
                this.MainBorder.Background = value;
            }
        }

        /// <summary>
        /// Called when Background property changed.
        /// </summary>
        /// <param name="property">Dependency Object</param>
        /// <param name="args">The Arguments</param>
        private static void BackgroundChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var window = (OctgnChrome)property;
            window.MainBorder.Background = (Brush)args.NewValue;
        }

        #endregion

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
            SendMessage(new WindowInteropHelper(this).Handle, WmSyscommand, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the window icon.
        /// </summary>
        public ImageSource WindowIcon
        {
            get
            {
                return this.Icon;
            }

            set
            {
                this.Icon = value;
                this.IconImage.Source = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimize button visibility.
        /// </summary>
        public Visibility MinimizeButtonVisibility
        {
            get { return this.WindowMinimizeButton.Visibility; }
            set { this.WindowMinimizeButton.Visibility = value; }
        }

        /// <summary>
        /// Gets or sets the min max button visibility.
        /// </summary>
        public Visibility MinMaxButtonVisibility
        {
            get { return this.WindowResizeButton.Visibility; }
            set { this.WindowResizeButton.Visibility = value; }
        }

        /// <summary>
        /// Gets or sets the close button visibility.
        /// </summary>
        public Visibility CloseButtonVisibility
        {
            get { return this.WindowCloseButton.Visibility; }
            set { this.WindowCloseButton.Visibility = value; }
        }

        public Visibility TitleBarVisibility
        {
            get
            {
                return this.TitleRow.Height.Value > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            set
            {
                switch (value)
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
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this window can resize.
        /// </summary>
        public bool CanResize { get; set; }

        #endregion

        #region private accessors

        /// <summary>
        /// Gets or sets the main border.
        /// </summary>
        private Border MainBorder { get; set; }

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
        /// Gets or sets the content area.
        /// </summary>
        private Border ContentArea { get; set; }

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
        /// Initializes static members of the <see cref="OctgnChrome"/> class.
        /// </summary>
        static OctgnChrome()
        {
            // this checks whether application runs in design mode or not; if not the DependencyProperties are initialized
            if (!ControlExtensions.IsInDesignMode())
            {
                ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(OctgnChrome), new UIPropertyMetadata(null, ContentChangedCallback));
                BackgroundProperty = DependencyProperty.Register("Background", typeof(object), typeof(OctgnChrome), new UIPropertyMetadata(Brushes.Transparent, BackgroundChangedCallback));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OctgnChrome"/> class.
        /// </summary>
        public OctgnChrome()
        {
            this.PreviewKeyUp += OnPreviewKeyUp;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.CanResize;
            this.CanResize = true;
            this.MainBorder = new Border();
            this.SourceInitialized += new EventHandler(win_SourceInitialized);
            if (!this.IsInDesignMode())
            {
                if (Prefs.UseWindowTransparency)
                {
                    this.AllowsTransparency = true;
                    base.Background = Brushes.Transparent;
                    //this.MainBorder.SetResourceReference(Border.BackgroundProperty, "ControlBackgroundBrush");
                    this.MainBorder.BorderThickness = new Thickness(2);
                    this.MainBorder.CornerRadius = new CornerRadius(5);
                    this.MainBorder.BorderBrush = new LinearGradientBrush(
                        Color.FromArgb(40, 30, 30, 30), Color.FromArgb(150, 200, 200, 200), 45);
                }
                else
                {
                    this.AllowsTransparency = false;
                    base.Background = new SolidColorBrush(Color.FromRgb(64, 64, 64));
                }

                var bimage = new BitmapImage(new Uri("pack://application:,,,/Resources/background.png"));

                var ib = new ImageBrush(bimage);
                ib.Stretch = Stretch.Fill;
                this.MainBorder.Background = ib;

                Program.OnOptionsChanged += ProgramOnOnOptionsChanged;
                SubscriptionModule.Get().IsSubbedChanged += OnIsSubbedChanged;
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
            base.Content = this.MainBorder;

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
            this.IconImage.Source = this.Icon;
            this.IconImage.VerticalAlignment = VerticalAlignment.Center;
            this.IconImage.HorizontalAlignment = HorizontalAlignment.Center;
            //this.MainGrid.Children.Add(this.IconImage);
            iconsp.Children.Add(this.IconImage);
            iconsp.Children.Add(new Border { Width = 20 });


            // Add label
            this.LabelTitle = new TextBlock();
            //this.LabelTitle.FontFamily = new FontFamily("Euphemia");
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
            //this.LabelTitle.FontStyle = FontStyles.Italic;
            this.LabelTitle.DataContext = this;
            this.LabelTitle.SetBinding(TextBlock.TextProperty, new Binding("Title") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            //this.MainGrid.Children.Add(this.LabelTitle);
            //Grid.SetColumn(this.LabelTitle, 1);
            iconsp.Children.Add(this.LabelTitle);

            // Setup content area
            this.ContentArea = new Border();
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
            this.WindowMinimizeButton.CornerRadius = new CornerRadius(0, 5, 0, 0);
            this.WindowMinimizeButton.MouseEnter += this.WindowControlMouseEnter;
            this.WindowMinimizeButton.MouseLeave += this.WindowControlMouseLeave;
            this.WindowMinimizeButton.Focusable = true;
            this.WindowMinimizeButton.Cursor = Cursors.Hand;
            this.WindowMinimizeButton.Width = 40;
            this.WindowMinimizeButton.PreviewMouseLeftButtonUp += (sender, args) =>
                                                {
                                                    WindowState = WindowState.Minimized;
                                                    args.Handled = true;
                                                };
            this.WindowMinimizeButton.Child = new Image() { Stretch = Stretch.None, Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/minimize.png")) };
            this.WcGrid.Children.Add(this.WindowMinimizeButton);
            Grid.SetColumn(this.WindowMinimizeButton, 0);

            this.WindowResizeButton = new Border();
            this.WindowResizeButton.CornerRadius = new CornerRadius(0, 5, 0, 0);
            this.WindowResizeButton.MouseEnter += this.WindowControlMouseEnter;
            this.WindowResizeButton.MouseLeave += this.WindowControlMouseLeave;
            this.WindowResizeButton.Focusable = true;
            this.WindowResizeButton.Cursor = Cursors.Hand;
            this.WindowResizeButton.Width = 40;
            this.WindowResizeButton.PreviewMouseLeftButtonUp += (sender, args) =>
                                                               {
                                                                   WindowState = (WindowState == WindowState.Maximized)
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
                                                                  Close();
                                                                  args.Handled = true;
                                                              };
            this.WindowCloseButton.Child = new Image() { Stretch = Stretch.None, Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/closewindow.png")) };
            this.WcGrid.Children.Add(this.WindowCloseButton);
            Grid.SetColumn(this.WindowCloseButton, 2);

            this.Loaded += OnLoaded;
            this.LocationChanged += OnLocationChanged;

        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.F12 && (Keyboard.IsKeyDown(Key.LeftCtrl & Key.RightCtrl)))
            {
                Octgn.Windows.Diagnostics.Instance.Show();
            }
        }

        private void OnIsSubbedChanged(bool b)
        {
            this.ProgramOnOnOptionsChanged();
        }

        private void ProgramOnOnOptionsChanged()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(this.ProgramOnOnOptionsChanged));
                return;
            }
            var issub = SubscriptionModule.Get().IsSubscribed ?? false;
            if (issub && !String.IsNullOrWhiteSpace(Prefs.WindowSkin))
            {
                var bimage = new BitmapImage(new Uri(Prefs.WindowSkin));

                var ib = new ImageBrush(bimage);
                if (Prefs.TileWindowSkin)
                {
                    ib.Stretch = Stretch.None;
                    ib.TileMode = TileMode.Tile;
                    ib.ViewportUnits = BrushMappingMode.Absolute;
                    ib.Viewport = new Rect(0, 0, bimage.PixelWidth, bimage.PixelHeight);
                }
                else
                {
                    ib.Stretch = Stretch.Fill;
                }
                this.MainBorder.Background = ib;
            }
            else
            {
                var bimage = new BitmapImage(new Uri("pack://application:,,,/Resources/background.png"));

                var ib = new ImageBrush(bimage);
                ib.Stretch = Stretch.Fill;
                this.MainBorder.Background = ib;
                //this.MainBorder.SetResourceReference(Border.BackgroundProperty, "ControlBackgroundBrush");
            }
        }

        private void UpdateBackground(bool subbed)
        {

        }

        private void win_SourceInitialized(object sender, EventArgs e)
        {
            System.IntPtr handle = (new WinInterop.WindowInteropHelper(this)).Handle;
            WinInterop.HwndSource.FromHwnd(handle).AddHook(WindowProc);
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
            System.IntPtr handle = (new WinInterop.WindowInteropHelper(this)).Handle;
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
            Dispatcher.BeginInvoke(new Action(ProgramOnOnOptionsChanged));
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
            if (!this.CanResize || this.WindowState == WindowState.Maximized)
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
            this.DragMove();
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
                mmi.ptMinTrackSize.x = (int)this.MinWidth;
                mmi.ptMinTrackSize.y = (int)this.MinHeight;
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.SourceInitialized -= win_SourceInitialized;
            this.LabelTitle.MouseDown -= this.BorderMouseDown1;
            this.WindowMinimizeButton.MouseEnter -= this.WindowControlMouseEnter;
            this.WindowMinimizeButton.MouseLeave -= this.WindowControlMouseLeave;
            this.WindowResizeButton.MouseEnter -= this.WindowControlMouseEnter;
            this.WindowResizeButton.MouseLeave -= this.WindowControlMouseLeave;
            this.WindowCloseButton.MouseEnter -= this.WindowControlMouseEnter;
            this.WindowCloseButton.MouseLeave -= this.WindowControlMouseLeave;
            this.Loaded -= OnLoaded;
            this.LocationChanged -= OnLocationChanged;
            recTopLeft.MouseDown -= this.DragMouseDown;
            recTop.MouseDown -= this.DragMouseDown;
            recTopRight.MouseDown -= this.DragMouseDown;
            recLeft.MouseDown -= this.DragMouseDown;
            recRight.MouseDown -= this.DragMouseDown;
            recBottomLeft.MouseDown -= this.DragMouseDown;
            recBottom.MouseDown -= this.DragMouseDown;
            recBottomRight.MouseDown -= this.DragMouseDown;
            this.PreviewKeyUp -= OnPreviewKeyUp;
        }

        #endregion
    }
}
