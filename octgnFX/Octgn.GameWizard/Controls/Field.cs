using System;
using System.Windows;
using System.Windows.Controls;

namespace Octgn.GameWizard.Controls
{
    public class Field : UserControl
    {
        static Field()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Field), new FrameworkPropertyMetadata(typeof(Field)));
        }

        public string FieldName {
            get { return (string)GetValue(FieldNameProperty); }
            set { SetValue(FieldNameProperty, value); }
        }

        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register(nameof(FieldName), typeof(string), typeof(Field), new PropertyMetadata(string.Empty));

    }
}
