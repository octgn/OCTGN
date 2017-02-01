using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Microsoft.Windows.Shell;
using System.Windows.Data;
using System.Windows.Controls;
using System;
using System.Globalization;

namespace Octgn.Controls.WindowDecorators
{
    class OctgnDecorator_2 : OctgnDecorator
    {
        private OctgnDecorator_2_Frame _frame;

        public OctgnDecorator_2( DecorableWindow decoratedWindow ) : base( decoratedWindow ) {
        }

        public override void Apply() {
            // Expected: Managed Debugging Assistant LoadFromContext
            // See Also: http://stackoverflow.com/questions/31362077/loadfromcontext-occured
            WindowChrome = new WindowChrome()
            {
                CaptionHeight = 35,
                GlassFrameThickness = new Thickness(0),
                ResizeBorderThickness = new Thickness(7),
                CornerRadius = new CornerRadius(0)
            };

            Decorated.PropertyChanged += DecoratedOnPropertyChanged;
            Decorated.SourceInitialized += Decorated_Initialized;

            WindowChrome.SetWindowChrome(Decorated, WindowChrome);

            Decorated.AllowsTransparency = false;
            //Decorated.UseLayoutRounding = true;
            RenderOptions.SetBitmapScalingMode( Decorated, BitmapScalingMode.HighQuality );

            var mainBorder = GetContainer();

            mainBorder.Child = _frame = new OctgnDecorator_2_Frame();

            ConfigureIcon();
            ConfigureTitle();

            _frame.ContentBorder.Child = GetContentArea();
            mainBorder.BorderBrush = new SolidColorBrush( Color.FromArgb( 255, 170, 170, 170 ) );
        }

        private void Decorated_Initialized( object sender, EventArgs e ) {
            Decorated.SourceInitialized -= Decorated_Initialized;

            ApplyBinding();
        }

        private void ApplyBinding() {
            ConfigureMinimizeButton();
            ConfigureCloseButton();
            ConfigureResizeButton();
        }

        private void ConfigureMinimizeButton() {
            WindowChrome.SetIsHitTestVisibleInChrome( _frame.MinimizeButton, true );
            _frame.MinimizeButton.Click += ( sender, args ) => {
                Decorated.WindowState = WindowState.Minimized;
            };

            var binding = new Binding() {
                Source=Decorated,
                Path=new PropertyPath( nameof(Decorated.MinimizeButtonVisibility ) ),
                Converter=new VisibilityToBooleanConverter(),
            };
            _frame.SetBinding( OctgnDecorator_2_Frame.MinimizeButtonVisibleProperty, binding );
        }

        private void ConfigureResizeButton() {
            WindowChrome.SetIsHitTestVisibleInChrome( _frame.ResizeButton, true );
            _frame.ResizeButton.Click += ( sender, args ) => {
                Decorated.WindowState = (Decorated.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
            };

            var binding = new Binding() {
                Source=Decorated,
                Path=new PropertyPath( nameof(Decorated.MinMaxButtonVisibility ) ),
                Converter=new VisibilityToBooleanConverter(),
            };
            _frame.SetBinding( OctgnDecorator_2_Frame.ResizeButtonVisibleProperty, binding );
        }

        private void ConfigureCloseButton() {
            WindowChrome.SetIsHitTestVisibleInChrome( _frame.CloseButton, true );
            _frame.CloseButton.Click += ( sender, args ) => {
                Decorated.Close();
            };

            var binding = new Binding() {
                Source=Decorated,
                Path=new PropertyPath( nameof(Decorated.CloseButtonVisibility ) ),
                Converter=new VisibilityToBooleanConverter(),
            };
            _frame.SetBinding( OctgnDecorator_2_Frame.CloseButtonVisibleProperty, binding );
        }

        private void ConfigureIcon() {
            var binding = new Binding() {
                Source=Decorated,
                Path=new PropertyPath( nameof(Decorated.Icon) )
            };
            _frame.SetBinding( OctgnDecorator_2_Frame.IconProperty, binding );
        }

        private void ConfigureTitle() {
            var binding = new Binding() {
                Source=Decorated,
                Path=new PropertyPath( nameof(Decorated.Title) )
            };
            _frame.SetBinding( OctgnDecorator_2_Frame.TitleProperty, binding );
        }

        public override bool Undo() {
            WindowChrome.SetWindowChrome( Decorated, null );
            return true;
        }


        private void DecoratedOnPropertyChanged( object sender, PropertyChangedEventArgs args ) {
            switch( args.PropertyName ) {
                case "TitleBarVisibility":
                    switch( Decorated.TitleBarVisibility ) {
                        case Visibility.Visible:
                            _frame.TitleRow.Height = new GridLength( 35 );
                            WindowChrome.CaptionHeight = 35;
                            break;
                        case Visibility.Hidden:
                        case Visibility.Collapsed:
                            _frame.TitleRow.Height = new GridLength( 0 );
                            WindowChrome.CaptionHeight = 0;
                            break;
                    }
                    break;
            }
        }

        private class VisibilityToBooleanConverter : IValueConverter
        {
            public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
                if( !(value is Visibility) ) return false;

                return (Visibility)value == Visibility.Visible;
            }

            public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
                throw new NotImplementedException();
            }
        }
    }

}