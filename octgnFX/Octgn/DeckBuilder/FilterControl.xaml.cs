using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Octgn.Data;

namespace Octgn.DeckBuilder
{
    public partial class FilterControl
    {
        private static readonly SqlComparison[] StringComparisons = new[]
                                                                        {
                                                                            new SqlComparison("Contains",
                                                                                              "{0} LIKE '*{1}*'")
                                                                                {EscapeQuotes = true},
                                                                            new SqlComparison("Starts with",
                                                                                              "{0} LIKE '{1}*'")
                                                                                {EscapeQuotes = true},
                                                                            new SqlComparison("Equals", "{0} = '{1}'")
                                                                                {EscapeQuotes = true}
                                                                        };

        private static readonly SqlComparison[] IntegerComparisons = new SqlComparison[]
                                                                         {
                                                                             new IntegerComparison("Equals", "{0} = {1}")
                                                                             ,
                                                                             new IntegerComparison("Greater than",
                                                                                                   "{0} > {1}"),
                                                                             new IntegerComparison("Less than",
                                                                                                   "{0} < {1}")
                                                                         };

        private static readonly SqlComparison[] CharComparisons = new[]
                                                                      {
                                                                          new SqlComparison("Equals", "{0} = '{1}'")
                                                                              {EscapeQuotes = true},
                                                                          new SqlComparison("Greater than",
                                                                                            "{0} > '{1}'")
                                                                              {EscapeQuotes = true},
                                                                          new SqlComparison("Less than", "{0} < '{1}'")
                                                                              {EscapeQuotes = true},
                                                                          new CharInComparison("One of", "{0} IN ({1})")
                                                                      };

        private PropertyDef _property;

        public FilterControl()
        {
            InitializeComponent();
            DataContextChanged += delegate
                                      {
                                          _property = DataContext as PropertyDef;
                                          if (_property == null) return; // Happens when the control is unloaded
                                          CreateComparisons();
                                      };
        }

        public event EventHandler RemoveFilter;

        public string GetSqlCondition()
        {
            if (_property is SetPropertyDef)
                return "set_id = '" + ((Set) comparisonList.SelectedItem).Id.ToString("D") + "'";
            return ((SqlComparison) comparisonList.SelectedItem).GetSql(_property.Name, comparisonText.Text);
        }

        private void CreateComparisons()
        {
            if (_property is SetPropertyDef)
            {
                comparisonList.Width = 262;
                comparisonText.Visibility = Visibility.Collapsed;

                comparisonList.ItemsSource = ((SetPropertyDef) _property).Sets;
            }
            else
            {
                comparisonList.Width = 100;
                comparisonText.Visibility = Visibility.Visible;

                switch (_property.Type)
                {
                    case PropertyType.String:
                        comparisonList.ItemsSource = StringComparisons;
                        break;
                    case PropertyType.Integer:
                        comparisonList.ItemsSource = IntegerComparisons;
                        break;
                    case PropertyType.Char:
                        comparisonList.ItemsSource = CharComparisons;
                        break;
                    default:
                        throw new ArgumentException("Unknown property type");
                }
            }
            comparisonList.SelectedIndex = 0;
        }

        private void RemoveClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (RemoveFilter != null)
                RemoveFilter(TemplatedParent, e);
        }

        private void ComparisonTextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemOpenBrackets || e.Key == Key.OemCloseBrackets) e.Handled = true;
        }
    }

    internal class SqlComparison
    {
        public readonly string SqlFormat;
        public bool EscapeQuotes;

        public SqlComparison(string name, string sql)
        {
            Name = name;
            SqlFormat = sql;
        }

        public string Name { get; protected set; }

        public virtual string GetSql(string field, string value)
        {
            if (EscapeQuotes) value = value.Replace("'", "''");
            return string.Format(SqlFormat, "[" + field + "]", value);
        }
    }

    internal class IntegerComparison : SqlComparison
    {
        public IntegerComparison(string name, string sql)
            : base(name, sql)
        {
        }

        public override string GetSql(string field, string value)
        {
            int parsedValue;
            if (!int.TryParse(value, out parsedValue))
                return "1 = 0";
            value = parsedValue.ToString(CultureInfo.InvariantCulture);
            // Prevents culture related problem, e.g. someone enters 1,000 which is valid in english, or 1'000 (same in french)
            return string.Format(SqlFormat, field, value);
        }
    }

    internal class CharInComparison : SqlComparison
    {
        public CharInComparison(string name, string sql)
            : base(name, sql)
        {
        }

        public override string GetSql(string field, string value)
        {
            var sb = new StringBuilder();
            foreach (char c in value)
            {
                if (sb.Length > 0) sb.AppendFormat(",");
                sb.AppendFormat("'{0}'", c == '\'' ? "''" : (object) c);
            }
            return string.Format(SqlFormat, field, sb);
        }
    }
}