using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace Octgn.Controls
{
    /// <summary>
    /// Interaction logic for OctgnChrome.xaml
    /// </summary>
    public partial class OctgnChrome : Window
    {
        #region Design Mode Detection
        private static bool? _isInDesignMode;

        /// <summary>
        /// Gets a value indicating whether the control is in design mode (running in Blend
        /// or Visual Studio).
        /// </summary>
        public static bool IsInDesignModeStatic
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
#if SILVERLIGHT
            _isInDesignMode = DesignerProperties.IsInDesignTool;
#else
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool)DependencyPropertyDescriptor
                        .FromProperty(prop, typeof(FrameworkElement))
                        .Metadata.DefaultValue;
#endif
                }

                return _isInDesignMode.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the control is in design mode (running under Blend
        /// or Visual Studio).
        /// </summary>
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "Non static member needed for data binding")]
        public bool IsInDesignMode
        {
            get
            {
                return IsInDesignModeStatic;
            }
        }
        #endregion

        #region Content Property
        /// <summary>
        /// Window content
        /// <remarks>Hides base window class 'Content' property</remarks>
        /// </summary>
        public new object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public new static readonly DependencyProperty ContentProperty;

        // called when 'Content' property changed
        static void ContentChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            OctgnChrome window = (OctgnChrome)property;
            window.ContentArea.Child = (UIElement)args.NewValue;
        }
        #endregion

        public ImageSource WindowIcon
        {
            get { return base.Icon; }
            set
            {
                base.Icon = value;
                IconImage.Source = value;
            }
        }

        #region Background Property
        /// <summary>
        /// Window background
        /// <remarks>Hides base window class 'Background' property</remarks>
        /// </summary>
        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set
            {
                SetValue(BackgroundProperty, value);
                MainBorder.Background = value;
            }
        }

        public new static readonly DependencyProperty BackgroundProperty;

        // called when 'Background' property changed
        static void BackgroundChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            OctgnChrome window = (OctgnChrome)property;
            window.MainBorder.Background = (Brush)args.NewValue;
        }
        #endregion

        #region Interop Junk
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private const int WM_SYSCOMMAND = 0x112;

        [DllImportAttribute("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImportAttribute("user32.dll",CharSet=CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lparam);

        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();

        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight=8
        }

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(new WindowInteropHelper(this).Handle, WM_SYSCOMMAND, (IntPtr) (61440 + direction), IntPtr.Zero);
        }

        #endregion

        public Visibility MinimizeButtonVisibility
        {
            get { return WindowMinimizeButton.Visibility; }
            set { WindowMinimizeButton.Visibility = value; }
        }

        public Visibility MinMaxButtonVisibility
        {
            get { return WindowResizeButton.Visibility; }
            set { WindowResizeButton.Visibility = value; }
        }

        public Visibility CloseButtonVisibility
        {
            get { return WindowCloseButton.Visibility; }
            set { WindowCloseButton.Visibility = value; }
        }

        public bool CanResize { get; set; }

        private Border MainBorder { get; set; }
        private Grid DragGrid { get; set; }
        private Grid MainGrid { get; set; }
        private Grid TitleGrid { get; set; }
        private TextBlock LabelTitle { get; set; }
        private Grid WCGrid { get; set; }
        private Image IconImage { get; set; }

        private Border WindowMinimizeButton { get; set; }
        private Border WindowResizeButton { get; set; }
        private Border WindowCloseButton { get; set; }
        private Border ContentArea { get; set; }

        private readonly Brush WindowControlHoverBrush = new SolidColorBrush(Colors.DimGray);
        private readonly Brush WindowControlOffBrush = new SolidColorBrush(Colors.Transparent);
        
        static OctgnChrome()
        {
            // this checks whether application runs in design mode or not; if not the DependencyProperties are initialized
            if (!IsInDesignModeStatic)
            {
                ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(OctgnChrome), new UIPropertyMetadata(null, new PropertyChangedCallback(ContentChangedCallback)));
                BackgroundProperty = DependencyProperty.Register("Background", typeof(object), typeof(OctgnChrome), new UIPropertyMetadata(Brushes.Transparent, new PropertyChangedCallback(BackgroundChangedCallback)));
            }                     
        }

        public OctgnChrome()
        {
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.CanResize;
            base.Background = Brushes.Transparent;
            CanResize = true;
            MainBorder = new Border();
            MainBorder.CornerRadius = new CornerRadius(5);
            MainBorder.SetResourceReference(Border.BackgroundProperty, "ControlBackgroundBrush");
            //MainBorder.Background = Resour new SolidColorBrush(Color.FromRgb(35, 35, 35));
            base.Content = MainBorder;

            MakeDrag();


            MainGrid = new Grid();
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100, GridUnitType.Star) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Star) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            DragGrid.Children.Add(MainGrid);
            Grid.SetColumn(MainGrid, 1);
            Grid.SetRow(MainGrid, 1);

             
            //IconImage = new Image{Source = new BitmapImage(new Uri("pack://application:,,,/Octgn;component/Resources/Icon.ico")) };
            IconImage = new Image();
            IconImage.Stretch = Stretch.Uniform;
            IconImage.Source = base.Icon;
            IconImage.VerticalAlignment = VerticalAlignment.Center;
            IconImage.HorizontalAlignment = HorizontalAlignment.Center;
            MainGrid.Children.Add(IconImage);
            //Setup content area
            ContentArea = new Border();
            MainGrid.Children.Add(ContentArea);
            Grid.SetRow(ContentArea,1);
            Grid.SetColumnSpan(ContentArea,3);

            //Add label
            LabelTitle = new TextBlock();
            LabelTitle.FontFamily = new FontFamily("Euphemia");
            LabelTitle.FontSize = 22;
            LabelTitle.Foreground = Brushes.DarkGray;
            LabelTitle.FontWeight = FontWeights.Bold;
            LabelTitle.FontStyle = FontStyles.Italic;
            LabelTitle.DataContext = this;
            LabelTitle.SetBinding(TextBlock.TextProperty, new Binding("Title") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            LabelTitle.MouseDown += Border_MouseDown_1;
            MainGrid.Children.Add(LabelTitle);
            Grid.SetColumn(LabelTitle, 1);

            //Add window controls
            WCGrid = new Grid();
            WCGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            WCGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            WCGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            MainGrid.Children.Add(WCGrid);
            Grid.SetColumn(WCGrid, 2);

            WindowMinimizeButton = new Border();
            WindowMinimizeButton.MouseEnter += WindowControlMouseEnter;
            WindowMinimizeButton.MouseLeave += WindowControlMouseLeave;
            WindowMinimizeButton.Focusable = true;
            WindowMinimizeButton.PreviewMouseLeftButtonUp += (sender, args) =>
                                                {
                                                    WindowState = WindowState.Minimized;
                                                    args.Handled = true;
                                                };
            WindowMinimizeButton.Child = new Image() { Stretch = Stretch.None, Source = new BitmapImage(new Uri("pack://application:,,,/Octgn;component/Resources/minimize.png")) };
            WCGrid.Children.Add(WindowMinimizeButton);
            Grid.SetColumn(WindowMinimizeButton, 0);

            WindowResizeButton = new Border();
            WindowResizeButton.MouseEnter += WindowControlMouseEnter;
            WindowResizeButton.MouseLeave += WindowControlMouseLeave;
            WindowResizeButton.Focusable = true;
            WindowResizeButton.PreviewMouseLeftButtonUp += (sender, args) =>
                                                               {
                                                                   WindowState = (WindowState == WindowState.Maximized)
                                                                                     ? WindowState.Normal
                                                                                     : WindowState.Maximized;
                                                                   args.Handled = true;
                                                               };
            WindowResizeButton.Child = new Image() { Stretch = Stretch.None, Source = new BitmapImage(new Uri("pack://application:,,,/Octgn;component/Resources/minmax.png")) };
            WCGrid.Children.Add(WindowResizeButton);
            Grid.SetColumn(WindowResizeButton, 1);

            WindowCloseButton = new Border();
            WindowCloseButton.MouseEnter += WindowControlMouseEnter;
            WindowCloseButton.MouseLeave += WindowControlMouseLeave;
            WindowCloseButton.Focusable = true;
            WindowCloseButton.PreviewMouseLeftButtonUp += (sender, args) =>
                                                              {
                                                                  Close();
                                                                  args.Handled = true;
                                                              };
            WindowCloseButton.Child = new Image() { Stretch = Stretch.None, Source = new BitmapImage(new Uri("pack://application:,,,/Octgn;component/Resources/closewindow.png")) };
            WCGrid.Children.Add(WindowCloseButton);
            Grid.SetColumn(WindowCloseButton, 2);

        }

        private void WindowControlMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            var s = sender as Border;
            s.Background = WindowControlOffBrush;
        }

        private void WindowControlMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            var s = sender as Border;
            s.Background = WindowControlHoverBrush;
        }

        private void MakeDrag()
        {
            DragGrid = new Grid();
            DragGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(8) });
            DragGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100, GridUnitType.Star) });
            DragGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(8) });
            DragGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8) });
            DragGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Star) });
            DragGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8) });

            var tTopLeft = new Rectangle { Cursor = Cursors.SizeNWSE, Name = "dTopLeft" ,Fill = Brushes.Transparent};
            tTopLeft.MouseDown += DragMouseDown;
            DragGrid.Children.Add(tTopLeft);

            var tTop = new Rectangle { Cursor = Cursors.SizeNS, Name = "dTop"  ,Fill = Brushes.Transparent};
            tTop.MouseDown += DragMouseDown;
            DragGrid.Children.Add(tTop);
            Grid.SetColumn(tTop, 1);

            var tTopRight = new Rectangle { Cursor = Cursors.SizeNESW, Name = "dTopRight"  ,Fill = Brushes.Transparent};
            tTopRight.MouseDown += DragMouseDown;
            DragGrid.Children.Add(tTopRight);
            Grid.SetColumn(tTopRight, 2);

            var tLeft = new Rectangle { Cursor = Cursors.SizeWE, Name = "dLeft"  ,Fill = Brushes.Transparent};
            tLeft.MouseDown += DragMouseDown;
            DragGrid.Children.Add(tLeft);
            Grid.SetRow(tLeft, 1);

            var tRight = new Rectangle { Cursor = Cursors.SizeWE, Name = "dRight"  ,Fill = Brushes.Transparent};
            tRight.MouseDown += DragMouseDown;
            DragGrid.Children.Add(tRight);
            Grid.SetRow(tRight, 1);
            Grid.SetColumn(tRight,2);

            var tBottomLeft = new Rectangle { Cursor = Cursors.SizeNESW, Name = "dBottomLeft"  ,Fill = Brushes.Transparent};
            tBottomLeft.MouseDown += DragMouseDown;
            DragGrid.Children.Add(tBottomLeft);
            Grid.SetRow(tBottomLeft, 2);

            var tBottom = new Rectangle { Cursor = Cursors.SizeNS, Name = "dBottom"  ,Fill = Brushes.Transparent};
            tBottom.MouseDown += DragMouseDown;
            DragGrid.Children.Add(tBottom);
            Grid.SetColumn(tBottom, 1);
            Grid.SetRow(tBottom, 2);

            var tBottomRight = new Rectangle { Cursor = Cursors.SizeNWSE, Name = "dBottomRight"  ,Fill = Brushes.Transparent};
            tBottomRight.MouseDown += DragMouseDown;
            DragGrid.Children.Add(tBottomRight);
            Grid.SetColumn(tBottomRight, 2);
            Grid.SetRow(tBottomRight, 2);

            MainBorder.Child = DragGrid;
        }

        private void DragMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (!CanResize) return;
            var s = sender as Rectangle;
            switch (s.Name)
            {
                case "dTopLeft":
                    ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "dTop":
                    ResizeWindow(ResizeDirection.Top);
                    break;
                case "dTopRight":
                    ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "dLeft":
                    ResizeWindow(ResizeDirection.Left);
                    break;
                case "dRight":
                    ResizeWindow(ResizeDirection.Right);
                    break;
                case "dBottomLeft":
                    ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                case "dBottom":
                    ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "dBottomRight":
                    ResizeWindow(ResizeDirection.BottomRight);
                    break;
            }
        }

        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            DragMove();
           // e.Handled = false;
            //MouseUp(sender, e);
            //ReleaseCapture();
            //SendMessage(new WindowInteropHelper(this).Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
    }
}
