using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Octgn.Controls
{
    public class TextOrIcon : ContentControl
    {       
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextOrIcon));
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(string), typeof(TextOrIcon));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value);  }
        }

        private Image img = new Image();
				private TextBlock tb = new TextBlock() { Style = null };

        public TextOrIcon()
        {
            img.VerticalAlignment = tb.VerticalAlignment = VerticalAlignment.Center;
            Focusable = false;
            Content = tb;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IconProperty)
            {
                if (Icon != null)
                {
                    img.Source = new BitmapImage(new Uri(Icon));
                    Content = img;
                }
                else
                    Content = tb;
            }
            else if (e.Property == TextProperty)
            { img.ToolTip = tb.Text = Text; }
        }
    }
}