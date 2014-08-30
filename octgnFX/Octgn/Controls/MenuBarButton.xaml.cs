using System.Windows;
using System.Windows.Media;

namespace Octgn.Controls
{
    public partial class MenuBarButton
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof (ImageSource), typeof (MenuBarButton), new PropertyMetadata(default(ImageSource)));

        public ImageSource Icon
        {
            get { return (ImageSource) GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof (string), typeof (MenuBarButton), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public MenuBarButton()
        {
            InitializeComponent();
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return ClipToBounds ? base.GetLayoutClip(layoutSlotSize) : null;
        }
    }
}
