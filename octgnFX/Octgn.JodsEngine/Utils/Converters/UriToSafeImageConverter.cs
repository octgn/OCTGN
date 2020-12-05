// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Octgn.Utils.Converters
{
    public class UriToSafeImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (string.IsNullOrEmpty(value.ToString()))
                return null;

            var bim = new BitmapImage();
            bim.BeginInit();
            bim.UriSource = new Uri(value.ToString());
            bim.CacheOption = BitmapCacheOption.OnLoad;
            bim.EndInit();
            return bim;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
