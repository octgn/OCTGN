using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Microsoft.Windows.Shell;

namespace Octgn.Controls.WindowDecorators
{
    class OctgnDecorator : WindowDecorator, IDisposable
    {

        /// <summary>
        /// Gets or sets the main border.
        /// </summary>
        private Border MainBorder { get; }

        /// <summary>
        /// UI element holding the content of the window
        /// </summary>
        private AdornerDecorator ContentArea { get; }

        /// <summary>
        /// Gets or sets the main grid.
        /// </summary>
        private Grid MainGrid { get; set; }

        /// <summary>
        /// Gets or sets the icon image.
        /// </summary>
        private Image IconImage { get; set; }

        /// <summary>
        /// Gets or sets the window minimize button.
        /// </summary>
        private Button WindowMinimizeButton { get; set; }

        /// <summary>
        /// Gets or sets the window resize button.
        /// </summary>
        private Button WindowResizeButton { get; set; }

        /// <summary>
        /// Gets or sets the window close button.
        /// </summary>
        private Button WindowCloseButton { get; set; }

        /// <summary>
        /// The title bar row
        /// </summary>
        private RowDefinition TitleRow { get; set; }

        /// <summary>
        /// The chrome that provides basic window shell functionality
        /// </summary>
        private WindowChrome WindowChrome { get; set; }

        public OctgnDecorator(DecorableWindow decoratedWindow) : base(decoratedWindow)
        {
            MainBorder = GetContainer();
            ContentArea = GetContentArea();
        }

        public override void Apply()
        {
            WindowChrome = new WindowChrome()
            {
                CaptionHeight = 35,
                GlassFrameThickness = new Thickness(0),
                ResizeBorderThickness = new Thickness(7),
                CornerRadius = new CornerRadius(10)
            };
            WindowChrome.SetWindowChrome(Decorated, WindowChrome);


            MainGrid = new Grid();
            MainGrid.RowDefinitions.Add(TitleRow = new RowDefinition { Height = new GridLength(35) });
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Star) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });

            MainBorder.Child = MainGrid;
            MainBorder.Padding = new Thickness(8);

            MainGrid.Children.Add(ContentArea);
            Grid.SetRow(ContentArea, 1);
            Grid.SetColumnSpan(ContentArea, 2);

            AddTitleBar();

            AddWindowControls();

            Decorated.PropertyChanged += DecoratedOnPropertyChanged;
        }

        private void AddTitleBar()
        {
            var titleBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                CornerRadius = new CornerRadius(5, 0, 0, 0),
                Padding = new Thickness(5, 2, 5, 2),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            MainGrid.Children.Add(titleBorder);

            var titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            titleBorder.Child = titlePanel;

            IconImage = new Image
            {
                Stretch = Stretch.Uniform,
                Source = Decorated.Icon,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            WindowChrome.SetIsHitTestVisibleInChrome(IconImage, true);
            IconImage.MouseDown += (sender, e) =>
            {
                var icon = (Image)sender;
                var window = Window.GetWindow(icon);
                if (e.ClickCount == 1)
                {
                    var p = icon.PointToScreen(e.ChangedButton == MouseButton.Left ? new Point(0, icon.ActualHeight) : e.GetPosition(icon));
                    Microsoft.Windows.Shell.SystemCommands.ShowSystemMenu(window, p);
                }
                else if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
                {
                    window?.Close();
                }
            };

            var titleLabel = new TextBlock
            {
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(248, 248, 248)),
                FontWeight = FontWeights.Bold,
                DataContext = Decorated,
                Effect = new DropShadowEffect()
                {
                    BlurRadius = 5,
                    Color = Color.FromRgb(64, 64, 64),
                    Direction = 0,
                    Opacity = .9,
                    ShadowDepth = 0,
                    RenderingBias = RenderingBias.Performance
                }
            };
            titleLabel.SetBinding(TextBlock.TextProperty, new Binding("Title") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            titlePanel.Children.Add(IconImage);
            titlePanel.Children.Add(new Border { Width = 20 });
            titlePanel.Children.Add(titleLabel);
        }

        private void AddWindowControls()
        {
            var controlsPanel = new StackPanel { Orientation = Orientation.Horizontal };
            var controlsBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(200, 64, 64, 64)),
                CornerRadius = new CornerRadius(0, 5, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = controlsPanel
            };
            MainGrid.Children.Add(controlsBorder);
            Grid.SetColumn(controlsBorder, 1);

            var styles = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/OCTGN;component/Controls/WindowDecorators/OctgnDecoratorResources.xaml", UriKind.RelativeOrAbsolute)
            };
            var buttonStyle = (Style)styles["TitleBarButtonStyle"];
            buttonStyle.Setters.Add(new Setter(Control.TemplateProperty, styles["TitleBarButtonTemplate"]));

            WindowMinimizeButton = new Button
            {
                Width = 40,
                Style = buttonStyle,
                Content = new Image
                {
                    Stretch = Stretch.None,
                    Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/minimize.png"))
                }
            };
            WindowChrome.SetIsHitTestVisibleInChrome(WindowMinimizeButton, true);
            WindowMinimizeButton.Click += (sender, args) =>
            {
                Decorated.WindowState = WindowState.Minimized;
            };
            controlsPanel.Children.Add(WindowMinimizeButton);

            WindowResizeButton = new Button
            {
                Width = 40,
                Style = buttonStyle,
                Content = new Image
                {
                    Stretch = Stretch.None,
                    Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/minmax.png"))
                }
            };
            WindowChrome.SetIsHitTestVisibleInChrome(WindowResizeButton, true);
            WindowResizeButton.Click += (sender, args) =>
            {
                Decorated.WindowState = (Decorated.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
            };
            controlsPanel.Children.Add(WindowResizeButton);

            WindowCloseButton = new Button
            {
                Width = 40,
                Style = buttonStyle,
                Tag = "CLOSE",
                Content = new Image
                {
                    Stretch = Stretch.None,
                    Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/closewindow.png"))
                }
            };
            WindowChrome.SetIsHitTestVisibleInChrome(WindowCloseButton, true);
            WindowCloseButton.Click += (sender, args) =>
            {
                Decorated.Close();
            };
            controlsPanel.Children.Add(WindowCloseButton);
        }

        public override bool Undo()
        {
            WindowChrome.SetWindowChrome(Decorated, null);
            return true;
        }


        private void DecoratedOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case "WindowIcon":
                    IconImage.Source = Decorated.Icon;
                    break;
                case "MinimizeButtonVisibility":
                    WindowMinimizeButton.Visibility = Decorated.MinimizeButtonVisibility == Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case "MinMaxButtonVisibility":
                    WindowResizeButton.Visibility = Decorated.MinMaxButtonVisibility == Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case "CloseButtonVisibility":
                    WindowCloseButton.Visibility = Decorated.CloseButtonVisibility == Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case "TitleBarVisibility":
                    switch (Decorated.TitleBarVisibility)
                    {
                        case Visibility.Visible:
                            TitleRow.Height = new GridLength(35);
                            WindowChrome.CaptionHeight = 35;
                            break;
                        case Visibility.Hidden:
                        case Visibility.Collapsed:
                            TitleRow.Height = new GridLength(0);
                            WindowChrome.CaptionHeight = 0;
                            break;
                    }
                    break;
            }
        }

        public void Dispose()
        {

        }
    }
}
