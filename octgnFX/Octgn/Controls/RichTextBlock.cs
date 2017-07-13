using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using Octgn.DataNew.Entities;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Octgn.Controls
{
    public class RichTextBlock : TextBlock
    {
        public static DependencyProperty InlineProperty;

        static RichTextBlock()
        {
            //OverrideMetadata call tells the system that this element wants to provide a style that is different than in base class
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RichTextBlock), new FrameworkPropertyMetadata(
                                typeof(RichTextBlock)));
            InlineProperty = DependencyProperty.Register("RichText", typeof(List<Inline>), typeof(RichTextBlock),
                            new PropertyMetadata(null, new PropertyChangedCallback(OnInlineChanged)));
        }
        public List<Inline> RichText
        {
            get { return (List<Inline>)GetValue(InlineProperty); }
            set { SetValue(InlineProperty, value); }
        }
        public static void OnInlineChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;
            RichTextBlock r = sender as RichTextBlock;
            List<Inline> i = e.NewValue as List<Inline>;
            if (r == null || i == null)
                return;
            r.Inlines.Clear();
            foreach (Inline inline in i)
            {
                r.Inlines.Add(inline);
            }
        }
    }

    public class StyledTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var propval = value as PropertyDefValue;
            if (propval == null) return new List<Inline>() { new Run(value.ToString()) };
            if (!(propval.Value is XElement)) return new List<Inline>() { new Run(propval.Value.ToString()) };

            Span span = new Span();
            InternalProcess(span, (XElement)propval.Value, parameter as Game);
            return span.Inlines.ToList();
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static void InternalProcess(Span span, XElement xmlNode, Game game)
        {
            foreach (XNode child in xmlNode.Nodes())
            {
                if (child is XText)
                {
                    span.Inlines.Add(new Run((child as XText).Value));
                }
                else if (child is XElement)
                {
                    switch ((child as XElement).Name.ToString().ToUpper())
                    {
                        case "B":
                        case "BOLD":
                            {
                                Span boldSpan = new Span();
                                InternalProcess(boldSpan, (child as XElement), game);
                                Bold bold = new Bold(boldSpan);
                                span.Inlines.Add(bold);
                                break;
                            }
                        case "I":
                        case "ITALIC":
                            {
                                Span italicSpan = new Span();
                                InternalProcess(italicSpan, (child as XElement), game);
                                Italic italic = new Italic(italicSpan);
                                span.Inlines.Add(italic);
                                break;
                            }
                        case "U":
                        case "UNDERLINE":
                            {
                                Span underlineSpan = new Span();
                                InternalProcess(underlineSpan, (child as XElement), game);
                                Underline underline = new Underline(underlineSpan);
                                span.Inlines.Add(underline);
                                break;
                            }
                        case "C":
                        case "COLOR":
                            {
                                Span colorSpan = new Span();
                                InternalProcess(colorSpan, (child as XElement), game);
                                try
                                { 
                                    colorSpan.Foreground = new BrushConverter().ConvertFromString((child as XElement).Attribute("value").Value) as SolidColorBrush;
                                }
                                catch { /* this was the easiest way to make sure that the color string was valid */ }
                                span.Inlines.Add(colorSpan);
                                break;
                            }
                        case "S":
                        case "SYMBOL":
                            {
                                Symbol symbol = game.Symbols.FirstOrDefault(x => x.Id == (child as XElement).Attribute("value").Value);
                                if (symbol == null) break;

                                var image = new Image
                                {
                                    Margin = new Thickness(0,0,0,-2),
                                    Height = span.FontSize + 2,
                                    Stretch = Stretch.Uniform,
                                    Source = new BitmapImage(new Uri(symbol.Source)),
                                    ToolTip = symbol.Name
                                };

                                var symbolSpan = new InlineUIContainer();
                                symbolSpan.Child = image;

                                span.Inlines.Add(symbolSpan);
                                break;
                            }
                    }
                }
            }
        }
    }
}
