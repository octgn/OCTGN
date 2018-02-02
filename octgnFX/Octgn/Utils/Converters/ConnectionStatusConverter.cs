using Octgn.Communication;
using Octgn.Library.Localization;
using Octgn.Online;
using Octgn.Site.Api;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Octgn.Utils.Converters
{
    public class ConnectionStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            var status = (ConnectionStatus)value;

            if (targetType == typeof(Brush)) {
                switch (status) {
                    case ConnectionStatus.Disconnected:
                        return Brushes.LightSlateGray;
                    case ConnectionStatus.Connecting:
                        return Brushes.White;
                    case ConnectionStatus.Connected:
                        return Brushes.LightGreen;
                    default:
                        throw new NotImplementedException(status.ToString());
                }
            } else {
                switch (status) {
                    case ConnectionStatus.Disconnected:
                        return L.D.ConnectionStatus__Disconnected;
                    case ConnectionStatus.Connecting:
                        return L.D.ConnectionStatus__Connecting;
                    case ConnectionStatus.Connected:
                        var client = new ApiClient();
                        throw new NotImplementedException();
                        return string.Format(L.D.ConnectionStatus__Connected, ApiUserCache.Instance.OnlineUsers);
                    default:
                        throw new NotImplementedException(status.ToString());
                }
            }
            throw new NotImplementedException(status.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}