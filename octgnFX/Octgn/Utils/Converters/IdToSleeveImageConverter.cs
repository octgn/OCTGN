using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Octgn.Utils.Converters
{
    [ValueConversion(typeof(int), typeof(ImageSource))]
    public class IdToSleeveImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(ImageSource))
                throw new InvalidOperationException("The target must be a ImageSource");
			if(values.Length != 2)
				throw new InvalidOperationException("You must have two values");

            var defUrl = values[1] as string;
            if (defUrl == null)
                return null;

            if ((values[0] is int) == false || (int)values[0] <= 0)
                return GetFromUrl(defUrl);
            var sleeve = SleeveManager.Instance.SleeveFromId((int)values[0]);
            if (sleeve == null)
                return GetFromUrl(defUrl);

            if (string.IsNullOrWhiteSpace(sleeve.Url))
                return GetFromUrl(defUrl);

            return GetFromUrl(sleeve.Url);
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private ImageSource GetFromUrl(string url)
        {
            return new BitmapImage(new Uri(url, UriKind.RelativeOrAbsolute));
        }
    }
}