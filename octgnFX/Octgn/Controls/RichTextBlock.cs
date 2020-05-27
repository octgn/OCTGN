using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;
using System.IO;
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
            RichTextBlock r = sender as RichTextBlock ?? throw new InvalidOperationException($"{nameof(sender)} of type {sender?.GetType().FullName} was not expected.");
            if (e.NewValue == null)
                r.Inlines.Clear();
            else
            {
                List<Inline> i = e.NewValue as List<Inline> ?? throw new InvalidOperationException($"{nameof(e.NewValue)} of type {e.NewValue?.GetType().FullName} was not expected."); ;
                r.Inlines.Clear();
                foreach (Inline inline in i)
                {
                    r.Inlines.Add(inline);
                }
            }
        }
    }
}