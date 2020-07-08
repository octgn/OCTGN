using System;
using System.Windows;
using System.Windows.Controls;

namespace Octgn.Desktop.Interfaces.Easy
{
    public abstract class Screen : UserControl
    {
        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Screen), new PropertyMetadata(string.Empty));

        public NavigationService NavigationService { get; internal set; }
    }
}
