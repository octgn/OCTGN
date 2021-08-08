using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Octide.Controls
{
    /// <summary>
    /// Adorner class which shows textbox over the text block when the Edit mode is on.
    /// </summary>
    public class EditableTextBlockAdorner : Adorner
    {
        private readonly VisualCollection _collection;

        private readonly TextBox _textBox;

        private readonly TextBlock _textBlock;

        public EditableTextBlockAdorner(EditableTextBlock adornedElement)
            : base(adornedElement)
        {
            _collection = new VisualCollection(this);
            _textBox = new TextBox()
            {
                FontSize = adornedElement.FontSize,
                FontWeight = adornedElement.FontWeight,
                FontStyle = adornedElement.FontStyle
            };
            _textBlock = adornedElement;
            Binding binding = new Binding("Text") { Source = adornedElement };
            _textBox.SetBinding(TextBox.TextProperty, binding);
            _textBox.AcceptsReturn = true;
            _textBox.MaxLength = adornedElement.MaxLength;
            _textBox.KeyUp += TextBox_KeyUp;
            _collection.Add(_textBox);
        }

        void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _textBox.Text = _textBox.Text.Replace("\r\n", string.Empty);
                BindingExpression expression = _textBox.GetBindingExpression(TextBox.TextProperty);
                if (null != expression)
                {
                    expression.UpdateSource();
                }
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _collection[index];
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return _collection.Count;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _textBox.Measure(constraint);
            return new Size(_textBox.DesiredSize.Width, _textBlock.ActualHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _textBox.Arrange(new Rect(new Point(0, 0), finalSize));
            _textBox.Focus();
            return new Size(_textBox.ActualWidth, _textBox.ActualHeight);
        }

        public event RoutedEventHandler TextBoxLostFocus
        {
            add
            {
                _textBox.LostFocus += value;
            }
            remove
            {
                _textBox.LostFocus -= value;
            }
        }

        public event KeyEventHandler TextBoxKeyUp
        {
            add
            {
                _textBox.KeyUp += value;
            }
            remove
            {
                _textBox.KeyUp -= value;
            }
        }
    }
}
