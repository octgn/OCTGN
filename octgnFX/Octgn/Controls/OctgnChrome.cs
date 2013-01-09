// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OctgnChrome.cs" company="OCTGN">
//   GNU Stuff
// </copyright>
// <summary>
//   Interaction logic for OctgnChrome.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Octgn.Controls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using Octgn.Extentions;

    /// <summary>
    /// Interaction logic for OctgnChrome.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. This rule fucks up using regions usefully."), SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Reviewed. Suppression is OK here.")]
    public partial class OctgnChrome : Window
    {
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
            SendMessage(new WindowInteropHelper(this).Handle, WmSyscommand, (IntPtr) (61440 + direction), IntPtr.Zero);
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
        private Grid WcGrid { get; set; }

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
        /// The window control hover brush.
        /// </summary>
        private readonly Brush windowControlHoverBrush = new SolidColorBrush(Colors.DimGray);

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
            this.AllowsTransparency = true;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.CanResize;
            base.Background = Brushes.Transparent;
            this.CanResize = true;
            this.MainBorder = new Border();
            this.MainBorder.CornerRadius = new CornerRadius(5);
            this.MainBorder.BorderBrush = new LinearGradientBrush(Color.FromArgb(150, 30, 30, 30), Color.FromArgb(150, 200, 200, 200), 45);
            this.MainBorder.BorderThickness = new Thickness(2);

            //this.MainBorder.SetResourceReference(Border.BackgroundProperty, "ControlBackgroundBrush");

            var bimage = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/testbackground.jpg"));

            this.MainBorder.Background = new ImageBrush(bimage)
                {
                    Stretch = Stretch.Fill

                    // Below for tiled image.
                    // Stretch = Stretch.None,
                    // TileMode = TileMode.Tile,
                    // ViewportUnits = BrushMappingMode.Absolute,
                    // Viewport = new Rect(0, 0, bimage.PixelWidth, bimage.PixelHeight)
                };
            base.Content = this.MainBorder;

            this.MakeDrag();


            this.MainGrid = new Grid();
            this.MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
            this.MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100, GridUnitType.Star) });
            this.MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            this.MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Star) });
            this.MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            this.DragGrid.Children.Add(this.MainGrid);
            Grid.SetColumn(this.MainGrid, 1);
            Grid.SetRow(this.MainGrid, 1);

            // IconImage = new Image{Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/Icon.ico")) };
            this.IconImage = new Image();
            this.IconImage.Stretch = Stretch.Uniform;
            this.IconImage.Source = this.Icon;
            this.IconImage.VerticalAlignment = VerticalAlignment.Center;
            this.IconImage.HorizontalAlignment = HorizontalAlignment.Center;
            this.MainGrid.Children.Add(this.IconImage);

            // Setup content area
            this.ContentArea = new Border();
            this.MainGrid.Children.Add(this.ContentArea);
            Grid.SetRow(this.ContentArea, 1);
            Grid.SetColumnSpan(this.ContentArea, 3);

            // Add label
            this.LabelTitle = new TextBlock();
            this.LabelTitle.FontFamily = new FontFamily("Euphemia");
            this.LabelTitle.FontSize = 22;
            this.LabelTitle.Foreground = Brushes.DarkGray;
            this.LabelTitle.FontWeight = FontWeights.Bold;
            this.LabelTitle.FontStyle = FontStyles.Italic;
            this.LabelTitle.DataContext = this;
            this.LabelTitle.SetBinding(TextBlock.TextProperty, new Binding("Title") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            this.LabelTitle.MouseDown += this.BorderMouseDown1;
            this.MainGrid.Children.Add(this.LabelTitle);
            Grid.SetColumn(this.LabelTitle, 1);

            // Add window controls
            this.WcGrid = new Grid();
            this.WcGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            this.WcGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            this.WcGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            this.MainGrid.Children.Add(this.WcGrid);
            Grid.SetColumn(this.WcGrid, 2);

            this.WindowMinimizeButton = new Border();
            this.WindowMinimizeButton.MouseEnter += this.WindowControlMouseEnter;
            this.WindowMinimizeButton.MouseLeave += this.WindowControlMouseLeave;
            this.WindowMinimizeButton.Focusable = true;
            this.WindowMinimizeButton.PreviewMouseLeftButtonUp += (sender, args) =>
                                                {
                                                    WindowState = WindowState.Minimized;
                                                    args.Handled = true;
                                                };
            this.WindowMinimizeButton.Child = new Image() { Stretch = Stretch.None, Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/minimize.png")) };
            this.WcGrid.Children.Add(this.WindowMinimizeButton);
            Grid.SetColumn(this.WindowMinimizeButton, 0);

            this.WindowResizeButton = new Border();
            this.WindowResizeButton.MouseEnter += this.WindowControlMouseEnter;
            this.WindowResizeButton.MouseLeave += this.WindowControlMouseLeave;
            this.WindowResizeButton.Focusable = true;
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
            this.WindowCloseButton.MouseEnter += this.WindowControlMouseEnter;
            this.WindowCloseButton.MouseLeave += this.WindowControlMouseLeave;
            this.WindowCloseButton.Focusable = true;
            this.WindowCloseButton.PreviewMouseLeftButtonUp += (sender, args) =>
                                                              {
                                                                  Close();
                                                                  args.Handled = true;
                                                              };
            this.WindowCloseButton.Child = new Image() { Stretch = Stretch.None, Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/closewindow.png")) };
            this.WcGrid.Children.Add(this.WindowCloseButton);
            Grid.SetColumn(this.WindowCloseButton, 2);
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

            var recTopLeft = new Rectangle { Cursor = Cursors.SizeNWSE, Name = "dTopLeft", Fill = Brushes.Transparent };
            recTopLeft.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recTopLeft);

            var recTop = new Rectangle { Cursor = Cursors.SizeNS, Name = "dTop", Fill = Brushes.Transparent };
            recTop.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recTop);
            Grid.SetColumn(recTop, 1);

            var recTopRight = new Rectangle
                { Cursor = Cursors.SizeNESW, Name = "dTopRight", Fill = Brushes.Transparent };
            recTopRight.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recTopRight);
            Grid.SetColumn(recTopRight, 2);

            var recLeft = new Rectangle { Cursor = Cursors.SizeWE, Name = "dLeft", Fill = Brushes.Transparent };
            recLeft.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recLeft);
            Grid.SetRow(recLeft, 1);

            var recRight = new Rectangle { Cursor = Cursors.SizeWE, Name = "dRight", Fill = Brushes.Transparent };
            recRight.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recRight);
            Grid.SetRow(recRight, 1);
            Grid.SetColumn(recRight, 2);

            var recBottomLeft = new Rectangle
                { Cursor = Cursors.SizeNESW, Name = "dBottomLeft", Fill = Brushes.Transparent };
            recBottomLeft.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recBottomLeft);
            Grid.SetRow(recBottomLeft, 2);

            var recBottom = new Rectangle { Cursor = Cursors.SizeNS, Name = "dBottom", Fill = Brushes.Transparent };
            recBottom.MouseDown += this.DragMouseDown;
            this.DragGrid.Children.Add(recBottom);
            Grid.SetColumn(recBottom, 1);
            Grid.SetRow(recBottom, 2);

            var recBottomRight = new Rectangle
                { Cursor = Cursors.SizeNWSE, Name = "dBottomRight", Fill = Brushes.Transparent };
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
            if (!this.CanResize)
            {
                return;
            }
            
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
            this.DragMove();
        }
    }
}
