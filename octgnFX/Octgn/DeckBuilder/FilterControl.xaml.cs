using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IronPython.Modules;
using Octgn.Annotations;

namespace Octgn.DeckBuilder
{
    using System.Linq;

    public partial class FilterControl: INotifyPropertyChanged
    {
        private static readonly SqlComparison[] StringComparisons = new[]
                                            {
                                                new SqlComparison("Contains", "Card.[{0}] LIKE '%{1}%'") { EscapeQuotes = true },
                                                new SqlComparison("Does Not Contain", "Card.[{0}] NOT LIKE '%{1}%'") { EscapeQuotes = true },
                                                new SqlComparison("Starts with", "Card.[{0}] LIKE '{1}%'") { EscapeQuotes = true },
                                                new SqlComparison("Ends with", "Card.[{0}] LIKE '%{1}'") { EscapeQuotes = true },
                                                new SqlComparison("Equals", "Card.[{0}] = '{1}'") { EscapeQuotes = true}
                                            };

        private static readonly SqlComparison[] IntegerComparisons = new SqlComparison[]
                                            {
                                                new IntegerComparison("Equals", "Card.[{0}] = {1}"),
                                                new IntegerComparison("Greater than", "Card.[{0}] > {1}"),
                                                new IntegerComparison("Less than", "Card.[{0}] < {1}")
                                            };

        private static readonly SqlComparison[] CharComparisons = new[]
                                            {
                                                new SqlComparison("Equals", "Card.[{0}] = '{1}'") {EscapeQuotes = true},
                                                new SqlComparison("Greater than", "Card.[{0}] > '{1}'") {EscapeQuotes = true},
                                                new SqlComparison("Less than", "Card.[{0}] < '{1}'") {EscapeQuotes = true},
                                                new CharInComparison("One of", "Card.[{0}] IN ({1})")
                                            };

        public string CompareAgainstText
        {
            get { return _compareAgainstText; }
            set
            {
                if (value == _compareAgainstText) return;
                _compareAgainstText = value;
                OnPropertyChanged("CompareAgainstText");
                OnPropertyChanged("FilterButtonText");
            }
        }

        public string FilterButtonText
        {
            get
            {
                if (comparisonList != null)
                {
                    if (comparisonList.SelectedItem == null)
                    {
                        return "Null";
                    }
                    if (_property is SetPropertyDef)
                    {
                        return string.Format("Set Equals `{0}`", ((DataNew.Entities.Set) comparisonList.SelectedItem).Name);
                    }

                    return string.Format("{0} {1} `{2}`", _property.Name,
                        ((SqlComparison) comparisonList.SelectedItem).Name,
                        CompareAgainstText);
                }
                return "Null";
            }
        }

        public DataNew.Entities.PropertyDef Property
        {
            get
            {
                return _property;
            }
        }

        private DataNew.Entities.PropertyDef _property;

        public FilterControl()
        {
            InitializeComponent();
            DataContextChanged += delegate
                                      {
                                          _property = DataContext as DataNew.Entities.PropertyDef;
                                          if (_property == null) return; // Happens when the control is unloaded
                                          CreateComparisons();
                                      };
            LinkPopUp.IsOpen = true;
        }

        private void OnAnyLinkClicked(object sender, EventArgs eventArgs)
        {
            if (sender == this) return;
            this.ClosePopUp();
        }

        public void SetFromSave(DataNew.Entities.Game loadedGame, SearchFilterItem search)
        {
                comparisonText.Text = search.CompareValue;
            if (search.IsSetProperty)
            {
                comparisonList.SelectedItem =
                    comparisonList.Items.OfType<DataNew.Entities.Set>()
                                  .FirstOrDefault(x => x.Id == Guid.Parse(search.SelectedComparison));
            }
            else
            {
                comparisonList.SelectedItem =
                    comparisonList.Items.OfType<SqlComparison>()
                                  .FirstOrDefault(
                                      x =>
                                      x.Name.Equals(search.SelectedComparison, StringComparison.InvariantCultureIgnoreCase));
            }
            LinkPopUp.IsOpen = false;
            //}
        }

