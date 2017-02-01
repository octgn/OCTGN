/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using log4net;

namespace Octgn.Controls
{
    using Core;
    using Extentions;
    using Windows;

    public class DecorableWindow : Window, IDisposable, INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Content Property

        /// <summary>
        /// The content property dependency property.
        /// </summary>
        public static new readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(DecorableWindow), new UIPropertyMetadata(null, ContentChangedCallback));

        /// <summary>
        /// Gets or sets Window content
        /// <remarks>Hides base window class 'Content' property</remarks>
        /// </summary>
        public new object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// Called when 'Content' property changed
        /// </summary>
        /// <param name="property">Dependency object</param>
        /// <param name="args">Dependency Property changed arguments</param>
        private static void ContentChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var window = (DecorableWindow)property;
            window.ContentArea.Child = (UIElement)args.NewValue;
        }
        #endregion

        #region Background Property

        /// <summary>
        /// The background property dependency property.
        /// </summary>
        public static new readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(object), typeof(DecorableWindow), new UIPropertyMetadata(Brushes.Transparent, BackgroundChangedCallback));

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
                SetValue(BackgroundProperty, value);
            }
        }

        /// <summary>
        /// Called when Background property changed.
        /// </summary>
        /// <param name="property">Dependency Object</param>
        /// <param name="args">The Arguments</param>
        private static void BackgroundChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var window = (DecorableWindow)property;
            window.MainContainer.Background = (Brush)args.NewValue;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the base window background brush.
        /// </summary>
        public Brush WindowBackground
        {
            get
            {
                return base.Background;
            }

            set
            {
                base.Background = value;
            }
        }

        /// <summary>
        /// Gets or sets the window icon.
        /// </summary>
        public ImageSource WindowIcon
        {
            get
            {
                return Icon;
            }

            set
            {
                Icon = value;
                NotifyPropertyChanged("WindowIcon");
            }
        }

        private Visibility _minimizeVisibility;
        /// <summary>
        /// Gets or sets the minimize button visibility.
        /// </summary>
        public Visibility MinimizeButtonVisibility
        {
            get
            {
                return _minimizeVisibility;
            }
            set
            {
                _minimizeVisibility = value;
                NotifyPropertyChanged("MinimizeButtonVisibility");
            }
        }

        private Visibility _maximizeVisibility;
        /// <summary>
        /// Gets or sets the min max button visibility.
        /// </summary>
        public Visibility MinMaxButtonVisibility
        {
            get
            {
                return _maximizeVisibility;
            }
            set
            {
                _maximizeVisibility = value;
                NotifyPropertyChanged("MinMaxButtonVisibility");
            }
        }

        private Visibility _closeVisibility;
        /// <summary>
        /// Gets or sets the close button visibility.
        /// </summary>
        public Visibility CloseButtonVisibility
        {
            get
            {
                return _closeVisibility;
            }
            set
            {
                _closeVisibility = value;
                NotifyPropertyChanged("CloseButtonVisibility");
            }
        }

        private Visibility _titleBarVisibility;
        /// <summary>
        /// Gets or sets the title bar visibility.
        /// </summary>
        public Visibility TitleBarVisibility
        {
            get
            {
                return _titleBarVisibility;
            }
            set
            {
                _titleBarVisibility = value;
                NotifyPropertyChanged("TitleBarVisibility");
            }
        }

        private bool _canResize = true;
        /// <summary>
        /// Gets or sets a value indicating whether this window can resize.
        /// </summary>
        public bool CanResize
        {
            get
            {
                return _canResize;
            }
            set
            {
                _canResize = value;
                ResizeMode = ResizeMode.NoResize;
                NotifyPropertyChanged("CanResize");
            }
        }

        #endregion

        #region private accessors
        // some are internal so that the WindowDecorator class can access them

        /// <summary>
        /// Gets or sets the main border.
        /// </summary>
        public Border MainContainer { get; set; }

        /// <summary>
        /// Gets or sets the content area.
        /// </summary>
        internal AdornerDecorator ContentArea { get; set; }

        /// <summary>
        /// Gets or sets the current window decorator
        /// </summary>
        private WindowDecorator Decorator { get; set; }


        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DecorableWindow"/> class.
        /// </summary>
        public DecorableWindow()
        {
            PreviewKeyUp += OnPreviewKeyUp;
            MainContainer = new Border();
            base.Content = MainContainer;
            ContentArea = new AdornerDecorator();
            MainContainer.Child = ContentArea;

            if (this.IsInDesignMode())
            {
                return;
                // none of the following statements are necessery in design mode
            }
            MainContainer.BorderThickness = new Thickness(0);
            MainContainer.BorderBrush = Brushes.Transparent;
            MainContainer.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/background.png"))) { Stretch = Stretch.Fill };

            SetWindowPosition();

            Program.OnOptionsChanged += Program_OnOptionsChanged;
            SubscriptionModule.Get().IsSubbedChanged += OnIsSubbedChanged;
            Loaded += OnLoaded;
            LocationChanged += OnLocationChanged;

            Decorate();
        }

        public bool Decorate()
        {
            if (Decorator != null)
            {
                Log.Warn("Decorate() called on window, but it is already decorated.");
                return false;
            }
            Decorator = WindowDecorator.Create(this);
            Decorator.Apply();
            return true;
        }

        public bool Undecorate()
        {
            if (Decorator == null)
            {
                Log.Warn("Undecorate() called on window, but it is not decorated.");
                return true;
            }
            if (!Decorator.IsUndoable)
            {
                Log.Warn("Undecorate() called on window, but decorator is not undoable.");
                return true;
            }
            if (Decorator.Undo())
            {
                (Decorator as IDisposable)?.Dispose();
                Decorator = null;
                return true;
            }
            return false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            HandleSizeToContent();
        }

        private void HandleSizeToContent()
        {
            if (SizeToContent == SizeToContent.Manual)
            {
                return;
            }

            var previousSizeToContent = SizeToContent;
            SizeToContent = SizeToContent.Manual;

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)(() =>
             {
                 SizeToContent = previousSizeToContent;
             }));
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.F12 && (Keyboard.IsKeyDown(Key.LeftCtrl & Key.RightCtrl)))
            {
                Diagnostics.Instance.Show();
            }
            if (args.Key == Key.F5 && (Keyboard.IsKeyDown(Key.LeftCtrl & Key.RightCtrl)))
            {
                if (Undecorate())
                {
                    //Decorate();
                }
            }
        }

        private void SetWindowPosition()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            try
            {
                var mainWindow = WindowManager.Main ?? Application.Current.MainWindow;
                if (mainWindow != null && mainWindow.Owner == null && !Equals(mainWindow, this) && mainWindow.IsVisible)
                {
                    WindowStartupLocation = WindowStartupLocation.Manual;
                    Left = mainWindow.Left + 10;
                    Top = mainWindow.Top + 10;
                }

            }
            catch (Exception e)
            {
                Log.Warn("Error setting window position", e);
            }
        }


        private void OnIsSubbedChanged(bool subbed)
        {
            Program_OnOptionsChanged();
        }

        private void Program_OnOptionsChanged()
        {
            if (Dispatcher.CheckAccess())
            {
                Task.Factory.StartNew(Program_OnOptionsChanged);
                return;
            }
            var isSubbed = SubscriptionModule.Get().IsSubscribed ?? false;
            Dispatcher.Invoke(new Action(() =>
            {
                ImageBrush ib;
                if (isSubbed && !string.IsNullOrWhiteSpace(Prefs.WindowSkin))
                {
                    var bimage = new BitmapImage(new Uri(Prefs.WindowSkin));

                    ib = new ImageBrush(bimage);
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
                }
                else
                {
                    ib = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/background.png"))) { Stretch = Stretch.Fill };
                }
                MainContainer.Background = ib;
            }));
        }


        private void OnLocationChanged(object sender, EventArgs eventArgs)
        {
            var widowBounds = new Rect(Left + 50, Top + 50, Width - 50, Height - 50);
            var screenBounds = new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
            if (widowBounds.IntersectsWith(screenBounds))
            {
                return;
            }
            var primaryScreenBounds = SystemParameters.WorkArea;
            Left = (primaryScreenBounds.Right - primaryScreenBounds.Left) / 2 - (Width / 2);
            Top = (primaryScreenBounds.Bottom - primaryScreenBounds.Top) / 2 - (Height / 2);
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            OnLocationChanged(sender, new EventArgs());
            Program_OnOptionsChanged();
        }

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
            Program.OnOptionsChanged -= Program_OnOptionsChanged;
            SubscriptionModule.Get().IsSubbedChanged -= OnIsSubbedChanged;
            Loaded -= OnLoaded;
            LocationChanged -= OnLocationChanged;
            PreviewKeyUp -= OnPreviewKeyUp;
            (Decorator as IDisposable)?.Dispose();
        }

        #endregion
    }
}
