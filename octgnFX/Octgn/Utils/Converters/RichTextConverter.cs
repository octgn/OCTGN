// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Octgn.DataNew.Entities;
using System.Windows.Media.Imaging;
using log4net;
using System.Reflection;

namespace Octgn.Utils.Converters
{
    public class RichTextConverter : IValueConverter
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static Game Game;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Game = parameter as Game;
            var propval = value as RichTextPropertyValue;
            if (propval == null) return null;
            if (!(propval.Value is IRichText)) throw new InvalidOperationException($"{nameof(RichTextPropertyValue)}.{nameof(value)} is the wrong type");

            Span span = new Span();
            InternalProcess(span, propval.Value);
            return span.Inlines.ToList();
        }

        public static Span ConvertToSpan(RichTextPropertyValue value, Game game)
        {
            Game = game;
            Span span = new Span();
            InternalProcess(span, value.Value);
            return span;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static void InternalProcess(Span span, IRichText node)
        {
            foreach (IRichText child in node.Items)
            {
                if (child is RichText text)
                {
                    span.Inlines.Add(new Run(text.Text.ToString()));
                }
                else if (child is RichBold richBold)
                {
                    Span boldSpan = new Span();
                    InternalProcess(boldSpan, richBold);
                    Bold bold = new Bold(boldSpan);
                    span.Inlines.Add(bold);
                }
                else if (child is RichItalic richItalic)
                {
                    Span italicSpan = new Span();
                    InternalProcess(italicSpan, richItalic);
                    Italic italic = new Italic(italicSpan);
                    span.Inlines.Add(italic);
                }
                else if (child is RichUnderline richUnderline)
                {
                    Span underlineSpan = new Span();
                    InternalProcess(underlineSpan, richUnderline);
                    Underline underline = new Underline(underlineSpan);
                    span.Inlines.Add(underline);
                }
                else if (child is RichColor richColor)
                {
                    Span colorSpan = new Span();
                    InternalProcess(colorSpan, richColor);
                    colorSpan.Foreground = new BrushConverter().ConvertFromString(richColor.Attribute) as SolidColorBrush;
                    span.Inlines.Add(colorSpan);
                }
                else if (child is RichSymbol richSymbol)
                {
                    Symbol symbol = richSymbol.Attribute;

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
                }
            }
        }
    }
}