        public event EventHandler RemoveFilter;
        public event RoutedEventHandler UpdateFilters;

        public bool IsOr;
        private bool JustClosed;
        private string _linkText;
        private string _compareAgainstText;
        private SqlComparison _selectedComparison;

        public string GetSqlCondition()
        {
            if (comparisonList.SelectedItem == null)
            {
                return "";
            }
            if (_property is SetPropertyDef)
            {
                IsOr = true;
                return "set_id = '" + ((DataNew.Entities.Set)comparisonList.SelectedItem).Id.ToString("D") + "'";
            }
            
            return ((SqlComparison)comparisonList.SelectedItem).GetSql(_property.Name, comparisonText.Text);
        }

        private void CreateComparisons()
        {
            if (_property is SetPropertyDef)
            {
                IsOr = true;
                comparisonList.Width = 262;
                comparisonText.Visibility = Visibility.Collapsed;

                comparisonList.ItemsSource = ((SetPropertyDef)_property).Sets;
            }
            else
            {
                comparisonList.Width = 100;
                comparisonText.Visibility = Visibility.Visible;

                switch (_property.Type)
                {
                    case DataNew.Entities.PropertyType.String:
                        comparisonList.ItemsSource = StringComparisons;
                        break;
                    case DataNew.Entities.PropertyType.Integer:
                        comparisonList.ItemsSource = IntegerComparisons;
                        break;
                    case DataNew.Entities.PropertyType.Char:
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
            if (UpdateFilters != null)
                UpdateFilters(TemplatedParent, e);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private void ComparisonListChanged(object sender, SelectionChangedEventArgs e)
        {
            OnPropertyChanged("FilterButtonText");
        }

        private void FilterButtonClicked(object sender, RoutedEventArgs e)
        {
            if (JustClosed)
            {
                this.FilterButton.IsChecked = false;
                JustClosed = false;
            }
            else
            {
                LinkPopUp.IsOpen = true;
            }
        }

        public void ClosePopUp()
        {
            LinkPopUp.IsOpen = false;
        }

        private void LinkPopUp_Opened(object sender, EventArgs e)
        {
            if (comparisonText != null)
                comparisonText.Focus();
            this.FilterButton.IsChecked = true;

        }

        private void LinkPopUp_Closed(object sender, EventArgs e)
        {
            this.FilterButton.IsChecked = false;
            JustClosed = true;
            if (UpdateFilters != null)
                UpdateFilters(TemplatedParent, null);
        }

        private void FilterButton_MouseEnter(object sender, MouseEventArgs e)
        {
            JustClosed = false;
        }

        private void FilterKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                ClosePopUp();
                return;
            }

            if (UpdateFilters != null && Octgn.Core.Prefs.InstantSearch && !(KeysDown().Any()))
                UpdateFilters(sender, e);
        }
        //only search when all keys have been lifted. 
        //this should reduce the number of searches during rappid key presses
        //where lag would be most noticeable.
        private static System.Collections.Generic.IEnumerable<Key> KeysDown()
        {
            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                if (key != Key.None && Keyboard.IsKeyDown(key))
                    yield return key;
            }
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

        /// <summary>
        /// Escapes special SQL characters like '*%[]
        /// </summary>
        /// <param name="original">the unescaped character</param>
        /// <returns>The escaped replacement string</returns>
        protected String EscapeChar(char original)
        {
            switch (original)
            {
                case '\'':
                    return "''";

                case '*':
                    return "[*]";

                case '%':
                    return "[%]";

                case '[':
                    return "[[]";

                case ']':
                    return "[]]";

                default:
                    return original.ToString();
            }
        }

        public virtual string GetSql(string field, string value)
        {
            if (EscapeQuotes)
            {
                StringBuilder strb = new StringBuilder();
                foreach (char c in value)
                {
                    strb.Append(EscapeChar(c));
                }
                value = strb.ToString();
            }
            return string.Format(SqlFormat, field, value);
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
                sb.AppendFormat("'{0}'", EscapeChar(c));
            }
            return string.Format(SqlFormat, field, sb);
        }
    }
}