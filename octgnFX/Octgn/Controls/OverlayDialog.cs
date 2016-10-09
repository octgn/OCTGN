using System;
using System.Windows;
using System.Windows.Controls;

namespace Octgn.Controls
{
    public class OverlayDialog : ContentControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(OverlayDialog));

        static OverlayDialog() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OverlayDialog), 
                new FrameworkPropertyMetadata(typeof(OverlayDialog)));
        }

        public string Title {
            get { return (string)this.GetValue(TitleProperty);}
            set { this.SetValue(TitleProperty, value); }
        }
    }
}
