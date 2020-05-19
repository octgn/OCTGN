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
using log4net;
using System.Reflection;

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
            RichTextBlock r = sender as RichTextBlock ?? throw new InvalidOperationException($"{nameof(sender)} of type {sender?.GetType().FullName} was not expected.");
            List<Inline> i = e.NewValue as List<Inline> ?? throw new InvalidOperationException($"{nameof(e.NewValue)} of type {e.NewValue?.GetType().FullName} was not expected."); ;
            r.Inlines.Clear();
            foreach (Inline inline in i)
            {
                r.Inlines.Add(inline);
            }
        }
    }

    public class RichTextBoxConverter : IValueConverter
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static Game Game;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Game = parameter as Game;
            var propval = value as RichTextPropertyValue;
            if (propval == null) return null;
            if (!(propval.Value is RichSpan)) throw new InvalidOperationException($"{nameof(RichTextPropertyValue)}.{nameof(value)} is the wrong type");

            Span span = new Span();
            InternalProcess(span, propval.Value);
            return span.Inlines.ToList();
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static void InternalProcess(Span span, RichSpan node)
        {
            foreach (RichSpan child in node.Items)
            {
                if (child is RichText text)
                {
                    span.Inlines.Add(new Run(text.Text.ToString()));
                }
                else if (child is RichSpan element)
                {
                    switch (element.Type)
                    {
                        case RichSpanType.Bold:
                            {
                                Span boldSpan = new Span();
                                InternalProcess(boldSpan, element);
                                Bold bold = new Bold(boldSpan);
                                span.Inlines.Add(bold);
                                break;
                            }
                        case RichSpanType.Italic:
                            {
                                Span italicSpan = new Span();
                                InternalProcess(italicSpan, element);
                                Italic italic = new Italic(italicSpan);
                                span.Inlines.Add(italic);
                                break;
                            }
                        case RichSpanType.Underline:
                            {
                                Span underlineSpan = new Span();
                                InternalProcess(underlineSpan, element);
                                Underline underline = new Underline(underlineSpan);
                                span.Inlines.Add(underline);
                                break;
                            }
                        case RichSpanType.Color:
                            {
                                Span colorSpan = new Span();
                                InternalProcess(colorSpan, element);
                                colorSpan.Foreground = new BrushConverter().ConvertFromString((element as RichColor).Attribute) as SolidColorBrush;
                                span.Inlines.Add(colorSpan);
                                break;
                            }
                        case RichSpanType.Symbol:
                            {
                                Symbol symbol = (element as RichSymbol).Attribute;

                                var image = new Image
                                {
                                    Margin = new Thickness(0, 0, 0, -2),
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