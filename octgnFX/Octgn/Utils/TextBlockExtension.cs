using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Octgn.Utils
{
    public class TextBlockExtension : FrameworkElement
    {
        public static string GetFormattedText(DependencyObject obj) { return (string)obj.GetValue(FormattedTextProperty); }

        public static void SetFormattedText(DependencyObject obj, string value) { obj.SetValue(FormattedTextProperty, value); }

        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached("FormattedText", typeof(string), typeof(TextBlockExtension),
            new PropertyMetadata(string.Empty, FormattedText_PropertyChanged));

        private static void FormattedText_PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var text = e.NewValue as string;

            var textBlock = (TextBlock)sender;

            textBlock.Inlines.Clear();

            var regx = new Regex(@"(http://[^\s]+)", RegexOptions.IgnoreCase);
            var textParts = regx.Split(text);
            for (var i = 0; i < textParts.Length; i++) {
                Inline inlineToAdd = null;
                if (i % 2 == 0)
                    inlineToAdd = new Run { Text = textParts[i] };
                else {
                    var link = new Hyperlink {
                        NavigateUri = new Uri(textParts[i])
                    };
                    link.RequestNavigate += Link_RequestNavigate;
                    link.Inlines.Add(new Run { Text = textParts[i] });
                    inlineToAdd = link;
                }
                textBlock.Inlines.Add(inlineToAdd);
            }
        }

        private static void Link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            var hyperlink = (Hyperlink)sender;
            var navigateUri = hyperlink.NavigateUri.ToString();

            Process.Start(new ProcessStartInfo(navigateUri));

            e.Handled = true;
        }
    }
}
