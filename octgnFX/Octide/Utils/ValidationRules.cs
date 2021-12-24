// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;

namespace Octide
{
    public class IntegerValidationRule : ValidationRule
    {
        public int Minimum { get; set; } = int.MinValue;
        public int Maximum { get; set; } = int.MaxValue;
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string text = value.ToString();
            if (int.TryParse(text, out int confirmedInteger) == false)
                return new ValidationResult(false, "Not a valid integer.");
            if (confirmedInteger > Maximum)
                return new ValidationResult(false, string.Format("Integer value must be smaller than {0}.", Maximum));
            if (confirmedInteger < Minimum)
                return new ValidationResult(false, string.Format("Integer value must be larger than {0}.", Minimum));

            return ValidationResult.ValidResult;
        }
    }
    public class DoubleValidationRule : ValidationRule
    {
        public double Minimum { get; set; } = double.MinValue;
        public double Maximum { get; set; } = double.MaxValue;
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string text = value.ToString();
            if (double.TryParse(text, out double confirmedDouble) == false)
                return new ValidationResult(false, "Not a valid number.");
            if (confirmedDouble > Maximum)
                return new ValidationResult(false, string.Format("Number value must be smaller than {0}.", Maximum));
            if (confirmedDouble < Minimum)
                return new ValidationResult(false, string.Format("Number value must be larger than {0}.", Minimum));

            return ValidationResult.ValidResult;
        }
    }
    public class StringRestrictionValidationRule : ValidationRule
    {
        public string Restriction { get; set; }
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string text = value.ToString();
            if (string.IsNullOrWhiteSpace(text))
                return new ValidationResult(false, "Value cannot be null, empty, or consist only of spaces.");
            if (Restriction != null && text.Equals(Restriction, StringComparison.InvariantCultureIgnoreCase))
                return new ValidationResult(false, string.Format("Value cannot be '{0}'.", text));
            return ValidationResult.ValidResult;
        }
    }
    public class RichTextValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            try
            {
                var xmlElement = XElement.Parse("<rich>" + value + "</rich>");
            }
            catch (XmlException)
            {
                return new ValidationResult(false, "Invalid XML structure.");

            }
            return ValidationResult.ValidResult;
        }
    }

    public class UniqueValueValidationRule : ValidationRule
    {
        public UniqueValueValidationWrapper Wrapper { get; set; }
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string text = value.ToString();
            if (Wrapper.UniqueValues.Contains(text))
                return new ValidationResult(false, "An item with this name already exists!");

            return ValidationResult.ValidResult;
        }
    }

    public class UniqueValueValidationWrapper : DependencyObject
    {
        public static readonly DependencyProperty UniqueValueProperty =
             DependencyProperty.Register("UniqueValues", typeof(IEnumerable<string>),
             typeof(UniqueValueValidationWrapper), new FrameworkPropertyMetadata(default(IEnumerable<string>)));

        public IEnumerable<string> UniqueValues
        {
            get { return (IEnumerable<string>)GetValue(UniqueValueProperty); }
            set { SetValue(UniqueValueProperty, value); }
        }
    }

    public class BindingProxy : System.Windows.Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new PropertyMetadata(null));
    }
}
