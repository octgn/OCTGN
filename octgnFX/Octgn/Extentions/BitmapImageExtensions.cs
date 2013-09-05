using System;
using System.Windows.Media.Imaging;

namespace Octgn.Extentions
{
    public static partial class StringExtensionMethods
    {
        public static BitmapImage BitmapFromUri(Uri uri)
        {
            var bim = new BitmapImage();
            bim.BeginInit();
            bim.CacheOption = BitmapCacheOption.OnLoad;
            bim.UriSource = uri;
            bim.EndInit();
            return bim;
        }
    }
}
