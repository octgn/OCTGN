using System;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Octgn.Controls
{
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    public class NumericUpDown : Control
    {
        static NumericUpDown()
        {
            InitializeCommands();

            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
        }

        public NumericUpDown()
            : base()
        {
            Focusable = false;
            updateValueString();
        }

        #region Properties

        #region Value

        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value", typeof(decimal), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(DefaultValue,
                    new PropertyChangedCallback(OnValueChanged),
                    new CoerceValueCallback(CoerceValue)
                )
            );

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NumericUpDown control = (NumericUpDown)obj;

            decimal oldValue = (decimal)args.OldValue;
            decimal newValue = (decimal)args.NewValue;

            #region Fire Automation events
            NumericUpDownAutomationPeer peer = UIElementAutomationPeer.FromElement(control) as NumericUpDownAutomationPeer;
            if (peer != null)
            {
                peer.RaiseValueChangedEvent(oldValue, newValue);
            }
            #endregion

            RoutedPropertyChangedEventArgs<decimal> e = new RoutedPropertyChangedEventArgs<decimal>(
                oldValue, newValue, ValueChangedEvent);

            control.OnValueChanged(e);

            control.updateValueString();
        }

        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<decimal> args)
        { RaiseEvent(args); }

        private static object CoerceValue(DependencyObject element, object value)
        {
            decimal newValue = (decimal)value;
            NumericUpDown control = (NumericUpDown)element;

            newValue = Math.Max(control.Minimum, Math.Min(control.Maximum, newValue));
            newValue = Decimal.Round(newValue, control.DecimalPlaces);

            return newValue;
        }

        #endregion

        #region Minimum

        public decimal Minimum
        {
            get { return (decimal)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                "Minimum", typeof(decimal), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(DefaultMinValue,
                    new PropertyChangedCallback(OnMinimumChanged), new CoerceValueCallback(CoerceMinimum)
                )
            );

        private static void OnMinimumChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            element.CoerceValue(MaximumProperty);
            element.CoerceValue(ValueProperty);
        }

        private static object CoerceMinimum(DependencyObject element, object value)
        {
            decimal minimum = (decimal)value;
            NumericUpDown control = (NumericUpDown)element;
            return Decimal.Round(minimum, control.DecimalPlaces);
        }

        #endregion

        #region Maximum

        public decimal Maximum
        {
            get { return (decimal)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                "Maximum", typeof(decimal), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(DefaultMaxValue,
                    new PropertyChangedCallback(OnMaximumChanged),
                    new CoerceValueCallback(CoerceMaximum)
                )
            );

        private static void OnMaximumChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            element.CoerceValue(ValueProperty);
        }

        private static object CoerceMaximum(DependencyObject element, object value)
        {
            NumericUpDown control = (NumericUpDown)element;
            decimal newMaximum = (decimal)value;
            return Decimal.Round(Math.Max(newMaximum, control.Minimum), control.DecimalPlaces);
        }

        #endregion

        #region Change

        public decimal Change
        {
            get { return (decimal)GetValue(ChangeProperty); }
            set { SetValue(ChangeProperty, value); }
        }

        public static readonly DependencyProperty ChangeProperty =
            DependencyProperty.Register(
                "Change", typeof(decimal), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(DefaultChange, new PropertyChangedCallback(OnChangeChanged), new CoerceValueCallback(CoerceChange)),
            new ValidateValueCallback(ValidateChange)
            );

        private static bool ValidateChange(object value)
        {
            decimal change = (decimal)value;
            return change > 0;
        }

        private static void OnChangeChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        { }

        private static object CoerceChange(DependencyObject element, object value)
        {
            decimal newChange = (decimal)value;
            NumericUpDown control = (NumericUpDown)element;

            decimal coercedNewChange = Decimal.Round(newChange, control.DecimalPlaces);

            //If Change is .1 and DecimalPlaces is changed from 1 to 0, we want Change to go to 1, not 0.
            //Put another way, Change should always be rounded to DecimalPlaces, but never smaller than the 
            //previous Change
            if (coercedNewChange < newChange)
            {
                coercedNewChange = smallestForDecimalPlaces(control.DecimalPlaces);
            }

            return coercedNewChange;
        }

        private static decimal smallestForDecimalPlaces(int decimalPlaces)
        {
            if (decimalPlaces < 0) throw new ArgumentException("decimalPlaces");

            decimal d = 1;
            for (int i = 0; i < decimalPlaces; i++)
                d /= 10;

            return d;
        }

        #endregion

        #region DecimalPlaces

        public int DecimalPlaces
        {
            get { return (int)GetValue(DecimalPlacesProperty); }
            set { SetValue(DecimalPlacesProperty, value); }
        }

        public static readonly DependencyProperty DecimalPlacesProperty =
            DependencyProperty.Register(
                "DecimalPlaces", typeof(int), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(DefaultDecimalPlaces,
                    new PropertyChangedCallback(OnDecimalPlacesChanged)
                ), new ValidateValueCallback(ValidateDecimalPlaces)
            );

        private static void OnDecimalPlacesChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            NumericUpDown control = (NumericUpDown)element;
            control.CoerceValue(ChangeProperty);
            control.CoerceValue(MinimumProperty);
            control.CoerceValue(MaximumProperty);
            control.CoerceValue(ValueProperty);
            control.updateValueString();
        }

        private static bool ValidateDecimalPlaces(object value)
        {
            int decimalPlaces = (int)value;
            return decimalPlaces >= 0;
        }

        #endregion

        #region ValueString

        public string ValueString
        { get { return (string)GetValue(ValueStringProperty); } }

        private static readonly DependencyPropertyKey ValueStringPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("ValueString", typeof(string), typeof(NumericUpDown), new PropertyMetadata());

        public static readonly DependencyProperty ValueStringProperty = ValueStringPropertyKey.DependencyProperty;

        private void updateValueString()
        {
            _numberFormatInfo.NumberDecimalDigits = this.DecimalPlaces;
            string newValueString = this.Value.ToString("f", _numberFormatInfo);
            this.SetValue(ValueStringPropertyKey, newValueString);
        }

        private NumberFormatInfo _numberFormatInfo = new NumberFormatInfo();

        #endregion

        #endregion

        #region Events
        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            "ValueChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<decimal>), typeof(NumericUpDown));

        /// <summary>
        /// Occurs when the Value property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<decimal> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
        #endregion

        #region Commands

        public static RoutedCommand IncreaseCommand
        { get { return _increaseCommand; } }

        public static RoutedCommand DecreaseCommand
        { get { return _decreaseCommand; } }

        private static void InitializeCommands()
        {
            _increaseCommand = new RoutedCommand("IncreaseCommand", typeof(NumericUpDown));
            CommandManager.RegisterClassCommandBinding(typeof(NumericUpDown), new CommandBinding(_increaseCommand, OnIncreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(NumericUpDown), new InputBinding(_increaseCommand, new KeyGesture(Key.Up)));

            _decreaseCommand = new RoutedCommand("DecreaseCommand", typeof(NumericUpDown));
            CommandManager.RegisterClassCommandBinding(typeof(NumericUpDown), new CommandBinding(_decreaseCommand, OnDecreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(NumericUpDown), new InputBinding(_decreaseCommand, new KeyGesture(Key.Down)));
        }

        private static void OnIncreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            NumericUpDown control = sender as NumericUpDown;
            if (control != null)
                control.OnIncrease();
        }

        private static void OnDecreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            NumericUpDown control = sender as NumericUpDown;
            if (control != null)
                control.OnDecrease();
        }

        protected virtual void OnIncrease()
        { this.Value += Change; }

        protected virtual void OnDecrease()
        { this.Value -= Change; }

        private static RoutedCommand _increaseCommand, _decreaseCommand;

        #endregion

        #region Automation
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new NumericUpDownAutomationPeer(this);
        }
        #endregion

        #region Template

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var editBox = GetTemplateChild("PART_TextBox") as TextBox;
            if (editBox != null)
            {
                editBox.GotFocus += delegate(object sender, RoutedEventArgs e)
                {
                    var box = (TextBox)sender;
                    box.SelectAll();
                };
                editBox.PreviewMouseDown += delegate(object sender, MouseButtonEventArgs e)
                {
                    var box = (TextBox)sender;
                    // If the box isn't focused, focus it (which selects its text, see GotFocus handler)
                    // and delete the event. If we let the Textbox further handle the event it will move 
                    // the caret position, i.e. deselect the text
                    if (!box.IsFocused)
                    {
                        box.Focus();
                        e.Handled = true;
                    }
                };
                editBox.LostKeyboardFocus += delegate(object sender, KeyboardFocusChangedEventArgs e)
                {
                    var box = (TextBox)sender;
                    BindingExpression be = box.GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                    be.UpdateTarget();  // if the value has been reject (e.g. bad format)
                };
                editBox.KeyDown += delegate(object sender, KeyEventArgs e)
                {
                    if (e.Key == Key.Enter)
                    {
                        var box = (TextBox)sender;
                        BindingExpression be = box.GetBindingExpression(TextBox.TextProperty);
                        be.UpdateSource();
                        be.UpdateTarget();  // if the value has been reject (e.g. bad format)
                        ((UIElement)Window.GetWindow(box).Content).MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                    }
                    else if (e.Key == Key.Escape)
                    {
                        var box = (TextBox)sender;
                        BindingExpression be = box.GetBindingExpression(TextBox.TextProperty);
                        be.UpdateTarget();
                        ((UIElement)Window.GetWindow(box).Content).MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                    }
                };
            }
        }

        #endregion

        private const decimal DefaultMinValue = 0;
        private const decimal DefaultValue = DefaultMinValue;
        private const decimal DefaultMaxValue = 100;
        private const decimal DefaultChange = 1;
        private const int DefaultDecimalPlaces = 0;
    }

    public class NumericUpDownAutomationPeer : FrameworkElementAutomationPeer, IRangeValueProvider
    {
        public NumericUpDownAutomationPeer(NumericUpDown control)
            : base(control)
        { }

        protected override string GetClassNameCore()
        { return "NumericUpDown"; }

        protected override AutomationControlType GetAutomationControlTypeCore()
        { return AutomationControlType.Spinner; }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.RangeValue) return this;
            return base.GetPattern(patternInterface);
        }

        internal void RaiseValueChangedEvent(decimal oldValue, decimal newValue)
        {
            base.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty,
                (double)oldValue, (double)newValue);
        }

        #region IRangeValueProvider Members

        bool IRangeValueProvider.IsReadOnly
        {
            get { return !IsEnabled(); }
        }

        double IRangeValueProvider.LargeChange
        {
            get { return (double)MyOwner.Change; }
        }

        double IRangeValueProvider.Maximum
        {
            get { return (double)MyOwner.Maximum; }
        }

        double IRangeValueProvider.Minimum
        {
            get { return (double)MyOwner.Minimum; }
        }

        void IRangeValueProvider.SetValue(double value)
        {
            if (!IsEnabled())
                throw new ElementNotEnabledException();

            decimal val = (decimal)value;
            if (val < MyOwner.Minimum || val > MyOwner.Maximum)
                throw new ArgumentOutOfRangeException("value");

            MyOwner.Value = val;
        }

        double IRangeValueProvider.SmallChange
        {
            get { return (double)MyOwner.Change; }
        }

        double IRangeValueProvider.Value
        {
            get { return (double)MyOwner.Value; }
        }

        #endregion

        private NumericUpDown MyOwner
        {
            get { return (NumericUpDown)base.Owner; }
        }
    }
}