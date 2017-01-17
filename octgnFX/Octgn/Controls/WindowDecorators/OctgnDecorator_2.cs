using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Windows.Shell;

namespace Octgn.Controls.WindowDecorators
{
    class OctgnDecorator_2 : OctgnDecorator
    {
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

            WindowChrome.SetWindowChrome(Decorated, WindowChrome);
            Decorated.AllowsTransparency = false;

            var mainBorder = GetContainer();

            var frame = new WindowFrame();
            mainBorder.Child = frame;

            frame.ContentBorder.Child = GetContentArea();
            mainBorder.BorderBrush = new SolidColorBrush( Color.FromArgb( 255, 170, 170, 170 ) );
        }

        public override bool Undo() {
            WindowChrome.SetWindowChrome( Decorated, null );
            return true;
        }


        private void DecoratedOnPropertyChanged( object sender, PropertyChangedEventArgs args ) {
            //switch( args.PropertyName ) {
            //    case "WindowIcon":
            //        IconImage.Source = Decorated.Icon;
            //        break;
            //    case "MinimizeButtonVisibility":
            //        WindowMinimizeButton.Visibility = Decorated.MinimizeButtonVisibility == Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
            //        break;
            //    case "MinMaxButtonVisibility":
            //        WindowResizeButton.Visibility = Decorated.MinMaxButtonVisibility == Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
            //        break;
            //    case "CloseButtonVisibility":
            //        WindowCloseButton.Visibility = Decorated.CloseButtonVisibility == Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
            //        break;
            //    case "TitleBarVisibility":
            //        switch( Decorated.TitleBarVisibility ) {
            //            case Visibility.Visible:
            //                TitleRow.Height = new GridLength( 35 );
            //                WindowChrome.CaptionHeight = 35;
            //                break;
            //            case Visibility.Hidden:
            //            case Visibility.Collapsed:
            //                TitleRow.Height = new GridLength( 0 );
            //                WindowChrome.CaptionHeight = 0;
            //                break;
            //        }
            //        break;
            //}
        }
    }

}