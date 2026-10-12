using System;
using System.Globalization;
using System.Windows.Data;

namespace Octgn.Utils.Converters
{
    /// <summary>
    /// Turns a date into friendly relative text ("3 days ago", "2 years ago").
    /// </summary>
    public class RelativeDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date;
            if (value is DateTimeOffset)
                date = ((DateTimeOffset)value).LocalDateTime;
            else if (value is DateTime)
                date = ((DateTime)value).ToLocalTime();
            else
                return value;

            var span = DateTime.Now - date;
            if (span.TotalSeconds < 0) return "Just now";
            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return Plural((int)span.TotalMinutes, "minute");
            if (span.TotalHours < 24) return Plural((int)span.TotalHours, "hour");
            if (span.TotalDays < 7) return Plural((int)span.TotalDays, "day");
            if (span.TotalDays < 30) return Plural((int)(span.TotalDays / 7), "week");
            if (span.TotalDays < 365) return Plural((int)(span.TotalDays / 30), "month");
            return Plural((int)(span.TotalDays / 365), "year");
        }

        private static string Plural(int n, string unit)
        {
            return n + " " + unit + (n == 1 ? "" : "s") + " ago";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}