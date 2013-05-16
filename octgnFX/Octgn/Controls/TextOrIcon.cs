using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Octgn.Controls
{
    public class TextOrIcon : ContentControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof (string),
                                                                                             typeof (TextOrIcon));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof (string),
                                                                                             typeof (TextOrIcon));

        private readonly Image _img = new Image();
        private readonly TextBlock _tb = new TextBlock {Style = null};

        public TextOrIcon()
        {
            _img.VerticalAlignment = _tb.VerticalAlignment = VerticalAlignment.Center;
            Focusable = false;
            Content = _tb;
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string Icon
        {
            get { return (string) GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IconProperty)
            {
                if (Icon != null)
                {
                    var bim = new BitmapImage();
                    bim.BeginInit();
                    bim.CacheOption = BitmapCacheOption.OnLoad;
                    bim.UriSource = new Uri(Icon);
                    bim.EndInit();
                    _img.Source = bim;
                    Content = _img;
                }
                else
                    Content = _tb;
            }
            else if (e.Property == TextProperty)
            {
                _img.ToolTip = _tb.Text = Text;
            }
        }
    }
}